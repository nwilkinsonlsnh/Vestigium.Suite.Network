using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public const decimal PulseMin = 1m;
    public const decimal PulseMax = 60m;
    public const decimal PulseDefault = 10m;

    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;

    public SettingsViewModel(ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _themes = themes;
        _chrome = chrome;
        _selectedTheme = themes.Current ?? themes.AvailableThemes.FirstOrDefault();
        _barPosition = chrome.Status.Position;
    }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    [ObservableProperty]
    private ThemeDefinition? _selectedTheme;

    [ObservableProperty]
    private VestigiumStatusBarPosition _barPosition;

    [ObservableProperty]
    private decimal _defaultBursts = PulseDefault;

    [ObservableProperty]
    private decimal _defaultSeconds = PulseDefault;

    public int BurstCount => Clamp((int)DefaultBursts);

    public int DurationSeconds => Clamp((int)DefaultSeconds);

    partial void OnSelectedThemeChanged(ThemeDefinition? value)
    {
        if (value is null)
            return;
        if (_themes.Current?.Id == value.Id)
            return;
        _themes.SwitchTheme(value.Id);
    }

    partial void OnBarPositionChanged(VestigiumStatusBarPosition value)
    {
        _chrome.Status.Position = value;
        if (value == VestigiumStatusBarPosition.Bottom)
            _chrome.DockStatusBarBottomCommand.Execute(null);
        else
            _chrome.DockStatusBarTopCommand.Execute(null);
    }

    partial void OnDefaultBurstsChanged(decimal value)
        => DefaultBursts = ClampDecimal(value);

    partial void OnDefaultSecondsChanged(decimal value)
        => DefaultSeconds = ClampDecimal(value);

    private static int Clamp(int value)
        => Math.Clamp(value, (int)PulseMin, (int)PulseMax);

    private static decimal ClampDecimal(decimal value)
    {
        if (value < PulseMin) return PulseMin;
        if (value > PulseMax) return PulseMax;
        return decimal.Truncate(value);
    }
}
