using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Vestigium.Controls.Shell;
using Vestigium.Suite.Network.RouteIQ.Views;

namespace Vestigium.Suite.Network.RouteIQ;

public partial class MainWindow : Window
{
    public MainWindow(VestigiumDefaultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.RootShell = RootShell;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    public void ShowSplash(string status, double percent)
    {
        Splash.Visibility = Visibility.Visible;
        SplashStatus.Text = status;
        SplashBar.Value = percent;
    }

    public void HideSplash()
        => Splash.Visibility = Visibility.Collapsed;

    public async Task Warm(string status, double percent, VestigiumNavItem? item)
    {
        ShowSplash(status, percent);
        if (item is not null)
            RootShell.SelectedItem = item;
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
    }

    private void MainNav_Checked(object sender, RoutedEventArgs e)
    {
        if (Splash.Visibility == Visibility.Visible)
            return;
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        CloseHelp();
        RootShell.SelectedItem = item;
    }

    private void MainNav_Click(object sender, RoutedEventArgs e)
    {
        if (Splash.Visibility == Visibility.Visible)
            return;
        if (HelpTab.IsChecked != true)
            return;
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        CloseHelp();
        RootShell.SelectedItem = item;
    }

    private HelpView? _help;

    private void HelpTab_Checked(object sender, RoutedEventArgs e)
    {
        if (Splash.Visibility == Visibility.Visible)
            return;
        foreach (var item in RootShell.NavItems.OfType<VestigiumNavItem>())
            item.IsSelected = false;
        _help ??= new HelpView();
        RootShell.Content = _help;
    }

    private void HelpTab_Unchecked(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(RootShell.Content, _help))
            RootShell.Content = null;
    }

    private void CloseHelp()
    {
        if (HelpTab.IsChecked == true)
            HelpTab.IsChecked = false;
    }
}
