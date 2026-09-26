using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public const decimal RequestMin = PulsePlan.MinRequests;
    public const decimal RequestMax = PulsePlan.MaxRequests;
    public const decimal RequestDefault = 1000m;
    public const decimal SecondsMin = PulsePlan.MinSeconds;
    public const decimal SecondsMax = PulsePlan.MaxSeconds;
    public const decimal SecondsDefault = 60m;

    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private bool _loading;

    public SettingsViewModel(ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _themes = themes;
        _chrome = chrome;
        _selectedThemeId = themes.Current?.Id ?? themes.AvailableThemes.FirstOrDefault()?.Id;
        _barPosition = chrome.Status.Position;
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

    public DnsIqSession? Session { get; set; }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThemePageOpen))]
    [NotifyPropertyChangedFor(nameof(ProbePageOpen))]
    private string _settingsPage = "Theme";

    public bool ThemePageOpen
    {
        get => SettingsPage == "Theme";
        set { if (value) SettingsPage = "Theme"; }
    }

    public bool ProbePageOpen
    {
        get => SettingsPage == "Probe";
        set { if (value) SettingsPage = "Probe"; }
    }

    [ObservableProperty]
    private string? _selectedThemeId;

    [ObservableProperty]
    private VestigiumStatusBarPosition _barPosition;

    [ObservableProperty]
    private decimal _defaultRequests = RequestDefault;

    [ObservableProperty]
    private decimal _defaultSeconds = SecondsDefault;

    [ObservableProperty]
    private decimal _defaultPort = DnsIqInput.DefaultPort;

    public void LoadFrom(DnsIqSettings data)
    {
        _loading = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(data.ThemeId))
                SelectedThemeId = data.ThemeId;
            DefaultRequests = data.Requests;
            DefaultSeconds = data.Seconds;
            DefaultPort = data.Port is >= DnsIqInput.MinPort and <= DnsIqInput.MaxPort
                ? data.Port
                : DnsIqInput.DefaultPort;
            BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
                ? VestigiumStatusBarPosition.Top
                : VestigiumStatusBarPosition.Bottom;
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

    partial void OnDefaultRequestsChanged(decimal value) => Persist();

    partial void OnDefaultSecondsChanged(decimal value) => Persist();

    partial void OnDefaultPortChanged(decimal value) => Persist();

    private void Persist()
    {
        if (_loading)
            return;
        Session?.Save();
    }
}
