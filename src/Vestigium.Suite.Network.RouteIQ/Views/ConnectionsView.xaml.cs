using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class ConnectionsView : UserControl
{
    public ConnectionsView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel host && host.Connections.Count == 0)
                await host.SnapshotConnectionsCommand.ExecuteAsync(null);
        };
    }

    private void OnWheel(object sender, MouseWheelEventArgs e)
    {
        if (FindScroll(ConnectionGrid) is not ScrollViewer viewer)
            return;
        var steps = Math.Max(1, Math.Abs(e.Delta) / 120);
        for (var i = 0; i < steps; i++)
        {
            if (e.Delta > 0)
                viewer.LineUp();
            else
                viewer.LineDown();
        }

        e.Handled = true;
    }

    private static ScrollViewer? FindScroll(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is ScrollViewer viewer)
                return viewer;
            var nested = FindScroll(child);
            if (nested is not null)
                return nested;
        }

        return null;
    }
}
