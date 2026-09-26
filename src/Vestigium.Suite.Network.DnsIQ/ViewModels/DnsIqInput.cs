using System.Net;
using System.Net.Sockets;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record DnsIqQuery(string Name, DnsLookupOptions Options, bool AllTypes);

public sealed record ServerOption(string Address, string Label);

public static class DnsIqInput
{
    public const int DefaultPort = 53;
    public const int MinPort = 1;
    public const int MaxPort = 65535;

    public static IReadOnlyList<string> RecordTypes { get; } =
    [
        "A", "AAAA", "CNAME", "MX", "NS", "PTR", "TXT", "SOA"
    ];

    public static IReadOnlyList<string> ComboTypes { get; } =
    [
        "All", "A", "AAAA", "CNAME", "MX", "NS", "PTR", "TXT", "SOA"
    ];

    public static IReadOnlyList<ServerOption> PublicServers { get; } =
    [
        new("8.8.8.8", "8.8.8.8  Google"),
        new("8.8.4.4", "8.8.4.4  Google"),
        new("1.1.1.1", "1.1.1.1  Cloudflare"),
        new("1.0.0.1", "1.0.0.1  Cloudflare"),
        new("9.9.9.9", "9.9.9.9  Quad9"),
        new("208.67.222.222", "208.67.222.222  OpenDNS")
    ];

    public static IReadOnlyList<ServerOption> ServerOptions()
    {
        var list = new List<ServerOption>();
        var system = FirstConfiguredDns();
        if (!string.IsNullOrWhiteSpace(system))
            list.Add(new ServerOption(system, $"{system}  (this PC)"));
        foreach (var item in PublicServers)
        {
            if (!list.Any(s => s.Address == item.Address))
                list.Add(item);
        }
        return list;
    }

    public static bool TryCreate(
        string? name,
        string? server,
        string? recordType,
        int interfaceIndex,
        string? sourceAddress,
        out DnsIqQuery? query,
        out string? reject)
        => TryCreate(name, server, recordType, interfaceIndex, sourceAddress, DefaultPort, out query, out reject);

    public static bool TryCreate(
        string? name,
        string? server,
        string? recordType,
        int interfaceIndex,
        string? sourceAddress,
        int port,
        out DnsIqQuery? query,
        out string? reject)
    {
        query = null;
        reject = null;

        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
            trimmedName = "localhost";

        if (!TryMapType(recordType, out var type, out var allTypes))
        {
            reject = "Type is not allowed.";
            return false;
        }

        if (interfaceIndex < 0)
        {
            reject = "Interface index cannot be negative.";
            return false;
        }

        if (port is < MinPort or > MaxPort)
        {
            reject = "Port must be 1–65535.";
            return false;
        }

        string? trimmedServer = NullIfBlank(server);
        if (trimmedServer is not null && !IPAddress.TryParse(trimmedServer, out _))
        {
            reject = "Server must be an IPv4 or IPv6 address.";
            return false;
        }

        string? trimmedSource = NullIfBlank(sourceAddress);
        if (trimmedSource is not null && !IPAddress.TryParse(trimmedSource, out _))
        {
            reject = "Source must be an IPv4 or IPv6 address.";
            return false;
        }

        query = new DnsIqQuery(
            trimmedName,
            new DnsLookupOptions
            {
                Type = type,
                Server = trimmedServer ?? FirstConfiguredDns(),
                InterfaceIndex = interfaceIndex,
                SourceAddress = trimmedSource,
                Port = port
            },
            allTypes);
        return true;
    }

    public static string DisplayType(DnsRecordType type) => type switch
    {
        DnsRecordType.A => "A",
        DnsRecordType.Aaaa => "AAAA",
        DnsRecordType.Cname => "CNAME",
        DnsRecordType.Mx => "MX",
        DnsRecordType.Ns => "NS",
        DnsRecordType.Ptr => "PTR",
        DnsRecordType.Txt => "TXT",
        DnsRecordType.Soa => "SOA",
        DnsRecordType.Srv => "SRV",
        DnsRecordType.Any => "ANY",
        _ => type.ToString()
    };

    public static string? FirstConfiguredDns()
    {
        try
        {
            return NetworkHelper.GetAdapters()
                .SelectMany(a => a.DnsServers)
                .Where(s => IPAddress.TryParse(s, out var ip) && !IPAddress.IsLoopback(ip))
                .OrderBy(s => IPAddress.Parse(s).AddressFamily == AddressFamily.InterNetwork ? 0 : 1)
                .FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? NullIfBlank(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool TryMapType(string? recordType, out DnsRecordType type, out bool allTypes)
    {
        type = DnsRecordType.A;
        allTypes = false;
        var key = recordType?.Trim();
        if (string.IsNullOrEmpty(key))
            return false;

        switch (key.ToUpperInvariant())
        {
            case "ALL":
                allTypes = true;
                type = DnsRecordType.A;
                return true;
            case "A":
                type = DnsRecordType.A;
                return true;
            case "AAAA":
                type = DnsRecordType.Aaaa;
                return true;
            case "CNAME":
                type = DnsRecordType.Cname;
                return true;
            case "MX":
                type = DnsRecordType.Mx;
                return true;
            case "NS":
                type = DnsRecordType.Ns;
                return true;
            case "PTR":
                type = DnsRecordType.Ptr;
                return true;
            case "TXT":
                type = DnsRecordType.Txt;
                return true;
            case "SOA":
                type = DnsRecordType.Soa;
                return true;
            default:
                return false;
        }
    }
}
