using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Vestigium.Controls.StatusBar;

namespace Vestigium.Suite.Network.DnsIQ;

internal static class ThemeChrome
{
    public static void Bind(DependencyObject root)
    {
        foreach (var bar in Find<VestigiumStatusBar>(root))
            BindStatusBar(bar);
    }

    private static void BindStatusBar(VestigiumStatusBar bar)
    {
        bar.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");
        bar.SetResourceReference(Control.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        if (bar.FindName("RootBorder") is Border named)
        {
            BindBorder(named);
            return;
        }

        if (VisualTreeHelper.GetChildrenCount(bar) == 0)
        {
            bar.Loaded += OnBarLoaded;
            return;
        }

        BindBorders(bar);
    }

    private static void OnBarLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not VestigiumStatusBar bar)
            return;
        bar.Loaded -= OnBarLoaded;
        BindBorders(bar);
    }

    private static void BindBorders(DependencyObject root)
    {
        foreach (var border in Find<Border>(root))
            BindBorder(border);
    }

    private static void BindBorder(Border border)
    {
        border.SetResourceReference(Border.BackgroundProperty, "Vestigium.Brushes.Surface.Card");
        border.SetResourceReference(Border.BorderBrushProperty, "Vestigium.Brushes.Stroke.Subtle");
    }

    private static IEnumerable<T> Find<T>(DependencyObject root) where T : DependencyObject
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node is T match && !ReferenceEquals(match, root))
                yield return match;

            var count = VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
                queue.Enqueue(VisualTreeHelper.GetChild(node, i));
        }
    }
}
