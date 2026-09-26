using System.Collections.ObjectModel;
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

    public BindFields Bind { get; } = new();

    public PingIqSession? Session { get; set; }

    public IReadOnlyList<AdapterChoice> Interfaces { get; }

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


    public ObservableCollection<ReplyRow> Replies { get; } = [];

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

    public bool CanEcho => !_busy;

    public bool CanCancel => _busy;

    [RelayCommand(CanExecute = nameof(CanEcho))]
    private async Task EchoAsync()
    {
        if (_busy)
            return;

        if (!PingIqInput.TryCreate(Target, Count, TimeoutMs, Bind.InterfaceIndex, Bind.SourceAddress, out var query, out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        _busy = true;
        RaiseBusy();
        Replies.Clear();
        Summary = string.Empty;
        Status = "Running";
        _cts = new CancellationTokenSource();
        try
        {
            _job = NetworkHelper.IcmpEcho(query!.Target, query.Options);
            var result = await _job.RunAsync(_cts.Token).ConfigureAwait(true);
            ApplyResult(result);
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

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _job?.Cancel();
        try
        {
            _cts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

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

        Status = result.Status.ToString();
        var avg = result.AverageMs is { } ms ? $"{ms:0.#}" : "—";
        var min = result.MinMs is { } lo ? lo.ToString() : "—";
        var max = result.MaxMs is { } hi ? hi.ToString() : "—";
        Summary = $"sent={result.Sent} recv={result.Received} lost={result.Lost} loss={result.LossPercent:0.#}% min={min} max={max} avg={avg} ms";
    }

    public void BeginLoad() => _loading = true;
    public void EndLoad() => _loading = false;

    private bool _loading;

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
        OnPropertyChanged(nameof(CanEcho));
        OnPropertyChanged(nameof(CanCancel));
        EchoCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}
