using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

internal static class ChartTheme
{
    public static bool ShowLegend { get; set; } = true;

    public static Action? LegendChanged { get; set; }

    public static ChartOptions Options(string title)
    {
        var color = Hex("Vestigium.Brushes.Accent.Primary")
                    ?? Hex("Vestigium.Brushes.Text.Primary")
                    ?? "#4C6B8A";
        return new ChartOptions
        {
            Title = title,
            Color = color,
            ShowLegend = ShowLegend,
            ShowGrid = true
        };
    }

    public static FrameworkElement Paint(FrameworkElement view)
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
            TrySetLegend(plot, ShowLegend);
            view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
        }

        view.Height = double.NaN;
        view.MinHeight = 140;
        view.VerticalAlignment = VerticalAlignment.Stretch;
        view.HorizontalAlignment = HorizontalAlignment.Stretch;
        view.ContextMenu = BuildMenu(view, plot);

        if (view is Control control)
            control.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");

        return view;
    }

    private static ContextMenu BuildMenu(FrameworkElement view, object? plot)
    {
        var menu = new ContextMenu();
        PaintMenu(menu);

        menu.Items.Add(Item("Save Image", () => SaveImage(view)));
        menu.Items.Add(Item("Copy to Clipboard", () => CopyImage(view)));
        menu.Items.Add(Item("Autoscale", () => AutoScale(view, plot)));
        menu.Items.Add(new Separator());

        var legend = new MenuItem { Header = "Show Legend", IsCheckable = true, IsChecked = ShowLegend };
        PaintItem(legend);
        legend.Click += (_, _) =>
        {
            ShowLegend = legend.IsChecked;
            TrySetLegend(plot, ShowLegend);
            view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
            LegendChanged?.Invoke();
        };
        menu.Items.Add(legend);
        menu.Opened += (_, _) =>
        {
            PaintMenu(menu);
            legend.IsChecked = ShowLegend;
        };
        return menu;
    }

    private static MenuItem Item(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        PaintItem(item);
        item.Click += (_, _) => action();
        return item;
    }

    private static void PaintMenu(ContextMenu menu)
    {
        var card = Brush("Vestigium.Brushes.Surface.Card") ?? Brushes.WhiteSmoke;
        var ink = Brush("Vestigium.Brushes.Text.Primary") ?? Brushes.Black;
        var line = Brush("Vestigium.Brushes.Stroke.Subtle") ?? Brushes.Gray;
        menu.Background = card;
        menu.Foreground = ink;
        menu.BorderBrush = line;
        foreach (var raw in menu.Items)
        {
            if (raw is MenuItem item)
                PaintItem(item, card, ink);
        }
    }

    private static void PaintItem(MenuItem item, Brush? card = null, Brush? ink = null)
    {
        item.Background = card ?? Brush("Vestigium.Brushes.Surface.Card") ?? Brushes.WhiteSmoke;
        item.Foreground = ink ?? Brush("Vestigium.Brushes.Text.Primary") ?? Brushes.Black;
    }

    private static void SaveImage(FrameworkElement view)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PNG image|*.png",
            FileName = "dnsiq-chart.png"
        };
        if (dialog.ShowDialog() != true)
            return;

        var image = Capture(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = File.Create(dialog.FileName);
        encoder.Save(stream);
    }

    private static void CopyImage(FrameworkElement view)
    {
        Clipboard.SetImage(Capture(view));
    }

    private static void AutoScale(FrameworkElement view, object? plot)
    {
        try
        {
            var axes = plot?.GetType().GetProperty("Axes")?.GetValue(plot);
            axes?.GetType().GetMethod("AutoScale", Type.EmptyTypes)?.Invoke(axes, null);
        }
        catch (Exception)
        {
        }

        view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
    }

    private static BitmapSource Capture(FrameworkElement view)
    {
        var width = Math.Max(1, (int)Math.Ceiling(view.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(view.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        return bitmap;
    }

    private static void TrySetLegend(object? plot, bool show)
    {
        if (plot is null)
            return;
        try
        {
            var legend = plot.GetType().GetProperty("Legend")?.GetValue(plot);
            legend?.GetType().GetProperty("IsVisible")?.SetValue(legend, show);
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

    private static Brush? Brush(string key)
        => Application.Current?.TryFindResource(key) as Brush;

    private static string? Hex(string resourceKey)
    {
        if (Application.Current?.TryFindResource(resourceKey) is not SolidColorBrush brush)
            return null;
        var c = brush.Color;
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
