using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.Network;
using Vestigium.Helpers.PerfMon;
using Vestigium.Helpers.PerfMon.Network;
using PdhNic = Vestigium.Helpers.PerfMon.Network.NetworkInterface;
using InventoryAdapter = Vestigium.Helpers.Network.NetworkAdapter;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _monitorCts;
    private NetworkJob<AdapterWatchResult>? _watchJob;
    private IReadOnlyList<string>? _live;
    private DateTimeOffset _liveAt;
    private string? _pdhInstance;
    private string? _pdhKey;
    private bool _busy;
    private bool _loading;
    private bool _syncingNic;
    private string? _lastMonitorNote;
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
    public string? PreferredMonitorNicId { get; set; }
    public VestigiumStatusBarViewModel? StatusBar { get; set; }
    public NicIqSession? Session { get; set; }
    public SettingsViewModel? Settings { get; set; }
    public void BeginLoad() => _loading = true;
    public void EndLoad() => _loading = false;

    [ObservableProperty] private string _header = "NicIQ";
    [ObservableProperty] private string _caption = "Idle";
    [ObservableProperty] private string _detail = string.Empty;
    [ObservableProperty] private bool _showUp = true;
    [ObservableProperty] private bool _showDown = true;
    [ObservableProperty] private bool _showIpv4 = true;
    [ObservableProperty] private bool _showIpv6 = true;
    [ObservableProperty] private bool _monitorReceive = true;
    [ObservableProperty] private bool _monitorSend = true;
    [ObservableProperty] private bool _monitorErrors;
    [ObservableProperty] private bool _monitorDiscards;
    [ObservableProperty] private decimal _durationSeconds = NicIqWatchInput.DefaultDurationSeconds;
    [ObservableProperty] private AdapterRow? _selectedAdapter;
    [ObservableProperty] private AdapterRow? _selectedMonitorNic;
    [ObservableProperty] private string _monitorInstance = string.Empty;
    [ObservableProperty] private bool _isMonitoring;
    [ObservableProperty] private bool _monitorPageVisible;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThroughputPageOpen))]
    [NotifyPropertyChangedFor(nameof(PacketsPageOpen))]
    [NotifyPropertyChangedFor(nameof(IntegrityPageOpen))]
    [NotifyPropertyChangedFor(nameof(UtilizationPageOpen))]
    [NotifyPropertyChangedFor(nameof(CpuPageOpen))]
    [NotifyPropertyChangedFor(nameof(MemoryPageOpen))]
    private string _chartPage = MonitorChartPages.Throughput;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasChart))]
    [NotifyPropertyChangedFor(nameof(ChartEmpty))]
    private FrameworkElement? _liveChart;
    [ObservableProperty] private string _chartStrip = "Waiting for samples.";

    public bool HasChart => LiveChart is not null;
    public bool ChartEmpty => LiveChart is null;
    public bool ThroughputPageOpen { get => ChartPage == MonitorChartPages.Throughput; set { if (value) ChartPage = MonitorChartPages.Throughput; } }
    public bool PacketsPageOpen { get => ChartPage == MonitorChartPages.Packets; set { if (value) ChartPage = MonitorChartPages.Packets; } }
    public bool IntegrityPageOpen { get => ChartPage == MonitorChartPages.Integrity; set { if (value) ChartPage = MonitorChartPages.Integrity; } }
    public bool UtilizationPageOpen { get => ChartPage == MonitorChartPages.Utilization; set { if (value) ChartPage = MonitorChartPages.Utilization; } }
    public bool CanRefresh => !_busy;
    public bool CanWatch => !_busy && SelectedAdapter is not null;
    public bool CanCancelWatch => _busy;

    partial void OnSelectedAdapterChanged(AdapterRow? value)
    {
        Detail = value?.Detail ?? string.Empty;
        WatchCommand.NotifyCanExecuteChanged();
        if (_loading || _syncingNic) return;
        PreferredAdapterId = value?.Id;
        Session?.Save();
    }

    partial void OnSelectedMonitorNicChanged(AdapterRow? value)
    {
        if (_syncingNic || value is null) return;
        PreferredMonitorNicId = value.Id;
        if (!_loading) Session?.Save();
        RestartMonitoring();
    }

    partial void OnChartPageChanged(string value) { RaiseMonitorPages(); PaintChart(force: true); }
    partial void OnMonitorPageVisibleChanged(bool value) { if (value) PaintChart(force: true); }

    public void ApplyLegend(bool visible)
    {
        ChartTheme.WatchLegend = visible;
        if (LiveChart is not null) ChartView.SetLegendVisible(LiveChart, visible);
        PaintChart(force: true);
    }

    partial void OnShowUpChanged(bool value) => PersistFilter();
    partial void OnShowDownChanged(bool value) => PersistFilter();
    partial void OnShowIpv4Changed(bool value) => PersistFilter();
    partial void OnShowIpv6Changed(bool value) => PersistFilter();
    partial void OnDurationSecondsChanged(decimal value)
    {
        if (_loading) return;
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

    private void PersistFilter()
    {
        if (_loading) return;
        if (Settings is not null)
        {
            Settings.BeginLoad();
            Settings.ShowUp = ShowUp; Settings.ShowDown = ShowDown; Settings.ShowIpv4 = ShowIpv4; Settings.ShowIpv6 = ShowIpv6;
            Settings.EndLoad();
        }
        Session?.Save();
        if (!_busy) Refresh();
    }

    private void PersistMonitors() { if (!_loading) Session?.Save(); }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private void Refresh()
    {
        if (_busy) return;
        var keep = SelectedAdapter?.Id ?? PreferredAdapterId;
        try
        {
            var box = NetworkHelper.GetWorkstation();
            Header = FormatHeader(box);
            var inventory = NetworkHelper.GetAdapters(new NetworkAdapterQuery(IncludeDown: true, IpEnabledOnly: false)).ToList();
            ReplaceRows(AdapterListFilter.Apply(inventory, ShowUp, ShowDown, ShowIpv4, ShowIpv6), keep);
            SyncActiveNics(inventory);
            Post("Idle");
        }
        catch (Exception ex) { Post("Failed", ex.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanWatch))]
    private async Task WatchAsync()
    {
        if (_busy) return;
        var key = SelectedAdapter?.Id ?? SelectedAdapter?.Name;
        if (!NicIqWatchInput.TryCreate(key, DurationSeconds, out var query, out var reject))
        {
            Post("Failed", reject);
            return;
        }
        _busy = true; RaiseBusy(); StatusBar?.Engine.SetIdlePolicy(0);
        Post($"Monitoring {SelectedAdapter?.Name ?? query.AdapterKey}  status Up \u2194 Down");
        _cts = new CancellationTokenSource();
        try
        {
            _watchJob = NetworkHelper.WatchAdapter(query!.AdapterKey, new AdapterWatchOptions { Duration = query.Duration, Interval = TimeSpan.FromSeconds(1) });
            var result = await _watchJob.RunAsync(_cts.Token).ConfigureAwait(true);
            var changed = WatchStatusFlipped(result);
            Post(FormatWatch(result, changed));
            Refresh();
            MarkStatusChanged(query.AdapterKey, changed);
        }
        catch (OperationCanceledException) { Post("Cancelled"); }
        catch (Exception ex) { Post("Failed", ex.Message); }
        finally
        {
            _busy = false; RaiseBusy(); _watchJob = null; _cts.Dispose(); _cts = null; StatusBar?.Engine.SetIdlePolicy(3000);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancelWatch))]
    private void Cancel() { _watchJob?.Cancel(); try { _cts?.Cancel(); } catch (ObjectDisposedException) { } }

    [RelayCommand]
    private void CopyDetail()
    {
        if (string.IsNullOrWhiteSpace(Detail)) return;
        Clipboard.SetText(Detail);
        Post("Idle", "Copied");
    }

    private void RaiseBusy()
    {
        OnPropertyChanged(nameof(CanRefresh)); OnPropertyChanged(nameof(CanWatch)); OnPropertyChanged(nameof(CanCancelWatch));
        RefreshCommand.NotifyCanExecuteChanged(); WatchCommand.NotifyCanExecuteChanged(); CancelCommand.NotifyCanExecuteChanged();
    }

    private void ReplaceRows(IReadOnlyList<AdapterRow> rows, string? keepId)
    {
        Adapters.Clear();
        foreach (var row in rows) Adapters.Add(row);
        if (!string.IsNullOrWhiteSpace(keepId))
        {
            SelectedAdapter = Adapters.FirstOrDefault(r => string.Equals(r.Id, keepId, StringComparison.Ordinal));
            return;
        }
        if (SelectedAdapter is not null && Adapters.Contains(SelectedAdapter)) return;
        var preferred = NicPrimaryAdapter.Pick(Adapters.Select(r => r.Source), PreferredAdapterId);
        SelectedAdapter = preferred is null ? null : Adapters.FirstOrDefault(r => string.Equals(r.Id, preferred.Id, StringComparison.Ordinal));
    }

    private void SyncActiveNics(IReadOnlyList<InventoryAdapter> inventory)
    {
        var keep = SelectedMonitorNic?.Id ?? PreferredMonitorNicId;
        ActiveNics.Clear();
        foreach (var adapter in inventory) ActiveNics.Add(new AdapterRow(adapter));
        AdapterRow? next = null;
        if (!string.IsNullOrWhiteSpace(keep))
            next = ActiveNics.FirstOrDefault(r => string.Equals(r.Id, keep, StringComparison.Ordinal));
        if (next is null)
        {
            var primary = NicPrimaryAdapter.Pick(inventory, preferredId: null);
            if (primary is not null)
                next = ActiveNics.FirstOrDefault(r => string.Equals(r.Id, primary.Id, StringComparison.Ordinal));
        }
        _syncingNic = true; SelectedMonitorNic = next; _syncingNic = false;
        if (next is not null && string.IsNullOrWhiteSpace(PreferredMonitorNicId)) PreferredMonitorNicId = next.Id;
    }

    public void StartMonitoring()
    {
        if (_monitorCts is not null) return;
        if (ActiveNics.Count == 0)
        {
            try { SyncActiveNics(NetworkHelper.GetAdapters(new NetworkAdapterQuery(IncludeDown: true, IpEnabledOnly: false))); }
            catch (Exception) { }
        }
        _monitorCts = new CancellationTokenSource();
        IsMonitoring = true;
        PaintChart(force: true);
        _ = RunMonitorLoopAsync(_monitorCts.Token);
    }

    public void StopMonitoring()
    {
        try { _monitorCts?.Cancel(); } catch (ObjectDisposedException) { }
        _monitorCts?.Dispose(); _monitorCts = null; IsMonitoring = false;
        _live = null; _pdhInstance = null; _pdhKey = null;
        DropSampleSource();
    }

    public void RestartMonitoring()
    {
        _ring.Clear(); LiveChart = null; ChartStrip = "Waiting for samples."; _lastMonitorNote = null;
        StopMonitoring(); StartMonitoring();
    }

    private SampleTick TakeTick(string name, string description, IReadOnlyList<string> selected, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var key = name + "\u001f" + description;
        if (_live is null || DateTimeOffset.UtcNow - _liveAt > TimeSpan.FromSeconds(30))
        {
            try { _live = NetworkCounterCatalog.LiveInstances(PdhNic.Category); }
            catch (Exception) { _live = []; }
            _liveAt = DateTimeOffset.UtcNow; _pdhInstance = null; _pdhKey = null;
        }
        string? instance;
        if (string.Equals(_pdhKey, key, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(_pdhInstance))
            instance = _pdhInstance;
        else { instance = NicPdhInstance.Resolve(name, description, _live); _pdhKey = key; _pdhInstance = instance; }
        if (string.IsNullOrWhiteSpace(instance)) return new SampleTick(null, null, $"No PDH instance for {name}", false);
        var counters = MonitorCounterList.ForSample(selected);
        if (counters.Count == 0) counters = MonitorCounterList.Sanitize(MonitorCounterList.SeedReceiveSend);
        var paths = NetworkCounterCatalog.Paths(PdhNic.Category, instance, counters).Concat(HostCounters.Preferred).ToList();
        if (paths.Count == 0) return new SampleTick(instance, null, "No counters selected", false);
        var result = RunSample(paths, token);
        return new SampleTick(instance, result, null, false);
    }

    private void ApplySamples(SampleJobResult result)
    {
        if (MonitorPaused) return;
        var latest = result.Samples.GroupBy(s => s.Counter, StringComparer.OrdinalIgnoreCase).Select(g => g.Last()).ToList();
        for (var i = 0; i < latest.Count; i++)
        {
            if (i < Samples.Count) Samples[i].Apply(latest[i]); else Samples.Add(new MonitorSampleRow(latest[i]));
            _ring.Add(latest[i]);
        }
        while (Samples.Count > latest.Count) Samples.RemoveAt(Samples.Count - 1);
        PaintChart(force: false);
    }

    private void PaintChart(bool force)
    {
        if (!force && !MonitorPageVisible) return;
        var (view, strip) = MonitorChart.Paint(ChartPage, _ring);
        if (view is not null) LiveChart = view;
        if (!string.Equals(ChartStrip, strip, StringComparison.Ordinal)) ChartStrip = strip;
    }

    private void MarkStatusChanged(string key, bool changed)
    {
        if (!changed) return;
        for (var i = 0; i < Adapters.Count; i++)
        {
            var row = Adapters[i];
            if (!string.Equals(row.Id, key, StringComparison.OrdinalIgnoreCase) && !string.Equals(row.Name, key, StringComparison.OrdinalIgnoreCase) && !string.Equals(row.Source.Description, key, StringComparison.OrdinalIgnoreCase)) continue;
            row.StatusChanged = true; Adapters.RemoveAt(i); Adapters.Insert(i, row); SelectedAdapter = row; Detail = row.Detail; break;
        }
    }

    private static bool WatchStatusFlipped(AdapterWatchResult result)
        => result.FirstStatus != result.LastStatus || result.Samples.Select(s => s.Status).Distinct().Count() > 1;

    private void Post(string status, string? detail = null)
    {
        Caption = string.IsNullOrWhiteSpace(detail) ? status : $"{status}  {detail}";
        if (StatusBar is not null) StatusBar.Message = Caption;
    }

    private void Note(string status, string? detail = null)
    {
        var text = string.IsNullOrWhiteSpace(detail) ? status : $"{status}  {detail}";
        if (string.Equals(_lastMonitorNote, text, StringComparison.Ordinal)) return;
        _lastMonitorNote = text; Caption = text;
        if (StatusBar is not null) StatusBar.Message = text;
    }

    private static string FormatWatch(AdapterWatchResult result, bool changed)
    {
        var last = result.Samples.Count > 0 ? result.Samples[^1] : null;
        var speed = last is null ? result.LastStatus.ToString() : $"{result.LastStatus}  {LinkSpeed.Format(last.SpeedBitsPerSecond)}";
        return changed ? $"{speed}  {result.FirstStatus} \u2192 {result.LastStatus}" : speed;
    }

    private static string FormatHeader(WorkstationNetwork box)
    {
        var host = string.IsNullOrWhiteSpace(box.HostName) ? "\u2014" : box.HostName.Trim();
        var line = string.IsNullOrWhiteSpace(box.DomainName) ? host : $"{host}  /  {box.DomainName.Trim()}";
        if (box.Stack is { } stack)
        {
            var dns = stack.DhcpNameServers.Count == 0 ? "\u2014" : string.Join(", ", stack.DhcpNameServers);
            line = $"{line}{Environment.NewLine}DHCP DNS  {dns}  router {(stack.IpEnableRouter == true ? "yes" : "no")}";
        }
        return box.DnsSuffixSearchList.Count == 0 ? line : $"{line}{Environment.NewLine}Search  {string.Join(", ", box.DnsSuffixSearchList)}";
    }

    private readonly record struct SampleTick(string? Instance, SampleJobResult? Result, string? Error, bool Primed);
}
