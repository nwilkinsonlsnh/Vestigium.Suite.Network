using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private const int WarmReadyDepth = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChartWarmup))]
    private bool _chartWarming = true;

    [ObservableProperty]
    private string _chartWarmStatus = "Opening performance counters\u2026";

    [ObservableProperty]
    private double _chartWarmProgress = 8;

    public bool ShowChartWarmup => ChartWarming;

    private void BeginWarm(string status)
    {
        ChartWarming = true;
        ChartWarmStatus = status;
        ChartWarmProgress = 8;
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

    private void SyncWarm()
    {
        var depth = _ring.MaxDepth();
        if (depth >= WarmReadyDepth)
        {
            EndWarm();
            return;
        }

        if (!ChartWarming)
            BeginWarm("Collecting samples\u2026");

        if (depth == 0)
            AdvanceWarm("Opening performance counters\u2026", 18);
        else
            AdvanceWarm($"Collected {depth} of {WarmReadyDepth} plot points\u2026", 45 + depth * 20);
    }
}
