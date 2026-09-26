using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LookupPageOpen))]
    [NotifyPropertyChangedFor(nameof(ProbePageOpen))]
    private string _dashboardPage = "Lookup";

    [ObservableProperty]
    private bool _hasLookupData;

    [ObservableProperty]
    private bool _hasProbeData;

    [ObservableProperty]
    private FrameworkElement? _lookupChart;

    [ObservableProperty]
    private FrameworkElement? _probeCurve;

    [ObservableProperty]
    private FrameworkElement? _probeShape;

    [ObservableProperty]
    private FrameworkElement? _probeControl;

    public bool LookupPageOpen
    {
        get => DashboardPage == "Lookup";
        set { if (value) DashboardPage = "Lookup"; }
    }

    public bool ProbePageOpen
    {
        get => DashboardPage == "Probe";
        set { if (value) DashboardPage = "Probe"; }
    }

    public bool ShowProbeControl => ProbeControl is not null;

    public void ShowLookup(IReadOnlyList<AnswerRow> rows)
    {
        var slices = rows
            .GroupBy(r => r.Type, StringComparer.OrdinalIgnoreCase)
            .Select(g => new ChartSlice { Label = g.Key, Value = g.Count() })
            .Where(s => s.Value > 0)
            .ToList();

        if (slices.Count == 0)
        {
            HasLookupData = false;
            LookupChart = null;
            return;
        }

        LookupChart = ChartView.Pie(slices, new ChartOptions { Title = "Lookup type mix" });
        HasLookupData = true;
    }

    public void ShowProbe(IReadOnlyList<double> rtts)
    {
        ProbeCurve = null;
        ProbeShape = null;
        ProbeControl = null;
        OnPropertyChanged(nameof(ShowProbeControl));

        if (rtts.Count == 0)
        {
            HasProbeData = false;
            return;
        }

        var series = NumericSeries.From(rtts, "dns-rtt-ms");
        ProbeCurve = ChartView.Line(series, new ChartOptions { Title = "Probe RTT (ms)" });
        ProbeShape = ChartView.Histogram(series, new ChartOptions
        {
            Title = "RTT distribution",
            ShowBellCurve = true,
            ShowKde = true
        });

        try
        {
            var limits = series.ControlLimits(ControlLimitMethod.MovingRange);
            if (limits.Upper > limits.Center && limits.Center > limits.Lower)
            {
                ProbeControl = ChartView.Control(series, limits, series.RunRules(), new ChartOptions
                {
                    Title = "Probe control"
                });
            }
        }
        catch (Exception)
        {
            ProbeControl = null;
        }

        OnPropertyChanged(nameof(ShowProbeControl));
        HasProbeData = true;
    }
}
