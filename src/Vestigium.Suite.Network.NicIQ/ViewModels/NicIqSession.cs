using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class NicIqSession
{
    private readonly NicIqSettingsStore _store;
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private MainViewModel? _main;
    private SettingsViewModel? _settings;
    private bool _ready;

    public NicIqSession(
        NicIqSettingsStore store,
        ThemeManager themes,
        VestigiumDefaultWindowViewModel chrome)
    {
        _store = store;
        _themes = themes;
        _chrome = chrome;
        Current = store.Load();
        MonitorRing.ApplyArchive(Current.ArchiveSeconds);
        ApplyThemeAndBar();
    }

    public NicIqSettings Current { get; private set; }

    public void Attach(MainViewModel main, SettingsViewModel settings)
    {
        _main = main;
        _settings = settings;
        main.BeginLoad();
        settings.BeginLoad();
        ApplyToViews();
        settings.EndLoad();
        main.EndLoad();
        _ready = true;
    }

    public void Save()
    {
        if (!_ready || _main is null || _settings is null)
            return;

        Current = new NicIqSettings
        {
            ThemeId = _settings.SelectedThemeId,
            DurationSeconds = (int)ClampDuration(_main.DurationSeconds),
            IncludeDown = _main.ShowDown,
            IpEnabledOnly = _main.ShowIpv4 || _main.ShowIpv6,
            ShowUp = _main.ShowUp,
            ShowDown = _main.ShowDown,
            IpEnabled = _main.ShowIpv4 || _main.ShowIpv6,
            ShowIpv4 = _main.ShowIpv4,
            ShowIpv6 = _main.ShowIpv6,
            MonitorReceive = _main.MonitorReceive,
            MonitorSend = _main.MonitorSend,
            MonitorErrors = _main.MonitorErrors,
            MonitorDiscards = _main.MonitorDiscards,
            MonitorCounters = [.. _settings.MonitorCounters],
            ShowLegend = _settings.ShowLegend,
            ArchiveSeconds = MonitorRing.ClampArchive((int)_settings.ArchiveMinutes * 60),
            SelectedAdapterId = _main.SelectedAdapter?.Id ?? _main.PreferredAdapterId,
            SelectedMonitorNicId = _main.SelectedMonitorNic?.Id ?? _main.PreferredMonitorNicId,
            StatusBarVisible = _settings.BarVisible,
            StatusBarDock = _settings.BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom"
        };
        _store.Save(Current);
    }

    private void ApplyThemeAndBar()
    {
        if (!string.IsNullOrWhiteSpace(Current.ThemeId))
            _themes.SwitchTheme(Current.ThemeId);

        _chrome.ShowStatusBar = Current.StatusBarVisible;
        if (string.Equals(Current.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase))
            _chrome.DockStatusBarTopCommand.Execute(null);
        else
            _chrome.DockStatusBarBottomCommand.Execute(null);
    }

    private void ApplyToViews()
    {
        if (_main is null || _settings is null)
            return;

        var duration = ClampDuration(Current.DurationSeconds);
        var ip = Current.IpEnabled ?? Current.IpEnabledOnly;
        _main.DurationSeconds = duration;
        _main.ShowUp = Current.ShowUp ?? true;
        _main.ShowDown = Current.ShowDown ?? Current.IncludeDown;
        _main.ShowIpv4 = Current.ShowIpv4 ?? ip;
        _main.ShowIpv6 = Current.ShowIpv6 ?? ip;
        _main.MonitorReceive = Current.MonitorReceive;
        _main.MonitorSend = Current.MonitorSend;
        _main.MonitorErrors = Current.MonitorErrors;
        _main.MonitorDiscards = Current.MonitorDiscards;
        _main.PreferredAdapterId = Current.SelectedAdapterId;
        _main.PreferredMonitorNicId = Current.SelectedMonitorNicId ?? Current.SelectedAdapterId;
        ChartTheme.WatchLegend = Current.ShowLegend;
        _main.ApplyMonitorArchive(Current.ArchiveSeconds);
        _settings.LoadFrom(Current);
    }

    public static decimal ClampDuration(decimal seconds)
    {
        if (seconds < NicIqWatchInput.MinDurationSeconds)
            return NicIqWatchInput.MinDurationSeconds;
        if (seconds > NicIqWatchInput.MaxDurationSeconds)
            return NicIqWatchInput.MaxDurationSeconds;
        return decimal.Truncate(seconds);
    }
}
