using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private const int WarmReadyDepth = 2;
    private const int WarmExpectedPaths = 22;
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
        if (!ChartWarming)
            return;

        var opened = _pdh?.OpenedCount ?? 0;
        var primed = _pdh?.PrimedCount ?? 0;
        if (opened > _warmPathTotal)
            _warmPathTotal = opened;
        if (primed > _warmPathReady)
            _warmPathReady = primed;
        if (_warmPathTotal < WarmExpectedPaths && _ring.MaxDepth() == 0)
            _warmPathTotal = WarmExpectedPaths;

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

        var expected = Math.Max(_warmPathTotal, 1);
        var pathShare = 60d * Math.Min(_warmPathReady, expected) / expected;
        var sampleShare = depth * 15d;
        var progress = Math.Min(95, 8 + pathShare + sampleShare);

        string status;
        if (depth > 0)
            status = $"Collecting plot points  {depth} of {WarmReadyDepth}";
        else if (_warmPathReady > 0)
            status = $"Opening counters  {_warmPathReady} of {expected}";
        else
            status = stage ?? "Resolving performance counters\u2026";

        AdvanceWarm(status, progress);
    }
}
