namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

public sealed class IpConfigRow
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Status { get; init; } = "";
    public string Mac { get; init; } = "";
    public string Addresses { get; init; } = "";
    public string Gateways { get; init; } = "";
    public string Dns { get; init; } = "";
    public string Dhcp { get; init; } = "";
}

public sealed class DnsConfigRow
{
    public string Name { get; init; } = "";
    public string DnsServers { get; init; } = "";
    public string Suffix { get; init; } = "";
}

public sealed class NicRow
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Mac { get; init; } = "";
    public string Status { get; init; } = "";
    public string SpeedMbps { get; init; } = "";
    public string Type { get; init; } = "";
}

public sealed class RouteRow
{
    public string Destination { get; init; } = "";
    public string Mask { get; init; } = "";
    public string NextHop { get; init; } = "";
    public string InterfaceIndex { get; init; } = "";
    public string Metric { get; init; } = "";
}

public sealed class ArpRow
{
    public string Address { get; init; } = "";
    public string Mac { get; init; } = "";
    public string State { get; init; } = "";
    public string InterfaceIndex { get; init; } = "";
}

public sealed class ListenerRow
{
    public string Protocol { get; init; } = "";
    public string LocalAddress { get; init; } = "";
    public string Port { get; init; } = "";
    public string Process { get; init; } = "";
}

public sealed class TcpRow
{
    public string LocalAddress { get; init; } = "";
    public string LocalPort { get; init; } = "";
    public string RemoteAddress { get; init; } = "";
    public string RemotePort { get; init; } = "";
    public string State { get; init; } = "";
    public string Process { get; init; } = "";
}

public sealed class NetworkSnapshot
{
    public const int ArpCap = 500;
    public const int TcpCap = 1000;

    public List<IpConfigRow> Ip { get; } = new();
    public List<DnsConfigRow> Dns { get; } = new();
    public List<NicRow> Nics { get; } = new();
    public List<RouteRow> Routes { get; } = new();
    public List<ArpRow> Arp { get; } = new();
    public List<ListenerRow> Listeners { get; } = new();
    public List<TcpRow> Tcp { get; } = new();
    public bool ArpTruncated { get; set; }
    public bool TcpTruncated { get; set; }
    public string? Error { get; set; }
}
