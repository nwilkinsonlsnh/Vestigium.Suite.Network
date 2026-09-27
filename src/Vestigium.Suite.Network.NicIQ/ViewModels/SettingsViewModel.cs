using CommunityToolkit.Mvvm.ComponentModel;
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
    }

    public NicIqSession? Session { get; set; }

    public MainViewModel? Host { get; set; }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NicIqPageOpen))]
    [NotifyPropertyChangedFor(nameof(ThemePageOpen))]
    private string _settingsPage = "NicIQ";

    public bool NicIqPageOpen
    {
        get => SettingsPage == "NicIQ";
        set { if (value) SettingsPage = "NicIQ"; }
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

    public void BeginLoad() => _loading = true;

    public void EndLoad() => _loading = false;

    public void LoadFrom(NicIqSettings data)
    {
        SelectedThemeId = string.IsNullOrWhiteSpace(data.ThemeId) ? SelectedThemeId : data.ThemeId;
        DefaultDurationSeconds = NicIqSession.ClampDuration(data.DurationSeconds);
        IncludeDown = data.IncludeDown;
        IpEnabledOnly = data.IpEnabledOnly;
        MonitorReceive = data.MonitorReceive;
        MonitorSend = data.MonitorSend;
        MonitorErrors = data.MonitorErrors;
        MonitorDiscards = data.MonitorDiscards;
        BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
            ? VestigiumStatusBarPosition.Top
            : VestigiumStatusBarPosition.Bottom;
        BarVisible = data.StatusBarVisible;
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

    partial void OnIncludeDownChanged(bool value)
    {
        if (_loading)
            return;
        if (Host is not null)
            Host.IncludeDown = value;
        Persist();
    }

    partial void OnIpEnabledOnlyChanged(bool value)
    {
        if (_loading)
            return;
        if (Host is not null)
            Host.IpEnabledOnly = value;
        Persist();
    }

    partial void OnMonitorReceiveChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorReceive = v; });
    partial void OnMonitorSendChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorSend = v; });
    partial void OnMonitorErrorsChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorErrors = v; });
    partial void OnMonitorDiscardsChanged(bool value) => PushMonitor(value, v => { if (Host is not null) Host.MonitorDiscards = v; });

    private void PushMonitor(bool value, Action<bool> apply)
    {
        if (_loading)
            return;
        apply(value);
        Persist();
    }

    private void Persist()
    {
        if (_loading)
            return;
        Session?.Save();
    }
}
