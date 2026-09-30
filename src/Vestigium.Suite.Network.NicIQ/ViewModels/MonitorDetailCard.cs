using System.Globalization;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using PdhNic = Vestigium.Helpers.PerfMon.Network.NetworkInterface;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static class MonitorDetailCard
{
    public static (string Headline, IReadOnlyList<MonitorFactColumn> Columns) Build(
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

    private static (string, IReadOnlyList<MonitorFactColumn>) Cpu(MonitorRing ring)
    {
        var total = Last(ring, HostCounters.ProcessorTime);
        var live = CpuHostFacts.Live();
        var host = CpuHostFacts.Host;
        var columns = new MonitorFactColumn[]
        {
            Column(Row("Speed", live.Speed), Row("Utilization", Pct(total))),
            Column(Row("Processes", Count(live.Processes)), Row("Threads", Count(live.Threads)), Row("Handles", Count(live.Handles))),
            Column(Row("Sockets", Count(host.Sockets)), Row("Cores", Count(host.Cores)), Row("Logical processors", Count(host.Logical))),
            Column(Row("L1 cache", host.L1), Row("L2 cache", host.L2)),
            Column(Row("L3 cache", host.L3), Row("L4 cache", host.L4))
        };
        return (Pct(total), columns);
    }

    private static (string, IReadOnlyList<MonitorFactColumn>) Memory(MonitorRing ring)
    {
        var availableMb = Last(ring, HostCounters.AvailableMBytes);
        var committed = Last(ring, HostCounters.CommittedBytes);
        var limit = Last(ring, HostCounters.CommitLimit);
        var cached = Last(ring, HostCounters.CacheBytes);
        var pct = Last(ring, HostCounters.CommittedPct);
        var columns = new MonitorFactColumn[]
        {
            Column(Row("Available", GbFromMb(availableMb))),
            Column(Row("Committed", GbFromBytes(committed))),
            Column(Row("Commit limit", GbFromBytes(limit))),
            Column(Row("Cached", GbFromBytes(cached))),
            Column(Row("Commit in use", Pct(pct)))
        };
        return (GbFromMb(availableMb) + " free", columns);
    }

    private static (string, IReadOnlyList<MonitorFactColumn>) Network(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, PdhNic.BytesReceivedPerSec) * 8m / 1000m;
        var tx = Last(ring, PdhNic.BytesSentPerSec) * 8m / 1000m;
        return (Rate(rx + tx, "Kbps"), NicColumns(nic, Row("Send", Rate(tx, "Kbps")), Row("Receive", Rate(rx, "Kbps"))));
    }

    private static (string, IReadOnlyList<MonitorFactColumn>) Packets(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, PdhNic.PacketsReceivedPerSec);
        var tx = Last(ring, PdhNic.PacketsSentPerSec);
        return (Rate(rx + tx, "pkt/s"), NicColumns(nic, Row("Send", Rate(tx, "pkt/s")), Row("Receive", Rate(rx, "pkt/s"))));
    }

    private static (string, IReadOnlyList<MonitorFactColumn>) Utilization(AdapterRow? nic, MonitorRing ring)
    {
        var bytes = Last(ring, PdhNic.BytesTotalPerSec);
        var band = Last(ring, PdhNic.CurrentBandwidth);
        var pct = band > 0 ? 8m * bytes / band * 100m : 0m;
        return (Pct(pct), NicColumns(nic, Row("Total", Rate(bytes * 8m / 1000m, "Kbps")), null));
    }

    private static (string, IReadOnlyList<MonitorFactColumn>) Integrity(AdapterRow? nic, MonitorRing ring)
    {
        var errors = Last(ring, PdhNic.PacketsReceivedErrors) + Last(ring, PdhNic.PacketsOutboundErrors);
        var discards = Last(ring, PdhNic.PacketsReceivedDiscarded) + Last(ring, PdhNic.PacketsOutboundDiscarded);
        var queue = Last(ring, PdhNic.OutputQueueLength);
        var headline = errors + discards + queue <= 0 ? "Clean" : "Attention";
        return (headline, NicColumns(nic, Row("Errors", Whole(errors)), Row("Discards", Whole(discards))));
    }

    private static IReadOnlyList<MonitorFactColumn> NicColumns(AdapterRow? nic, MonitorDetailRow? topLive, MonitorDetailRow? bottomLive)
    {
        var wireless = WirelessLinkLookup.TryRead(nic);
        var ip = nic is null ? string.Empty : FirstAddress(nic, AddressFamily.InterNetwork);
        var columns = new List<MonitorFactColumn>();
        if (topLive is not null || bottomLive is not null)
        {
            var live = new List<MonitorDetailRow>();
            if (topLive is not null)
                live.Add(topLive);
            if (bottomLive is not null)
                live.Add(bottomLive);
            columns.Add(new MonitorFactColumn(live));
        }

        if (wireless is not null)
        {
            columns.Add(Column(Row("Connection type", wireless.ConnectionType), Row("SSID", wireless.Ssid)));
            columns.Add(Column(Row("IPv4", string.IsNullOrWhiteSpace(ip) ? "\u2014" : ip), Row("Signal", wireless.Signal)));
        }
        else
        {
            columns.Add(Column(Row("Connection type", nic is null ? "\u2014" : ConnectionType(nic))));
            columns.Add(Column(Row("IPv4", string.IsNullOrWhiteSpace(ip) ? "\u2014" : ip)));
        }

        columns.Add(Column(Row("Domain", nic is null ? "\u2014" : DomainName(nic))));
        return columns;
    }

    private static MonitorDetailRow Row(string label, string value)
        => new MonitorDetailRow(label, value);

    private static MonitorFactColumn Column(params MonitorDetailRow[] rows)
        => new MonitorFactColumn(rows);

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

        return "\u2014";
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

    private static string Count(int value)
        => value.ToString("N0", CultureInfo.CurrentCulture);

    private static string GbFromMb(decimal megaBytes)
        => (megaBytes / 1024m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";

    private static string GbFromBytes(decimal bytes)
        => (bytes / 1073741824m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";
}
