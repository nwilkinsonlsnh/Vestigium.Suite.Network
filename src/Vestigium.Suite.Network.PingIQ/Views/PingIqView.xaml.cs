using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Vestigium.Suite.Network.PingIQ.Views;

public partial class PingIqView : UserControl
{
    private readonly PageViewport _viewport;

    public PingIqView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }

    private void ReplyGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;

        var viewer = FindScrollViewer(grid);
        if (viewer is null)
            return;

        viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private static ScrollViewer? FindScrollViewer(System.Windows.DependencyObject root)
    {
        if (root is ScrollViewer viewer)
            return viewer;

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var found = FindScrollViewer(child);
            if (found is not null)
                return found;
        }

        return null;
    }
}
