using System.Windows.Controls;
using System.Windows.Input;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class DashboardView : UserControl
{
    private readonly PageViewport _viewport;

    public DashboardView()
    {
        InitializeComponent();
        _viewport = new PageViewport(this);
    }

    private void Probe_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var viewer = FindScrollViewer(this);
        if (viewer is null || viewer.ScrollableHeight <= 0)
            return;

        viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
        e.Handled = true;
    }

    private static ScrollViewer? FindScrollViewer(System.Windows.DependencyObject root)
    {
        if (root is ScrollViewer viewer)
            return viewer;

        var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            var found = FindScrollViewer(child);
            if (found is not null)
                return found;
        }

        return null;
    }
}
