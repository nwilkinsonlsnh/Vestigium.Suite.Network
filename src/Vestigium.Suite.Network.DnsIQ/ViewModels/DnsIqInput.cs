using System.Net;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record DnsIqQuery(string Name, DnsLookupOptions Options);

public static class DnsIqInput
{
    public static IReadOnlyList<string> RecordTypes { get; } =
    [
        "A", "AAAA", "CNAME", "MX", "NS", "PTR", "TXT", "SOA"
    ];

    public static bool TryCreate(
        string? name,
        string? server,
        string? recordType,
        int interfaceIndex,
        string? sourceAddress,
        out DnsIqQuery? query,
        out string? reject)
    {
        query = null;
        reject = null;

        var trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
        {
            reject = "Name is required.";
            return false;
        }

        if (!TryMapType(recordType, out var type))
        {
            reject = "Type is not allowed.";
            return false;
        }

        if (interfaceIndex < 0)
        {
            reject = "Interface index cannot be negative.";
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
                Server = trimmedServer,
                InterfaceIndex = interfaceIndex,
                SourceAddress = trimmedSource
            });
        return true;
    }

    private static string? NullIfBlank(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool TryMapType(string? recordType, out DnsRecordType type)
    {
        type = DnsRecordType.A;
        var key = recordType?.Trim();
        if (string.IsNullOrEmpty(key))
            return false;

        switch (key.ToUpperInvariant())
        {
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
