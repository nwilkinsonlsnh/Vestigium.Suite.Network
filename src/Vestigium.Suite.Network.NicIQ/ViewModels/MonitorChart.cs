using System.Globalization;
using System.Windows;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.PerfMon.Cpu;
using Vestigium.Helpers.PerfMon.Memory;
using Vestigium.Helpers.PerfMon.Network;
using MemoryInfo = Vestigium.Helpers.SystemInfo.Memory.MemoryFacts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public static class MonitorChartPages
{
    public const string Throughput = "Throughput";
    public const string Packets = "Packets";
    public const string Integrity = "Integrity";
    public const string Utilization = "Utilization";
    public const string Cpu = "CPU";
    public const string Memory = "Memory";
}

internal static class ChartHorizon
{
    public static bool Review { get; set; }
}

internal static class MonitorChart
{
    public static (FrameworkElement? View, string Strip) Paint(string page, MonitorRing ring)
    {
        ArgumentNullException.ThrowIfNull(ring);
        return page switch
        {
            MonitorChartPages.Packets => Pair(ring, NetworkInterface.PacketsReceivedPerSec, NetworkInterface.PacketsSentPerSec, "Receive", "Send", "Packets", "Packets/sec", 1m, 1m, true),
            MonitorChartPages.Integrity => Integrity(ring),
            MonitorChartPages.Utilization => Utilization(ring),
            MonitorChartPages.Cpu => Pair(ring, Processor.PercentUserTime, Processor.PercentPrivilegedTime, "User", "Privileged", "CPU", "%", 1m, 1m, true),
            MonitorChartPages.Memory => MemoryChart(ring),
            _ => Pair(ring, NetworkInterface.BytesReceivedPerSec, NetworkInterface.BytesSentPerSec, "Receive", "Send", "Throughput", "Kbps", 8m / 1000m, 8m / 1000m, true)
        };
    }

    private static (FrameworkElement? View, string Strip) MemoryChart(MonitorRing ring)
    {
        var total = MemoryInfo.Read().TotalBytes;
        double? yMax = total.IsOk && total.Value > 0 ? total.Value / 1073741824d : null;
        return Pair(ring, Memory.CommittedBytes, Memory.AvailableMBytes, "Committed", "Available", "Memory", "GB", 1m / 1073741824m, 1m / 1024m, true, yMax);
    }

    private static int PlotSeconds(int depth)
        => Math.Clamp(Math.Max(depth, 1), 1, MonitorRing.ArchiveSeconds);

    private static string TimeLabel(int seconds) => seconds + " s";

    private static (FrameworkElement? View, string Strip) Pair(
        MonitorRing ring,
        string leftCounter,
        string rightCounter,
        string leftName,
        string rightName,
        string title,
        string yLabel,
        decimal scaleLeft,
        decimal scaleRight,
        bool rates,
        double? yMax = null)
    {
        var leftAll = Scale(ring.Of(leftCounter), scaleLeft);
        var rightAll = Scale(ring.Of(rightCounter), scaleRight);
        var span = PlotSeconds(Math.Max(leftAll.Count, rightAll.Count));
        var left = Window(leftAll, leftName, span);
        var right = Window(rightAll, rightName, span);
        var options = PairColors(Span(WithLimits(ChartTheme.Options(title, TimeLabel(span), yLabel), LimitsOf(leftAll), LimitsOf(rightAll)), span));
        if (yMax is > 0)
            options = options with { YMin = 0, YMax = yMax };
        return Draw(left, right, SeriesOf(leftAll, leftName), SeriesOf(rightAll, rightName), leftName, rightName, options, rates);
    }

    private static ChartOptions PairColors(ChartOptions options)
    {
        var colors = options.SeriesColors;
        if (colors is null || colors.Count < 3)
            return options;
        return options with { SeriesColors = [colors[1], colors[2]] };
    }

