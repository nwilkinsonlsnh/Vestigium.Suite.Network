using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MonitorViewModel : ObservableObject
{
    public const int DefaultSeconds = 5;
    public const int StepSeconds = 5;
    public const int MaxSeconds = 180;
    public const int UacDeclined = 1223;

    public IReadOnlyList<string> Sources { get; } = ["Event", "Port", "Both"];

    public IReadOnlyList<int> Durations { get; } = Enumerable.Range(1, MaxSeconds / StepSeconds).Select(i => i * StepSeconds).ToList();

    public ObservableCollection<MonitorRow> Rows { get; } = [];

    [ObservableProperty]
    private string _source = "Both";

    [ObservableProperty]
    private int _seconds = DefaultSeconds;

    [ObservableProperty]
    private string _monitorStatus = "Idle";

    public string? PipeName { get; private set; }

    [RelayCommand]
    private void Start()
    {
        if (!Sources.Contains(Source) || Seconds < StepSeconds || Seconds > MaxSeconds || Seconds % StepSeconds != 0)
        {
            MonitorStatus = "Rejected";
            return;
        }

        var exe = FindWatchExe();
        if (exe is null)
        {
            MonitorStatus = "Watch exe was not found.";
            return;
        }

        PipeName = "Vestigium.Watch.Dns." + Guid.NewGuid().ToString("N");
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = Source + " " + Seconds + " pipe:" + PipeName
            });
            MonitorStatus = "Started";
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == UacDeclined)
        {
            MonitorStatus = "Watch was not started.";
        }
    }

    [RelayCommand]
    private void Stop()
        => MonitorStatus = "Stopped";

    public static string? FindWatchExe()
    {
        var name = "Vestigium.Helpers.Watch.Dns.exe";
        var beside = Path.Combine(AppContext.BaseDirectory, name);
        return File.Exists(beside) ? beside : null;
    }
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
