namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public static class ConnectionServices
{
    public static IReadOnlyList<PortName> Catalog { get; } = Rows();

    private static readonly Dictionary<string, string> Names = Build();

    public static string Label(string? protocol, int? remotePort, int localPort)
    {
        if (remotePort is > 0 && Try(protocol, remotePort.Value, out var remote))
            return remote;
        if (localPort > 0 && Try(protocol, localPort, out var local))
            return local;
        return "--";
    }

    private static bool Try(string? protocol, int port, out string name)
    {
        var key = (string.IsNullOrWhiteSpace(protocol) ? "TCP" : protocol.Trim()) + "|" + port;
        return Names.TryGetValue(key, out name!);
    }

    private static Dictionary<string, string> Build()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in Rows())
        {
            foreach (var protocol in row.Protocols.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                map[protocol + "|" + row.Port] = row.Name;
        }

        return map;
    }

    private static PortName[] Rows()
        =>
        [
            Row(1, "TCP", "tcpmux"),
            Row(7, "TCP, UDP", "echo"),
            Row(9, "TCP, UDP", "discard"),
            Row(11, "TCP", "systat"),
            Row(13, "TCP, UDP", "daytime"),
            Row(17, "TCP, UDP", "qotd"),
            Row(19, "TCP, UDP", "chargen"),
            Row(20, "TCP", "ftp-data"),
            Row(21, "TCP", "ftp"),
            Row(22, "TCP", "ssh"),
            Row(23, "TCP", "telnet"),
            Row(25, "TCP", "smtp"),
            Row(37, "TCP, UDP", "time"),
            Row(42, "TCP, UDP", "nameserver"),
            Row(43, "TCP", "whois"),
            Row(49, "TCP, UDP", "tacacs"),
            Row(53, "TCP, UDP", "dns"),
            Row(67, "UDP", "dhcp-server"),
            Row(68, "UDP", "dhcp-client"),
            Row(69, "UDP", "tftp"),
            Row(70, "TCP", "gopher"),
            Row(79, "TCP", "finger"),
            Row(80, "TCP", "http"),
            Row(88, "TCP, UDP", "kerberos"),
            Row(102, "TCP", "iso-tsap"),
            Row(110, "TCP", "pop3"),
            Row(111, "TCP, UDP", "portmap"),
            Row(113, "TCP", "ident"),
            Row(119, "TCP", "nntp"),
            Row(123, "UDP", "ntp"),
            Row(135, "TCP", "rpc"),
            Row(137, "UDP", "netbios-ns"),
            Row(138, "UDP", "netbios-dgm"),
            Row(139, "TCP", "netbios-ssn"),
            Row(143, "TCP", "imap"),
            Row(161, "UDP", "snmp"),
            Row(162, "UDP", "snmptrap"),
            Row(179, "TCP", "bgp"),
            Row(194, "TCP", "irc"),
            Row(220, "TCP", "imap3"),
            Row(389, "TCP, UDP", "ldap"),
            Row(427, "TCP, UDP", "svrloc"),
            Row(443, "TCP", "https"),
            Row(443, "UDP", "quic"),
            Row(445, "TCP", "smb"),
            Row(464, "TCP, UDP", "kpasswd"),
            Row(465, "TCP", "smtps"),
            Row(500, "UDP", "ike"),
            Row(512, "TCP", "rexec"),
            Row(513, "TCP", "rlogin"),
            Row(514, "TCP", "rsh"),
            Row(514, "UDP", "syslog"),
            Row(515, "TCP", "lpd"),
            Row(520, "UDP", "rip"),
            Row(524, "TCP, UDP", "ncp"),
            Row(540, "TCP", "uucp"),
            Row(546, "UDP", "dhcpv6-client"),
            Row(547, "UDP", "dhcpv6-server"),
            Row(548, "TCP", "afp"),
            Row(554, "TCP, UDP", "rtsp"),
            Row(563, "TCP", "nntps"),
            Row(587, "TCP", "submission"),
            Row(593, "TCP", "rpc-http"),
            Row(631, "TCP, UDP", "ipp"),
            Row(636, "TCP", "ldaps"),
            Row(646, "TCP, UDP", "ldp"),
            Row(647, "TCP", "dhcp-failover"),
            Row(691, "TCP", "msexch-routing"),
            Row(749, "TCP", "kerberos-adm"),
            Row(853, "TCP", "dns-over-tls"),
            Row(860, "TCP", "iscsi"),
            Row(873, "TCP", "rsync"),
            Row(989, "TCP", "ftps-data"),
            Row(990, "TCP", "ftps"),
            Row(992, "TCP", "telnets"),
            Row(993, "TCP", "imaps"),
            Row(995, "TCP", "pop3s"),
            Row(1433, "TCP", "mssql"),
            Row(1521, "TCP", "oracle"),
            Row(1723, "TCP", "pptp"),
            Row(1812, "UDP", "radius"),
            Row(1813, "UDP", "radius-acct"),
            Row(1900, "UDP", "ssdp"),
            Row(2049, "TCP, UDP", "nfs"),
            Row(3268, "TCP", "gc"),
            Row(3269, "TCP", "gc-ssl"),
            Row(3306, "TCP", "mysql"),
            Row(3389, "TCP, UDP", "rdp"),
            Row(4500, "UDP", "ike-nat"),
            Row(5353, "UDP", "mdns"),
            Row(5355, "UDP", "llmnr"),
            Row(5432, "TCP", "postgres"),
            Row(5900, "TCP", "vnc"),
            Row(5985, "TCP", "winrm"),
            Row(5986, "TCP", "winrm-ssl"),
            Row(6379, "TCP", "redis"),
            Row(8080, "TCP", "http-alt"),
            Row(8443, "TCP", "https-alt"),
            Row(9100, "TCP", "jetdirect"),
            Row(27017, "TCP", "mongo")
        ];

    private static PortName Row(int port, string protocols, string name)
        => new(port, protocols, name);

    public readonly record struct PortName(int Port, string Protocols, string Name);
}
