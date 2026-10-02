using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private const int WarmReadyDepth = 1;
    private int _warmPathTotal;
    private int _warmPathReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChartWarmup))]
    private bool _chartWarming = true;

    [ObservableProperty]
    private string _chartWarmStatus = "Opening performance counters\u2026";

    [ObservableProperty]
    private double _chartWarmProgress = 5;

    public bool ShowChartWarmup => ChartWarming;

    private void BeginWarm(string status)
    {
        _warmPathReady = 0;
        _warmPathTotal = 0;
        ChartWarming = true;
        ChartWarmStatus = status;
        ChartWarmProgress = 5;
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

    public void PollWarm()
    {
        PublishMonitorProgress();
        if (!ChartWarming)
            return;

        SyncWarm();
    }

    private void ReportWarm(int ready, int total, string stage)
    {
        _warmPathReady = Math.Max(_warmPathReady, ready);
        _warmPathTotal = Math.Max(Math.Max(total, ready), _warmPathTotal);
        SyncWarm(stage);
    }

    private void SyncWarm(string? stage = null)
    {
        var depth = _ring.MaxDepth();
        if (depth >= WarmReadyDepth)
        {
            EndWarm();
            return;
        }

        if (!ChartWarming)
            ChartWarming = true;

        var sampleShare = depth * 40d;
        var progress = Math.Min(95, 8 + sampleShare);

        string status;
        if (depth > 0)
            status = $"Collecting plot points  {depth} of {WarmReadyDepth}";
        else
            status = stage ?? "Resolving performance counters\u2026";

        AdvanceWarm(status, progress);
    }
}
