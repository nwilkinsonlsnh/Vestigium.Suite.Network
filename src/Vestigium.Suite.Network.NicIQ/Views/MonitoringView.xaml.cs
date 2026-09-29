using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.NicIQ.ViewModels;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class MonitoringView : UserControl
{
    public MonitoringView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.MonitorPageVisible = true;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.MonitorPageVisible = false;
    }
}
