using System.Globalization;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Vestigium.Helpers.PerfMon.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static class MonitorDetailCard
{
    public static (string Headline, IReadOnlyList<MonitorDetailRow> Rows) Build(
        string page,
        AdapterRow? nic,
        MonitorRing ring)
    {
        ArgumentNullException.ThrowIfNull(ring);
        return page switch
        {
            MonitorChartPages.Cpu => Cpu(ring),
            MonitorChartPages.Memory => Memory(ring),
            MonitorChartPages.Integrity => Integrity(nic, ring),
            MonitorChartPages.Packets => Packets(nic, ring),
            MonitorChartPages.Utilization => Utilization(nic, ring),
            _ => Network(nic, ring)
        };
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Cpu(MonitorRing ring)
    {
        var total = Last(ring, HostCounters.ProcessorTime);
        var user = Last(ring, HostCounters.UserTime);
        var priv = Last(ring, HostCounters.PrivilegedTime);
        var rows = new List<MonitorDetailRow>
        {
            new("User", Pct(user)),
            new("Privileged", Pct(priv)),
            new("Logical processors", Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture)),
            new("64-bit OS", Environment.Is64BitOperatingSystem ? "Yes" : "No")
        };
        return (Pct(total), rows);
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Memory(MonitorRing ring)
    {
        var availableMb = Last(ring, HostCounters.AvailableMBytes);
        var committed = Last(ring, HostCounters.CommittedBytes);
        var limit = Last(ring, HostCounters.CommitLimit);
        var cached = Last(ring, HostCounters.CacheBytes);
        var pct = Last(ring, HostCounters.CommittedPct);
        var rows = new List<MonitorDetailRow>
        {
            new("Available", GbFromMb(availableMb)),
            new("Committed", GbFromBytes(committed)),
            new("Commit limit", GbFromBytes(limit)),
            new("Cached", GbFromBytes(cached)),
            new("Commit in use", Pct(pct))
        };
        return (GbFromMb(availableMb) + " free", rows);
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Network(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, NetworkInterface.BytesReceivedPerSec) * 8m / 1000m;
        var tx = Last(ring, NetworkInterface.BytesSentPerSec) * 8m / 1000m;
        return (Rate(rx + tx, "Kbps"), NicFacts(nic, [
            new("Receive", Rate(rx, "Kbps")),
            new("Send", Rate(tx, "Kbps"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Packets(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, NetworkInterface.PacketsReceivedPerSec);
        var tx = Last(ring, NetworkInterface.PacketsSentPerSec);
        return (Rate(rx + tx, "pkt/s"), NicFacts(nic, [
            new("Receive", Rate(rx, "pkt/s")),
            new("Send", Rate(tx, "pkt/s"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Utilization(AdapterRow? nic, MonitorRing ring)
    {
        var bytes = Last(ring, NetworkInterface.BytesTotalPerSec);
        var band = Last(ring, NetworkInterface.CurrentBandwidth);
        var pct = band > 0 ? 8m * bytes / band * 100m : 0m;
        return (Pct(pct), NicFacts(nic, [
            new("Total", Rate(bytes * 8m / 1000m, "Kbps"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Integrity(AdapterRow? nic, MonitorRing ring)
    {
        var errors = Last(ring, NetworkInterface.PacketsReceivedErrors) + Last(ring, NetworkInterface.PacketsOutboundErrors);
        var discards = Last(ring, NetworkInterface.PacketsReceivedDiscarded) + Last(ring, NetworkInterface.PacketsOutboundDiscarded);
        var queue = Last(ring, NetworkInterface.OutputQueueLength);
        var headline = errors + discards + queue <= 0 ? "Clean" : "Attention";
        return (headline, NicFacts(nic, [
            new("Errors", Whole(errors)),
            new("Discards", Whole(discards)),
            new("Queue", Whole(queue))
        ]));
    }

    private static IReadOnlyList<MonitorDetailRow> NicFacts(AdapterRow? nic, IEnumerable<MonitorDetailRow> live)
    {
        var rows = live.ToList();
        if (nic is null)
            return rows;

        var wireless = WirelessLinkLookup.TryRead(nic);
        if (wireless is not null)
        {
            rows.Add(new("SSID", wireless.Ssid));
            rows.Add(new("Connection type", wireless.ConnectionType));
            rows.Add(new("Signal", wireless.Signal));
        }
        else
        {
            rows.Add(new("Connection type", ConnectionType(nic)));
        }

        rows.Add(new("Domain", DomainName(nic)));
        var ip = FirstAddress(nic, AddressFamily.InterNetwork);
        if (!string.IsNullOrWhiteSpace(ip))
            rows.Add(new("IPv4", ip));
        return rows;
    }

    private static string ConnectionType(AdapterRow nic)
    {
        if (!string.IsNullOrWhiteSpace(nic.Type) && !string.Equals(nic.Type, nic.Source.Type.ToString(), StringComparison.Ordinal))
            return nic.Type;
        return nic.Source.Type switch
        {
            NetworkInterfaceType.Ethernet or NetworkInterfaceType.GigabitEthernet => "Ethernet",
            NetworkInterfaceType.Wireless80211 => "Wi-Fi",
            NetworkInterfaceType.Loopback => "Loopback",
            NetworkInterfaceType.Tunnel => "Tunnel",
            NetworkInterfaceType.Ppp => "PPP",
            _ => nic.Type
        };
    }

    private static string DomainName(AdapterRow nic)
    {
        if (!string.IsNullOrWhiteSpace(nic.Source.DnsSuffix))
            return nic.Source.DnsSuffix.Trim();
        try
        {
            var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (!string.IsNullOrWhiteSpace(domain))
                return domain.Trim();
        }
        catch (NetworkInformationException)
        {
        }

        return "—";
    }

    private static string FirstAddress(AdapterRow nic, AddressFamily family)
        => nic.Source.UnicastAddresses.FirstOrDefault(a => a.Family == family)?.Address ?? string.Empty;

    private static decimal Last(MonitorRing ring, string counter)
    {
        var rows = ring.Of(counter);
        return rows.Count == 0 ? 0 : rows[^1].Value;
    }

    private static string Pct(decimal value)
        => value.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    private static string Rate(decimal value, string unit)
        => value.ToString("0.###", CultureInfo.InvariantCulture) + " " + unit;

    private static string Whole(decimal value)
        => decimal.Truncate(value).ToString("0", CultureInfo.InvariantCulture);

    private static string GbFromMb(decimal megaBytes)
        => (megaBytes / 1024m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";

    private static string GbFromBytes(decimal bytes)
        => (bytes / 1073741824m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
}
