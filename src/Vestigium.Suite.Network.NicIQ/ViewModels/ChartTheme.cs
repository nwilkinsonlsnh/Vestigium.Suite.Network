using System.Windows;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal enum ChartSlot
{
    Throughput,
    Packets,
    Integrity,
    Utilization
}

internal static class ChartTheme
{
    public static bool WatchLegend { get; set; } = true;

    static ChartTheme()
    {
        ChartView.LegendToggled += (view, visible) =>
        {
            if (view.Tag is ChartSlot)
                WatchLegend = visible;
        };
    }

    public static ChartOptions Options(string title, string? xLabel = null, string? yLabel = null)
    {
        return Vestigium.Helpers.Charts.ChartTheme.Apply(new ChartOptions
        {
            Title = title,
            XLabel = xLabel,
            YLabel = yLabel,
            ShowLegend = WatchLegend,
            ShowGrid = true,
            Stretch = true,
            HostMenu = false
        });
    }

    public static FrameworkElement Paint(FrameworkElement view, ChartSlot slot = ChartSlot.Throughput)
    {
        view.Tag = slot;
        ChartView.SetLegendVisible(view, WatchLegend);
        return view;
    }
}
