using System.Globalization;
using System.Windows;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.PerfMon.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public static class MonitorChartPages
{
    public const string Throughput = "Throughput";
    public const string Packets = "Packets";
    public const string Integrity = "Integrity";
    public const string Utilization = "Utilization";
}

internal static class MonitorChart
{
    public static (FrameworkElement? View, string Strip) Paint(string page, MonitorRing ring)
    {
        ArgumentNullException.ThrowIfNull(ring);
        return page switch
        {
            MonitorChartPages.Packets => Pair(
                ring,
                NetworkInterface.PacketsReceivedPerSec,
                NetworkInterface.PacketsSentPerSec,
                "Receive",
                "Send",
                ChartTheme.Options("Packets", "60 s", "Packets/sec"),
                scale: 1m,
                rates: true),
            MonitorChartPages.Integrity => Integrity(ring),
            MonitorChartPages.Utilization => Utilization(ring),
            _ => Pair(
                ring,
                NetworkInterface.BytesReceivedPerSec,
                NetworkInterface.BytesSentPerSec,
                "Receive",
                "Send",
                ChartTheme.Options("Throughput", "60 s", "Kbps"),
                scale: 8m / 1000m,
                rates: true)
        };
    }

    private static (FrameworkElement? View, string Strip) Pair(
        MonitorRing ring,
        string leftCounter,
        string rightCounter,
        string leftName,
        string rightName,
        ChartOptions options,
        decimal scale,
        bool rates)
    {
        var left = Window(ring.Of(leftCounter), leftName, scale);
        var right = Window(ring.Of(rightCounter), rightName, scale);
        return Draw(left, right, leftName, rightName, options, rates);
    }

    private static (FrameworkElement? View, string Strip) Integrity(MonitorRing ring)
    {
        var receive = Window(
            Sum(
                ring,
                NetworkInterface.PacketsReceivedErrors,
                NetworkInterface.PacketsReceivedDiscarded,
                NetworkInterface.PacketsReceivedUnknown),
            "Receive faults",
            1m);
        var send = Window(
            Sum(
                ring,
                NetworkInterface.PacketsOutboundErrors,
                NetworkInterface.PacketsOutboundDiscarded,
                NetworkInterface.OutputQueueLength),
            "Send faults + queue",
            1m);
        var drawn = Draw(
            receive,
            send,
            "Receive faults",
            "Send faults + queue",
            ChartTheme.Options("Integrity", "60 s", "count"),
            rates: false);
        if (drawn.View is null)
            return drawn;
        if (IsClean(receive) && IsClean(send))
            return (drawn.View, "Clean  no errors, discards, or queue.");
        return drawn;
    }

    private static (FrameworkElement? View, string Strip) Utilization(MonitorRing ring)
    {
        var bytes = ring.Of(NetworkInterface.BytesTotalPerSec);
        var band = ring.Of(NetworkInterface.CurrentBandwidth);
        var points = new List<Observation>();
        var n = Math.Min(bytes.Count, band.Count);
        for (var i = 0; i < n; i++)
        {
            var width = band[i].Value;
            if (width <= 0)
                continue;
            var pct = 8m * bytes[i].Value / width * 100m;
            if (pct < 0)
                pct = 0;
            points.Add(new Observation(pct, bytes[i].At ?? band[i].At));
        }

        var series = Window(points, "Utilization", 1m);
        if (series is null)
            return (null, "Need Bytes Total/sec and Current Bandwidth.");

        try
        {
            var view = ChartTheme.Paint(
                ChartView.Line(series, ChartTheme.Options("Utilization", "60 s", "%")));
            return (view, RateStrip(series, "Utilization"));
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static (FrameworkElement? View, string Strip) Draw(
        NumericSeries? left,
        NumericSeries? right,
        string leftName,
        string rightName,
        ChartOptions options,
        bool rates)
    {
        var rows = new List<NumericSeries>();
        if (left is not null)
            rows.Add(left);
        if (right is not null)
            rows.Add(right);
        if (rows.Count == 0)
            return (null, "Waiting for samples.");

        try
        {
            var view = ChartTheme.Paint(ChartView.Line(rows, options));
            return (view, Strip(left, leftName, right, rightName, rates));
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static List<Observation> Sum(MonitorRing ring, params string[] counters)
    {
        var rows = counters.Select(ring.Of).Where(list => list.Count > 0).ToList();
        if (rows.Count == 0)
            return [];

        var n = rows.Max(list => list.Count);
        var points = new List<Observation>(n);
        for (var i = 0; i < n; i++)
        {
            decimal total = 0;
            DateTimeOffset? at = null;
            foreach (var list in rows)
            {
                var index = i - (n - list.Count);
                if (index < 0)
                    continue;
                total += list[index].Value;
                at ??= list[index].At;
            }

            points.Add(new Observation(total, at));
        }

        return points;
    }

    private static NumericSeries? Window(IReadOnlyList<Observation> points, string name, decimal scale)
    {
        if (points.Count == 0)
            return null;

        var values = new decimal[MonitorRing.Cap];
        var take = Math.Min(points.Count, MonitorRing.Cap);
        var dest = MonitorRing.Cap - take;
        var src = points.Count - take;
        for (var i = 0; i < take; i++)
            values[dest + i] = points[src + i].Value * scale;

        try
        {
            return NumericSeries.FromDecimal(values, name);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsClean(NumericSeries? series)
        => series is null || series.Full.Max is null || series.Full.Max <= 0;

    private static string Strip(NumericSeries? left, string leftName, NumericSeries? right, string rightName, bool rates)
    {
        var parts = new List<string>();
        if (left is not null)
            parts.Add(rates ? RateStrip(left, leftName) : CountStrip(left, leftName));
        if (right is not null)
            parts.Add(rates ? RateStrip(right, rightName) : CountStrip(right, rightName));
        return string.Join("   ·   ", parts);
    }

    private static string RateStrip(NumericSeries series, string name)
    {
        var s = series.Full;
        var last = s.Values.Count == 0 ? (decimal?)null : s.Values[^1];
        return string.Create(CultureInfo.InvariantCulture, $"{name}  last {N(last)}  mean {N(s.Mean)}  min {N(s.Min)}  max {N(s.Max)}  p95 {N(TryP95(s))}");
    }

    private static string CountStrip(NumericSeries series, string name)
    {
        var s = series.Full;
        var last = s.Values.Count == 0 ? (decimal?)null : s.Values[^1];
        return string.Create(CultureInfo.InvariantCulture, $"{name}  last {N(last)}  max {N(s.Max)}");
    }

    private static decimal? TryP95(SeriesSlice slice)
    {
        try
        {
            return slice.Count < 2 ? null : slice.Percentile(0.95);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string N(double? value)
        => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string N(decimal? value)
        => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
}
