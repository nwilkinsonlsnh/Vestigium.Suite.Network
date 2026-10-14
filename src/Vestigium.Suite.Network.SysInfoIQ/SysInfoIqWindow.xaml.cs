using System.Windows;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.SysInfoIQ;

public partial class SysInfoIqWindow : Window
{
    public SysInfoIqWindow(VestigiumDefaultWindowViewModel viewModel)
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
}
