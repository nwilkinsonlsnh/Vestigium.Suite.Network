using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

internal enum ChartSlot
{
    Lookup,
    ProbeRtt,
    ProbeDist,
    ProbeControl
}

internal static class ChartTheme
{
    private static readonly SolidColorBrush MenuBack = Brushes.White;
    private static readonly SolidColorBrush MenuInk = Brushes.Black;

    public static bool LookupLegend { get; set; } = true;
    public static bool ProbeRttLegend { get; set; } = true;
    public static bool ProbeDistLegend { get; set; } = true;
    public static bool ProbeControlLegend { get; set; } = true;

    public static Action? LegendChanged { get; set; }

    public static bool GetLegend(ChartSlot slot) => slot switch
    {
        ChartSlot.ProbeRtt => ProbeRttLegend,
        ChartSlot.ProbeDist => ProbeDistLegend,
        ChartSlot.ProbeControl => ProbeControlLegend,
        _ => LookupLegend
    };

    public static void SetLegend(ChartSlot slot, bool value)
    {
        switch (slot)
        {
            case ChartSlot.ProbeRtt: ProbeRttLegend = value; break;
            case ChartSlot.ProbeDist: ProbeDistLegend = value; break;
            case ChartSlot.ProbeControl: ProbeControlLegend = value; break;
            default: LookupLegend = value; break;
        }
    }

    public static void LoadFrom(DnsIqSettings data)
    {
        LookupLegend = data.ShowLegendLookup;
        ProbeRttLegend = data.ShowLegendProbeRtt;
        ProbeDistLegend = data.ShowLegendProbeDist;
        ProbeControlLegend = data.ShowLegendProbeControl;
    }

    public static void CopyTo(DnsIqSettings data)
    {
        data.ShowLegendLookup = LookupLegend;
        data.ShowLegendProbeRtt = ProbeRttLegend;
        data.ShowLegendProbeDist = ProbeDistLegend;
        data.ShowLegendProbeControl = ProbeControlLegend;
    }

    public static ChartOptions Options(ChartSlot slot, string title, string? xLabel = null, string? yLabel = null)
    {
        var color = Hex("Vestigium.Brushes.Accent.Primary")
                    ?? Hex("Vestigium.Brushes.Text.Primary")
                    ?? "#4C6B8A";
        return new ChartOptions
        {
            Title = title,
            XLabel = xLabel,
            YLabel = yLabel,
            Color = color,
            ShowLegend = GetLegend(slot),
            ShowGrid = true
        };
    }

    public static FrameworkElement Paint(FrameworkElement view, ChartSlot slot)
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
            TrySetLegend(plot, GetLegend(slot));
            view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
        }

        view.Height = double.NaN;
        view.MinHeight = 140;
        view.VerticalAlignment = VerticalAlignment.Stretch;
        view.HorizontalAlignment = HorizontalAlignment.Stretch;
        DisableStockMenu(view);
        AttachMenu(view, plot, slot);

        if (view is Control control)
            control.SetResourceReference(Control.BackgroundProperty, "Vestigium.Brushes.Surface.Card");

        return view;
    }

    private static void DisableStockMenu(FrameworkElement view)
    {
        TrySetProp(view, "MenuOnRightClick", false);
        TrySetProp(view, "EnableContextMenu", false);
    }

    private static void AttachMenu(FrameworkElement view, object? plot, ChartSlot slot)
    {
        void Assign()
        {
            DisableStockMenu(view);
            view.ContextMenu = BuildMenu(view, plot, slot);
        }

        Assign();
        view.Loaded -= OnChartLoaded;
        view.Loaded += OnChartLoaded;
        view.PreviewMouseRightButtonUp -= OnRightClick;
        view.PreviewMouseRightButtonUp += OnRightClick;

        void OnChartLoaded(object sender, RoutedEventArgs e) => Assign();

        void OnRightClick(object sender, MouseButtonEventArgs e)
        {
            Assign();
            if (view.ContextMenu is null)
                return;
            view.ContextMenu.PlacementTarget = view;
            view.ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }

    private static ContextMenu BuildMenu(FrameworkElement view, object? plot, ChartSlot slot)
    {
        var menu = new ContextMenu();
        PaintMenu(menu);
        menu.Items.Add(Item("Save Image", () => SaveImage(view)));
        menu.Items.Add(Item("Copy to Clipboard", () => CopyImage(view)));
        menu.Items.Add(Item("Auto Scale", () => AutoScale(view, plot)));
        menu.Items.Add(Item("Open in New Window", () => OpenWindow(view)));
        menu.Items.Add(new Separator());

        var legend = new MenuItem { Header = "Show Legend", IsCheckable = true, IsChecked = GetLegend(slot) };
        PaintItem(legend);
        legend.Click += (_, _) =>
        {
            SetLegend(slot, legend.IsChecked);
            TrySetLegend(plot, GetLegend(slot));
            view.GetType().GetMethod("Refresh", Type.EmptyTypes)?.Invoke(view, null);
            LegendChanged?.Invoke();
        };
        menu.Items.Add(legend);
        menu.Opened += (_, _) =>
        {
            PaintMenu(menu);
            legend.IsChecked = GetLegend(slot);
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
        menu.Background = MenuBack;
        menu.Foreground = MenuInk;
        menu.BorderBrush = Brushes.Silver;
        foreach (var raw in menu.Items)
        {
            if (raw is MenuItem item)
                PaintItem(item);
        }
    }

    private static void PaintItem(MenuItem item)
    {
        item.Background = MenuBack;
        item.Foreground = MenuInk;
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

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(Capture(view)));
        using var stream = File.Create(dialog.FileName);
        encoder.Save(stream);
    }

    private static void CopyImage(FrameworkElement view) => Clipboard.SetImage(Capture(view));

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

    private static void OpenWindow(FrameworkElement view)
    {
        try
        {
            var open = view.GetType().GetMethod("OpenInNewWindow", Type.EmptyTypes);
            if (open is not null)
            {
                open.Invoke(view, null);
                return;
            }
        }
        catch (Exception)
        {
        }

        new Window
        {
            Title = "DnsIQ Chart",
            Width = Math.Max(900, view.ActualWidth + 80),
            Height = Math.Max(560, view.ActualHeight + 80),
            Background = Brushes.White,
            Content = new Image { Source = Capture(view), Stretch = Stretch.Uniform, Margin = new Thickness(8) }
        }.Show();
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

    private static void TrySetProp(object target, string name, object value)
    {
        try
        {
            target.GetType().GetProperty(name)?.SetValue(target, value);
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
        var fromHex = colorProp.PropertyType.GetMethod("FromHex", BindingFlags.Public | BindingFlags.Static, [typeof(string)]);
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