    private static (FrameworkElement? View, string Strip) Integrity(MonitorRing ring)
    {
        (string Label, string Counter)[] rows =
        [
            ("Rx errors", NetworkInterface.PacketsReceivedErrors),
            ("Rx discarded", NetworkInterface.PacketsReceivedDiscarded),
            ("Rx unknown", NetworkInterface.PacketsReceivedUnknown),
            ("Tx errors", NetworkInterface.PacketsOutboundErrors),
            ("Tx discarded", NetworkInterface.PacketsOutboundDiscarded),
            ("Queue", NetworkInterface.OutputQueueLength)
        ];
        var labels = new string[rows.Length];
        var values = new double[rows.Length];
        var parts = new List<string>(rows.Length);
        var peak = 0d;
        var counts = new List<decimal>();
        for (var i = 0; i < rows.Length; i++)
        {
            var history = ring.Of(rows[i].Counter);
            var last = Last(history);
            labels[i] = rows[i].Label;
            values[i] = Math.Max(0, (double)decimal.Truncate(last));
            foreach (var point in history)
                counts.Add(point.Value);
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"{rows[i].Label} {N(last)}"));
            if (values[i] > peak) peak = values[i];
        }

        try
        {
            var limits = LimitsOf(counts);
            var options = ChartTheme.Options("Integrity", null, "count") with { CountAxis = true, Limits = limits };
            var spec = new ChartSpec
            {
                Kind = ChartKind.Column,
                Title = "Integrity",
                Series = [new ChartSeries { Name = "Integrity", X = [1, 2, 3, 4, 5, 6], Y = values, Labels = labels }],
                Options = options
            };
            var view = ChartTheme.Paint(ChartView.From(spec));
            var strip = peak <= 0 ? "Clean  no errors, discards, or queue." : string.Join("   \u00b7   ", parts);
            return (view, AppendLimits(strip, limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
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
            if (width <= 0) continue;
            var pct = 8m * bytes[i].Value / width * 100m;
            if (pct < 0) pct = 0;
            points.Add(new Observation(pct, bytes[i].At ?? band[i].At));
        }

        var span = PlotSeconds(points.Count);
        var series = Window(points, "Utilization", span);
        var stats = SeriesOf(points, "Utilization");
        var options = Span(WithLimits(ChartTheme.Options("Utilization", TimeLabel(span), "%"), LimitsOf(points)), span);
        try
        {
            var view = series is null ? null : ChartTheme.Paint(ChartView.Line(series, options));
            var strip = points.Count == 0 ? "Waiting for samples." : RateStrip(stats!, "Utilization");
            return (view, AppendLimits(strip, options.Limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    private static (FrameworkElement? View, string Strip) Draw(NumericSeries? left, NumericSeries? right, NumericSeries? leftStats, NumericSeries? rightStats, string leftName, string rightName, ChartOptions options, bool rates)
    {
        var rows = new List<NumericSeries>();
        if (left is not null) rows.Add(left);
        if (right is not null) rows.Add(right);
        if (rows.Count == 0) return (null, "Waiting for samples.");
        try
        {
            var view = ChartTheme.Paint(ChartView.Line(rows, options));
            return (view, AppendLimits(Strip(leftStats ?? left, leftName, rightStats ?? right, rightName, rates), options.Limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    private static ChartOptions Span(ChartOptions options, int seconds)
        => options with { XMin = 0, XMax = Math.Max(seconds, 1) };

    private static ChartOptions WithLimits(ChartOptions options, params ControlLimits?[] candidates)
    {
        foreach (var limits in candidates)
            if (limits is not null) return options with { Limits = limits };
        return options;
    }

    private static ControlLimits? LimitsOf(IReadOnlyList<Observation> points)
        => points.Count < 2 ? null : LimitsOf(points.Select(p => p.Value).ToList());

    private static ControlLimits? LimitsOf(IReadOnlyList<decimal> values)
    {
        if (values.Count < 2) return null;
        try { return NumericSeries.FromDecimal(values, "limits").ControlLimits(ControlLimitMethod.MeanPlusKSigma, 3, floor: 0); }
        catch (Exception) { return null; }
    }

    private static List<Observation> Scale(IReadOnlyList<Observation> points, decimal scale)
        => scale == 1m || points.Count == 0 ? [.. points] : points.Select(p => new Observation(p.Value * scale, p.At)).ToList();

    private static NumericSeries? Window(IReadOnlyList<Observation> points, string name, int take)
    {
        if (points.Count == 0) return null;
        var n = Math.Min(points.Count, Math.Max(take, 1));
        var values = new decimal[n];
        var src = points.Count - n;
        for (var i = 0; i < n; i++) values[i] = points[src + i].Value;
        return NumericSeries.FromDecimal(values, name);
    }

    private static NumericSeries? SeriesOf(IReadOnlyList<Observation> points, string name)
        => points.Count == 0 ? null : NumericSeries.FromDecimal(points.Select(p => p.Value).ToArray(), name);

    private static decimal Last(IReadOnlyList<Observation> points) => points.Count == 0 ? 0 : points[^1].Value;

    private static string AppendLimits(string strip, ControlLimits? limits)
        => limits is null ? strip : string.Create(CultureInfo.InvariantCulture, $"{strip}   \u00b7   CL {N((decimal)limits.Center)}  UCL {N((decimal)limits.Upper)}  LCL {N((decimal)limits.Lower)}");

    private static string Strip(NumericSeries? left, string leftName, NumericSeries? right, string rightName, bool rates)
    {
        var parts = new List<string>();
        if (left is not null) parts.Add(rates ? RateStrip(left, leftName) : CountStrip(left, leftName));
        if (right is not null) parts.Add(rates ? RateStrip(right, rightName) : CountStrip(right, rightName));
        return string.Join("   \u00b7   ", parts);
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
        try { return slice.Count < 2 ? null : slice.Percentile(0.95); }
        catch (Exception) { return null; }
    }

    private static string N(double? value) => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string N(decimal? value) => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
}
