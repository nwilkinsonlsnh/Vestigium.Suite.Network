using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    [NotifyPropertyChangedFor(nameof(LookupEmpty))]
    [NotifyPropertyChangedFor(nameof(HasAnyChart))]
    private bool _hasLookupData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProbeEmpty))]
    [NotifyPropertyChangedFor(nameof(HasAnyChart))]
    private bool _hasProbeData;

    [ObservableProperty]
    private FrameworkElement? _lookupChart;

    [ObservableProperty]
    private FrameworkElement? _probeCurve;

    [ObservableProperty]
    private FrameworkElement? _probeShape;

    [ObservableProperty]
    private FrameworkElement? _probeControl;

    public Action? GoToDnsIq { get; set; }

    public Action<bool>? DashboardAvailabilityChanged { get; set; }

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

    public bool LookupEmpty => !HasLookupData;

    public bool ProbeEmpty => !HasProbeData;

    public bool HasAnyChart => HasLookupData || HasProbeData;

    public bool ShowProbeControl => ProbeControl is not null;

    [RelayCommand]
    private void OpenDnsIq() => GoToDnsIq?.Invoke();

    public void ShowLookup(IReadOnlyList<AnswerRow> rows)
    {
        try
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
                RaiseAvailability();
                return;
            }

            LookupChart = ChartTheme.Paint(ChartView.Pie(slices, ChartTheme.Options("Lookup type mix")));
            HasLookupData = true;
        }
        catch (Exception)
        {
            LookupChart = null;
            HasLookupData = false;
        }

        RaiseAvailability();
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
            RaiseAvailability();
            return;
        }

        try
        {
            var series = NumericSeries.From(rtts, "dns-rtt-ms");
            var line = ChartTheme.Options("Probe RTT (ms)");
            var hist = ChartTheme.Options("RTT distribution") with { ShowBellCurve = true, ShowKde = true };
            ProbeCurve = ChartTheme.Paint(ChartView.Line(series, line));
            ProbeShape = ChartTheme.Paint(ChartView.Histogram(series, hist));

            try
            {
                var limits = series.ControlLimits(ControlLimitMethod.MovingRange);
                if (limits.Upper > limits.Center && limits.Center > limits.Lower)
                {
                    ProbeControl = ChartTheme.Paint(ChartView.Control(
                        series,
                        limits,
                        series.RunRules(ControlLimitMethod.MovingRange),
                        ChartTheme.Options("Probe control")));
                }
            }
            catch (Exception)
            {
                ProbeControl = null;
            }

            HasProbeData = true;
        }
        catch (Exception)
        {
            HasProbeData = false;
        }

        OnPropertyChanged(nameof(ShowProbeControl));
        RaiseAvailability();
    }

    private void RaiseAvailability()
    {
        OnPropertyChanged(nameof(HasAnyChart));
        DashboardAvailabilityChanged?.Invoke(HasAnyChart);
    }
}
