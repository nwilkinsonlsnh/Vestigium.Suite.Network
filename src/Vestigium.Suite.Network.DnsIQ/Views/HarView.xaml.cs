using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class HarView : UserControl
{
    private ScrollViewer? _hostScroll;
    private ScrollBarVisibility _hostScrollWas;

    public HarView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _hostScroll = FindAncestor<ScrollViewer>(this);
        if (_hostScroll is null)
            return;
        _hostScrollWas = _hostScroll.VerticalScrollBarVisibility;
        _hostScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_hostScroll is null)
            return;
        _hostScroll.VerticalScrollBarVisibility = _hostScrollWas;
        _hostScroll = null;
    }

    private void OnOpenDetails(object sender, MouseButtonEventArgs e)
    {
        if (CaptureGrid.SelectedItem is not CaptureLine line || DataContext is not MainViewModel host)
            return;
        var lines = host.Lines
            .Where(item => string.Equals(item.Host, line.Host, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (lines.Count == 0)
            lines.Add(line);
        CaptureDetailsWindow.Open(Window.GetWindow(this), host, line, lines);
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is T found)
                return found;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
