using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

internal static class NeighborTables
{
    private const int AfInet = 2;
    private const int AfInet6 = 23;
    private const int Ipv6NeighborRowSize = 88;

    public static NetworkNeighbor[] ReadIpv4()
        => ReadIpv4Rows().ToArray();

    public static NetworkNeighbor[] ReadIpv6()
        => ReadIpv6Rows().ToArray();

    public static IReadOnlyDictionary<int, string> Names()
    {
        var map = new Dictionary<int, string>();
        try
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var index = Index(nic);
                if (index >= 0)
                    map.TryAdd(index, nic.Name);
            }
        }
        catch (NetworkInformationException)
        {
        }

        return map;
    }

    private static IEnumerable<NetworkNeighbor> ReadIpv4Rows()
    {
        var size = 0;
        GetIpNetTable(IntPtr.Zero, ref size, false);
        if (size <= 0)
            yield break;
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            if (GetIpNetTable(buffer, ref size, false) != 0)
                yield break;
            var count = Marshal.ReadInt32(buffer);
            var offset = buffer + 4;
            for (var i = 0; i < count; i++)
            {
                var index = Marshal.ReadInt32(offset);
                var physLen = Marshal.ReadInt32(offset + 4);
                var mac = ReadMac(offset + 8, physLen);
                var addr = ReadIpv4(offset + 16);
                var type = Marshal.ReadInt32(offset + 20);
                yield return new NetworkNeighbor(AddressFamily.InterNetwork, addr, mac, null, NeighborType(type), index);
                offset += 24;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static IEnumerable<NetworkNeighbor> ReadIpv6Rows()
    {
        if (GetIpNetTable2(AfInet6, out var table) != 0 || table == 0)
            yield break;
        try
        {
            var count = Marshal.ReadInt32(table);
            var row = table + 8;
            for (var i = 0; i < count; i++)
            {
                var address = ReadSockAddress(row);
                var index = Marshal.ReadInt32(row + 28);
                var physLen = Marshal.ReadInt32(row + 72);
                var mac = ReadMac(row + 40, physLen);
                var state = Marshal.ReadInt32(row + 76);
                var flags = Marshal.ReadByte(row + 80);
                var lastReachable = unchecked((uint)Marshal.ReadInt32(row + 84));
                yield return new NetworkNeighbor(
                    AddressFamily.InterNetworkV6,
                    address,
                    mac,
                    null,
                    NeighborState(state),
                    index,
                    null,
                    (flags & 1) != 0,
                    (flags & 2) != 0,
                    lastReachable);
                row += Ipv6NeighborRowSize;
            }
        }
        finally
        {
            FreeMibTable(table);
        }
    }

    private static int Index(NetworkInterface nic)
    {
        try
        {
            var properties = nic.GetIPProperties();
            try
            {
                var v4 = properties.GetIPv4Properties();
                if (v4.Index != 0)
                    return v4.Index;
            }
            catch (NetworkInformationException)
            {
            }

            return properties.GetIPv6Properties().Index;
        }
        catch (NetworkInformationException)
        {
            return -1;
        }
    }

    private static string ReadSockAddress(nint ptr)
    {
        var family = Marshal.ReadInt16(ptr);
        if (family == AfInet)
            return ReadIpv4(ptr + 4);
        if (family != AfInet6)
            return string.Empty;
        var bytes = new byte[16];
        Marshal.Copy(ptr + 8, bytes, 0, 16);
        return new IPAddress(bytes).ToString();
    }

    private static string ReadIpv4(nint ptr)
    {
        var bytes = BitConverter.GetBytes(unchecked((uint)Marshal.ReadInt32(ptr)));
        return new IPAddress(bytes).ToString();
    }

    private static string? ReadMac(nint ptr, int length)
    {
        if (length <= 0)
            return null;
        var bytes = new byte[Math.Min(length, 8)];
        Marshal.Copy(ptr, bytes, 0, bytes.Length);
        return bytes.All(b => b == 0) ? null : string.Join(":", bytes.Take(6).Select(b => b.ToString("X2")));
    }

    private static string NeighborType(int type) => type switch
    {
        3 => "Dynamic",
        4 => "Static",
        2 => "Invalid",
        _ => "Other"
    };

    private static string NeighborState(int state) => state switch
    {
        1 => "Unreachable",
        2 => "Incomplete",
        3 => "Probe",
        4 => "Delay",
        5 => "Stale",
        6 => "Reachable",
        7 => "Permanent",
        _ => "Other"
    };

    [DllImport("iphlpapi.dll")]
    private static extern uint GetIpNetTable(nint table, ref int size, bool order);

    [DllImport("iphlpapi.dll")]
    private static extern int GetIpNetTable2(ushort family, out nint table);

    [DllImport("iphlpapi.dll")]
    private static extern void FreeMibTable(nint table);
}
