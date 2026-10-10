using System.Windows;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class MonitorDetailsWindow : Window
{
    private static MonitorDetailsWindow? _current;
    private MonitorViewModel? _host;
    private string _name = "";

    public MonitorDetailsWindow()
    {
        InitializeComponent();
    }

    public static void Open(Window? owner, MonitorViewModel host, string name)
    {
        if (_current is null || !_current.IsLoaded)
        {
            _current = new MonitorDetailsWindow { Owner = owner };
            _current.Closed += (_, _) => _current = null;
        }

        _current.ShowHost(host, name);
        if (!_current.IsVisible)
            _current.Show();
        _current.Activate();
    }

    public void ShowHost(MonitorViewModel host, string name)
    {
        _host = host;
        _name = name;
        Title = "DnsIQ — " + name;
        var lines = host.LinesFor(name);
        var sent = lines.GroupBy(l => l.Type).Select(g => g.OrderByDescending(l => l.Time).First()).Sum(l => l.ResolverCount);
        var received = lines.GroupBy(l => l.Type).Select(g => g.OrderByDescending(l => l.Time).First()).Sum(l => l.PacketCount);
        var total = sent + received;
        Caption.Text = $"{name}  —  Requests {total}  Sent {sent}  Received {received}  Total {total}";
        Lines.ItemsSource = lines.OrderBy(l => l.Type).ThenBy(l => l.Time).ToList();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private async void OnLookup(object sender, RoutedEventArgs e)
    {
        if (_host?.Lookup is null || string.IsNullOrWhiteSpace(_name))
            return;
        await _host.Lookup(_name);
    }
}
