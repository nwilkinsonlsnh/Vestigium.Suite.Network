using System.Runtime.InteropServices;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static class MemoryHostFacts
{
    public static (string Total, string InUse) Read()
    {
        var mem = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref mem) || mem.TotalPhys == 0)
            return ("\u2014", "\u2014");

        var used = mem.TotalPhys > mem.AvailPhys ? mem.TotalPhys - mem.AvailPhys : 0;
        return (FormatGb(mem.TotalPhys), FormatGb(used));
    }

    private static string FormatGb(ulong bytes)
        => (bytes / 1073741824d).ToString("0.0") + " GB";

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx status);

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
}
