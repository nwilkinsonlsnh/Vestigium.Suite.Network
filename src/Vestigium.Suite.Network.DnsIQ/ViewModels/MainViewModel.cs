using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private NetworkJob<DnsProbeResult>? _probeJob;

    public BindFields Bind { get; } = new();

    public IReadOnlyList<string> RecordTypes => DnsIqInput.RecordTypes;

    public ObservableCollection<AnswerRow> Answers { get; } = [];

    [ObservableProperty]
    private string _name = "localhost";

    [ObservableProperty]
    private string _server = string.Empty;

    [ObservableProperty]
    private string _recordType = "A";

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LookupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProbeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

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
        Answers.Clear();

        try
        {
            if (lookup)
            {
                var result = await NetworkHelper.LookupAsync(query!.Name, query.Options, token).ConfigureAwait(true);
                ReplaceAnswers(result.Answers);
                Status = result.Rcode.ToString();
                return;
            }

            _probeJob = NetworkHelper.ProbeDns(query!.Name, query.Options);
            var probe = await _probeJob.RunAsync(token).ConfigureAwait(true);
            ReplaceAnswers(probe.Lookup.Answers);
            Status = probe.Status.ToString();
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        catch (Exception ex)
        {
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

    private void ReplaceAnswers(IReadOnlyList<DnsRecord> records)
    {
        Answers.Clear();
        foreach (var answer in records)
        {
            Answers.Add(new AnswerRow(
                answer.Type.ToString(),
                answer.Name,
                answer.Data,
                answer.Ttl));
        }
    }
}
