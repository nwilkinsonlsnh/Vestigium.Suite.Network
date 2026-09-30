using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Vestigium.Suite.Network.NicIQ.ViewModels;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class MonitoringView : UserControl
{
    private readonly DispatcherTimer _warmTimer;

    public MonitoringView()
    {
        InitializeComponent();
        _warmTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _warmTimer.Tick += (_, _) =>
        {
            if (DataContext is MainViewModel vm)
                vm.PollWarm();
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.MonitorPageVisible = true;
        _warmTimer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _warmTimer.Stop();
        if (DataContext is MainViewModel vm)
            vm.MonitorPageVisible = false;
    }
}
