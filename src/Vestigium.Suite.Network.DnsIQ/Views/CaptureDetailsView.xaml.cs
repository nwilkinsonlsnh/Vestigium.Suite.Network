using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class CaptureDetailsView : UserControl
{
    private readonly DetailsCheck _checks = new();
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
        TitleBlock.Text = Title(line);
        Lines.ItemsSource = _checks.Start(
            line,
            host.Server,
            (int)host.Port,
            host.Bind.InterfaceIndex,
            host.Bind.SourceAddress,
            lines,
            SetTitle);
    }

    public void CancelChecks() => _checks.Cancel();

    private void SetTitle(string check)
    {
        if (_line is null)
            return;
        TitleBlock.Text = Title(_line) + " \u00b7 " + check;
    }

    private static string Title(CaptureLine line)
        => string.IsNullOrWhiteSpace(line.Dns) ? line.Host : line.Host + " \u00b7 " + line.Dns;

    private void OnClose(object sender, RoutedEventArgs e)
    {
        CancelChecks();
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

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
}
