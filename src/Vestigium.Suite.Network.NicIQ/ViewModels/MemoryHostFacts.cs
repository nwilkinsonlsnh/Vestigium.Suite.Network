using System.Runtime.InteropServices;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal sealed record MemorySnapshot(
    string Total,
    string InUse,
    string Peak,
    string Paged,
    string Nonpaged);

/// <summary>Physical and commit snapshot. Host-owned. Not a PerfMon.Memory type.</summary>
internal static class MemoryHostFacts
{
    public static MemorySnapshot Read()
    {
        var total = 0UL;
        var used = 0UL;
        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (GlobalMemoryStatusEx(ref mem) && mem.TotalPhys > 0)
        {
            total = mem.TotalPhys;
            used = mem.TotalPhys > mem.AvailPhys ? mem.TotalPhys - mem.AvailPhys : 0;
        }

        var peak = 0UL;
        var paged = 0UL;
        var nonpaged = 0UL;
        var perf = new PerformanceInformation { Size = Marshal.SizeOf<PerformanceInformation>() };
        if (GetPerformanceInfo(out perf, perf.Size))
        {
            var page = perf.PageSize == 0 ? 4096UL : (ulong)perf.PageSize.ToInt64();
            peak = (ulong)Math.Max(0, perf.CommitPeak.ToInt64()) * page;
            paged = (ulong)Math.Max(0, perf.KernelPaged.ToInt64()) * page;
            nonpaged = (ulong)Math.Max(0, perf.KernelNonpaged.ToInt64()) * page;
        }

        return new MemorySnapshot(FormatGb(total), FormatGb(used), FormatGb(peak), FormatGb(paged), FormatGb(nonpaged));
    }

    private static string FormatGb(ulong bytes)
        => bytes == 0 ? "\u2014" : (bytes / 1073741824d).ToString("0.0") + " GB";

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx status);

    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool GetPerformanceInfo(out PerformanceInformation info, int size);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

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
}
