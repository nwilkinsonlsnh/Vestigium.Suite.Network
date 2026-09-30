using System.Runtime.InteropServices;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal sealed record CpuTopology(
    int Sockets,
    int Cores,
    int Logical,
    string L1,
    string L2,
    string L3,
    string L4);

internal sealed record CpuLive(int Processes, int Threads, int Handles, string Speed, string BaseSpeed);

internal static class CpuHostFacts
{
    private static readonly CpuTopology Topology = ReadTopology();
    private static CpuLive _live = new(0, 0, 0, Dash, Dash);
    private static DateTimeOffset _liveAt;
    private static readonly object Gate = new();

    public static CpuTopology Host => Topology;

    public static CpuLive Live()
    {
        lock (Gate)
        {
            if (DateTimeOffset.UtcNow - _liveAt < TimeSpan.FromSeconds(1))
                return _live;
            _live = ReadLive();
            _liveAt = DateTimeOffset.UtcNow;
            return _live;
        }
    }

    private static CpuLive ReadLive()
    {
        var processes = 0;
        var threads = 0;
        var handles = 0;
        var perf = new PerformanceInformation { Size = Marshal.SizeOf<PerformanceInformation>() };
        if (GetPerformanceInfo(out perf, perf.Size))
        {
            processes = perf.ProcessCount;
            threads = perf.ThreadCount;
            handles = perf.HandleCount;
        }

        var (speed, baseSpeed) = ReadSpeeds();
        return new CpuLive(processes, threads, handles, speed, baseSpeed);
    }

    private static (string Speed, string BaseSpeed) ReadSpeeds()
    {
        var count = Math.Max(1, Environment.ProcessorCount);
        var size = Marshal.SizeOf<ProcessorPowerInformation>() * count;
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            if (CallNtPowerInformation(11, nint.Zero, 0, buffer, size) != 0)
                return (Dash, Dash);
            var max = 0u;
            var current = 0u;
            for (var i = 0; i < count; i++)
            {
                var row = Marshal.PtrToStructure<ProcessorPowerInformation>(buffer + (i * Marshal.SizeOf<ProcessorPowerInformation>()));
                if (row.CurrentMhz > current)
                    current = row.CurrentMhz;
                if (row.MaxMhz > max)
                    max = row.MaxMhz;
            }

            if (current == 0)
                current = max;
            return (FormatGhz(current), FormatGhz(max));
        }
        catch (DllNotFoundException)
        {
            return (Dash, Dash);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string FormatGhz(uint mhz)
        => mhz == 0 ? Dash : (mhz / 1000d).ToString("0.00") + " GHz";

    private static CpuTopology ReadTopology()
    {
        var logical = Math.Max(1, Environment.ProcessorCount);
        var length = 0;
        GetLogicalProcessorInformationEx(RelationAll, nint.Zero, ref length);
        if (length <= 0)
            return new CpuTopology(1, logical, logical, Dash, Dash, Dash, Dash);

        var buffer = Marshal.AllocHGlobal(length);
        try
        {
            var size = length;
            if (!GetLogicalProcessorInformationEx(RelationAll, buffer, ref size))
                return new CpuTopology(1, logical, logical, Dash, Dash, Dash, Dash);

            var cores = 0;
            var sockets = 0;
            var l1 = 0L;
            var l2 = 0L;
            var l3 = 0L;
            var l4 = 0L;
            var offset = 0;
            while (offset + 8 <= size)
            {
                var relationship = Marshal.ReadInt32(buffer, offset);
                var block = Marshal.ReadInt32(buffer, offset + 4);
                if (block <= 0)
                    break;
                if (relationship == RelationProcessorCore)
                    cores++;
                else if (relationship == RelationProcessorPackage)
                    sockets++;
                else if (relationship == RelationCache && offset + 16 <= size)
                {
                    var level = Marshal.ReadByte(buffer, offset + 8);
                    var cacheSize = Marshal.ReadInt32(buffer, offset + 12);
                    if (cacheSize > 0)
                    {
                        switch (level)
                        {
                            case 1: l1 += cacheSize; break;
                            case 2: l2 += cacheSize; break;
                            case 3: l3 += cacheSize; break;
                            default: if (level >= 4) l4 += cacheSize; break;
                        }
                    }
                }

                offset += block;
            }

            return new CpuTopology(
                sockets > 0 ? sockets : 1,
                cores > 0 ? cores : logical,
                logical,
                FormatCache(l1),
                FormatCache(l2),
                FormatCache(l3),
                FormatCache(l4));
        }
        catch
        {
            return new CpuTopology(1, logical, logical, Dash, Dash, Dash, Dash);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string FormatCache(long bytes)
    {
        if (bytes <= 0)
            return Dash;
        var kilo = bytes / 1024d;
        if (kilo >= 1024)
            return (kilo / 1024d).ToString("0.0") + " MB";
        return Math.Round(kilo).ToString("0") + " KB";
    }

    private const string Dash = "\u2014";
    private const int RelationProcessorCore = 0;
    private const int RelationCache = 2;
    private const int RelationProcessorPackage = 3;
    private const int RelationAll = 0xFFFF;

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool GetPerformanceInfo(out PerformanceInformation info, int size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetLogicalProcessorInformationEx(int relationship, nint buffer, ref int length);

    [DllImport("powrprof.dll")]
    private static extern uint CallNtPowerInformation(int level, nint input, int inputSize, nint output, int outputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct PerformanceInformation
    {
        public int Size;
        public nint CommitTotal;
        public nint CommitLimit;
        public nint CommitPeak;
        public nint PhysicalTotal;
        public nint PhysicalAvailable;
        public nint SystemCache;
        public nint KernelTotal;
        public nint KernelPaged;
        public nint KernelNonpaged;
        public nint PageSize;
        public int HandleCount;
        public int ProcessCount;
        public int ThreadCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessorPowerInformation
    {
        public uint Number;
        public uint MaxMhz;
        public uint CurrentMhz;
        public uint MhzLimit;
        public uint MaxIdleState;
        public uint CurrentIdleState;
    }
}
