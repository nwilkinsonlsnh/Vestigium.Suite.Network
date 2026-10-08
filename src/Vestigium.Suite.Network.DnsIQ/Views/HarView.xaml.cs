using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class HarView : UserControl
{
    private CaptureDetailsWindow? _details;

    public HarView()
    {
        InitializeComponent();
    }

    private void OnOpenDetails(object sender, MouseButtonEventArgs e)
    {
        if (CaptureGrid.SelectedItem is not CaptureLine line || DataContext is not MainViewModel host)
            return;
        var lines = host.Lines
            .Where(item => string.Equals(item.Host, line.Host, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (lines.Count == 0)
            lines.Add(line);

        if (_details is null || !_details.IsLoaded)
        {
            _details = new CaptureDetailsWindow { Owner = Window.GetWindow(this) };
            _details.Closed += (_, _) => _details = null;
        }

        _details.ShowHost(host, line, lines);
        if (!_details.IsVisible)
            _details.Show();
        _details.Activate();
        e.Handled = true;
    }
}
