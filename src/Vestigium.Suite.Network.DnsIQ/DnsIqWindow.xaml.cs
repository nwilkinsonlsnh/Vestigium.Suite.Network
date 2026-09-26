using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class DnsIqWindow : Window
{
    private bool _themeMenuHooked;
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
        ThemeChrome.Bind(this);
        BuildThemeMenu();
    }

    private void BuildThemeMenu()
    {
        ThemeMenu.Items.Clear();
        var settings = App.Settings;
        if (settings is null)
            return;

        foreach (var theme in settings.Themes)
        {
            var item = new MenuItem
            {
                Header = theme.DisplayName,
                Tag = theme.Id,
                IsCheckable = true,
                IsChecked = string.Equals(theme.Id, settings.SelectedThemeId, StringComparison.Ordinal),
                Style = TryFindResource("MenuItem.Standard") as Style
            };
            item.Click += ThemeItem_Click;
            ThemeMenu.Items.Add(item);
        }

        if (!_themeMenuHooked)
        {
            App.Themes.ThemeChanged += (_, _) => Dispatcher.Invoke(MarkCurrentTheme);
            _themeMenuHooked = true;
        }
    }

    private void ThemeItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string id } || App.Settings is null)
            return;
        App.Settings.SelectedThemeId = id;
        MarkCurrentTheme();
    }

    private void MarkCurrentTheme()
    {
        var current = App.Settings?.SelectedThemeId ?? App.Themes.Current?.Id;
        foreach (var raw in ThemeMenu.Items)
        {
            if (raw is MenuItem item)
                item.IsChecked = string.Equals(item.Tag as string, current, StringComparison.Ordinal);
        }
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
