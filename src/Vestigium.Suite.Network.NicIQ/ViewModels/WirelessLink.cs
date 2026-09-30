using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal sealed record WirelessLink(string Ssid, string ConnectionType, int Quality, string Signal);

internal static class WirelessLinkLookup
{
    private static readonly object Gate = new();
    private static string? _cachedId;
    private static WirelessLink? _cached;
    private static DateTimeOffset _cachedAt;

    public static WirelessLink? TryRead(AdapterRow? nic)
    {
        if (nic is null || nic.Source.Type != NetworkInterfaceType.Wireless80211)
            return null;

        var id = nic.Id;
        lock (Gate)
        {
            if (string.Equals(_cachedId, id, StringComparison.OrdinalIgnoreCase)
                && DateTimeOffset.UtcNow - _cachedAt < TimeSpan.FromSeconds(2))
            {
                return _cached;
            }
        }

        var link = Query(id);
        lock (Gate)
        {
            _cachedId = id;
            _cached = link;
            _cachedAt = DateTimeOffset.UtcNow;
        }

        return link;
    }

    private static WirelessLink? Query(string adapterId)
    {
        if (!Guid.TryParse(adapterId.Trim('{', '}'), out var guid))
            return null;

        var handle = IntPtr.Zero;
        var list = IntPtr.Zero;
        try
        {
            if (WlanOpenHandle(2, IntPtr.Zero, out _, out handle) != 0)
                return null;
            if (WlanEnumInterfaces(handle, IntPtr.Zero, out list) != 0 || list == IntPtr.Zero)
                return null;

            var count = Marshal.ReadInt32(list);
            var item = list + 8;
            for (var i = 0; i < count; i++)
            {
                var info = Marshal.PtrToStructure<WlanInterfaceInfo>(item)!;
                item += Marshal.SizeOf<WlanInterfaceInfo>();
                if (info.InterfaceGuid != guid)
                    continue;

                var data = IntPtr.Zero;
                try
                {
                    if (WlanQueryInterface(
                            handle,
                            ref guid,
                            WlanIntfOpcodeCurrentConnection,
                            IntPtr.Zero,
                            out _,
                            out data,
                            out _) != 0
                        || data == IntPtr.Zero)
                    {
                        return null;
                    }

                    var connection = Marshal.PtrToStructure<WlanConnectionAttributes>(data);
                    var ssid = ReadSsid(connection.Association.Ssid);
                    var quality = Math.Clamp((int)connection.Association.SignalQuality, 0, 100);
                    return new WirelessLink(
                        string.IsNullOrWhiteSpace(ssid) ? "—" : ssid,
                        PhyName(connection.Association.PhyType),
                        quality,
                        SignalLabel(quality));
                }
                finally
                {
                    if (data != IntPtr.Zero)
                        WlanFreeMemory(data);
                }
            }
        }
        catch (DllNotFoundException)
        {
            return null;
        }
        catch (EntryPointNotFoundException)
        {
            return null;
        }
        finally
        {
            if (list != IntPtr.Zero)
                WlanFreeMemory(list);
            if (handle != IntPtr.Zero)
                WlanCloseHandle(handle, IntPtr.Zero);
        }

        return null;
    }

    private static string ReadSsid(Dot11Ssid ssid)
    {
        var length = (int)Math.Clamp(ssid.Length, 0, 32);
        if (length == 0 || ssid.Bytes is null)
            return string.Empty;
        return Encoding.UTF8.GetString(ssid.Bytes, 0, length).Trim().Trim('\0');
    }

    private static string PhyName(uint phy) => phy switch
    {
        4 or 5 => "802.11a",
        3 => "802.11b",
        6 => "802.11g",
        7 => "802.11n",
        8 => "802.11ac",
        10 => "802.11ax",
        11 => "802.11be",
        _ => "Wi-Fi"
    };

    private static string SignalLabel(int quality)
    {
        var word = quality >= 80 ? "Excellent"
            : quality >= 60 ? "Good"
            : quality >= 40 ? "Fair"
            : quality >= 20 ? "Weak"
            : "Poor";
        return $"{word}  {quality}%";
    }

    private const uint WlanIntfOpcodeCurrentConnection = 7;

    [DllImport("wlanapi.dll")]
    private static extern uint WlanOpenHandle(uint clientVersion, IntPtr reserved, out uint negotiated, out IntPtr handle);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanCloseHandle(IntPtr handle, IntPtr reserved);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanEnumInterfaces(IntPtr handle, IntPtr reserved, out IntPtr list);

    [DllImport("wlanapi.dll")]
    private static extern uint WlanQueryInterface(
        IntPtr handle,
        ref Guid interfaceGuid,
        uint opcode,
        IntPtr reserved,
        out uint dataSize,
        out IntPtr data,
        out uint opcodeValueType);

    [DllImport("wlanapi.dll")]
    private static extern void WlanFreeMemory(IntPtr memory);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WlanInterfaceInfo
    {
        public Guid InterfaceGuid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;
        public uint State;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Dot11Ssid
    {
        public uint Length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Bytes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WlanAssociationAttributes
    {
        public Dot11Ssid Ssid;
        public uint BssType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Mac;
        public uint PhyType;
        public uint PhyIndex;
        public uint SignalQuality;
        public uint RxRate;
        public uint TxRate;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WlanSecurityAttributes
    {
        [MarshalAs(UnmanagedType.Bool)] public bool SecurityEnabled;
        [MarshalAs(UnmanagedType.Bool)] public bool OneXEnabled;
        public uint AuthAlgorithm;
        public uint CipherAlgorithm;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WlanConnectionAttributes
    {
        public uint IsState;
        public uint ConnectionMode;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string ProfileName;
        public WlanAssociationAttributes Association;
        public WlanSecurityAttributes Security;
    }
}
