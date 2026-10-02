using System.Globalization;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Vestigium.Helpers.Network;
using Vestigium.Helpers.PerfMon.Cpu;
using Vestigium.Helpers.PerfMon.Memory;
using Vestigium.Helpers.SystemInfo;
using CpuInfo = Vestigium.Helpers.SystemInfo.Cpu.CpuFacts;
using MemoryInfo = Vestigium.Helpers.SystemInfo.Memory.MemoryFacts;
using PdhNic = Vestigium.Helpers.PerfMon.Network.NetworkInterface;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static class MonitorDetailCard
{
    private const string Dash = "\u2014";

    public static (string Title, string Value, IReadOnlyList<MonitorFactColumn> Columns) Build(
        string page,
        AdapterRow? nic,
        MonitorRing ring)
    {
        ArgumentNullException.ThrowIfNull(ring);
        var built = page switch
        {
            MonitorChartPages.Cpu => Cpu(ring),
            MonitorChartPages.Memory => MemoryPage(ring),
            MonitorChartPages.Integrity => Integrity(nic, ring),
            MonitorChartPages.Packets => Packets(nic, ring),
            MonitorChartPages.Utilization => Utilization(nic, ring),
            _ => Network(nic, ring)
        };
        if (page is MonitorChartPages.Cpu or MonitorChartPages.Memory)
            return built;

        var name = string.IsNullOrWhiteSpace(nic?.Name) ? "Network" : nic.Name.Trim();
        return (name, built.Item2, built.Item3);
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) Cpu(MonitorRing ring)
    {
        var total = Last(ring, Processor.PercentProcessorTime);
        var live = CpuInfo.Live();
        var host = CpuInfo.Host;
        var columns = new MonitorFactColumn[]
        {
            Column(Row("Base speed", Ghz(live, v => v.MaxMhz)), Row("Speed", Ghz(live, v => v.CurrentMhz)), Row("Utilization", Pct(total))),
            Column(Row("Processes", CountOf(live, v => v.Processes)), Row("Threads", CountOf(live, v => v.Threads)), Row("Handles", CountOf(live, v => v.Handles))),
            Column(Row("Sockets", CountOf(host, v => v.Sockets)), Row("Cores", CountOf(host, v => v.Cores)), Row("Logical processors", CountOf(host, v => v.Logical))),
            Column(Row("L1 cache", Cache(host, v => v.L1Bytes)), Row("L2 cache", Cache(host, v => v.L2Bytes))),
            Column(Row("L3 cache", Cache(host, v => v.L3Bytes)), Row("L4 cache", Cache(host, v => v.L4Bytes)))
        };
        return ("Utilization", Pct(total), columns);
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) MemoryPage(MonitorRing ring)
    {
        var availableMb = Last(ring, Memory.AvailableMBytes);
        var committed = Last(ring, Memory.CommittedBytes);
        var limit = Last(ring, Memory.CommitLimit);
        var cached = Last(ring, Memory.CacheBytes);
        var pct = Last(ring, Memory.PercentCommittedBytesInUse);
        var phys = MemoryInfo.Read();
        var columns = new MonitorFactColumn[]
        {
            Column(Row("In use", Gb(phys.InUseBytes)), Row("Total", Gb(phys.TotalBytes))),
            Column(Row("Committed", GbFromBytes(committed)), Row("Commit peak", Gb(phys.CommitPeakBytes))),
            Column(Row("Commit limit", GbFromBytes(limit)), Row("Commit in use", Pct(pct))),
            Column(Row("Cached", GbFromBytes(cached)), Row("Paged pool", Gb(phys.PagedBytes))),
            Column(Row("Non-paged pool", Gb(phys.NonpagedBytes)))
        };
        return ("Available", GbFromMb(availableMb), columns);
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) Network(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, PdhNic.BytesReceivedPerSec) * 8m / 1000m;
        var tx = Last(ring, PdhNic.BytesSentPerSec) * 8m / 1000m;
        return ("Kbps", Number(rx + tx), NicColumns(nic, Row("Send", Rate(tx, "Kbps")), Row("Receive", Rate(rx, "Kbps"))));
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) Packets(AdapterRow? nic, MonitorRing ring)
    {
        var rx = Last(ring, PdhNic.PacketsReceivedPerSec);
        var tx = Last(ring, PdhNic.PacketsSentPerSec);
        return ("pkt/s", Number(rx + tx), NicColumns(nic, Row("Send", Rate(tx, "pkt/s")), Row("Receive", Rate(rx, "pkt/s"))));
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) Utilization(AdapterRow? nic, MonitorRing ring)
    {
        var bytes = Last(ring, PdhNic.BytesTotalPerSec);
        var band = Last(ring, PdhNic.CurrentBandwidth);
        var pct = band > 0 ? 8m * bytes / band * 100m : 0m;
        return ("Utilization", Pct(pct), NicColumns(nic, Row("Total", Rate(bytes * 8m / 1000m, "Kbps")), Row("Link", LinkLabel(nic, band))));
    }

    private static (string, string, IReadOnlyList<MonitorFactColumn>) Integrity(AdapterRow? nic, MonitorRing ring)
    {
        var errors = Last(ring, PdhNic.PacketsReceivedErrors) + Last(ring, PdhNic.PacketsOutboundErrors);
        var discards = Last(ring, PdhNic.PacketsReceivedDiscarded) + Last(ring, PdhNic.PacketsOutboundDiscarded);
        var queue = Last(ring, PdhNic.OutputQueueLength);
        var value = errors + discards + queue <= 0 ? "Clean" : "Attention";
        return (string.Empty, value, NicColumns(nic, Row("Errors", Whole(errors)), Row("Discards", Whole(discards))));
    }

    private static IReadOnlyList<MonitorFactColumn> NicColumns(AdapterRow? nic, MonitorDetailRow? topLive, MonitorDetailRow? bottomLive)
    {
        var wireless = NetworkHelper.TryWirelessAssociation(nic?.Source);
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
            columns.Add(Column(Row("Connection type", wireless.Phy), Row("SSID", wireless.Ssid)));
            columns.Add(Column(Row("IPv4", string.IsNullOrWhiteSpace(ip) ? Dash : ip), Row("Signal", SignalLabel(wireless.Quality))));
        }
        else
        {
            columns.Add(Column(Row("Connection type", nic is null ? Dash : ConnectionType(nic))));
            columns.Add(Column(Row("IPv4", string.IsNullOrWhiteSpace(ip) ? Dash : ip)));
        }

        columns.Add(Column(Row("Domain", nic is null ? Dash : DomainName(nic))));
        return columns;
    }

    private static string SignalLabel(int quality)
    {
        var word = quality >= 80 ? "Excellent"
            : quality >= 60 ? "Good"
            : quality >= 40 ? "Fair"
            : quality >= 20 ? "Weak"
            : "Poor";
        return $"{word}  {quality}%";
    }

    private static MonitorDetailRow Row(string label, string value)
        => new MonitorDetailRow(label, value);

    private static MonitorFactColumn Column(params MonitorDetailRow[] rows)
        => new MonitorFactColumn(rows);

    private static string LinkLabel(AdapterRow? nic, decimal bandwidthBits)
    {
        if (!string.IsNullOrWhiteSpace(nic?.Speed))
            return nic.Speed;
        if (bandwidthBits <= 0)
            return Dash;
        var bits = (long)decimal.Truncate(bandwidthBits);
        return bits <= 0 ? Dash : LinkSpeed.Format(bits);
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

        return Dash;
    }

    private static string FirstAddress(AdapterRow nic, AddressFamily family)
        => nic.Source.UnicastAddresses.FirstOrDefault(a => a.Family == family)?.Address ?? string.Empty;

    private static decimal Last(MonitorRing ring, string counter)
    {
        var rows = ring.Of(counter);
        return rows.Count == 0 ? 0 : rows[^1].Value;
    }

    private static string Pct(decimal value)
        => value < 1m
            ? value.ToString("0.###", CultureInfo.InvariantCulture) + "%"
            : value.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    private static string Number(decimal value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Rate(decimal value, string unit)
        => value.ToString("0.###", CultureInfo.InvariantCulture) + " " + unit;

    private static string Whole(decimal value)
        => decimal.Truncate(value).ToString("0", CultureInfo.InvariantCulture);

    private static string GbFromMb(decimal megaBytes)
        => (megaBytes / 1024m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";

    private static string GbFromBytes(decimal bytes)
        => (bytes / 1073741824m).ToString("0.0", CultureInfo.InvariantCulture) + " GB";

    private static string CountOf<T>(Fact<T> fact, Func<T, int> pick)
        => fact.IsOk ? pick(fact.Value).ToString("N0", CultureInfo.CurrentCulture) : Dash;

    private static string Ghz<T>(Fact<T> fact, Func<T, uint> mhz)
    {
        if (!fact.IsOk)
            return Dash;
        var value = mhz(fact.Value);
        return value == 0 ? Dash : (value / 1000d).ToString("0.00") + " GHz";
    }

    private static string Cache<T>(Fact<T> fact, Func<T, long> bytes)
    {
        if (!fact.IsOk)
            return Dash;
        var value = bytes(fact.Value);
        if (value <= 0)
            return Dash;
        var kilo = value / 1024d;
        return kilo >= 1024 ? (kilo / 1024d).ToString("0.0") + " MB" : Math.Round(kilo).ToString("0") + " KB";
    }

    private static string Gb(Fact<ulong> fact)
        => fact.IsOk ? (fact.Value / 1073741824d).ToString("0.0", CultureInfo.InvariantCulture) + " GB" : Dash;
}
