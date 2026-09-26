using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private NetworkJob<DnsProbeResult>? _probeJob;

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
        _probeJob?.Cancel();
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
            _probeJob = null;
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
        if (!query.AllTypes)
        {
            var one = await RunProbeAsync(query, token).ConfigureAwait(true);
            Status = one.Status.ToString();
            return;
        }

        var answered = false;
        var timedOut = false;
        var refused = false;
        DnsProbeStatus last = DnsProbeStatus.Answered;
        foreach (var typed in TypedQueries(query))
        {
            token.ThrowIfCancellationRequested();
            var probe = await RunProbeAsync(typed, token).ConfigureAwait(true);
            last = probe.Status;
            answered |= probe.Status == DnsProbeStatus.Answered;
            timedOut |= probe.Status == DnsProbeStatus.TimedOut;
            refused |= probe.Status == DnsProbeStatus.Refused;
        }

        Status = answered
            ? DnsProbeStatus.Answered.ToString()
            : timedOut
                ? DnsProbeStatus.TimedOut.ToString()
                : refused
                    ? DnsProbeStatus.Refused.ToString()
                    : last.ToString();
    }

    private async Task<DnsProbeResult> RunProbeAsync(DnsIqQuery query, CancellationToken token)
    {
        _probeJob = NetworkHelper.ProbeDns(query.Name, query.Options);
        return await _probeJob.RunAsync(token).ConfigureAwait(true);
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
        var host = string.IsNullOrWhiteSpace(server) ? "—" : server.Trim();
        var line = $"{rcode} · {host} · {ms} ms";
        return types > 1 ? $"{line} · {types} types" : line;
    }
}
