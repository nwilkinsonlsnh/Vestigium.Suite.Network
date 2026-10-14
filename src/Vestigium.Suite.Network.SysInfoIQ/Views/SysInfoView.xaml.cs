using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels;

namespace Vestigium.Suite.Network.SysInfoIQ.Views;

public partial class SysInfoView : UserControl
{
    public SysInfoView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PinToViewport();
        if (DataContext is MainViewModel vm && !vm.IsBusy && vm.SystemRows.Count == 0)
            vm.CollectCommand.Execute(null);
    }

    private void PinToViewport()
    {
        var scroller = FindAncestor<ScrollViewer>(this);
        if (scroller is null)
            return;

        scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        scroller.CanContentScroll = false;
    }

    private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;
        var scroller = FindChild<ScrollViewer>(grid);
        if (scroller is null)
            return;
        scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static T? FindChild<T>(DependencyObject start) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(start);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(start, i);
            if (child is T match)
                return match;
            var nested = FindChild<T>(child);
            if (nested is not null)
                return nested;
        }
        return null;
    }
}
