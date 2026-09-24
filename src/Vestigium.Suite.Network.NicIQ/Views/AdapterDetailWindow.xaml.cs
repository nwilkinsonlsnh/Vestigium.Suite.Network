using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.NicIQ.ViewModels;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class AdapterDetailWindow : Window
{
    private static AdapterDetailWindow? _open;
    private MainViewModel? _viewModel;
    private bool _followMonitor;

    public AdapterDetailWindow()
    {
        InitializeComponent();
    }

    public static void ShowFor(Window? owner, MainViewModel viewModel, bool followMonitor = false)
    {
        if (viewModel.SelectedAdapter is null && viewModel.SelectedMonitorNic is null)
            return;

        if (_open is { IsLoaded: true })
        {
            _open._followMonitor = followMonitor;
            _open.Bind(viewModel);
            _open.Activate();
            return;
        }

        var window = new AdapterDetailWindow { Owner = owner, _followMonitor = followMonitor };
        window.Bind(viewModel);
        window.Closed += (_, _) => _open = null;
        if (owner is not null)
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _open = window;
        window.Show();
    }

    private void Bind(MainViewModel viewModel)
    {
        if (!ReferenceEquals(_viewModel, viewModel))
        {
            if (_viewModel is not null)
                _viewModel.PropertyChanged -= OnViewModelChanged;
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnViewModelChanged;
        }

        Refresh();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedMonitorNic))
            _followMonitor = true;
        else if (e.PropertyName == nameof(MainViewModel.SelectedAdapter))
            _followMonitor = false;
        else
            return;

        Refresh();
    }

    private void Refresh()
    {
        var row = _followMonitor
            ? _viewModel?.SelectedMonitorNic ?? _viewModel?.SelectedAdapter
            : _viewModel?.SelectedAdapter ?? _viewModel?.SelectedMonitorNic;
        if (row is null)
            return;
        DataContext = AdapterDetailForm.From(row);
        if (WifiTab.Visibility != Visibility.Visible && WifiTab.IsChecked == true)
            IdentityTab.IsChecked = true;
        ShowTab();
    }

    private void TabChecked(object sender, RoutedEventArgs e)
        => ShowTab();

    private void ShowTab()
    {
        if (IdentityPanel is null)
            return;
        IdentityPanel.Visibility = Shown(IdentityTab);
        LinkPanel.Visibility = Shown(LinkTab);
        WifiPanel.Visibility = Shown(WifiTab);
        AddressesPanel.Visibility = Shown(AddressesTab);
        DriverPanel.Visibility = Shown(DriverTab);
    }

    private static Visibility Shown(RadioButton tab)
        => tab.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdapterDetailForm form)
            Clipboard.SetText(form.CopyText);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelChanged;
        base.OnClosed(e);
    }
}
