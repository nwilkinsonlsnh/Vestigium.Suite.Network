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
        Loaded += OnLoaded;
        SizeChanged += (_, _) => FitGrid();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        FitGrid();
        if (Window.GetWindow(this) is Window window)
            window.SizeChanged += (_, _) => FitGrid();
        if (DataContext is MainViewModel host && host.Connections.Count == 0)
            await host.SnapshotConnectionsCommand.ExecuteAsync(null);
    }

    private void OnLegend(object sender, RoutedEventArgs e)
        => LegendPopup.IsOpen = !LegendPopup.IsOpen;

    private void FitGrid()
    {
        var window = Window.GetWindow(this);
        if (window?.Content is not FrameworkElement root || ConnectionGrid.ActualWidth <= 0)
            return;
        foreach (var viewer in FindViewers(this))
            viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        var origin = ConnectionGrid.TranslatePoint(new Point(0, 0), root);
        var height = root.ActualHeight - origin.Y - 12;
        if (height < 240)
            height = 240;
        ConnectionGrid.Height = height;
        ConnectionGrid.MaxHeight = height;
        ConnectionGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
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

    private static IEnumerable<ScrollViewer> FindViewers(DependencyObject start)
    {
        var node = VisualTreeHelper.GetParent(start);
        while (node is not null)
        {
            if (node is ScrollViewer viewer)
                yield return viewer;
            node = VisualTreeHelper.GetParent(node);
        }
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
