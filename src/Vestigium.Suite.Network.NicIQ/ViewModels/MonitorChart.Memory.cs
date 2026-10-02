using System.Windows;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.PerfMon.Memory;
using MemoryInfo = Vestigium.Helpers.SystemInfo.Memory.MemoryFacts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static partial class MonitorChart
{
    private static (FrameworkElement? View, string Strip) MemoryChart(MonitorRing ring)
    {
        var total = MemoryInfo.Read().TotalBytes;
        var gb = total.IsOk && total.Value > 0 ? total.Value / 1073741824m : 0m;
        var available = Scale(ring.Of(Memory.AvailableMBytes), 1m / 1024m);
        var inUse = available.Select(point => new Observation(gb > point.Value ? gb - point.Value : 0m, point.At)).ToList();
        var span = PlotSeconds(Math.Max(inUse.Count, available.Count));
        var options = PairColors(Span(ChartTheme.Options("Memory", TimeLabel(span), "GB"), span));
        if (gb > 0)
            options = options with { YMin = 0, YMax = (double)gb };
        return Draw(
            Window(inUse, "In use", span),
            Window(available, "Available", span),
            SeriesOf(inUse, "In use"),
            SeriesOf(available, "Available"),
            "In use",
            "Available",
            options,
            true);
    }
}
