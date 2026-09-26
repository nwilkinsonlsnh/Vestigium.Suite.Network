using System.Windows;
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
        Loaded += (_, _) => viewModel.RootShell = RootShell;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}
