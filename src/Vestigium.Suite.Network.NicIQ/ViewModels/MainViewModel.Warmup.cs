using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private const int WarmReadyDepth = 2;
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

    private void ReportWarm(int ready, int total, string stage)
    {
        _warmPathReady = Math.Max(0, ready);
        _warmPathTotal = Math.Max(total, _warmPathReady);
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

        var pathShare = _warmPathTotal <= 0 ? 0 : 60d * _warmPathReady / _warmPathTotal;
        var sampleShare = depth * 15d;
        var progress = Math.Min(95, 10 + pathShare + sampleShare);

        string status;
        if (depth > 0)
            status = $"Collecting plot points  {depth} of {WarmReadyDepth}";
        else if (_warmPathTotal > 0)
            status = $"Opening counters  {_warmPathReady} of {_warmPathTotal}";
        else
            status = stage ?? "Resolving performance counters\u2026";

        AdvanceWarm(status, progress);
    }
}
