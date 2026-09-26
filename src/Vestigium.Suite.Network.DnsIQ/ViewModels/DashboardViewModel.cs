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

    public void Unlock()
    {
        DashboardAvailabilityChanged?.Invoke(true);
    }

    public void ShowLookup(IReadOnlyList<AnswerRow> rows)
    {
        Unlock();
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
                return;
            }

            LookupChart = ChartTheme.Paint(
                ChartView.Pie(slices, ChartTheme.Options(ChartSlot.Lookup, "Lookup type mix", "Record type", "Answers")),
                ChartSlot.Lookup);
            HasLookupData = LookupChart is not null;
        }
        catch (Exception)
        {
            LookupChart = null;
            HasLookupData = false;
        }
    }

    public void ShowProbe(IReadOnlyList<double> rtts)
    {
        Unlock();
        ProbeCurve = null;
        ProbeShape = null;
        ProbeControl = null;

        if (rtts.Count == 0)
        {
            HasProbeData = false;
            OnPropertyChanged(nameof(ShowProbeControl));
            return;
        }

        var series = NumericSeries.From(rtts, "dns-rtt-ms");
        ProbeCurve = TryChart(
            () => ChartView.Line(series, ChartTheme.Options(ChartSlot.ProbeRtt, "Probe RTT (ms)", "Request", "RTT (ms)")),
            ChartSlot.ProbeRtt);

        var hist = ChartTheme.Options(ChartSlot.ProbeDist, "RTT distribution", "RTT (ms)", "Count") with
        {
            ShowBellCurve = true,
            ShowKde = true
        };
        ProbeShape = TryChart(() => ChartView.Histogram(series, hist), ChartSlot.ProbeDist);

        try
        {
            var limits = series.ControlLimits(ControlLimitMethod.MovingRange);
            if (limits.Upper > limits.Center && limits.Center > limits.Lower)
            {
                ProbeControl = TryChart(
                    () => ChartView.Control(
                        series,
                        limits,
                        series.RunRules(ControlLimitMethod.MovingRange),
                        ChartTheme.Options(ChartSlot.ProbeControl, "Probe control", "Request", "RTT (ms)")),
                    ChartSlot.ProbeControl);
            }
        }
        catch (Exception)
        {
            ProbeControl = null;
        }

        HasProbeData = ProbeCurve is not null || ProbeShape is not null || ProbeControl is not null;
        OnPropertyChanged(nameof(ShowProbeControl));
    }

    private static FrameworkElement? TryChart(Func<FrameworkElement> build, ChartSlot slot)
    {
        try
        {
            return ChartTheme.Paint(build(), slot);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
