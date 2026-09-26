using System.Windows;
using System.Windows.Controls;
using Vestigium.Controls.Shell;

namespace Vestigium.Suite.Network.DnsIQ;

internal static class HostMenu
{
    public static void StripDemoCommands(VestigiumDefaultWindow window)
    {
        if (window.Content is not Panel root)
            return;

        foreach (var menu in FindMenus(root))
            Strip(menu);
    }

    private static IEnumerable<Menu> FindMenus(DependencyObject root)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node is Menu menu)
                yield return menu;

            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(node);
            if (count == 0 && node is Panel panel)
            {
                foreach (var child in panel.Children.OfType<DependencyObject>())
                    queue.Enqueue(child);
                continue;
            }

            for (var i = 0; i < count; i++)
                queue.Enqueue(System.Windows.Media.VisualTreeHelper.GetChild(node, i));
        }
    }

    private static void Strip(Menu menu)
    {
        foreach (var top in menu.Items.OfType<MenuItem>())
        {
            for (var i = top.Items.Count - 1; i >= 0; i--)
            {
                if (top.Items[i] is not MenuItem item)
                    continue;
                var header = item.Header?.ToString() ?? string.Empty;
                if (IsDemo(header))
                    top.Items.RemoveAt(i);
            }

            TrimSeparators(top);
        }
    }

    private static bool IsDemo(string header)
    {
        return header.Contains("PingIQ", StringComparison.OrdinalIgnoreCase)
            || header.Contains("Reset Home", StringComparison.OrdinalIgnoreCase)
            || header.Contains("inner form", StringComparison.OrdinalIgnoreCase);
    }

    private static void TrimSeparators(MenuItem top)
    {
        while (top.Items.Count > 0 && top.Items[^1] is Separator)
            top.Items.RemoveAt(top.Items.Count - 1);
        while (top.Items.Count > 0 && top.Items[0] is Separator)
            top.Items.RemoveAt(0);
    }
}
