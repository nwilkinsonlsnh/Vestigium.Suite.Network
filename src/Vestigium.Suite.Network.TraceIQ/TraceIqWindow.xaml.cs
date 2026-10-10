using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.TraceIQ;

public partial class TraceIqWindow : Window
{
    public TraceIqWindow(VestigiumDefaultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.RootShell = RootShell;
        Loaded += (_, _) => ViewModel.RootShell = RootShell;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    private void MainNav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        RootShell.SelectedItem = item;
    }
}
