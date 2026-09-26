using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

internal static class ChartTheme
{
    public static ChartOptions Options(string title, bool showLegend = true)
    {
        var color = Hex("Vestigium.Brushes.Accent.Primary")
                    ?? Hex("Vestigium.Brushes.Text.Primary")
                    ?? "#4C6B8A";
        return new ChartOptions
        {
            Title = title,
            Color = color,
            ShowLegend = showLegend,
            ShowGrid = true
        };
    }

    public static FrameworkElement Paint(FrameworkElement view, bool showLegend = true)
    {
        var figure = Hex("Vestigium.Brushes.Surface.Window") ?? "#1B1B1B";
        var data = Hex("Vestigium.Brushes.Surface.Card") ?? "#242424";
        var ink = Hex("Vestigium.Brushes.Text.Primary") ?? "#E6E6E6";
        var grid = Hex("Vestigium.Brushes.Stroke.Subtle") ?? "#3A3A3A";

        var plot = view.GetType().GetProperty("Plot")?.GetValue(view);
        if (plot is not null)
        {
            SetSlotColor(plot, "FigureBackground", figure);
            SetSlotColor(plot, "DataBackground", data);
            TrySetAxes(plot, ink, grid);
            TrySetLegend(plot, showLegend);
            view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
        }

        view.Height = double.NaN;
        view.MinHeight = 140;
        view.VerticalAlignment = VerticalAlignment.Stretch;
        view.HorizontalAlignment = HorizontalAlignment.Stretch;
        ThemeContextMenu(view);

        if (view is Control control)
            control.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");

        return view;
    }

    private static void ThemeContextMenu(FrameworkElement view)
    {
        view.ContextMenuOpening -= OnContextMenuOpening;
        view.ContextMenuOpening += OnContextMenuOpening;
        if (view.ContextMenu is not null)
            PaintMenu(view.ContextMenu);
    }

    private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is FrameworkElement { ContextMenu: { } menu })
            PaintMenu(menu);
    }

    private static void PaintMenu(ContextMenu menu)
    {
        menu.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");
        menu.SetResourceReference(Control.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        menu.SetResourceReference(Control.BorderBrushProperty, "Vestigium.Brushes.Stroke.Subtle");
        menu.SetResourceReference(TextElement.ForegroundProperty, "Vestigium.Brushes.Text.Primary");

        foreach (var raw in menu.Items)
        {
            if (raw is not MenuItem item)
                continue;
            item.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");
            item.SetResourceReference(Control.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
            item.SetResourceReference(TextElement.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        }
    }

    private static void TrySetLegend(object plot, bool show)
    {
        try
        {
            var legend = plot.GetType().GetProperty("Legend")?.GetValue(plot);
            if (legend is null)
                return;
            var visible = legend.GetType().GetProperty("IsVisible");
            visible?.SetValue(legend, show);
        }
        catch (Exception)
        {
        }
    }

    private static void TrySetAxes(object plot, string ink, string grid)
    {
        try
        {
            var axes = plot.GetType().GetProperty("Axes")?.GetValue(plot);
            axes?.GetType().GetMethod("Color", new[] { typeof(string) })?.Invoke(axes, [ink]);
        }
        catch (Exception)
        {
        }

        try
        {
            var grids = plot.GetType().GetProperty("Grid")?.GetValue(plot);
            SetSlotColor(grids ?? plot, "MajorLineColor", grid);
        }
        catch (Exception)
        {
        }
    }

    private static void SetSlotColor(object target, string slotName, string hex)
    {
        var slot = target.GetType().GetProperty(slotName)?.GetValue(target);
        if (slot is null)
            return;

        var colorProp = slot.GetType().GetProperty("Color");
        if (colorProp is null)
            return;

        var colorType = colorProp.PropertyType;
        var fromHex = colorType.GetMethod("FromHex", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
        var color = fromHex?.Invoke(null, [hex]);
        if (color is not null)
            colorProp.SetValue(slot, color);
    }

    private static string? Hex(string resourceKey)
    {
        if (Application.Current?.TryFindResource(resourceKey) is not SolidColorBrush brush)
            return null;
        var c = brush.Color;
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
