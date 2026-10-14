using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Suite.Network.SysInfoIQ.Views;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.SysInfoIQ;

public partial class SysInfoIqWindow : Window
{
    private HelpView? _help;

    public SysInfoIqWindow(VestigiumDefaultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.RootShell = RootShell;
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        BuildThemeMenu();
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

    private void BuildThemeMenu()
    {
        ThemeMenu.Items.Clear();
        var themes = App.Themes.AvailableThemes;
        var current = App.Themes.Current?.Id;
        foreach (var theme in themes)
        {
            var item = new MenuItem
            {
                Header = theme.DisplayName,
                IsCheckable = true,
                IsChecked = string.Equals(theme.Id, current, StringComparison.Ordinal),
                Tag = theme.Id
            };
            item.Click += Theme_Click;
            ThemeMenu.Items.Add(item);
        }

        App.Themes.ThemeChanged += (_, _) =>
        {
            var id = App.Themes.Current?.Id;
            foreach (MenuItem item in ThemeMenu.Items)
                item.IsChecked = string.Equals(item.Tag as string, id, StringComparison.Ordinal);
        };
    }

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string id })
            return;
        if (App.Themes.Current?.Id != id)
            App.Themes.SwitchTheme(id);
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void BarTop_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Status.Position = VestigiumStatusBarPosition.Top;
        ViewModel.DockStatusBarTopCommand.Execute(null);
    }

    private void BarBottom_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Status.Position = VestigiumStatusBarPosition.Bottom;
        ViewModel.DockStatusBarBottomCommand.Execute(null);
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
}
