using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private NetworkJob<IcmpEchoResult>? _job;
    private bool _busy;
    private bool _loading;

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public PingIqSession? Session { get; set; }

    public DashboardViewModel? Dashboard { get; set; }

    public IReadOnlyList<AdapterChoice> Interfaces { get; }

    public ObservableCollection<ReplyRow> Replies { get; } = [];

    public MainViewModel()
    {
        try
        {
            Interfaces = AdapterChoices.From(NetworkHelper.GetAdapters());
        }
        catch (Exception)
        {
            Interfaces = AdapterChoices.From([]);
        }

        Bind.PropertyChanged += (_, _) => Persist();
    }

    [ObservableProperty]
    private string _target = "127.0.0.1";

    [ObservableProperty]
    private decimal _count = PingIqInput.DefaultCount;

    [ObservableProperty]
    private decimal _timeoutMs = PingIqInput.DefaultDelayMs;

    [ObservableProperty]
    private int _selectedInterfaceIndex;

    [ObservableProperty]
    private decimal _requestCount = SettingsViewModel.RequestDefault;

    [ObservableProperty]
    private decimal _durationSeconds = SettingsViewModel.SecondsDefault;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _summary = string.Empty;

    public bool CanStart => !_busy;

    public bool CanCancel => _busy;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private Task PingAsync() => RunAsync(probe: false);

    [RelayCommand(CanExecute = nameof(CanStart))]
    private Task ProbeAsync() => RunAsync(probe: true);

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _job?.Cancel();
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    private async Task RunAsync(bool probe)
    {
        if (_busy)
            return;

        if (!PingIqInput.TryCreate(Target, Count, TimeoutMs, Bind.InterfaceIndex, Bind.SourceAddress, out var query, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        if (probe && !PulsePlan.TryCreate(RequestCount, DurationSeconds, out _, out var pulseReject))
        {
            Status = pulseReject ?? "Failed";
            return;
        }

        _busy = true;
        RaiseBusy();
        Replies.Clear();
        Summary = string.Empty;
        Status = "Running";
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        try
        {
            if (!probe)
            {
                await PingLoopAsync(query!, token).ConfigureAwait(true);
                return;
            }

            var prelude = await EchoOnceAsync(query!, token).ConfigureAwait(false);
            var preludeOk = prelude.Status == NetworkJobStatus.Success;
            await OnUiAsync(() => ApplyShot(prelude, sequence: 1, replace: true)).ConfigureAwait(false);
            if (!PulsePrelude.MayStartPulse(preludeOk))
                return;

            await ProbeLoopAsync(query!, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            await OnUiAsync(() => Status = "Cancelled").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            await OnUiAsync(() =>
            {
                Replies.Clear();
                Status = "Failed";
                Summary = message;
            }).ConfigureAwait(false);
        }
        finally
        {
            HideProgress();
            _job = null;
            _cts.Dispose();
            _cts = null;
            _busy = false;
            RaiseBusy();
        }
    }

    private async Task PingLoopAsync(PingIqQuery query, CancellationToken token)
    {
        var n = query.Options.Count;
        var delay = query.Options.Interval;
        var clock = Stopwatch.StartNew();
        var samples = new List<double>();
        StatusBar?.Engine.SetIdlePolicy(0);

        for (var i = 1; i <= n; i++)
        {
            token.ThrowIfCancellationRequested();
            await WaitUntilAsync(clock, TimeSpan.FromTicks(delay.Ticks * (i - 1)), token).ConfigureAwait(false);
            var shot = await EchoOnceAsync(query, token).ConfigureAwait(false);
            samples.AddRange(SeriesRtts(shot));
            var sent = i;
            var nLocal = n;
            var elapsed = clock.Elapsed;
            await OnUiAsync(() =>
            {
                ApplyShot(shot, sequence: sent, replace: false);
                Status = $"Ping {sent} / {nLocal}";
                PostBar(sent, nLocal, elapsed);
            }).ConfigureAwait(false);
        }

        var pingSamples = samples;
        var pingTotal = n;
        await OnUiAsync(() =>
        {
            WriteSummary();
            Dashboard?.ShowEcho(pingSamples);
            Dashboard?.Unlock();
            Status = $"Ping {pingTotal} / {pingTotal}";
            StatusBar?.Engine.SetIdlePolicy(3000, "Idle. . .");
        }).ConfigureAwait(false);
    }

    private async Task ProbeLoopAsync(PingIqQuery query, CancellationToken token)
    {
        if (!PulsePlan.TryCreate(RequestCount, DurationSeconds, out var plan, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        var clock = Stopwatch.StartNew();
        var last = default(IcmpEchoResult);
        var samples = new List<double>();
        StatusBar?.Engine.SetIdlePolicy(0);
        for (var i = 1; i <= plan.Requests; i++)
        {
            token.ThrowIfCancellationRequested();
            await WaitUntilAsync(clock, plan.DueAt(i), token).ConfigureAwait(false);
            last = await EchoOnceAsync(query, token).ConfigureAwait(false);
            samples.AddRange(SeriesRtts(last));
            var sent = i;
            var total = plan.Requests;
            var elapsed = clock.Elapsed;
            var shot = last;
            await OnUiAsync(() =>
            {
                ApplyShot(shot, sequence: Replies.Count + 1, replace: false);
                Status = $"Probe {sent} / {total}";
                PostBar(sent, total, elapsed);
            }).ConfigureAwait(false);
        }

        var probeSamples = samples;
        var probeTotal = plan.Requests;
        await OnUiAsync(() =>
        {
            Dashboard?.ShowProbe(probeSamples);
            Status = $"Probe {probeTotal} / {probeTotal}";
        }).ConfigureAwait(false);
        StatusBar?.Engine.SetIdlePolicy(3000, "Idle. . .");
    }

    private async Task<IcmpEchoResult> EchoOnceAsync(PingIqQuery query, CancellationToken token)
    {
        var options = new IcmpEchoOptions
        {
            Count = 1,
            Timeout = query.Options.Timeout,
            InterfaceIndex = query.Options.InterfaceIndex,
            SourceAddress = query.Options.SourceAddress
        };
        _job = NetworkHelper.IcmpEcho(query.Target, options);
        return await _job.RunAsync(token).ConfigureAwait(true);
    }

    private static async Task WaitUntilAsync(Stopwatch clock, TimeSpan due, CancellationToken token)
    {
        while (clock.Elapsed < due)
        {
            token.ThrowIfCancellationRequested();
            var remaining = due - clock.Elapsed;
            var slice = remaining > TimeSpan.FromMilliseconds(15) ? TimeSpan.FromMilliseconds(15) : remaining;
            if (slice > TimeSpan.Zero)
                await Task.Delay(slice, token).ConfigureAwait(false);
        }
    }

    private static List<double> SeriesRtts(IcmpEchoResult result)
        => result.Replies
            .Where(r => r.Status is IcmpEchoStatus.Success
                or IcmpEchoStatus.DestinationUnreachable
                or IcmpEchoStatus.TtlExpired)
            .Select(r => (double)r.RoundtripTimeMs)
            .ToList();

    private void ApplyShot(IcmpEchoResult result, int sequence, bool replace)
    {
        if (replace)
            Replies.Clear();

        foreach (var reply in result.Replies)
        {
            Replies.Add(new ReplyRow(
                sequence,
                reply.Status.ToString(),
                reply.Address,
                reply.RoundtripTimeMs,
                reply.Ttl,
                reply.Detail));
        }

        WriteSummary();
        Status = result.Status.ToString();
    }

    private void WriteSummary()
    {
        var sent = Replies.Count;
        var recv = Replies.Count(r => r.Status == nameof(IcmpEchoStatus.Success));
        var lost = Math.Max(0, sent - recv);
        var times = Replies.Where(r => r.Status == nameof(IcmpEchoStatus.Success)).Select(r => r.RttMs).ToList();
        var avg = times.Count == 0 ? "—" : times.Average().ToString("0.#");
        var min = times.Count == 0 ? "—" : times.Min().ToString();
        var max = times.Count == 0 ? "—" : times.Max().ToString();
        var loss = sent == 0 ? 0 : 100.0 * lost / sent;
        Summary = $"sent={sent} recv={recv} lost={lost} loss={loss:0.#}% min={min} max={max} avg={avg} ms";
    }

    private void PostBar(int sent, int total, TimeSpan elapsed)
    {
        if (StatusBar is null)
            return;
        StatusBar.Engine.PostImmediate("message", new StatusBarUpdate { Text = $"{sent} / {total}" });
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = total == 0 ? 0 : Math.Clamp(100.0 * sent / total, 0, 100),
            IsProgressVisible = true,
            IsIndeterminate = false
        });
        StatusBar.Engine.PostImmediate("detail", new StatusBarUpdate { Text = FormatElapsed(elapsed) });
    }

    private void HideProgress()
    {
        if (StatusBar is null)
            return;
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = 0,
            IsProgressVisible = false,
            IsIndeterminate = false
        });
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var total = Math.Max(0, (int)Math.Floor(elapsed.TotalSeconds));
        var minutes = total / 60;
        var seconds = total % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    private static Task OnUiAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
    }

    public void BeginLoad() => _loading = true;
    public void EndLoad() => _loading = false;

    partial void OnCountChanged(decimal value) => Persist();
    partial void OnTimeoutMsChanged(decimal value) => Persist();
    partial void OnSelectedInterfaceIndexChanged(int value)
    {
        Bind.InterfaceIndex = value;
        Persist();
    }
    partial void OnRequestCountChanged(decimal value) => Persist();
    partial void OnDurationSecondsChanged(decimal value) => Persist();

    private void Persist()
    {
        if (!_loading)
            Session?.Save();
    }

    private void RaiseBusy()
    {
        OnPropertyChanged(nameof(CanStart));
        OnPropertyChanged(nameof(CanCancel));
        PingCommand.NotifyCanExecuteChanged();
        ProbeCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}
