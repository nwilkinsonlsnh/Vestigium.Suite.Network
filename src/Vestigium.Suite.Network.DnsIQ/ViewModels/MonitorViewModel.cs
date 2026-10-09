using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MonitorViewModel : ObservableObject
{
    public const int DefaultSeconds = 5;
    public const int StepSeconds = 5;
    public const int MaxSeconds = 180;

    public IReadOnlyList<string> Sources { get; } = ["Event", "Port", "Both"];

    public IReadOnlyList<int> Durations { get; } = Enumerable.Range(1, MaxSeconds / StepSeconds).Select(i => i * StepSeconds).ToList();

    public ObservableCollection<MonitorRow> Rows { get; } = [];

    [ObservableProperty]
    private string _source = "Both";

    [ObservableProperty]
    private int _seconds = DefaultSeconds;

    [ObservableProperty]
    private string _monitorStatus = "Idle";

    [RelayCommand]
    private void Start()
    {
        if (!Sources.Contains(Source) || Seconds < StepSeconds || Seconds > MaxSeconds || Seconds % StepSeconds != 0)
        {
            MonitorStatus = "Rejected";
            return;
        }

        MonitorStatus = "Ready";
    }

    [RelayCommand]
    private void Stop()
        => MonitorStatus = "Stopped";
}

public sealed class MonitorRow
{
    public DateTimeOffset Time { get; init; }
    public int Pid { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public int ResolverCount { get; init; }
    public int PacketCount { get; init; }
    public int Total { get; init; }
}
