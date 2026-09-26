using System.Windows;
using System.Windows.Media;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

internal enum ChartSlot
{
    Lookup,
    ProbeRtt,
    ProbeDist,
    ProbeControl
}

internal static class ChartTheme
{
    public static bool LookupLegend { get; set; } = true;
    public static bool ProbeRttLegend { get; set; } = true;
    public static bool ProbeDistLegend { get; set; } = true;
    public static bool ProbeControlLegend { get; set; } = true;

    public static Action? LegendChanged { get; set; }

    static ChartTheme()
    {
        ChartView.LegendToggled += (view, visible) =>
        {
            if (view.Tag is not ChartSlot slot)
                return;
            SetLegend(slot, visible);
            LegendChanged?.Invoke();
        };
    }

    public static bool GetLegend(ChartSlot slot) => slot switch
    {
        ChartSlot.ProbeRtt => ProbeRttLegend,
        ChartSlot.ProbeDist => ProbeDistLegend,
        ChartSlot.ProbeControl => ProbeControlLegend,
        _ => LookupLegend
    };

    public static void SetLegend(ChartSlot slot, bool value)
    {
        switch (slot)
        {
            case ChartSlot.ProbeRtt: ProbeRttLegend = value; break;
            case ChartSlot.ProbeDist: ProbeDistLegend = value; break;
            case ChartSlot.ProbeControl: ProbeControlLegend = value; break;
            default: LookupLegend = value; break;
        }
    }

    public static void LoadFrom(DnsIqSettings data)
    {
        LookupLegend = data.ShowLegendLookup;
        ProbeRttLegend = data.ShowLegendProbeRtt;
        ProbeDistLegend = data.ShowLegendProbeDist;
        ProbeControlLegend = data.ShowLegendProbeControl;
    }

    public static void CopyTo(DnsIqSettings data)
    {
        data.ShowLegendLookup = LookupLegend;
        data.ShowLegendProbeRtt = ProbeRttLegend;
        data.ShowLegendProbeDist = ProbeDistLegend;
        data.ShowLegendProbeControl = ProbeControlLegend;
    }

    public static ChartOptions Options(ChartSlot slot, string title, string? xLabel = null, string? yLabel = null)
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
            ShowLegend = GetLegend(slot),
            ShowGrid = true,
            Stretch = true,
            HostMenu = true
        };
    }

    public static FrameworkElement Paint(FrameworkElement view, ChartSlot slot)
    {
        view.Tag = slot;
        ChartView.SetLegendVisible(view, GetLegend(slot));
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
