using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Vestigium.Controls.QueryBar;
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        FitGrid();
        if (Window.GetWindow(this) is Window window)
            window.SizeChanged += (_, _) => FitGrid();
    }

    private void OnLegend(object sender, RoutedEventArgs e)
        => LegendPopup.IsOpen = !LegendPopup.IsOpen;

    private CustomPopupPlacement[] PlaceLegend(Size popupSize, Size targetSize, Point offset)
        => [new CustomPopupPlacement(new Point(targetSize.Width - popupSize.Width, targetSize.Height + 4), PopupPrimaryAxis.Horizontal)];

    private void OnConnectionCommit(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel host)
            host.CommitConnectionQueryCommand.Execute(null);
    }

    private void OnConnectionKey(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainViewModel host)
            return;
        CloseSaved(sender as DependencyObject);
        host.CommitConnectionQueryCommand.Execute(null);
        e.Handled = true;
    }

    private void OnConnectionPin(object sender, VestigiumQueryRowEventArgs e)
    {
        if (DataContext is MainViewModel host && e.Row is RouteIqQueryEntry entry)
            host.ToggleConnectionStickyCommand.Execute(entry);
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
