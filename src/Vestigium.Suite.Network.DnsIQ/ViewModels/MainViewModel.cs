using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public IReadOnlyList<string> RecordTypes => DnsIqInput.ComboTypes;

    public IReadOnlyList<ServerOption> ServerOptions { get; } = DnsIqInput.ServerOptions();

    public ObservableCollection<AnswerRow> Answers { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNameHint))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _server = DnsIqInput.FirstConfiguredDns() ?? string.Empty;

    [ObservableProperty]
    private string _recordType = "All";

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
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private async Task RunJobAsync(bool lookup)
    {
        if (!DnsIqInput.TryCreate(
                Name,
                Server,
                RecordType,
                Bind.InterfaceIndex,
                Bind.SourceAddress,
                out var query,
                out var reject))
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
        if (lookup)
            Answers.Clear();

        try
        {
            if (lookup)
                await LookupAnswersAsync(query!, token).ConfigureAwait(true);
            else
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

    private async Task LookupAnswersAsync(DnsIqQuery query, CancellationToken token)
    {
        if (!query.AllTypes)
        {
            var result = await NetworkHelper.LookupAsync(query.Name, query.Options, token).ConfigureAwait(true);
            AppendAnswers(result.Answers);
            SortAnswers();
            Status = FormatLookupStatus(result.Rcode, result.Server ?? query.Options.Server, result.Elapsed, types: 1);
            return;
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
            if (result.Rcode == DnsRcode.NoError)
                anyOk = true;
            AppendAnswers(result.Answers);
        }

        SortAnswers();
        var rcode = anyOk || Answers.Count > 0
            ? DnsRcode.NoError
            : (last ?? DnsRcode.Failed);
        Status = FormatLookupStatus(rcode, server, elapsed, types);
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
        StatusBar?.Engine.SetIdlePolicy(0);
        var clock = Stopwatch.StartNew();
        PostPulseBar(0, plan.Requests, clock.Elapsed, TimeSpan.FromSeconds(plan.Seconds));

        var samples = new List<double>();
        var answered = 0;
        var timeout = 0;
        var refused = 0;
        string? server = query.Options.Server;
        var window = TimeSpan.FromSeconds(plan.Seconds);
        var sent = 0;

        for (var i = 1; i <= plan.Requests; i++)
        {
            token.ThrowIfCancellationRequested();
            var due = plan.DueAt(i);
            await WaitUntilAsync(clock, due, window, sent, plan.Requests, token).ConfigureAwait(true);

            var typed = cycle[(i - 1) % cycle.Count];
            var result = await NetworkHelper.LookupAsync(typed.Name, typed.Options, token).ConfigureAwait(true);
            sent = i;
            server ??= result.Server;
            Classify(result, ref answered, ref timeout, ref refused, samples);
            var lastMs = (int)Math.Round(result.Elapsed.TotalMilliseconds);
            PostPulseBar(sent, plan.Requests, clock.Elapsed, window);
            Status = $"pulse {sent}/{plan.Requests} · {FormatServer(server)} · {lastMs} ms";
        }

        PostPulseBar(plan.Requests, plan.Requests, clock.Elapsed, window);
        var med = Median(samples);
        var rate = plan.Seconds == 0 ? 0 : plan.Requests / (double)plan.Seconds;
        Status =
            $"{plan.Requests}/{plan.Requests} · {FormatServer(server)} · med {med} ms · {rate:0.0}/s · {timeout} timeout · {answered} answered · {refused} refused";
    }

    private async Task WaitUntilAsync(
        Stopwatch clock,
        TimeSpan due,
        TimeSpan window,
        int sent,
        int total,
        CancellationToken token)
    {
        while (clock.Elapsed < due)
        {
            token.ThrowIfCancellationRequested();
            PostPulseBar(sent, total, clock.Elapsed, window);
            var remaining = due - clock.Elapsed;
            var slice = remaining > TimeSpan.FromMilliseconds(100)
                ? TimeSpan.FromMilliseconds(100)
                : remaining;
            if (slice > TimeSpan.Zero)
                await Task.Delay(slice, token).ConfigureAwait(true);
        }
    }

    private void PostPulseBar(int sent, int total, TimeSpan elapsed, TimeSpan window)
    {
        if (StatusBar is null)
            return;

        StatusBar.Engine.PostImmediate("message", new StatusBarUpdate
        {
            Text = $"{sent}/{total}"
        });
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = PercentOfWindow(elapsed, window),
            IsProgressVisible = true,
            IsIndeterminate = false
        });
        StatusBar.Engine.PostImmediate("detail", new StatusBarUpdate
        {
            Text = FormatElapsed(elapsed)
        });
    }

    private static double PercentOfWindow(TimeSpan elapsed, TimeSpan window)
    {
        if (window <= TimeSpan.Zero)
            return 100;
        return Math.Clamp(100.0 * elapsed.TotalSeconds / window.TotalSeconds, 0, 100);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
            return elapsed.ToString(@"h\:mm\:ss");
        return elapsed.ToString(@"mm\:ss");
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
        DnsLookupResult result,
        ref int answered,
        ref int timeout,
        ref int refused,
        List<double> samples)
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
                    query.Name,
                    query.Options.Server,
                    typeName,
                    query.Options.InterfaceIndex,
                    query.Options.SourceAddress,
                    out var typed,
                    out _) && typed is not null)
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
                DnsIqInput.DisplayType(answer.Type),
                answer.Name,
                answer.Data,
                answer.Ttl));
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
        var line = $"{rcode} · {FormatServer(server)} · {ms} ms";
        return types > 1 ? $"{line} · {types} types" : line;
    }

    private static string FormatServer(string? server)
        => string.IsNullOrWhiteSpace(server) ? "—" : server.Trim();

    private static int Median(List<double> samples)
    {
        if (samples.Count == 0)
            return 0;
        samples.Sort();
        var mid = samples.Count / 2;
        var value = samples.Count % 2 == 1
            ? samples[mid]
            : (samples[mid - 1] + samples[mid]) / 2d;
        return (int)Math.Round(value);
    }
}
