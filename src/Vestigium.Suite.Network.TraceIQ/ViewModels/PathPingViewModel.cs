using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class PathPingViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private CancellationTokenSource? _cts;

    public PathPingViewModel(SettingsViewModel settings) => _settings = settings;

    public ObservableCollection<PathPingRow> Rows { get; } = [];

    [ObservableProperty] private string _target = "";
    [ObservableProperty] private bool _ready;
    [ObservableProperty] private bool _running;
    [ObservableProperty] private string _status = "Run a trace that reaches the target.";

    public bool CanRun => Ready && !Running && !string.IsNullOrWhiteSpace(Target);
    public bool CanCancel => Running;

    public void Offer(string target, IReadOnlyList<HopRow> hops)
    {
        if (Running)
            return;
        var valid = hops.Where(h => !string.IsNullOrWhiteSpace(h.Address) && h.Address != "*").ToList();
        if (valid.Count == 0 || string.IsNullOrWhiteSpace(target))
            return;
        Target = target;
        Rows.Clear();
        foreach (var hop in valid)
            Rows.Add(new PathPingRow { Ttl = hop.Ttl, Address = hop.Address, Name = hop.Name });
        Ready = true;
        Status = $"{Rows.Count} hops from the walk. Path Ping grades those hops.";
        OnPropertyChanged(nameof(CanRun));
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Running = true;
        Notify();
        Status = "Grading";
        var level = SaturationLevel.Find(_settings.PathSaturation);
        var width = Math.Clamp((int)_settings.PathWidth, 1, 20);
        var seconds = Math.Clamp((int)_settings.PathSeconds, 1, 180);
        var count = Math.Max(1, seconds * 1000 / level.IntervalMs);
        var gate = new SemaphoreSlim(width);
        try
        {
            var tasks = Rows.Select(row => GradeAsync(row, count, level.IntervalMs, gate, token)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(true);
            Status = token.IsCancellationRequested ? "Cancelled" : "Done";
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        finally
        {
            Running = false;
            Notify();
            _cts.Dispose();
            _cts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cts?.Cancel();

    private async Task GradeAsync(PathPingRow row, int count, int intervalMs, SemaphoreSlim gate, CancellationToken token)
    {
        await gate.WaitAsync(token).ConfigureAwait(true);
        try
        {
            var job = NetworkHelper.IcmpEcho(row.Address, new IcmpEchoOptions
            {
                Count = count,
                Interval = TimeSpan.FromMilliseconds(intervalMs),
                Timeout = TimeSpan.FromSeconds(2),
                MaxDuration = TimeSpan.FromSeconds(Math.Clamp((int)_settings.PathSeconds, 1, 180))
            });
            var result = await job.RunAsync(token).ConfigureAwait(true);
            await Dispatch(() => row.Apply(result)).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    private static Task Dispatch(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }
        return dispatcher.InvokeAsync(action).Task;
    }

    private void Notify()
    {
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(CanCancel));
        RunCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}

public partial class PathPingRow : ObservableObject
{
    [ObservableProperty] private int _ttl;
    [ObservableProperty] private string _address = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private int _sent;
    [ObservableProperty] private int _received;
    [ObservableProperty] private string _loss = "";
    [ObservableProperty] private string _min = "";
    [ObservableProperty] private string _avg = "";
    [ObservableProperty] private string _max = "";

    public void Apply(IcmpEchoResult result)
    {
        Sent = result.Sent;
        Received = result.Received;
        Loss = $"{result.LossPercent:0.#}%";
        Min = result.MinMs?.ToString() ?? "";
        Avg = result.AverageMs is { } avg ? $"{avg:0.#}" : "";
        Max = result.MaxMs?.ToString() ?? "";
    }
}
