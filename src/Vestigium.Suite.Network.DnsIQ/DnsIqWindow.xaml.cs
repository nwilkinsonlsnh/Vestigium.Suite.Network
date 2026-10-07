using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Vestigium.Controls.Shell;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Vestigium.Suite.Network.DnsIQ.Views;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class DnsIqWindow : Window
{
    private HelpView? _help;

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
        CloseHelp();
        RootShell.SelectedItem = item;
    }

    private void MainNav_Click(object sender, RoutedEventArgs e)
    {
        if (HelpTab.IsChecked != true)
            return;
        if (sender is not RadioButton { DataContext: VestigiumNavItem item })
            return;
        if (!item.IsEnabled)
            return;
        CloseHelp();
        RootShell.SelectedItem = item;
    }

    private void HelpTab_Checked(object sender, RoutedEventArgs e)
    {
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

    private void OpenCapture_Click(object sender, RoutedEventArgs e)
    {
        if (RootShell["DnsIQ"]?.Content is not DnsIqView { DataContext: MainViewModel dns })
            return;

        var dialog = new OpenFileDialog
        {
            Filter = "Capture or text (*.har;*.txt)|*.har;*.txt|HAR (*.har)|*.har|Text (*.txt)|*.txt",
            Title = "Open capture"
        };
        if (dialog.ShowDialog(this) != true)
            return;
        dns.LoadCapture(dialog.FileName);
    }
}
