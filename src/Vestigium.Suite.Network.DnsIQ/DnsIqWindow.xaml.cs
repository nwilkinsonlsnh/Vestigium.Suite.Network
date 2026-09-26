using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class DnsIqWindow : Window
{
    public DnsIqWindow(VestigiumDefaultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.RootShell = RootShell;
        Loaded += OnLoaded;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RootShell = RootShell;
        ThemeChrome.Bind(this);
    }

    private void MainNav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        RootShell.SelectedItem = item;
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}
