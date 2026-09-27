using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed class PingIqSession
{
    private readonly PingIqSettingsStore _store;
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private MainViewModel? _main;
    private SettingsViewModel? _settings;
    private bool _ready;

    public PingIqSession(
        PingIqSettingsStore store,
        ThemeManager themes,
        VestigiumDefaultWindowViewModel chrome)
    {
        _store = store;
        _themes = themes;
        _chrome = chrome;
        Current = store.Load();
        ApplyThemeAndBar();
    }

    public PingIqSettings Current { get; private set; }

    public void Attach(MainViewModel main, SettingsViewModel settings)
    {
        _main = main;
        _settings = settings;
        main.BeginLoad();
        ApplyToViews();
        main.EndLoad();
        _ready = true;
    }

    public void Save(bool applyTimeoutSeed = false)
    {
        if (!_ready || _main is null || _settings is null)
            return;

        _main.Count = _settings.DefaultCount;
        _main.TimeoutMs = _settings.DefaultTimeoutMs;
        _main.SelectedInterfaceIndex = _settings.SelectedInterfaceIndex;
        _main.RequestCount = _settings.DefaultRequests;
        _main.DurationSeconds = _settings.DefaultSeconds;
        _main.Bind.SourceAddress = _settings.SelectedSource;
        _main.Bind.InterfaceIndex = _settings.SelectedInterfaceIndex;
        _settings.NarrowSources(_settings.SelectedInterfaceIndex);

        Current = new PingIqSettings
        {
            ThemeId = _settings.SelectedThemeId,
            Count = (int)_main.Count,
            TimeoutMs = (int)_main.TimeoutMs,
            InterfaceIndex = _main.SelectedInterfaceIndex,
            Requests = (int)_settings.DefaultRequests,
            Seconds = (int)_settings.DefaultSeconds,
            StatusBarVisible = _settings.BarVisible,
            StatusBarDock = _settings.BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom",
            Source = _settings.SelectedSource,
            TargetHistorySize = (int)_settings.TargetHistorySize,
            RecentTargets = _main.Targets.ToArray(),
            ResolveOnce = _settings.ResolveOnce,
            ShowLegendEcho = Current.ShowLegendEcho,
            ShowLegendProbeRtt = Current.ShowLegendProbeRtt,
            ShowLegendProbeDist = Current.ShowLegendProbeDist,
            ShowLegendProbeControl = Current.ShowLegendProbeControl
        };
        ChartTheme.CopyTo(Current);
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

        _main.Count = Current.Count is >= PingIqInput.MinCount and <= PingIqInput.MaxCount
            ? Current.Count
            : PingIqInput.DefaultCount;
        _main.TimeoutMs = ResolveDelay(Current.TimeoutMs);
        _main.SelectedInterfaceIndex = _main.Interfaces.Any(i => i.Index == Current.InterfaceIndex)
            ? Current.InterfaceIndex
            : 0;
        _main.RequestCount = Current.Requests;
        _main.DurationSeconds = Current.Seconds;
        _settings.NarrowSources(_main.SelectedInterfaceIndex);
        _settings.LoadFrom(Current);
        _main.ReplaceTargets(Current.RecentTargets, Current.TargetHistorySize);
        _main.Bind.SourceAddress = _settings.SelectedSource;
        _main.Bind.InterfaceIndex = _main.SelectedInterfaceIndex;
    }

    private static decimal ResolveDelay(int stored)
    {
        // 4000 was the old reply-timeout default written into Delay.
        if (stored == 4000)
            return PingIqInput.DefaultDelayMs;
        return stored is >= PingIqInput.MinDelayMs and <= PingIqInput.MaxDelayMs
            ? stored
            : PingIqInput.DefaultDelayMs;
    }
}
