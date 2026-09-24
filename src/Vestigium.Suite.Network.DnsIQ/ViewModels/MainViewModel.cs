using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private bool _pulseActive;
    private bool _loading;
    private TimeSpan _lastPulseUi;

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public DnsIqSession? Session { get; set; }

    public DashboardViewModel? Dashboard { get; set; }

    public IReadOnlyList<string> RecordTypes => DnsIqInput.ComboTypes;

    public IReadOnlyList<ServerOption> ServerOptions { get; } = DnsIqInput.ServerOptions();

    public IReadOnlyList<AdapterChoice> Interfaces { get; }

    public ObservableCollection<AnswerRow> Answers { get; } = [];

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
    [NotifyPropertyChangedFor(nameof(ShowNameHint))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _server = DnsIqInput.FirstConfiguredDns() ?? string.Empty;

    [ObservableProperty]
    private string _recordType = "All";

    [ObservableProperty]
    private decimal _port = DnsIqInput.DefaultPort;

    [ObservableProperty]
    private int _selectedInterfaceIndex;

    [ObservableProperty]
    private decimal _requestCount = SettingsViewModel.RequestDefault;

    [ObservableProperty]
    private decimal _durationSeconds = SettingsViewModel.SecondsDefault;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LookupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProbeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    public bool ShowNameHint => string.IsNullOrWhiteSpace(Name);

    public void BeginLoad() => _loading = true;
    public void EndLoad() => _loading = false;

    partial void OnServerChanged(string value) => Persist();
    partial void OnRecordTypeChanged(string value) => Persist();
    partial void OnPortChanged(decimal value) => Persist();
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

    partial void OnStatusChanged(string value)
    {
        if (_pulseActive || StatusBar is null)
            return;
        StatusBar.Message = value;
    }

    private bool CanStartJob() => !IsBusy;
    private bool CanCancelJob() => IsBusy;

    [RelayCommand(CanExecute = nameof(CanStartJob))]
    private Task LookupAsync() => RunJobAsync(lookup: true);

    [RelayCommand(CanExecute = nameof(CanStartJob))]
    private Task ProbeAsync() => RunJobAsync(lookup: false);

    [RelayCommand(CanExecute = nameof(CanCancelJob))]
    private void Cancel()
    {
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    private async Task RunJobAsync(bool lookup)
    {
        if (!DnsIqInput.TryCreate(
                Name, Server, RecordType, Bind.InterfaceIndex, Bind.SourceAddress, (int)Port,
                out var query, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        if (!lookup && !PulsePlan.TryCreate(RequestCount, DurationSeconds, out _, out var pulseReject))
        {
            Status = pulseReject ?? "Failed";
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        IsBusy = true;
        Status = "Running";
        Answers.Clear();

        try
        {
            var preludeOk = await LookupAnswersAsync(query!, token).ConfigureAwait(true);
            if (preludeOk)
            {
                Dashboard?.Unlock();
                Dashboard?.ShowLookup(Answers.ToList());
            }
            if (lookup)
                return;
            if (!PulsePrelude.MayStartPulse(preludeOk))
                return;
            await ProbeAnswersAsync(query!, token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        catch (Exception ex)
        {
            if (lookup)
                Answers.Clear();
            Status = string.IsNullOrWhiteSpace(ex.Message) ? "Failed" : $"Failed: {ex.Message}";
        }
        finally
        {
            _pulseActive = false;
            if (!lookup)
            {
                ShowProgress(0, visible: false);
                StatusBar?.Engine.SetIdlePolicy(3000, "Idle. . .");
            }
            _cts.Dispose();
            _cts = null;
            IsBusy = false;
        }
    }

    private async Task<bool> LookupAnswersAsync(DnsIqQuery query, CancellationToken token)
    {
        if (!query.AllTypes)
        {
            var result = await NetworkHelper.LookupAsync(query.Name, query.Options, token).ConfigureAwait(true);
            if (result.Rcode is DnsRcode.Timeout or DnsRcode.Failed)
            {
                Answers.Clear();
                Status = FormatLookupStatus(result.Rcode, result.Server ?? query.Options.Server, result.Elapsed, types: 1);
                return false;
            }

            AppendAnswers(result.Answers);
            SortAnswers();
            Status = FormatLookupStatus(result.Rcode, result.Server ?? query.Options.Server, result.Elapsed, types: 1);
            return true;
        }

        DnsRcode? last = null;
        var anyOk = false;
        var elapsed = TimeSpan.Zero;
        string? server = query.Options.Server;
        var types = 0;
        foreach (var typed in TypedQueries(query))
        {
            token.ThrowIfCancellationRequested();
            var result = await NetworkHelper.LookupAsync(typed.Name, typed.Options, token).ConfigureAwait(true);
            last = result.Rcode;
            elapsed += result.Elapsed;
            server ??= result.Server;
            types++;
            if (result.Rcode is not DnsRcode.Timeout and not DnsRcode.Failed)
                anyOk = true;
            AppendAnswers(result.Answers);
        }

        SortAnswers();
        var ok = anyOk || Answers.Count > 0;
        if (!ok)
            Answers.Clear();
        var rcode = ok ? DnsRcode.NoError : (last ?? DnsRcode.Failed);
        Status = FormatLookupStatus(rcode, server, elapsed, types);
        return ok;
    }

    private async Task ProbeAnswersAsync(DnsIqQuery query, CancellationToken token)
    {
        if (!PulsePlan.TryCreate(RequestCount, DurationSeconds, out var plan, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        var cycle = query.AllTypes ? TypedQueries(query).ToList() : [query];
        if (cycle.Count == 0)
            cycle.Add(query);

        _pulseActive = true;
        _lastPulseUi = TimeSpan.Zero;
        StatusBar?.Engine.SetIdlePolicy(0);
        var clock = Stopwatch.StartNew();
        var window = TimeSpan.FromSeconds(plan.Seconds);
        TickPulseUi(0, plan.Requests, clock.Elapsed, window, "Pulse: 0 / " + plan.Requests, force: true);

        var samples = new List<double>();
        var answered = 0;
        var timeout = 0;
        var refused = 0;
        string? server = query.Options.Server;
        var sent = 0;
        var inflight = new List<Task<DnsLookupResult>>();

        for (var i = 1; i <= plan.Requests; i++)
        {
            token.ThrowIfCancellationRequested();
            await WaitUntilAsync(clock, plan.DueAt(i), token).ConfigureAwait(false);

            var typed = cycle[(i - 1) % cycle.Count];
            inflight.Add(NetworkHelper.LookupAsync(typed.Name, typed.Options, token));
            sent = i;
            DrainCompleted(inflight, ref answered, ref timeout, ref refused, samples, ref server);
            TickPulseUi(
                sent, plan.Requests, clock.Elapsed, window,
                FormatPulseLive(sent, plan.Requests, inflight.Count, server, clock.Elapsed, draining: false),
                force: i == plan.Requests);
        }

        while (inflight.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            var finished = await Task.WhenAny(inflight).ConfigureAwait(false);
            inflight.Remove(finished);
            ApplyResult(await finished.ConfigureAwait(false), ref answered, ref timeout, ref refused, samples, ref server);
            TickPulseUi(
                sent, plan.Requests, clock.Elapsed, window,
                FormatPulseLive(sent, plan.Requests, inflight.Count, server, clock.Elapsed, draining: true),
                force: inflight.Count == 0);
        }

        var elapsed = clock.Elapsed;
        var med = Median(samples);
        var rate = plan.Seconds == 0 ? 0 : plan.Requests / (double)plan.Seconds;
        var summary =
            $"Pulse: {plan.Requests} / {plan.Requests} \u00b7 {FormatServer(server)} \u00b7 Med {med} ms \u00b7 {rate:0.0}/s \u00b7 {timeout} Timeout \u00b7 {answered} Answered \u00b7 {refused} Refused \u00b7 Elapsed {FormatElapsed(elapsed)}";

        await OnUiAsync(() =>
        {
            Dashboard?.Unlock();
            Dashboard?.ShowProbe(samples);
            Status = summary;
            PostPulseBar(plan.Requests, plan.Requests, elapsed, window);
        }).ConfigureAwait(true);
    }

    private static string FormatPulseLive(
        int sent, int total, int inflight, string? server, TimeSpan elapsed, bool draining)
    {
        var phase = draining ? "Drain" : "In Flight";
        return $"Pulse: {sent} / {total} \u00b7 {phase} {inflight} \u00b7 {FormatServer(server)} \u00b7 Elapsed {FormatElapsed(elapsed)}";
    }

    private void TickPulseUi(int sent, int total, TimeSpan elapsed, TimeSpan window, string status, bool force)
    {
        if (!force && elapsed - _lastPulseUi < TimeSpan.FromMilliseconds(100))
            return;
        _lastPulseUi = elapsed;
        OnUi(() =>
        {
            Status = status;
            PostPulseBar(sent, total, elapsed, window);
        });
    }

    private static void OnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            action();
        else
            dispatcher.BeginInvoke(action, DispatcherPriority.Background);
    }

    private static Task OnUiAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }

        return dispatcher.InvokeAsync(action, DispatcherPriority.Background).Task;
    }

    private static void DrainCompleted(
        List<Task<DnsLookupResult>> inflight,
        ref int answered,
        ref int timeout,
        ref int refused,
        List<double> samples,
        ref string? server)
    {
        for (var i = inflight.Count - 1; i >= 0; i--)
        {
            if (!inflight[i].IsCompleted)
                continue;
            var task = inflight[i];
            inflight.RemoveAt(i);
            ApplyResult(task.GetAwaiter().GetResult(), ref answered, ref timeout, ref refused, samples, ref server);
        }
    }

    private static void ApplyResult(
        DnsLookupResult result,
        ref int answered,
        ref int timeout,
        ref int refused,
        List<double> samples,
        ref string? server)
    {
        server ??= result.Server;
        Classify(result, ref answered, ref timeout, ref refused, samples);
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

    private void PostPulseBar(int sent, int total, TimeSpan elapsed, TimeSpan window)
    {
        if (StatusBar is null)
            return;
        StatusBar.Engine.PostImmediate("message", new StatusBarUpdate { Text = $"{sent} / {total}" });
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = PercentOfWindow(elapsed, window),
            IsProgressVisible = true,
            IsIndeterminate = false
        });
        StatusBar.Engine.PostImmediate("detail", new StatusBarUpdate { Text = FormatElapsed(elapsed) });
    }

    private static double PercentOfWindow(TimeSpan elapsed, TimeSpan window)
    {
        if (window <= TimeSpan.Zero)
            return 100;
        return Math.Clamp(100.0 * elapsed.TotalSeconds / window.TotalSeconds, 0, 100);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        var total = Math.Max(0, (int)Math.Floor(elapsed.TotalSeconds));
        var hours = total / 3600;
        var minutes = total % 3600 / 60;
        var seconds = total % 60;
        return hours > 0
            ? $"{hours}:{minutes:00}:{seconds:00}"
            : $"{minutes:00}:{seconds:00}";
    }

    private void ShowProgress(double percent, bool visible)
    {
        if (StatusBar is null)
            return;
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = Math.Clamp(percent, 0, 100),
            IsProgressVisible = visible,
            IsIndeterminate = false
        });
    }

    private static void Classify(
        DnsLookupResult result, ref int answered, ref int timeout, ref int refused, List<double> samples)
    {
        if (result.Rcode is DnsRcode.Timeout or DnsRcode.Failed)
        {
            timeout++;
            return;
        }
        if (result.Rcode == DnsRcode.Refused)
        {
            refused++;
            return;
        }
        answered++;
        samples.Add(result.Elapsed.TotalMilliseconds);
    }

    private static IEnumerable<DnsIqQuery> TypedQueries(DnsIqQuery query)
    {
        foreach (var typeName in DnsIqInput.RecordTypes)
        {
            if (DnsIqInput.TryCreate(
                    query.Name, query.Options.Server, typeName,
                    query.Options.InterfaceIndex, query.Options.SourceAddress, query.Options.Port,
                    out var typed, out _) && typed is not null)
            {
                yield return typed;
            }
        }
    }

    private void AppendAnswers(IReadOnlyList<DnsRecord> records)
    {
        foreach (var answer in records)
        {
            Answers.Add(new AnswerRow(
                DnsIqInput.DisplayType(answer.Type), answer.Name, answer.Data, answer.Ttl));
        }
    }

    private void SortAnswers()
    {
        var ordered = Answers
            .OrderBy(a => a.Type, StringComparer.OrdinalIgnoreCase)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(a => a.Data, StringComparer.OrdinalIgnoreCase)
            .ToList();
        Answers.Clear();
        foreach (var row in ordered)
            Answers.Add(row);
    }

    private static string FormatLookupStatus(DnsRcode rcode, string? server, TimeSpan elapsed, int types)
    {
        var ms = Math.Max(0, (int)Math.Round(elapsed.TotalMilliseconds));
        var core = $"{FormatServer(server)} \u00b7 {ms} ms";
        if (types > 1)
            core += $" \u00b7 {types} Types";
        return rcode is DnsRcode.NoError ? core : $"{rcode} \u00b7 {core}";
    }

    private static string FormatServer(string? server)
        => string.IsNullOrWhiteSpace(server) ? "\u2014" : server.Trim();

    private static int Median(IReadOnlyList<double> samples)
    {
        if (samples.Count == 0)
            return 0;
        var ordered = samples.OrderBy(v => v).ToArray();
        var mid = ordered.Length / 2;
        var value = ordered.Length % 2 == 1 ? ordered[mid] : (ordered[mid - 1] + ordered[mid]) / 2d;
        return (int)Math.Round(value);
    }
}
