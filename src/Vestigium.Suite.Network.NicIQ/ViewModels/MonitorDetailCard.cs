using System.Globalization;
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
        return (nic?.Name ?? "Network", BaseNic(nic, ring, [
            new("Receive", Rate(rx, "Kbps")),
            new("Send", Rate(tx, "Kbps"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Packets(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, NetworkInterface.PacketsReceivedPerSec);
        var tx = Last(ring, NetworkInterface.PacketsSentPerSec);
        return (nic?.Name ?? "Packets", BaseNic(nic, ring, [
            new("Receive", Rate(rx, "pkt/s")),
            new("Send", Rate(tx, "pkt/s"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Utilization(AdapterRow? nic, MonitorRing ring)
    {
        var bytes = Last(ring, NetworkInterface.BytesTotalPerSec);
        var band = Last(ring, NetworkInterface.CurrentBandwidth);
        var pct = band > 0 ? 8m * bytes / band * 100m : 0m;
        return (Pct(pct), BaseNic(nic, ring, [
            new("Bandwidth", nic?.Speed ?? Dash),
            new("Total", Rate(bytes * 8m / 1000m, "Kbps"))
        ]));
    }

    private static (string, IReadOnlyList<MonitorDetailRow>) Integrity(AdapterRow? nic, MonitorRing ring)
    {
        var errors = Last(ring, NetworkInterface.PacketsReceivedErrors) + Last(ring, NetworkInterface.PacketsOutboundErrors);
        var discards = Last(ring, NetworkInterface.PacketsReceivedDiscarded) + Last(ring, NetworkInterface.PacketsOutboundDiscarded);
        var queue = Last(ring, NetworkInterface.OutputQueueLength);
        var headline = errors + discards + queue <= 0 ? "Clean" : "Attention";
        return (headline, BaseNic(nic, ring, [
            new("Errors", Whole(errors)),
            new("Discards", Whole(discards)),
            new("Queue", Whole(queue))
        ]));
    }

    private static IReadOnlyList<MonitorDetailRow> BaseNic(AdapterRow? nic, MonitorRing ring, IEnumerable<MonitorDetailRow> extra)
    {
        var rows = extra.ToList();
        if (nic is null)
            return rows;

        rows.Add(new("Status", nic.Status));
        rows.Add(new("Type", nic.Type));
        if (!string.IsNullOrWhiteSpace(nic.Speed))
            rows.Add(new("Link", nic.Speed));
        var ip = FirstAddress(nic, AddressFamily.InterNetwork);
        if (!string.IsNullOrWhiteSpace(ip))
            rows.Add(new("IPv4", ip));
        var ip6 = FirstAddress(nic, AddressFamily.InterNetworkV6);
        if (!string.IsNullOrWhiteSpace(ip6))
            rows.Add(new("IPv6", ShortIp6(ip6)));
        return rows;
    }

    private static string FirstAddress(AdapterRow nic, AddressFamily family)
        => nic.Source.UnicastAddresses.FirstOrDefault(a => a.Family == family)?.Address ?? string.Empty;

    private static string ShortIp6(string value)
        => value.Length <= 24 ? value : value[..22] + "\u2026";

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

    private const string Dash = "\u2014";
}
