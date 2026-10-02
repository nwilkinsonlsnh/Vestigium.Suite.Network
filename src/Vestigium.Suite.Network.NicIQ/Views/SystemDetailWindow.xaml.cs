using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.NicIQ.ViewModels;

namespace Vestigium.Suite.Network.NicIQ.Views;

public partial class SystemDetailWindow : Window
{
    private static SystemDetailWindow? _open;

    public SystemDetailWindow()
    {
        InitializeComponent();
    }

    public static void ShowFor(Window? owner)
    {
        var form = SystemDetailForm.Read();
        if (_open is { IsLoaded: true })
        {
            _open.DataContext = form;
            _open.Activate();
            return;
        }

        var window = new SystemDetailWindow { DataContext = form, Owner = owner };
        window.Closed += (_, _) => _open = null;
        if (owner is not null)
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        _open = window;
        window.Show();
    }

    private void TabChecked(object sender, RoutedEventArgs e)
    {
        if (WindowsPanel is null)
            return;
        WindowsPanel.Visibility = Shown(WindowsTab);
        ComputerPanel.Visibility = Shown(ComputerTab);
        MemoryPanel.Visibility = Shown(MemoryTab);
        PageFilePanel.Visibility = Shown(PageFileTab);
        DisplayPanel.Visibility = Shown(DisplayTab);
    }

    private static Visibility Shown(RadioButton tab)
        => tab.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is SystemDetailForm form)
            Clipboard.SetText(form.CopyText);
    }
}
