using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Helpers.PerfMon;
using Vestigium.Helpers.PerfMon.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _monitorCts;
    private NetworkJob<AdapterWatchResult>? _watchJob;
    private bool _busy;
    private bool _loading;
    private bool _syncingNic;
    private int _paintSkip;
    private readonly MonitorRing _ring = new();

    public MainViewModel()
    {
        Adapters = [];
        ActiveNics = [];
        Samples = [];
    }

    public ObservableCollection<AdapterRow> Adapters { get; }

    public ObservableCollection<AdapterRow> ActiveNics { get; }

    public ObservableCollection<MonitorSampleRow> Samples { get; }

    public string? PreferredAdapterId { get; set; }

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

    [ObservableProperty]
    private AdapterRow? _selectedMonitorNic;

    [ObservableProperty]
    private string _monitorInstance = string.Empty;

    [ObservableProperty]
    private bool _isMonitoring;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThroughputPageOpen))]
    [NotifyPropertyChangedFor(nameof(PacketsPageOpen))]
    [NotifyPropertyChangedFor(nameof(IntegrityPageOpen))]
    [NotifyPropertyChangedFor(nameof(UtilizationPageOpen))]
    private string _chartPage = MonitorChartPages.Throughput;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChart))]
    [NotifyPropertyChangedFor(nameof(ChartEmpty))]
    private FrameworkElement? _liveChart;

    [ObservableProperty]
    private string _chartStrip = "Waiting for samples.";

    public bool HasChart => LiveChart is not null;

    public bool ChartEmpty => LiveChart is null;

    public bool ThroughputPageOpen
    {
        get => ChartPage == MonitorChartPages.Throughput;
        set { if (value) ChartPage = MonitorChartPages.Throughput; }
    }

    public bool PacketsPageOpen
    {
        get => ChartPage == MonitorChartPages.Packets;
        set { if (value) ChartPage = MonitorChartPages.Packets; }
    }

    public bool IntegrityPageOpen
    {
        get => ChartPage == MonitorChartPages.Integrity;
        set { if (value) ChartPage = MonitorChartPages.Integrity; }
    }

    public bool UtilizationPageOpen
    {
        get => ChartPage == MonitorChartPages.Utilization;
        set { if (value) ChartPage = MonitorChartPages.Utilization; }
    }

    public bool CanRefresh => !_busy;

    public bool CanWatch => !_busy && SelectedAdapter is not null;

    public bool CanCancelWatch => _busy;

    partial void OnSelectedAdapterChanged(AdapterRow? value)
    {
        Detail = value?.Detail ?? string.Empty;
        WatchCommand.NotifyCanExecuteChanged();
        if (_syncingNic)
            return;
        if (value is not null && value.Source.Status == OperationalStatus.Up)
            SelectedMonitorNic = ActiveNics.FirstOrDefault(r => string.Equals(r.Id, value.Id, StringComparison.Ordinal));
    }

    partial void OnSelectedMonitorNicChanged(AdapterRow? value)
    {
        if (_syncingNic || value is null)
            return;
        _syncingNic = true;
        SelectedAdapter = Adapters.FirstOrDefault(r => string.Equals(r.Id, value.Id, StringComparison.Ordinal)) ?? value;
        _syncingNic = false;
        PreferredAdapterId = value.Id;
        if (!_loading)
            Session?.Save();
        RestartMonitoring();
    }

    partial void OnChartPageChanged(string value) => PaintChart();

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
            SyncActiveNics();
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
        StatusBar?.Engine.SetIdlePolicy(0);
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
            StatusBar?.Engine.SetIdlePolicy(3000);
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

        var preferred = NicPrimaryAdapter.Pick(Adapters.Select(r => r.Source), PreferredAdapterId);
        SelectedAdapter = preferred is null
            ? null
            : Adapters.FirstOrDefault(r => string.Equals(r.Id, preferred.Id, StringComparison.Ordinal));
    }

    private void SyncActiveNics()
    {
        var keep = SelectedMonitorNic?.Id ?? PreferredAdapterId ?? SelectedAdapter?.Id;
        ActiveNics.Clear();
        foreach (var row in Adapters.Where(r => r.Source.Status == OperationalStatus.Up))
            ActiveNics.Add(row);

        AdapterRow? next = null;
        if (!string.IsNullOrWhiteSpace(keep))
            next = ActiveNics.FirstOrDefault(r => string.Equals(r.Id, keep, StringComparison.Ordinal));
        next ??= ActiveNics.FirstOrDefault(r => SelectedAdapter is not null && string.Equals(r.Id, SelectedAdapter.Id, StringComparison.Ordinal));
        if (next is null)
        {
            var primary = NicPrimaryAdapter.Pick(ActiveNics.Select(r => r.Source), PreferredAdapterId);
            if (primary is not null)
                next = ActiveNics.FirstOrDefault(r => string.Equals(r.Id, primary.Id, StringComparison.Ordinal));
        }

        _syncingNic = true;
        SelectedMonitorNic = next;
        _syncingNic = false;
        if (next is not null)
            PreferredAdapterId = next.Id;
    }

    public void StartMonitoring()
    {
        if (_monitorCts is not null)
            return;
        _monitorCts = new CancellationTokenSource();
        IsMonitoring = true;
        _ = RunMonitorLoopAsync(_monitorCts.Token);
    }

    public void StopMonitoring()
    {
        try { _monitorCts?.Cancel(); }
        catch (ObjectDisposedException) { }
        _monitorCts?.Dispose();
        _monitorCts = null;
        IsMonitoring = false;
    }

    public void RestartMonitoring()
    {
        _ring.Clear();
        LiveChart = null;
        ChartStrip = "Waiting for samples.";
        _paintSkip = 0;
        StopMonitoring();
        StartMonitoring();
    }

    private async Task RunMonitorLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var nic = SelectedMonitorNic ?? SelectedAdapter;
                if (nic is null)
                {
                    MonitorInstance = string.Empty;
                    Post("Idle", "No active NIC");
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(true);
                    continue;
                }

                IReadOnlyList<string> live;
                try
                {
                    live = NetworkCounterCatalog.LiveInstances(NetworkInterface.Category);
                }
                catch (Exception)
                {
                    live = [];
                }

                var instance = NicPdhInstance.Resolve(nic.Name, nic.Source.Description, live);
                MonitorInstance = instance ?? string.Empty;
                if (string.IsNullOrWhiteSpace(instance))
                {
                    Post("Failed", $"No PDH instance for {nic.Name}");
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(true);
                    continue;
                }

                var selected = Settings is null
                    ? MonitorCounterList.FromSettings(Session?.Current ?? new NicIqSettings())
                    : MonitorCounterList.Sanitize(Settings.MonitorCounters);
                var counters = MonitorCounterList.ForSample(selected);
                if (counters.Count == 0)
                    counters = MonitorCounterList.Sanitize(MonitorCounterList.SeedReceiveSend);

                var paths = NetworkCounterCatalog.Paths(NetworkInterface.Category, instance, counters);
                if (paths.Count == 0)
                {
                    Post("Failed", "No counters selected");
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(true);
                    continue;
                }

                var job = new SampleJob(paths, new SampleJobOptions
                {
                    Interval = TimeSpan.FromSeconds(1),
                    Count = 1
                });
                var result = await job.RunAsync(token).ConfigureAwait(true);
                ApplySamples(result);
                Post($"Monitoring {nic.Name}", instance);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Post("Failed", ex.Message);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }

    private void ApplySamples(SampleJobResult result)
    {
        var latest = result.Samples
            .GroupBy(s => s.Counter, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();
        Samples.Clear();
        foreach (var row in latest)
        {
            Samples.Add(new MonitorSampleRow(row));
            _ring.Add(row);
        }

        _paintSkip++;
        if (_paintSkip >= 3 || LiveChart is null)
        {
            _paintSkip = 0;
            PaintChart();
        }
    }

    private void PaintChart()
    {
        var (view, strip) = MonitorChart.Paint(ChartPage, _ring);
        LiveChart = view;
        ChartStrip = strip;
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
        var last = result.Samples.Count == 0 ? result.LastStatus.ToString() : $"{result.LastStatus}  {LinkSpeed.Format(last.SpeedBitsPerSecond)}";
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
