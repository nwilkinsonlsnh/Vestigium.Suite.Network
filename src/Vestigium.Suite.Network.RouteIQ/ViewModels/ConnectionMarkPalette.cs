using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class ConnectionMarkPalette : ObservableObject
{
    public const string DefaultAddedBackground = "#1F6B45";
    public const string DefaultAddedForeground = "#F4FFF8";
    public const string DefaultDroppedBackground = "#8C2F39";
    public const string DefaultDroppedForeground = "#FFF6F6";
    public const string DefaultReopenedBackground = "#8A6A12";
    public const string DefaultReopenedForeground = "#FFF8E8";

    [ObservableProperty]
    private bool _colorChangedRows = true;

    [ObservableProperty]
    private string _addedBackground = DefaultAddedBackground;

    [ObservableProperty]
    private string _addedForeground = DefaultAddedForeground;

    [ObservableProperty]
    private string _droppedBackground = DefaultDroppedBackground;

    [ObservableProperty]
    private string _droppedForeground = DefaultDroppedForeground;

    [ObservableProperty]
    private string _reopenedBackground = DefaultReopenedBackground;

    [ObservableProperty]
    private string _reopenedForeground = DefaultReopenedForeground;

    public ConnectionMarkPalette()
    {
        AddedBackgroundBrush = Paint(AddedBackground, DefaultAddedBackground);
        AddedForegroundBrush = Paint(AddedForeground, DefaultAddedForeground);
        DroppedBackgroundBrush = Paint(DroppedBackground, DefaultDroppedBackground);
        DroppedForegroundBrush = Paint(DroppedForeground, DefaultDroppedForeground);
        ReopenedBackgroundBrush = Paint(ReopenedBackground, DefaultReopenedBackground);
        ReopenedForegroundBrush = Paint(ReopenedForeground, DefaultReopenedForeground);
    }

    public MediaBrush AddedBackgroundBrush { get; private set; }

    public MediaBrush AddedForegroundBrush { get; private set; }

    public MediaBrush DroppedBackgroundBrush { get; private set; }

    public MediaBrush DroppedForegroundBrush { get; private set; }

    public MediaBrush ReopenedBackgroundBrush { get; private set; }

    public MediaBrush ReopenedForegroundBrush { get; private set; }

    public void Load(ConnectionMarkSettings? data)
    {
        data ??= new ConnectionMarkSettings();
        ColorChangedRows = data.ColorChangedRows;
        AddedBackground = Clean(data.AddedBackground, DefaultAddedBackground);
        AddedForeground = Clean(data.AddedForeground, DefaultAddedForeground);
        DroppedBackground = Clean(data.DroppedBackground, DefaultDroppedBackground);
        DroppedForeground = Clean(data.DroppedForeground, DefaultDroppedForeground);
        ReopenedBackground = Clean(data.ReopenedBackground, DefaultReopenedBackground);
        ReopenedForeground = Clean(data.ReopenedForeground, DefaultReopenedForeground);
    }

    public ConnectionMarkSettings ToSettings() => new()
    {
        ColorChangedRows = ColorChangedRows,
        AddedBackground = AddedBackground,
        AddedForeground = AddedForeground,
        DroppedBackground = DroppedBackground,
        DroppedForeground = DroppedForeground,
        ReopenedBackground = ReopenedBackground,
        ReopenedForeground = ReopenedForeground
    };

    public void ResetColors()
    {
        AddedBackground = DefaultAddedBackground;
        AddedForeground = DefaultAddedForeground;
        DroppedBackground = DefaultDroppedBackground;
        DroppedForeground = DefaultDroppedForeground;
        ReopenedBackground = DefaultReopenedBackground;
        ReopenedForeground = DefaultReopenedForeground;
    }

    partial void OnAddedBackgroundChanged(string value) => AddedBackgroundBrush = Replace(value, DefaultAddedBackground, nameof(AddedBackgroundBrush));

    partial void OnAddedForegroundChanged(string value) => AddedForegroundBrush = Replace(value, DefaultAddedForeground, nameof(AddedForegroundBrush));

    partial void OnDroppedBackgroundChanged(string value) => DroppedBackgroundBrush = Replace(value, DefaultDroppedBackground, nameof(DroppedBackgroundBrush));

    partial void OnDroppedForegroundChanged(string value) => DroppedForegroundBrush = Replace(value, DefaultDroppedForeground, nameof(DroppedForegroundBrush));

    partial void OnReopenedBackgroundChanged(string value) => ReopenedBackgroundBrush = Replace(value, DefaultReopenedBackground, nameof(ReopenedBackgroundBrush));

    partial void OnReopenedForegroundChanged(string value) => ReopenedForegroundBrush = Replace(value, DefaultReopenedForeground, nameof(ReopenedForegroundBrush));

    private MediaBrush Replace(string value, string fallback, string name)
    {
        var brush = Paint(value, fallback);
        OnPropertyChanged(name);
        return brush;
    }

    private static MediaBrush Paint(string value, string fallback)
    {
        var brush = new SolidColorBrush(Parse(value, fallback));
        brush.Freeze();
        return brush;
    }

    private static string Clean(string? value, string fallback) => Parse(value, fallback) == default && !IsHex(value) ? fallback : ToHex(Parse(value, fallback));

    private static bool IsHex(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var text = value.Trim();
        if (text.StartsWith('#'))
            text = text[1..];
        return text.Length is 6 or 8 && text.All(Uri.IsHexDigit);
    }

    private static MediaColor Parse(string? value, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        if (!text.StartsWith('#'))
            text = "#" + text;
        try
        {
            return (MediaColor)ColorConverter.ConvertFromString(text)!;
        }
        catch (FormatException)
        {
            return (MediaColor)ColorConverter.ConvertFromString(fallback)!;
        }
    }

    private static string ToHex(MediaColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
