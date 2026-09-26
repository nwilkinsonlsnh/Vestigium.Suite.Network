using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public const decimal RequestMin = 1;
    public const decimal RequestMax = 10_000;
    public const decimal RequestDefault = 1000m;
    public const decimal SecondsMin = 1;
    public const decimal SecondsMax = 600;
    public const decimal SecondsDefault = 60m;

    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private readonly IReadOnlyList<NetworkAdapter> _adapters;
    private bool _loading;

    public SettingsViewModel(ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _themes = themes;
        _chrome = chrome;
        try
        {
            _adapters = NetworkHelper.GetAdapters();
        }
        catch (Exception)
        {
            _adapters = [];
        }

        Sources = SourceChoices.From(_adapters);
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

    public PingIqSession? Session { get; set; }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<SourceChoice> Sources { get; private set; }

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PingIqPageOpen))]
    [NotifyPropertyChangedFor(nameof(ProbePageOpen))]
    [NotifyPropertyChangedFor(nameof(ThemePageOpen))]
    private string _settingsPage = "PingIQ";

    public bool PingIqPageOpen
    {
        get => SettingsPage == "PingIQ";
        set { if (value) SettingsPage = "PingIQ"; }
    }

    public bool ProbePageOpen
    {
        get => SettingsPage == "Probe";
        set { if (value) SettingsPage = "Probe"; }
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
    private decimal _defaultTimeoutMs = PingIqInput.DefaultTimeoutMs;

    [ObservableProperty]
    private decimal _defaultRequests = RequestDefault;

    [ObservableProperty]
    private decimal _defaultSeconds = SecondsDefault;

    [ObservableProperty]
    private string? _selectedSource;

    public void LoadFrom(PingIqSettings data)
    {
        _loading = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(data.ThemeId))
                SelectedThemeId = data.ThemeId;
            DefaultTimeoutMs = data.TimeoutMs is >= PingIqInput.MinTimeoutMs and <= PingIqInput.MaxTimeoutMs
                ? data.TimeoutMs
                : PingIqInput.DefaultTimeoutMs;
            DefaultRequests = data.Requests;
            DefaultSeconds = data.Seconds;
            BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
                ? VestigiumStatusBarPosition.Top
                : VestigiumStatusBarPosition.Bottom;
            BarVisible = data.StatusBarVisible;
            SelectedSource = SourceChoices.Resolve(Sources, data.Source).Address;
        }
        finally
        {
            _loading = false;
        }
    }

    public void NarrowSources(int interfaceIndex)
    {
        var keep = SelectedSource;
        _loading = true;
        try
        {
            Sources = SourceChoices.From(_adapters, interfaceIndex);
            OnPropertyChanged(nameof(Sources));
            SelectedSource = SourceChoices.Resolve(Sources, keep).Address;
        }
        finally
        {
            _loading = false;
        }
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

    partial void OnDefaultTimeoutMsChanged(decimal value)
    {
        if (_loading)
            return;
        Session?.Save(applyTimeoutSeed: true);
    }

    partial void OnDefaultRequestsChanged(decimal value) => Persist();

    partial void OnDefaultSecondsChanged(decimal value) => Persist();

    partial void OnSelectedSourceChanged(string? value) => Persist();

    private void Persist()
    {
        if (_loading)
            return;
        Session?.Save();
    }
}
