using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class CaptureDetailsView : UserControl
{
    private CaptureLine? _line;
    private MainViewModel? _host;

    public CaptureDetailsView()
    {
        InitializeComponent();
    }

    public event EventHandler? CloseRequested;

    public void Show(MainViewModel host, CaptureLine line, IReadOnlyList<CaptureLine> lines)
    {
        _host = host;
        _line = line;
        TitleBlock.Text = string.IsNullOrWhiteSpace(line.Dns) ? line.Host : line.Host + " \u00b7 " + line.Dns;
        Lines.ItemsSource = lines.Select(item => new DetailRow(item.Category, item.Answer)).ToList();
    }

    private void OnClose(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void OnLookup(object sender, RoutedEventArgs e)
    {
        if (_host is null || _line is null)
            return;
        _host.LookupLineCommand.Execute(_line);
    }

    private void OnLookupAndProbe(object sender, RoutedEventArgs e)
    {
        if (_host is null || _line is null)
            return;
        _host.LookupLineAndProbeCommand.Execute(_line);
    }

    private sealed record DetailRow(string Category, string Answer)
    {
        public string Check => "";
        public string Question => "";
    }
}
