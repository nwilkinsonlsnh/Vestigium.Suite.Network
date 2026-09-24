using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vestigium.Suite.Network.DnsIQ.Views;

internal sealed class PageViewport
{
    private readonly FrameworkElement _page;
    private ScrollViewer? _host;

    public PageViewport(FrameworkElement page)
    {
        _page = page;
        page.Loaded += OnLoaded;
        page.Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _host = FindAncestorScrollViewer(_page);
        if (_host is null)
            return;

        _host.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        _host.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        _host.SizeChanged += OnHostSizeChanged;
        Fit();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_host is not null)
        {
            _host.SizeChanged -= OnHostSizeChanged;
            _host.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        _host = null;
        _page.ClearValue(FrameworkElement.HeightProperty);
    }

    private void OnHostSizeChanged(object sender, SizeChangedEventArgs e) => Fit();

    private void Fit()
    {
        if (_host is null)
            return;

        var height = _host.ViewportHeight;
        if (height <= 0)
            height = _host.ActualHeight;
        if (height > 0)
            _page.Height = height;
    }

    private static ScrollViewer? FindAncestorScrollViewer(DependencyObject start)
    {
        for (var current = start; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is ScrollViewer viewer)
                return viewer;
        }

        return null;
    }
}
