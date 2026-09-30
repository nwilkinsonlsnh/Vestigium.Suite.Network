using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private const int WarmReadyDepth = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChartWarmup))]
    private bool _chartWarming = true;

    [ObservableProperty]
    private string _chartWarmStatus = "Starting monitor\u2026";

    [ObservableProperty]
    private double _chartWarmProgress;

    public bool ShowChartWarmup => ChartWarming;

    private void BeginWarm(string status)
    {
        ChartWarming = true;
        ChartWarmStatus = status;
        ChartWarmProgress = 4;
    }

    private void AdvanceWarm(string status, double progress)
    {
        ChartWarmStatus = status;
        ChartWarmProgress = Math.Clamp(progress, 0, 100);
    }

    private void EndWarm()
    {
        ChartWarming = false;
        ChartWarmProgress = 100;
    }
}
