using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class HarView : UserControl
{
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
        Details.Show(host, line, lines);
        GridHost.Visibility = Visibility.Collapsed;
        Details.Visibility = Visibility.Visible;
        e.Handled = true;
    }

    private void OnCloseDetails(object sender, EventArgs e)
    {
        Details.Visibility = Visibility.Collapsed;
        GridHost.Visibility = Visibility.Visible;
    }
}
