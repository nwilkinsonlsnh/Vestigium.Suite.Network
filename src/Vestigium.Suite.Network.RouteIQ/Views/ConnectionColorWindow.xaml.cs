using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class ConnectionColorWindow : Window
{
    private bool _writing;

    public ConnectionColorWindow(string hex)
    {
        InitializeComponent();
        if (!TryParse(hex, out var color))
            color = Colors.Black;
        _writing = true;
        Red.Value = color.R;
        Green.Value = color.G;
        Blue.Value = color.B;
        _writing = false;
        Paint();
    }

    public string SelectedHex { get; private set; } = "#000000";

    private void OnChannel(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_writing || Red is null || Green is null || Blue is null)
            return;
        Paint();
    }

    private void OnHex(object sender, RoutedEventArgs e) => ApplyHex();

    private void OnHexKey(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        ApplyHex();
        e.Handled = true;
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        ApplyHex();
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ApplyHex()
    {
        if (!TryParse(Hex.Text, out var color))
        {
            Paint();
            return;
        }

        _writing = true;
        Red.Value = color.R;
        Green.Value = color.G;
        Blue.Value = color.B;
        _writing = false;
        Paint();
    }

    private void Paint()
    {
        var color = Color.FromRgb((byte)Red.Value, (byte)Green.Value, (byte)Blue.Value);
        SelectedHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        Preview.Background = new SolidColorBrush(color);
        RedText.Text = color.R.ToString();
        GreenText.Text = color.G.ToString();
        BlueText.Text = color.B.ToString();
        if (!Hex.IsKeyboardFocused)
            Hex.Text = SelectedHex;
    }

    private static bool TryParse(string? text, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;
        var hex = text.Trim();
        if (!hex.StartsWith('#'))
            hex = "#" + hex;
        try
        {
            color = (Color)ColorConverter.ConvertFromString(hex)!;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
