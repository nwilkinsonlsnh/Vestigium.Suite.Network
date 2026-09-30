using System.Globalization;
using System.Windows;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
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
    public const string Cpu = "CPU";
    public const string Memory = "Memory";
}

internal static class ChartHorizon
{
    public static bool Review { get; set; }
}

internal static class MonitorChart
{
    private static readonly ScottPlot.Color ClColor = new(232, 196, 90);
    private static readonly ScottPlot.Color UclColor = new(214, 92, 92);
    private static readonly ScottPlot.Color LclColor = new(90, 168, 224);

    public static (FrameworkElement? View, string Strip) Paint(string page, MonitorRing ring)
    {
        ArgumentNullException.ThrowIfNull(ring);
        return page switch
        {
            MonitorChartPages.Packets => Pair(ring, NetworkInterface.PacketsReceivedPerSec, NetworkInterface.PacketsSentPerSec, "Receive", "Send", "Packets", "Packets/sec", 1m, 1m, true),
            MonitorChartPages.Integrity => Integrity(ring),
            MonitorChartPages.Utilization => Utilization(ring),
            MonitorChartPages.Cpu => Pair(ring, HostCounters.UserTime, HostCounters.PrivilegedTime, "User", "Privileged", "CPU", "%", 1m, 1m, true),
            MonitorChartPages.Memory => Pair(ring, HostCounters.CommittedBytes, HostCounters.AvailableMBytes, "Committed", "Available", "Memory", "GB", 1m / 1073741824m, 1m / 1024m, true),
            _ => Pair(ring, NetworkInterface.BytesReceivedPerSec, NetworkInterface.BytesSentPerSec, "Receive", "Send", "Throughput", "Kbps", 8m / 1000m, 8m / 1000m, true)
        };
    }

    private static int PlotSeconds(int depth)
        => ChartHorizon.Review ? Math.Max(depth, 1) : MonitorRing.DisplaySeconds;

    private static string TimeLabel(int seconds) => seconds + " s";

