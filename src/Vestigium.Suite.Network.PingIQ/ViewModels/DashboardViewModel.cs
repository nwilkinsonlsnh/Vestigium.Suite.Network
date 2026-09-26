using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EchoPageOpen))]
    [NotifyPropertyChangedFor(nameof(ProbePageOpen))]
    private string _dashboardPage = "Ping";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EchoEmpty))]
    [NotifyPropertyChangedFor(nameof(HasAnyChart))]
    private bool _hasEchoData;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProbeEmpty))]
    [NotifyPropertyChangedFor(nameof(HasAnyChart))]
    private bool _hasProbeData;

    [ObservableProperty]
    private FrameworkElement? _echoChart;

    [ObservableProperty]
    private FrameworkElement? _probeCurve;

    [ObservableProperty]
    private FrameworkElement? _probeShape;

    [ObservableProperty]
    private FrameworkElement? _probeControl;

    public Action? GoToPingIq { get; set; }

    public Action<bool>? DashboardAvailabilityChanged { get; set; }

    public bool EchoPageOpen
    {
        get => DashboardPage == "Ping";
        set { if (value) DashboardPage = "Ping"; }
    }

    public bool ProbePageOpen
    {
        get => DashboardPage == "Probe";
        set { if (value) DashboardPage = "Probe"; }
    }

    public bool EchoEmpty => !HasEchoData;

    public bool ProbeEmpty => !HasProbeData;

    public bool HasAnyChart => HasEchoData || HasProbeData;

    public bool ShowProbeControl => ProbeControl is not null;

    [RelayCommand]
    private void OpenPingIq() => GoToPingIq?.Invoke();

    public void Unlock() => DashboardAvailabilityChanged?.Invoke(true);

    public void ShowEcho(IReadOnlyList<double> rtts)
    {
        try
        {
            if (rtts.Count == 0)
            {
                EchoChart = null;
                HasEchoData = false;
                return;
            }

            var series = NumericSeries.From(rtts, "icmp-echo-ms");
            EchoChart = TryChart(
                () => ChartView.Line(series, ChartTheme.Options(ChartSlot.Echo, "Ping RTT (ms)", "Reply", "RTT (ms)")),
                ChartSlot.Echo);
            HasEchoData = EchoChart is not null;
        }
        catch (Exception)
        {
            EchoChart = null;
            HasEchoData = false;
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

        var series = NumericSeries.From(rtts, "icmp-rtt-ms");
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
