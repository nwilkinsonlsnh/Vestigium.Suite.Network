using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;
using Vestigium.Suite.Network.TraceIQ.Views;

namespace Vestigium.Suite.Network.TraceIQ;

public partial class TraceIqWindow : Window
{
    private HelpView? _help;

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
}
