using Vestigium.Helpers.Analytics;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class PopulationStats
{
    public static IReadOnlyList<decimal> Rtts(IEnumerable<ReplyRow> rows)
        => rows
            .Where(r => r.Status == "Success" && r.RttMs > 0)
            .Select(r => (decimal)r.RttMs)
            .ToList();

    public static IReadOnlyList<double> ChartPoints(IReadOnlyList<decimal> rtts)
        => rtts.Select(v => (double)v).ToList();

    public static string Format(IReadOnlyList<decimal> rtts)
    {
        if (rtts.Count == 0)
            return string.Empty;

        var full = NumericSeries.From(rtts, "icmp-rtt-ms").Full;
        return string.Join("  ",
            $"n={full.Count}",
            $"min={Num(full.Min)}",
            $"max={Num(full.Max)}",
            $"mean={Num(full.Mean)}",
            $"median={Num(full.Median)}",
            $"p95={full.Percentile(0.95):0.###}",
            $"IQR={Num(full.Iqr)}",
            $"σp={Num(full.PopulationStdDev)}",
            $"varp={Num(full.PopulationVariance)}");
    }

    private static string Num(decimal? value)
        => value is { } v ? v.ToString("0.###") : "—";

    private static string Num(double? value)
        => value is { } v ? v.ToString("0.###") : "—";
}
