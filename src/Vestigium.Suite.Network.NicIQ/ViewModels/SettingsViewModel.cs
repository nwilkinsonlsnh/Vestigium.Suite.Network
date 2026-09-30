using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private bool _loading;

    public SettingsViewModel(ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _themes = themes;
        _chrome = chrome;
        _selectedThemeId = themes.Current?.Id ?? themes.AvailableThemes.FirstOrDefault()?.Id;
        _barPosition = chrome.Status.Position;
        _barVisible = chrome.ShowStatusBar;
        themes.ThemeChanged += (_, _) =>
        {
            var id = themes.Current?.Id;
            if (_selectedThemeId != id)
            {
                _selectedThemeId = id;
                OnPropertyChanged(nameof(SelectedThemeId));
            }
        };
        RefreshCounterLists(MonitorCounterList.FromSettings(new NicIqSettings()));
    }

    public NicIqSession? Session { get; set; }

    public MainViewModel? Host { get; set; }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    public ObservableCollection<string> AvailableCounters { get; } = [];

    public ObservableCollection<string> MonitorCounters { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NicIqPageOpen))]
    [NotifyPropertyChangedFor(nameof(MonitoringPageOpen))]
    [NotifyPropertyChangedFor(nameof(ThemePageOpen))]
    private string _settingsPage = "NicIQ";

    public bool NicIqPageOpen
    {
        get => SettingsPage == "NicIQ";
        set { if (value) SettingsPage = "NicIQ"; }
    }

    public bool MonitoringPageOpen
    {
        get => SettingsPage == "Monitoring";
        set { if (value) SettingsPage = "Monitoring"; }
    }

    public bool ThemePageOpen
    {
        get => SettingsPage == "Theme";
        set { if (value) SettingsPage = "Theme"; }
    }

    [ObservableProperty]
    private string? _selectedThemeId;

    [ObservableProperty]
    private VestigiumStatusBarPosition _barPosition;

    [ObservableProperty]
    private bool _barVisible;

    [ObservableProperty]
    private decimal _defaultDurationSeconds = NicIqWatchInput.DefaultDurationSeconds;

    [ObservableProperty]
    private bool _showUp = true;

    [ObservableProperty]
    private bool _showDown = true;

    [ObservableProperty]
    private bool _showIpv4 = true;

    [ObservableProperty]
    private bool _showIpv6 = true;

    [ObservableProperty]
    private bool _monitorReceive = true;

    [ObservableProperty]
    private bool _monitorSend = true;

    [ObservableProperty]
    private bool _monitorErrors;

    [ObservableProperty]
    private bool _monitorDiscards;

    [ObservableProperty]
    private bool _showLegend = true;

    [ObservableProperty]
    private string? _selectedAvailableCounter;

    [ObservableProperty]
    private string? _selectedMonitorCounter;

    public void BeginLoad() => _loading = true;

    public void EndLoad() => _loading = false;

    public void LoadFrom(NicIqSettings data)
    {
        SelectedThemeId = string.IsNullOrWhiteSpace(data.ThemeId) ? SelectedThemeId : data.ThemeId;
        DefaultDurationSeconds = NicIqSession.ClampDuration(data.DurationSeconds);
        ShowUp = data.ShowUp ?? true;
        ShowDown = data.ShowDown ?? data.IncludeDown;
        var ip = data.IpEnabled ?? data.IpEnabledOnly;
        ShowIpv4 = data.ShowIpv4 ?? ip;
        ShowIpv6 = data.ShowIpv6 ?? ip;
        MonitorReceive = data.MonitorReceive;
        MonitorSend = data.MonitorSend;
        MonitorErrors = data.MonitorErrors;
        MonitorDiscards = data.MonitorDiscards;
        ShowLegend = data.ShowLegend;
        ChartTheme.WatchLegend = data.ShowLegend;
        Host?.RefreshLegendButton();
        RefreshCounterLists(MonitorCounterList.FromSettings(data));
        BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
            ? VestigiumStatusBarPosition.Top
            : VestigiumStatusBarPosition.Bottom;
        BarVisible = data.StatusBarVisible;
    }

    [RelayCommand]
    private void AddCounter()
    {
        var name = SelectedAvailableCounter;
        if (string.IsNullOrWhiteSpace(name))
            return;
        if (MonitorCounters.Contains(name, StringComparer.OrdinalIgnoreCase))
            return;
        MonitorCounters.Add(name);
        RefreshCounterLists(MonitorCounters);
        PersistCounters();
    }

    [RelayCommand]
    private void RemoveCounter()
    {
        var name = SelectedMonitorCounter;
        if (string.IsNullOrWhiteSpace(name))
            return;
        var match = MonitorCounters.FirstOrDefault(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            return;
        MonitorCounters.Remove(match);
        RefreshCounterLists(MonitorCounters);
        PersistCounters();
    }

    partial void OnSelectedThemeIdChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        if (_themes.Current?.Id != value)
            _themes.SwitchTheme(value);
        Persist();
    }

    partial void OnBarPositionChanged(VestigiumStatusBarPosition value)
    {
        _chrome.Status.Position = value;
        if (value == VestigiumStatusBarPosition.Bottom)
            _chrome.DockStatusBarBottomCommand.Execute(null);
        else
            _chrome.DockStatusBarTopCommand.Execute(null);
        Persist();
    }

    partial void OnBarVisibleChanged(bool value)
    {
        _chrome.ShowStatusBar = value;
        Persist();
    }

    partial void OnShowLegendChanged(bool value)
    {
        if (_loading)
            return;
        ChartTheme.WatchLegend = value;
        Host?.ApplyLegend(value);
        Host?.RefreshLegendButton();
        Persist();
    }

    partial void OnDefaultDurationSecondsChanged(decimal value)
    {
        if (_loading)
            return;
        if (Host is not null)
        {
            Host.BeginLoad();
            Host.DurationSeconds = NicIqSession.ClampDuration(value);
            Host.EndLoad();
        }

        Persist();
    }

    partial void OnShowUpChanged(bool value) => PushFilter(host => host.ShowUp = value);
    partial void OnShowDownChanged(bool value) => PushFilter(host => host.ShowDown = value);
    partial void OnShowIpv4Changed(bool value) => PushFilter(host => host.ShowIpv4 = value);
    partial void OnShowIpv6Changed(bool value) => PushFilter(host => host.ShowIpv6 = value);

    partial void OnMonitorReceiveChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorReceive = v; });
    partial void OnMonitorSendChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorSend = v; });
    partial void OnMonitorErrorsChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorErrors = v; });
    partial void OnMonitorDiscardsChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorDiscards = v; });

    private void PushFilter(Action<MainViewModel> apply)
    {
        if (_loading)
            return;
        if (Host is not null)
            apply(Host);
        Persist();
    }

    private void PushMonitor(bool value, Action<bool> apply)
    {
        if (_loading)
            return;
        apply(value);
        Persist();
    }

    private void PersistCounters()
    {
        if (_loading)
            return;
        Session?.Save();
        Host?.RestartMonitoring();
    }

    private void Persist()
    {
        if (_loading)
            return;
        Session?.Save();
    }

    private void RefreshCounterLists(IEnumerable<string> selected)
    {
        var picked = MonitorCounterList.Sanitize(selected);
        MonitorCounters.Clear();
        foreach (var name in picked)
            MonitorCounters.Add(name);

        AvailableCounters.Clear();
        foreach (var name in MonitorCounterList.Available(picked))
            AvailableCounters.Add(name);

        if (SelectedAvailableCounter is not null
            && !AvailableCounters.Contains(SelectedAvailableCounter, StringComparer.OrdinalIgnoreCase))
            SelectedAvailableCounter = AvailableCounters.FirstOrDefault();

        if (SelectedMonitorCounter is not null
            && !MonitorCounters.Contains(SelectedMonitorCounter, StringComparer.OrdinalIgnoreCase))
            SelectedMonitorCounter = MonitorCounters.FirstOrDefault();
    }
}
