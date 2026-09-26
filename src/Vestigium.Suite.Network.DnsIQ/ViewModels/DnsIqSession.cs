using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed class DnsIqSession
{
    private readonly DnsIqSettingsStore _store;
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private MainViewModel? _main;
    private SettingsViewModel? _settings;
    private bool _ready;

    public DnsIqSession(
        DnsIqSettingsStore store,
        ThemeManager themes,
        VestigiumDefaultWindowViewModel chrome)
    {
        _store = store;
        _themes = themes;
        _chrome = chrome;
        Current = store.Load();
        ApplyThemeAndBar();
    }

    public DnsIqSettings Current { get; private set; }

    public void Attach(MainViewModel main, SettingsViewModel settings)
    {
        _main = main;
        _settings = settings;
        main.BeginLoad();
        ApplyToViews();
        main.EndLoad();
        _ready = true;
    }

    public void Save()
    {
        if (!_ready || _main is null || _settings is null)
            return;

        _main.RequestCount = _settings.DefaultRequests;
        _main.DurationSeconds = _settings.DefaultSeconds;
        _main.Port = _settings.DefaultPort;
        _main.Bind.SourceAddress = _settings.SelectedSource;
        _settings.NarrowSources(_main.SelectedInterfaceIndex);

        Current = new DnsIqSettings
        {
            ThemeId = _settings.SelectedThemeId,
            Server = _main.Server,
            Type = _main.RecordType,
            InterfaceIndex = _main.SelectedInterfaceIndex,
            Port = (int)_settings.DefaultPort,
            Requests = (int)_settings.DefaultRequests,
            Seconds = (int)_settings.DefaultSeconds,
            StatusBarVisible = _settings.BarVisible,
            StatusBarDock = _settings.BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom",
            Source = _settings.SelectedSource
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

        if (!string.IsNullOrWhiteSpace(Current.Server))
            _main.Server = Current.Server;
        if (!string.IsNullOrWhiteSpace(Current.Type))
            _main.RecordType = Current.Type;
        _main.Port = Current.Port is >= DnsIqInput.MinPort and <= DnsIqInput.MaxPort
            ? Current.Port
            : DnsIqInput.DefaultPort;
        _main.SelectedInterfaceIndex = _main.Interfaces.Any(i => i.Index == Current.InterfaceIndex)
            ? Current.InterfaceIndex
            : 0;
        _main.RequestCount = Current.Requests;
        _main.DurationSeconds = Current.Seconds;
        _settings.NarrowSources(_main.SelectedInterfaceIndex);
        _settings.LoadFrom(Current);
        _main.Bind.SourceAddress = _settings.SelectedSource;
    }
}
