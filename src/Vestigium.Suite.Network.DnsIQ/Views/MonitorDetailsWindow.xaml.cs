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
        var lines = host.LinesFor(name).OrderBy(l => l.Type).ThenBy(l => l.Time).ToList();
        var events = ToEvents(lines);
        var sent = events.Sum(e => e.Sent);
        var received = events.Sum(e => e.Received);
        Caption.Text = $"{name}  —  Requests {sent + received}  Sent {sent}  Received {received}  Total {sent + received}";
        Lines.ItemsSource = events;
    }

    private static List<MonitorEvent> ToEvents(List<MonitorLine> lines)
    {
        var prior = new Dictionary<string, (int Sent, int Received)>(StringComparer.Ordinal);
        var events = new List<MonitorEvent>();
        foreach (var line in lines)
        {
            prior.TryGetValue(line.Type, out var last);
            var sentDelta = Math.Max(0, line.ResolverCount - last.Sent);
            var receivedDelta = Math.Max(0, line.PacketCount - last.Received);
            prior[line.Type] = (line.ResolverCount, line.PacketCount);

            var isReceive = receivedDelta > 0 || !string.IsNullOrWhiteSpace(line.Status) || !string.IsNullOrWhiteSpace(line.Answers);
            var sent = isReceive ? 0 : (sentDelta > 0 ? 1 : 1);
            var received = isReceive ? 1 : 0;
            events.Add(new MonitorEvent
            {
                Type = line.Type,
                TimeText = line.Time.ToLocalTime().ToString("M/d/yyyy h:mm:ss.fff tt"),
                Pid = line.Pid,
                Sent = sent,
                Received = received,
                Total = sent + received,
                Status = line.Status,
                Answers = string.IsNullOrWhiteSpace(line.Answers)
                    ? ""
                    : string.Join(Environment.NewLine, line.Answers.Split("; ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            });
        }

        return events;
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private async void OnLookup(object sender, RoutedEventArgs e)
    {
        if (_host?.Lookup is null || string.IsNullOrWhiteSpace(_name))
            return;
        await _host.Lookup(_name);
    }
}

public sealed class MonitorEvent
{
    public string Type { get; init; } = "";
    public string TimeText { get; init; } = "";
    public int Pid { get; init; }
    public int Sent { get; init; }
    public int Received { get; init; }
    public int Total { get; init; }
    public string Status { get; init; } = "";
    public string Answers { get; init; } = "";
}
