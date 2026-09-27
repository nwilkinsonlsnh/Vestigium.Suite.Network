using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private NetworkJob<AdapterWatchResult>? _watchJob;
    private bool _busy;
    private bool _loading;

    public MainViewModel()
    {
        Adapters = [];
    }

    public ObservableCollection<AdapterRow> Adapters { get; }

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public NicIqSession? Session { get; set; }

    public SettingsViewModel? Settings { get; set; }

    public void BeginLoad() => _loading = true;

    public void EndLoad() => _loading = false;

    [ObservableProperty]
    private string _header = "NicIQ";

    [ObservableProperty]
    private string _caption = "Idle";

    [ObservableProperty]
    private string _detail = string.Empty;

    [ObservableProperty]
    private bool _includeDown = true;

    [ObservableProperty]
    private bool _ipEnabledOnly = true;

    [ObservableProperty]
    private bool _monitorReceive = true;

    [ObservableProperty]
    private bool _monitorSend = true;

    [ObservableProperty]
    private bool _monitorErrors;

    [ObservableProperty]
    private bool _monitorDiscards;

    [ObservableProperty]
    private decimal _durationSeconds = NicIqWatchInput.DefaultDurationSeconds;

    [ObservableProperty]
    private AdapterRow? _selectedAdapter;

    public bool CanRefresh => !_busy;

    public bool CanWatch => !_busy && SelectedAdapter is not null;

    public bool CanCancelWatch => _busy;

    partial void OnSelectedAdapterChanged(AdapterRow? value)
    {
        Detail = value?.Detail ?? string.Empty;
        WatchCommand.NotifyCanExecuteChanged();
    }

    partial void OnIncludeDownChanged(bool value)
    {
        if (_loading)
            return;
        if (Settings is not null)
        {
            Settings.BeginLoad();
            Settings.IncludeDown = value;
            Settings.EndLoad();
        }

        Session?.Save();
        if (_busy)
            return;
        Refresh();
    }

    partial void OnIpEnabledOnlyChanged(bool value)
    {
        if (_loading)
            return;
        if (Settings is not null)
        {
            Settings.BeginLoad();
            Settings.IpEnabledOnly = value;
            Settings.EndLoad();
        }

        Session?.Save();
        if (_busy)
            return;
        Refresh();
    }

    partial void OnDurationSecondsChanged(decimal value)
    {
        if (_loading)
            return;
        if (Settings is not null)
        {
            Settings.BeginLoad();
            Settings.DefaultDurationSeconds = NicIqSession.ClampDuration(value);
            Settings.EndLoad();
        }

        Session?.Save();
    }

    partial void OnMonitorReceiveChanged(bool value) => PersistMonitors();
    partial void OnMonitorSendChanged(bool value) => PersistMonitors();
    partial void OnMonitorErrorsChanged(bool value) => PersistMonitors();
    partial void OnMonitorDiscardsChanged(bool value) => PersistMonitors();

    private void PersistMonitors()
    {
        if (_loading)
            return;
        Session?.Save();
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private void Refresh()
    {
        if (_busy)
            return;

        var keep = SelectedAdapter?.Id;
        try
        {
            var box = NetworkHelper.GetWorkstation();
            Header = FormatHeader(box);
            var query = new NetworkAdapterQuery(IncludeDown: IncludeDown, IpEnabledOnly: IpEnabledOnly);
            var rows = NetworkHelper.GetAdapters(query).Select(static a => new AdapterRow(a)).ToList();
            ReplaceRows(rows, keep);
            Post("Idle");
        }
        catch (Exception ex)
        {
            Post("Failed", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanWatch))]
    private async Task WatchAsync()
    {
        if (_busy)
            return;

        var key = SelectedAdapter?.Id;
        if (string.IsNullOrWhiteSpace(key))
            key = SelectedAdapter?.Name;

        if (!NicIqWatchInput.TryCreate(key, DurationSeconds, out var query, out var reject))
        {
            Post("Failed", reject);
            return;
        }

        _busy = true;
        RaiseBusy();
        var name = SelectedAdapter?.Name ?? query.AdapterKey;
        Post($"Monitoring {name}  status Up ↔ Down");
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        try
        {
            _watchJob = NetworkHelper.WatchAdapter(query!.AdapterKey, new AdapterWatchOptions
            {
                Duration = query.Duration,
                Interval = TimeSpan.FromSeconds(1)
            });
            var result = await _watchJob.RunAsync(token).ConfigureAwait(true);
            var changed = WatchStatusFlipped(result);
            Post(FormatWatch(result, changed));
            Refresh();
            MarkStatusChanged(query.AdapterKey, changed);
        }
        catch (OperationCanceledException)
        {
            Post("Cancelled");
        }
        catch (Exception ex)
        {
            Post("Failed", ex.Message);
        }
        finally
        {
            _busy = false;
            RaiseBusy();
            _watchJob = null;
            _cts.Dispose();
            _cts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelWatch))]
    private void Cancel()
    {
        _watchJob?.Cancel();
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    [RelayCommand]
    private void CopyDetail()
    {
        if (string.IsNullOrWhiteSpace(Detail))
            return;
        Clipboard.SetText(Detail);
        Post("Idle", "Copied");
    }

    private void RaiseBusy()
    {
        OnPropertyChanged(nameof(CanRefresh));
        OnPropertyChanged(nameof(CanWatch));
        OnPropertyChanged(nameof(CanCancelWatch));
        RefreshCommand.NotifyCanExecuteChanged();
        WatchCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void ReplaceRows(IReadOnlyList<AdapterRow> rows, string? keepId)
    {
        Adapters.Clear();
        foreach (var row in rows)
            Adapters.Add(row);

        if (!string.IsNullOrWhiteSpace(keepId))
        {
            SelectedAdapter = Adapters.FirstOrDefault(r => string.Equals(r.Id, keepId, StringComparison.Ordinal));
            return;
        }

        if (SelectedAdapter is not null && Adapters.Contains(SelectedAdapter))
            return;

        SelectedAdapter = Adapters.Count > 0 ? Adapters[0] : null;
    }

    private void MarkStatusChanged(string key, bool changed)
    {
        if (!changed)
            return;

        for (var i = 0; i < Adapters.Count; i++)
        {
            var row = Adapters[i];
            if (!string.Equals(row.Id, key, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(row.Name, key, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(row.Source.Description, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            row.StatusChanged = true;
            Adapters.RemoveAt(i);
            Adapters.Insert(i, row);
            SelectedAdapter = row;
            Detail = row.Detail;
            break;
        }
    }

    private static bool WatchStatusFlipped(AdapterWatchResult result)
    {
        if (result.FirstStatus != result.LastStatus)
            return true;
        return result.Samples.Select(s => s.Status).Distinct().Count() > 1;
    }

    private void Post(string status, string? detail = null)
    {
        Caption = string.IsNullOrWhiteSpace(detail) ? status : $"{status}  {detail}";
        if (StatusBar is not null)
            StatusBar.Message = Caption;
    }

    private static string FormatWatch(AdapterWatchResult result, bool changed)
    {
        var last = result.Samples.Count > 0 ? result.Samples[^1] : null;
        var speed = last is null ? result.LastStatus.ToString() : $"{result.LastStatus}  {LinkSpeed.Format(last.SpeedBitsPerSecond)}";
        if (!changed)
            return speed;
        return $"{speed}  {result.FirstStatus} → {result.LastStatus}";
    }

    private static string FormatHeader(WorkstationNetwork box)
    {
        var host = string.IsNullOrWhiteSpace(box.HostName) ? "—" : box.HostName.Trim();
        var line = string.IsNullOrWhiteSpace(box.DomainName) ? host : $"{host}  /  {box.DomainName.Trim()}";
        if (box.Stack is { } stack)
        {
            var dns = stack.DhcpNameServers.Count == 0 ? "—" : string.Join(", ", stack.DhcpNameServers);
            line = $"{line}{Environment.NewLine}DHCP DNS  {dns}  router {(stack.IpEnableRouter == true ? "yes" : "no")}";
        }

        if (box.DnsSuffixSearchList.Count == 0)
            return line;
        return $"{line}{Environment.NewLine}Search  {string.Join(", ", box.DnsSuffixSearchList)}";
    }
}
