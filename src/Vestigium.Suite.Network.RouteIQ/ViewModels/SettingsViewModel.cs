using System.Windows;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Suite.Network.RouteIQ.Views;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

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
        Marks.PropertyChanged += (_, _) => Persist();
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

    public RouteIqSession? Session { get; set; }

    public ConnectionMarkPalette Marks { get; } = new();

    public System.ComponentModel.ICollectionView RouteQueries { get; private set; } = new CollectionViewSource { Source = Array.Empty<RouteIqQueryEntry>() }.View;

    public System.ComponentModel.ICollectionView NeighborQueries { get; private set; } = new CollectionViewSource { Source = Array.Empty<RouteIqQueryEntry>() }.View;

    public System.ComponentModel.ICollectionView ConnectionQueries { get; private set; } = new CollectionViewSource { Source = Array.Empty<RouteIqQueryEntry>() }.View;

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;

    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    [ObservableProperty]
    private string? _selectedThemeId;

    [ObservableProperty]
    private VestigiumStatusBarPosition _barPosition;

    [ObservableProperty]
    private bool _barVisible;

    [ObservableProperty]
    private int _ouiPoolSize = 10;

    [ObservableProperty]
    private int _queryMruLimit = 10;

    [ObservableProperty]
    private bool _closeMruOnApply = true;

    [ObservableProperty]
    private bool _liveVendorLookup;

    public void BeginLoad() => _loading = true;

    public void EndLoad() => _loading = false;

    public void BindQueries()
    {
        var source = Session?.Queries ?? [];
        RouteQueries = Filter(source, RouteIqSession.RouteTab);
        NeighborQueries = Filter(source, RouteIqSession.NeighborTab);
        ConnectionQueries = Filter(source, RouteIqSession.ConnectionTab);
        OnPropertyChanged(nameof(RouteQueries));
        OnPropertyChanged(nameof(NeighborQueries));
        OnPropertyChanged(nameof(ConnectionQueries));
    }

    public void LoadFrom(RouteIqSettings data)
    {
        SelectedThemeId = string.IsNullOrWhiteSpace(data.ThemeId) ? SelectedThemeId : data.ThemeId;
        BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
            ? VestigiumStatusBarPosition.Top
            : VestigiumStatusBarPosition.Bottom;
        BarVisible = data.StatusBarVisible;
        OuiPoolSize = data.OuiPoolSize is >= 1 and <= 20 ? data.OuiPoolSize : 10;
        QueryMruLimit = data.QueryMruLimit is >= 0 and <= 30 ? data.QueryMruLimit : 10;
        CloseMruOnApply = data.CloseMruOnApply;
        LiveVendorLookup = data.LiveVendorLookup;
        Marks.Load(data.ConnectionMarks);
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

    partial void OnOuiPoolSizeChanged(int value) => Persist();

    partial void OnLiveVendorLookupChanged(bool value) => Persist();

    partial void OnQueryMruLimitChanged(int value)
    {
        if (_loading)
            return;
        Session?.SetQueryMruLimit(value);
    }

    partial void OnCloseMruOnApplyChanged(bool value)
    {
        if (_loading || Session is null)
            return;
        Session.Current.CloseMruOnApply = value;
        Session.Save();
    }

    [RelayCommand]
    private void ToggleSticky(RouteIqQueryEntry? entry)
    {
        if (entry is null)
            return;
        Session?.SetSticky(entry, !entry.Sticky);
    }

    [RelayCommand]
    private void RemoveQuery(RouteIqQueryEntry? entry)
    {
        if (entry is null)
            return;
        Session?.Remove(entry);
    }

    [RelayCommand]
    private void PickMarkColor(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
            return;

        var dialog = new ConnectionColorWindow(CurrentHex(target))
        {
            Owner = Application.Current?.MainWindow
        };
        if (dialog.ShowDialog() != true)
            return;

        var hex = dialog.SelectedHex;
        switch (target)
        {
            case "AddedBackground": Marks.AddedBackground = hex; break;
            case "AddedForeground": Marks.AddedForeground = hex; break;
            case "DroppedBackground": Marks.DroppedBackground = hex; break;
            case "DroppedForeground": Marks.DroppedForeground = hex; break;
            case "ReopenedBackground": Marks.ReopenedBackground = hex; break;
            case "ReopenedForeground": Marks.ReopenedForeground = hex; break;
        }
    }

    [RelayCommand]
    private void ResetMarkColors() => Marks.ResetColors();

    private void Persist()
    {
        if (_loading)
            return;
        Session?.Save();
    }

    private string CurrentHex(string target) => target switch
    {
        "AddedBackground" => Marks.AddedBackground,
        "AddedForeground" => Marks.AddedForeground,
        "DroppedBackground" => Marks.DroppedBackground,
        "DroppedForeground" => Marks.DroppedForeground,
        "ReopenedBackground" => Marks.ReopenedBackground,
        "ReopenedForeground" => Marks.ReopenedForeground,
        _ => "#000000"
    };

    private static System.ComponentModel.ICollectionView Filter(System.Collections.IEnumerable source, string tab)
    {
        var view = new CollectionViewSource { Source = source }.View;
        view.Filter = item => item is RouteIqQueryEntry entry && entry.Tab == tab;
        return view;
    }
}
