using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class ExportsView : UserControl
{
    private bool _loading;

    public ExportsView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel host)
            return;
        _loading = true;
        RoutesBox.IsChecked = host.ExportRoutes;
        NeighborsBox.IsChecked = host.ExportNeighbors;
        ConnectionsBox.IsChecked = host.ExportConnections;
        NetBiosBox.IsChecked = host.ExportNetBios;
        LmHostsBox.IsChecked = host.ExportLmHosts;
        OpenBox.IsChecked = host.ExportOpenAfter;
        _loading = false;
        host.ExportSelectedCommand.NotifyCanExecuteChanged();
        host.ExportAllCommand.NotifyCanExecuteChanged();
    }

    private void OnCheck(object sender, RoutedEventArgs e)
    {
        if (_loading || DataContext is not MainViewModel host)
            return;
        host.RememberExportChecks(
            RoutesBox.IsChecked == true,
            NeighborsBox.IsChecked == true,
            ConnectionsBox.IsChecked == true,
            NetBiosBox.IsChecked == true,
            LmHostsBox.IsChecked == true,
            OpenBox.IsChecked == true);
    }
}
