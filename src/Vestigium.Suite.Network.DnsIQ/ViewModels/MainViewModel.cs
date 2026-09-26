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

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public IReadOnlyList<string> RecordTypes => DnsIqInput.ComboTypes;

    public ObservableCollection<AnswerRow> Answers { get; } = [];

    [ObservableProperty]
    private string _name = "localhost";

    [ObservableProperty]
    private string _server = string.Empty;

    [ObservableProperty]
    private string _recordType = "All";

    [ObservableProperty]
    private decimal _burstCount = SettingsViewModel.PulseDefault;

    [ObservableProperty]
    private decimal _durationSeconds = SettingsViewModel.PulseDefault;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LookupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProbeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    partial void OnStatusChanged(string value)
    {
        if (StatusBar is not null)
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

        if (!lookup && !PulsePlan.TryCreate(BurstCount, DurationSeconds, out _, out var pulseReject))
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
        if (!PulsePlan.TryCreate(BurstCount, DurationSeconds, out var plan, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        var clock = Stopwatch.StartNew();
        var samples = new List<double>();
        var answered = 0;
        var timeout = 0;
        var refused = 0;
        string? server = query.Options.Server;

        for (var i = 1; i <= plan.Bursts; i++)
        {
            token.ThrowIfCancellationRequested();
            var wait = plan.DueAt(i) - clock.Elapsed;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, token).ConfigureAwait(true);

            var burst = await RunBurstAsync(query, token).ConfigureAwait(true);
            server ??= burst.Server;
            answered += burst.Answered;
            timeout += burst.Timeout;
            refused += burst.Refused;
            samples.AddRange(burst.AnsweredMs);
            var maxMs = burst.AnsweredMs.Count == 0 ? 0 : (int)Math.Round(burst.AnsweredMs.Max());
            Status = $"pulse {i}/{plan.Bursts} · {FormatServer(server)} · {maxMs} ms";
        }

        var med = Median(samples);
        var rate = plan.Seconds == 0 ? 0 : plan.Bursts / (double)plan.Seconds;
        Status =
            $"{plan.Bursts}/{plan.Bursts} · {FormatServer(server)} · med {med} ms · {rate:0.0} burst/s · {timeout} timeout · {answered} answered · {refused} refused";
    }

    private async Task<BurstStats> RunBurstAsync(DnsIqQuery query, CancellationToken token)
    {
        IReadOnlyList<DnsIqQuery> questions = query.AllTypes
            ? TypedQueries(query).ToList()
            : [query];

        var tasks = questions
            .Select(q => NetworkHelper.LookupAsync(q.Name, q.Options, token))
            .ToArray();
        var results = await Task.WhenAll(tasks).ConfigureAwait(true);

        var answeredMs = new List<double>();
        var answered = 0;
        var timeout = 0;
        var refused = 0;
        string? server = query.Options.Server;
        foreach (var result in results)
        {
            server ??= result.Server;
            if (result.Rcode is DnsRcode.Timeout or DnsRcode.Failed)
            {
                timeout++;
                continue;
            }

            if (result.Rcode == DnsRcode.Refused)
            {
                refused++;
                continue;
            }

            answered++;
            answeredMs.Add(result.Elapsed.TotalMilliseconds);
        }

        return new BurstStats(server, answered, timeout, refused, answeredMs);
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

    private sealed record BurstStats(
        string? Server,
        int Answered,
        int Timeout,
        int Refused,
        List<double> AnsweredMs);
}