    private static (FrameworkElement? View, string Strip) Pair(MonitorRing ring, string leftCounter, string rightCounter, string leftName, string rightName, string title, string yLabel, decimal scaleLeft, decimal scaleRight, bool rates)
    {
        var leftAll = Scale(ring.Of(leftCounter), scaleLeft);
        var rightAll = Scale(ring.Of(rightCounter), scaleRight);
        var span = PlotSeconds(Math.Max(leftAll.Count, rightAll.Count));
        var left = Window(leftAll, leftName, span);
        var right = Window(rightAll, rightName, span);
        var options = WithLimits(ChartTheme.Options(title, TimeLabel(span), yLabel), LimitsOf(leftAll), LimitsOf(rightAll));
        return Draw(left, right, SeriesOf(leftAll, leftName), SeriesOf(rightAll, rightName), leftName, rightName, options, rates, span);
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
            var spec = new ChartSpec { Kind = ChartKind.Column, Title = "Integrity", Series = [new ChartSeries { Name = "Integrity", X = [1, 2, 3, 4, 5, 6], Y = values, Labels = labels }], Options = ChartTheme.Options("Integrity", null, "count") };
            var view = ChartTheme.Paint(ChartView.From(spec));
            var limits = LimitsOf(counts);
            FitCountAxis(view, peak, limits);
            var strip = peak <= 0 ? "Clean  no errors, discards, or queue." : string.Join("   ·   ", parts);
            return (view, AppendLimits(strip, limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    private static void FitCountAxis(FrameworkElement view, double peak, ControlLimits? limits)
    {
        if (view is not WpfPlot plot) return;
        var max = peak <= 10 ? 10 : CountCeiling(peak);
        if (limits is not null && limits.Upper > max) max = CountCeiling(limits.Upper);
        var step = max <= 10 ? 1 : CountStep(max);
        plot.Plot.Axes.SetLimitsY(-1, max);
        plot.Plot.Axes.Left.TickGenerator = new NumericFixedInterval(step);
        DrawLimitLines(plot, limits);
        plot.Refresh();
    }

    private static void DrawLimitLines(WpfPlot plot, ControlLimits? limits)
    {
        if (limits is null) return;
        StyleOrAdd(plot, "UCL", limits.Upper, UclColor, ScottPlot.LinePattern.Dashed, 1.5f);
        StyleOrAdd(plot, "LCL", limits.Lower, LclColor, ScottPlot.LinePattern.Dashed, 1.5f);
        StyleOrAdd(plot, "CL", limits.Center, ClColor, ScottPlot.LinePattern.DenselyDashed, 2.25f);
    }

    private static void StyleLimitLines(FrameworkElement view, ControlLimits? limits)
    {
        if (view is not WpfPlot plot) return;
        DrawLimitLines(plot, limits);
        plot.Refresh();
    }

    private static void StyleOrAdd(WpfPlot plot, string name, double y, ScottPlot.Color color, ScottPlot.LinePattern pattern, float width)
    {
        foreach (var plottable in plot.Plot.GetPlottables())
        {
            if (plottable is not ScottPlot.Plottables.HorizontalLine line)
                continue;
            if (!string.Equals(line.LegendText, name, StringComparison.OrdinalIgnoreCase)
                && Math.Abs(line.Y - y) > 0.0001)
                continue;
            line.Y = y;
            line.Color = color;
            line.LinePattern = pattern;
            line.LineWidth = width;
            line.LegendText = name;
            return;
        }

        var added = plot.Plot.Add.HorizontalLine(y);
        added.Color = color;
        added.LinePattern = pattern;
        added.LineWidth = width;
        added.LegendText = name;
    }

    private static double CountCeiling(double peak) => peak <= 10 ? 10 : peak <= 20 ? 20 : peak <= 50 ? 50 : peak <= 100 ? 100 : Math.Ceiling(peak / 50d) * 50d;
    private static double CountStep(double max) => max <= 20 ? 2 : max <= 50 ? 5 : max <= 100 ? 10 : 25;

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
        var options = WithLimits(ChartTheme.Options("Utilization", TimeLabel(span), "%"), LimitsOf(points));
        try
        {
            var view = ChartTheme.Paint(ChartView.Line(series, options));
            FitTimeAxis(view, span);
            StyleLimitLines(view, options.Limits);
            var strip = points.Count == 0 ? "Waiting for samples." : RateStrip(stats, "Utilization");
            return (view, AppendLimits(strip, options.Limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

    private static (FrameworkElement? View, string Strip) Draw(NumericSeries? left, NumericSeries? right, NumericSeries? leftStats, NumericSeries? rightStats, string leftName, string rightName, ChartOptions options, bool rates, int span)
    {
        var rows = new List<NumericSeries>();
        if (left is not null) rows.Add(left);
        if (right is not null) rows.Add(right);
        if (rows.Count == 0) return (null, "Waiting for samples.");
        try
        {
            var view = ChartTheme.Paint(ChartView.Line(rows, options));
            FitTimeAxis(view, span);
            StyleLimitLines(view, options.Limits);
            return (view, AppendLimits(Strip(leftStats ?? left, leftName, rightStats ?? right, rightName, rates), options.Limits));
        }
        catch (Exception ex) { return (null, ex.Message); }
    }

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

    private static void FitTimeAxis(FrameworkElement view, int seconds)
    {
        if (view is not WpfPlot plot) return;
        plot.Plot.Axes.SetLimitsX(0, Math.Max(seconds, 1));
        plot.Refresh();
    }

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
        => limits is null ? strip : string.Create(CultureInfo.InvariantCulture, $"{strip}   ·   CL {N((decimal)limits.Center)}  UCL {N((decimal)limits.Upper)}  LCL {N((decimal)limits.Lower)}");

    private static string Strip(NumericSeries? left, string leftName, NumericSeries? right, string rightName, bool rates)
    {
        var parts = new List<string>();
        if (left is not null) parts.Add(rates ? RateStrip(left, leftName) : CountStrip(left, leftName));
        if (right is not null) parts.Add(rates ? RateStrip(right, rightName) : CountStrip(right, rightName));
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
        try { return slice.Count < 2 ? null : slice.Percentile(0.95); }
        catch (Exception) { return null; }
    }

    private static string N(double? value) => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string N(decimal? value) => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
}
