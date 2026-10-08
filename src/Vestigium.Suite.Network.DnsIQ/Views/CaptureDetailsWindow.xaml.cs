using System.Windows;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class CaptureDetailsWindow : Window
{
    private static CaptureDetailsWindow? _current;

    public CaptureDetailsWindow()
    {
        InitializeComponent();
    }

    public static void Open(Window? owner, MainViewModel host, CaptureLine line, IReadOnlyList<CaptureLine> lines)
    {
        if (_current is null || !_current.IsLoaded)
        {
            _current = new CaptureDetailsWindow { Owner = owner };
            _current.Closed += (_, _) => _current = null;
        }

        _current.ShowHost(host, line, lines);
        if (!_current.IsVisible)
            _current.Show();
        _current.Activate();
    }

    public void ShowHost(MainViewModel host, CaptureLine line, IReadOnlyList<CaptureLine> lines)
    {
        Title = "DnsIQ \u2014 " + line.Host;
        Details.Show(host, line, lines);
    }

    private void OnCloseRequested(object sender, EventArgs e) => Close();
}
