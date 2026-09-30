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
                "Packets",
                "60 s",
                "Packets/sec",
                scaleLeft: 1m,
                scaleRight: 1m,
                rates: true),
            MonitorChartPages.Integrity => Integrity(ring),
            MonitorChartPages.Utilization => Utilization(ring),
            MonitorChartPages.Cpu => Pair(
                ring,
                HostCounters.UserTime,
                HostCounters.PrivilegedTime,
                "User",
                "Privileged",
                "CPU",
                "60 s",
                "%",
                scaleLeft: 1m,
                scaleRight: 1m,
                rates: true),
            MonitorChartPages.Memory => Pair(
                ring,
                HostCounters.CommittedBytes,
                HostCounters.AvailableMBytes,
                "Committed",
                "Available",
                "Memory",
                "60 s",
                "GB",
                scaleLeft: 1m / 1073741824m,
                scaleRight: 1m / 1024m,
                rates: true),
            _ => Pair(
                ring,
                NetworkInterface.BytesReceivedPerSec,
                NetworkInterface.BytesSentPerSec,
                "Receive",
                "Send",
                "Throughput",
                "60 s",
                "Kbps",
                scaleLeft: 8m / 1000m,
                scaleRight: 8m / 1000m,
                rates: true)
        };
    }

    private static (FrameworkElement? View, string Strip) Pair(
        MonitorRing ring,
        string leftCounter,
        string rightCounter,
        string leftName,
        string rightName,
        string title,
        string xLabel,
        string yLabel,
        decimal scaleLeft,
        decimal scaleRight,
        bool rates)
    {
        var leftPoints = Scale(ring.Of(leftCounter), scaleLeft);
        var rightPoints = Scale(ring.Of(rightCounter), scaleRight);
        var left = Window(leftPoints, leftName);
        var right = Window(rightPoints, rightName);
        var options = WithLimits(
            ChartTheme.Options(title, xLabel, yLabel),
            LimitsOf(leftPoints),
            LimitsOf(rightPoints));
        return Draw(left, right, leftName, rightName, options, rates);
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
        var counts = new List<decimal>(rows.Length);
        for (var i = 0; i < rows.Length; i++)
        {
            var last = Last(ring.Of(rows[i].Counter));
            labels[i] = rows[i].Label;
            values[i] = Math.Max(0, (double)decimal.Truncate(last));
            counts.Add((decimal)values[i]);
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"{rows[i].Label} {N(last)}"));
            if (values[i] > peak)
                peak = values[i];
        }

        try
        {
            var spec = new ChartSpec
            {
                Kind = ChartKind.Column,
                Title = "Integrity",
                Series =
                [
                    new ChartSeries
                    {
                        Name = "Integrity",
                        X = [1, 2, 3, 4, 5, 6],
                        Y = values,
                        Labels = labels
                    }
                ],
                Options = ChartTheme.Options("Integrity", null, "count")
            };
            var view = ChartTheme.Paint(ChartView.From(spec));
            var limits = LimitsOf(counts);
            FitCountAxis(view, peak, limits);
            var strip = peak <= 0 ? "Clean  no errors, discards, or queue." : string.Join("   ·   ", parts);
            return (view, AppendLimits(strip, limits));
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static void FitCountAxis(FrameworkElement view, double peak, ControlLimits? limits)
    {
        if (view is not WpfPlot plot)
            return;

        var max = peak <= 10 ? 10 : CountCeiling(peak);
        if (limits is not null && limits.Upper > max)
            max = CountCeiling(limits.Upper);
        var step = max <= 10 ? 1 : CountStep(max);
        plot.Plot.Axes.SetLimitsY(-1, max);
        plot.Plot.Axes.Left.TickGenerator = new NumericFixedInterval(step);
        DrawLimitLines(plot, limits);
        plot.Refresh();
    }

    private static void DrawLimitLines(WpfPlot plot, ControlLimits? limits)
    {
        if (limits is null)
            return;
        var ucl = plot.Plot.Add.HorizontalLine(limits.Upper);
        ucl.LinePattern = ScottPlot.LinePattern.Dashed;
        ucl.LegendText = "UCL";
        var lcl = plot.Plot.Add.HorizontalLine(limits.Lower);
        lcl.LinePattern = ScottPlot.LinePattern.Dashed;
        lcl.LegendText = "LCL";
        var cl = plot.Plot.Add.HorizontalLine(limits.Center);
        cl.LinePattern = ScottPlot.LinePattern.Dotted;
        cl.LegendText = "CL";
    }

    private static double CountCeiling(double peak)
    {
        if (peak <= 10)
            return 10;
        if (peak <= 20)
            return 20;
        if (peak <= 50)
            return 50;
        if (peak <= 100)
            return 100;
        return Math.Ceiling(peak / 50d) * 50d;
    }

    private static double CountStep(double max)
    {
        if (max <= 20)
            return 2;
        if (max <= 50)
            return 5;
        if (max <= 100)
            return 10;
        return 25;
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

        var series = Window(points, "Utilization");
        var options = WithLimits(ChartTheme.Options("Utilization", "60 s", "%"), LimitsOf(points));
        try
        {
            var view = ChartTheme.Paint(ChartView.Line(series, options));
            var strip = points.Count == 0 ? "Waiting for samples." : RateStrip(series, "Utilization");
            return (view, AppendLimits(strip, options.Limits));
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
            return (view, AppendLimits(Strip(left, leftName, right, rightName, rates), options.Limits));
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    private static ChartOptions WithLimits(ChartOptions options, params ControlLimits?[] candidates)
    {
        foreach (var limits in candidates)
        {
            if (limits is not null)
                return options with { Limits = limits };
        }

        return options;
    }

    private static ControlLimits? LimitsOf(IReadOnlyList<Observation> points)
    {
        if (points.Count < 2)
            return null;
        return LimitsOf(points.Select(p => p.Value).ToList());
    }

    private static ControlLimits? LimitsOf(IReadOnlyList<decimal> values)
    {
        if (values.Count < 2)
            return null;
        try
        {
            return NumericSeries.FromDecimal(values, "limits")
                .ControlLimits(ControlLimitMethod.MeanPlusKSigma, 3, floor: 0);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static List<Observation> Scale(IReadOnlyList<Observation> points, decimal scale)
    {
        if (scale == 1m || points.Count == 0)
            return [.. points];
        return points.Select(p => new Observation(p.Value * scale, p.At)).ToList();
    }

    private static NumericSeries Window(IReadOnlyList<Observation> points, string name)
    {
        var values = new decimal[MonitorRing.Cap];
        if (points.Count > 0)
        {
            var take = Math.Min(points.Count, MonitorRing.Cap);
            var dest = MonitorRing.Cap - take;
            var src = points.Count - take;
            for (var i = 0; i < take; i++)
                values[dest + i] = points[src + i].Value;
        }

        return NumericSeries.FromDecimal(values, name);
    }

    private static decimal Last(IReadOnlyList<Observation> points)
        => points.Count == 0 ? 0 : points[^1].Value;

    private static string AppendLimits(string strip, ControlLimits? limits)
    {
        if (limits is null)
            return strip;
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{strip}   ·   CL {N((decimal)limits.Center)}  UCL {N((decimal)limits.Upper)}  LCL {N((decimal)limits.Lower)}");
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
        => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string N(decimal? value)
        => value is null ? "\u2014" : value.Value.ToString("0.###", CultureInfo.InvariantCulture);
}
