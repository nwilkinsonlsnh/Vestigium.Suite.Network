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
            IncludeDown = _main.IncludeDown,
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
        _main.DurationSeconds = duration;
        _main.IncludeDown = Current.IncludeDown;
        _settings.LoadFrom(Current);
    }

    internal static decimal ClampDuration(decimal seconds)
    {
        if (seconds < NicIqWatchInput.MinDurationSeconds)
            return NicIqWatchInput.MinDurationSeconds;
        if (seconds > NicIqWatchInput.MaxDurationSeconds)
            return NicIqWatchInput.MaxDurationSeconds;
        return decimal.Truncate(seconds);
    }
}
