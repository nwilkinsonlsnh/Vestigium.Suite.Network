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
                ChartTheme.Options("Packets", "s", "Packets/sec"),
                rates: true),
            MonitorChartPages.Integrity => Pair(
                ring,
                NetworkInterface.PacketsReceivedErrors,
                NetworkInterface.PacketsOutboundErrors,
                "Receive errors",
                "Send errors",
                ChartTheme.Options("Integrity", "s", "count"),
                rates: false),
            MonitorChartPages.Utilization => Utilization(ring),
            _ => Pair(
                ring,
                NetworkInterface.BytesReceivedPerSec,
                NetworkInterface.BytesSentPerSec,
                "Receive",
                "Send",
                ChartTheme.Options("Throughput", "s", "Bytes/sec"),
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
        bool rates)
    {
        var left = Series(ring.Of(leftCounter), leftName);
        var right = Series(ring.Of(rightCounter), rightName);
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

        var series = Series(points, "Utilization");
        if (series is null)
            return (null, "Need Bytes Total/sec and Current Bandwidth.");

        try
        {
            var view = ChartTheme.Paint(
                ChartView.Line(series, ChartTheme.Options("Utilization", "s", "%")));
            return (view, RateStrip(series, "Utilization"));
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static NumericSeries? Series(IReadOnlyList<Observation> points, string name)
    {
        if (points.Count < 2)
            return null;
        try
        {
            return NumericSeries.FromObservations(points, name);
        }
        catch (Exception)
        {
            return null;
        }
    }

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
        => value is null ? "—" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string N(decimal? value)
        => value is null ? "—" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
}
