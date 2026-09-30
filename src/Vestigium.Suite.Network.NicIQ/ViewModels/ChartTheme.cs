using System.Windows;
using System.Windows.Media;
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
        return new ChartOptions
        {
            Title = title,
            XLabel = xLabel,
            YLabel = yLabel,
            Color = Hex("Vestigium.Brushes.Accent.Primary") ?? "#4C6B8A",
            FigureColor = Hex("Vestigium.Brushes.Surface.Window") ?? "#FFFFFF",
            DataColor = Hex("Vestigium.Brushes.Surface.Card") ?? "#FFFFFF",
            AxisColor = Hex("Vestigium.Brushes.Text.Primary") ?? "#1F2A33",
            GridColor = Hex("Vestigium.Brushes.Stroke.Subtle") ?? "#D9DEE4",
            ShowLegend = WatchLegend,
            ShowGrid = true,
            Stretch = true,
            HostMenu = false
        };
    }

    public static FrameworkElement Paint(FrameworkElement view, ChartSlot slot = ChartSlot.Throughput)
    {
        view.Tag = slot;
        ChartView.SetLegendVisible(view, WatchLegend);
        return view;
    }

    private static string? Hex(string resourceKey)
    {
        if (Application.Current?.TryFindResource(resourceKey) is not SolidColorBrush brush)
            return null;
        var c = brush.Color;
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
