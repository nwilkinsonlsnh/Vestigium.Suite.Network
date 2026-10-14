using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed class SysInfoSession
{
    private readonly SysInfoSettingsStore _store;
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private SettingsViewModel? _settings;
    private bool _ready;

    public SysInfoSession(
        SysInfoSettingsStore store,
        ThemeManager themes,
        VestigiumDefaultWindowViewModel chrome)
    {
        _store = store;
        _themes = themes;
        _chrome = chrome;
        Current = store.Load();
        ApplyThemeAndBar();
    }

    public SysInfoSettings Current { get; private set; }

    public void Attach(SettingsViewModel settings)
    {
        _settings = settings;
        settings.LoadFrom(Current);
        _ready = true;
    }

    public void Save()
    {
        if (!_ready || _settings is null)
            return;

        Current = new SysInfoSettings
        {
            ThemeId = _settings.SelectedThemeId,
            StatusBarVisible = _settings.BarVisible,
            StatusBarDock = _settings.BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom",
            OutputFolder = _settings.OutputFolder
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
}
