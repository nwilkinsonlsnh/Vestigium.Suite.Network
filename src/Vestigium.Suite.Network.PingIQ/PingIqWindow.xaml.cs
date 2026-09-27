using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.PingIQ;

public partial class PingIqWindow : Window
{
    private bool _themeMenuHooked;

    public PingIqWindow(VestigiumDefaultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
        viewModel.RootShell = RootShell;
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        StateChanged += OnWindowStateChanged;
    }

    public VestigiumDefaultWindowViewModel ViewModel { get; }

    public VestigiumShell HostShell => RootShell;

    private bool _sizing;

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        FitNormalSize();
        CenterOnWorkArea();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RootShell = RootShell;
        ThemeChrome.Bind(this);
        BuildThemeMenu();
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (_sizing)
            return;

        if (WindowState == WindowState.Maximized)
        {
            MinWidth = 720;
            MinHeight = 600;
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
            return;
        }

        if (WindowState == WindowState.Normal)
        {
            _sizing = true;
            FitNormalSize();
            CenterOnWorkArea();
            _sizing = false;
        }
    }

    private void FitNormalSize()
    {
        var work = SystemParameters.WorkArea;
        var width = Math.Min(1100, Math.Max(720, work.Width - 48));
        var height = Math.Min(980, Math.Max(600, work.Height - 48));
        MinWidth = 720;
        MinHeight = 600;
        MaxWidth = width;
        MaxHeight = height;
        Width = width;
        Height = height;
    }

    private void CenterOnWorkArea()
    {
        var work = SystemParameters.WorkArea;
        Left = work.Left + Math.Max(0, (work.Width - Width) / 2);
        Top = work.Top + Math.Max(0, (work.Height - Height) / 2);
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
                IsCheckable = false,
                Style = TryFindResource("MenuItem.Standard") as Style
            };
            item.Click += ThemeItem_Click;
            ThemeMenu.Items.Add(item);
        }

        MarkCurrentTheme();

        if (!_themeMenuHooked)
        {
            App.Themes.ThemeChanged += (_, _) => Dispatcher.Invoke(MarkCurrentTheme);
            ThemeMenu.SubmenuOpened += (_, _) => MarkCurrentTheme();
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
            if (raw is not MenuItem item)
                continue;
            var on = string.Equals(item.Tag as string, current, StringComparison.Ordinal);
            item.Icon = on ? CheckGlyph() : null;
        }
    }

    private static TextBlock CheckGlyph()
    {
        return new TextBlock
        {
            Text = "✓",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = TryCheckBrush()
        };
    }

    private static Brush TryCheckBrush()
    {
        if (Application.Current?.TryFindResource("Vestigium.Brushes.Accent.Primary") is Brush accent)
            return accent;
        return Brushes.White;
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
