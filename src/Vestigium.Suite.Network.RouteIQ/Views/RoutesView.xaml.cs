using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Vestigium.Controls.QueryBar;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class RoutesView : UserControl
{
    public RoutesView()
    {
        InitializeComponent();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => FitIpv4();

    private void OnRouteCommit(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel host)
            host.CommitRouteQueryCommand.Execute(null);
    }

    private void OnRouteKey(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainViewModel host)
            return;
        CloseSaved(sender as DependencyObject);
        host.CommitRouteQueryCommand.Execute(null);
        e.Handled = true;
    }

    private void OnRoutePin(object sender, VestigiumQueryRowEventArgs e)
    {
        if (DataContext is MainViewModel host && e.Row is RouteIqQueryEntry entry)
            host.ToggleRouteStickyCommand.Execute(entry);
    }

    private static void CloseSaved(DependencyObject? root)
    {
        if (root is null)
            return;
        if (FindChevron(root) is ToggleButton chevron)
            chevron.IsChecked = false;
    }

    private static ToggleButton? FindChevron(DependencyObject root)
    {
        if (root is ToggleButton { Name: "PART_Chevron" } named)
            return named;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var found = FindChevron(VisualTreeHelper.GetChild(root, i));
            if (found is not null)
                return found;
        }

        return null;
    }

    private void FitIpv4()
    {
        var window = Window.GetWindow(this);
        var budget = window?.ActualHeight ?? ActualHeight;
        var used = Toolbar.ActualHeight + Ipv4Caption.ActualHeight + Ipv6Caption.ActualHeight + 276 + 210;
        var left = budget - used;
        Ipv4Host.Height = left > 160 ? left : 160;
        Ipv4Host.MaxHeight = Ipv4Host.Height;
    }
}
