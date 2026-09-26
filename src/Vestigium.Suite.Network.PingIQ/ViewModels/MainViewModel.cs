using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private decimal _timeoutMs = PingIqInput.DefaultTimeoutMs;

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
    private Task EchoAsync() => RunAsync(probe: false);

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
            var prelude = await EchoOnceAsync(query!, count: probe ? 1 : query!.Options.Count, token).ConfigureAwait(true);
            var preludeOk = prelude.Status == NetworkJobStatus.Success;
            ApplyResult(prelude);
            if (!probe)
            {
                Dashboard?.ShowEcho(SuccessRtts(prelude));
                return;
            }
            if (!PulsePrelude.MayStartPulse(preludeOk))
                return;

            await ProbeLoopAsync(query!, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        catch (Exception ex)
        {
            Replies.Clear();
            Summary = string.Empty;
            Status = "Failed";
            Summary = ex.Message;
        }
        finally
        {
            _job = null;
            _cts.Dispose();
            _cts = null;
            _busy = false;
            RaiseBusy();
        }
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
        for (var i = 1; i <= plan.Requests; i++)
        {
            token.ThrowIfCancellationRequested();
            await WaitUntilAsync(clock, plan.DueAt(i), token).ConfigureAwait(false);
            last = await EchoOnceAsync(query, count: 1, token).ConfigureAwait(true);
            samples.AddRange(SuccessRtts(last));
            Status = $"Probe {i} / {plan.Requests}";
        }

        if (last is not null)
            ApplyResult(last);
        Dashboard?.ShowProbe(samples);
        Status = $"Probe {plan.Requests} / {plan.Requests}";
    }

    private async Task<IcmpEchoResult> EchoOnceAsync(PingIqQuery query, int count, CancellationToken token)
    {
        var options = new IcmpEchoOptions
        {
            Count = count,
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

    private static List<double> SuccessRtts(IcmpEchoResult result)
        => result.Replies
            .Where(r => r.Status == IcmpEchoStatus.Success)
            .Select(r => (double)r.RoundtripTimeMs)
            .ToList();

    private void ApplyResult(IcmpEchoResult result)
    {
        Replies.Clear();
        foreach (var reply in result.Replies)
        {
            Replies.Add(new ReplyRow(
                reply.Sequence,
                reply.Status.ToString(),
                reply.Address,
                reply.RoundtripTimeMs,
                reply.Ttl,
                reply.Detail));
        }

        var avg = result.AverageMs is { } ms ? $"{ms:0.#}" : "—";
        var min = result.MinMs is { } lo ? lo.ToString() : "—";
        var max = result.MaxMs is { } hi ? hi.ToString() : "—";
        Summary = $"sent={result.Sent} recv={result.Received} lost={result.Lost} loss={result.LossPercent:0.#}% min={min} max={max} avg={avg} ms";
        Status = result.Status.ToString();
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
        EchoCommand.NotifyCanExecuteChanged();
        ProbeCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}
