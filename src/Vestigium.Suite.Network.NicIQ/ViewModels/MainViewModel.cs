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

    public DashboardViewModel? Dashboard { get; set; }

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
        Post("Running");
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        try
        {
            _watchJob = NetworkHelper.WatchAdapter(query!.AdapterKey, new AdapterWatchOptions
            {
                Duration = query.Duration
            });
            var result = await _watchJob.RunAsync(token).ConfigureAwait(true);
            Post(FormatWatch(result));
            Dashboard?.ShowWatch(result.Samples.Select(s => s.SpeedBitsPerSecond).ToList());
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

    private void Post(string status, string? detail = null)
    {
        Caption = string.IsNullOrWhiteSpace(detail) ? status : $"{status}  {detail}";
        if (StatusBar is not null)
            StatusBar.Message = Caption;
    }

    private static string FormatWatch(AdapterWatchResult result)
    {
        var last = result.Samples.Count > 0 ? result.Samples[^1] : null;
        if (last is null)
            return result.LastStatus.ToString();
        return $"{result.LastStatus}  {LinkSpeed.Format(last.SpeedBitsPerSecond)}";
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
