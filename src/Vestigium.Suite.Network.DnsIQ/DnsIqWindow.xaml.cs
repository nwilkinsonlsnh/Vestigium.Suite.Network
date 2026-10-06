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
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var work = SystemParameters.WorkArea;
        Left = work.Left + Math.Max(0, (work.Width - Width) / 2);
        Top = work.Top + Math.Max(0, (work.Height - Height) / 2);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RootShell = RootShell;
    }

    private void MainNav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        RootShell.SelectedItem = item;
    }
}
