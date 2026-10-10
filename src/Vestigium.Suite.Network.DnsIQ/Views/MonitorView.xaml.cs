using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class MonitorView
{
    private ScrollViewer? _host;
    private ScrollBarVisibility _hostScroll;

    public MonitorView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _host = FindScrollViewer(this);
        if (_host is null)
            return;
        _hostScroll = _host.VerticalScrollBarVisibility;
        _host.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_host is null)
            return;
        _host.VerticalScrollBarVisibility = _hostScroll;
        _host = null;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject start)
    {
        var current = VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is ScrollViewer viewer)
                return viewer;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
