using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class MonitorDetailsWindow : Window
{
    private static MonitorDetailsWindow? _current;
    private MonitorViewModel? _host;
    private string _name = "";
    private readonly List<Conversation> _conversations = [];
    private readonly ObservableCollection<ConversationRow> _rows = [];

    public MonitorDetailsWindow()
    {
        InitializeComponent();
        Lines.ItemsSource = _rows;
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
        var lines = host.LinesFor(name).OrderBy(l => l.Time).ToList();
        _conversations.Clear();
        _conversations.AddRange(Pair(lines));
        var paired = _conversations.Count(c => c.Receive is not null);
        var open = _conversations.Count - paired;
        Caption.Text = $"{name}  —  Sequences {_conversations.Count}  Paired {paired}  Open {open}";
        Rebuild();
    }

    private static List<Conversation> Pair(List<MonitorLine> lines)
    {
        var open = new Dictionary<string, Queue<Conversation>>(StringComparer.Ordinal);
        var all = new List<Conversation>();
        var sequence = 0;
        foreach (var line in lines)
        {
            var isReceive = !string.IsNullOrWhiteSpace(line.Status) || !string.IsNullOrWhiteSpace(line.Answers);
            if (!open.TryGetValue(line.Type, out var queue))
            {
                queue = new Queue<Conversation>();
                open[line.Type] = queue;
            }

            if (!isReceive)
            {
                sequence++;
                var conversation = new Conversation
                {
                    Sequence = sequence,
                    Type = line.Type,
                    Sent = line,
                    IsExpanded = true
                };
                all.Add(conversation);
                queue.Enqueue(conversation);
                continue;
            }

            if (queue.Count > 0)
            {
                var conversation = queue.Dequeue();
                conversation.Receive = line;
                continue;
            }

            sequence++;
            all.Add(new Conversation
            {
                Sequence = sequence,
                Type = line.Type,
                Receive = line,
                IsExpanded = true
            });
        }

        return all;
    }

    private void Rebuild()
    {
        _rows.Clear();
        foreach (var conversation in _conversations)
        {
            _rows.Add(Row(conversation, null, conversation.Sequence.ToString(), conversation.Sent?.Time ?? conversation.Receive?.Time, "", "Sent"));
            if (!conversation.IsExpanded)
                continue;

            if (conversation.Receive is null)
                continue;

            _rows.Add(Row(conversation, null, "", conversation.Receive.Time, conversation.Receive.Status, "Received"));
            foreach (var answer in Answers(conversation.Receive))
                _rows.Add(Row(conversation, answer, "", null, "", answer));
        }
    }

    private static ConversationRow Row(Conversation conversation, string? answer, string sequence, DateTimeOffset? time, string status, string detail)
        => new()
        {
            Conversation = conversation,
            Answer = answer,
            SequenceText = sequence,
            Type = string.IsNullOrEmpty(sequence) ? "" : conversation.Type,
            TimeText = time is null ? "" : time.Value.ToLocalTime().ToString("M/d/yyyy h:mm:ss.fff tt"),
            Status = status,
            Detail = detail,
            Indent = new Thickness(string.IsNullOrEmpty(sequence) ? (answer is null ? 16 : 32) : 0, 0, 0, 0),
            Glyph = conversation.Receive is null && string.IsNullOrEmpty(answer) ? "" : (conversation.IsExpanded ? "\u25bc" : "\u25b6"),
            ExpanderVisibility = string.IsNullOrEmpty(sequence) || conversation.Receive is null ? Visibility.Hidden : Visibility.Visible,
            IsExpanded = conversation.IsExpanded
        };

    private void OnExpand(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not ConversationRow row)
            return;
        row.Conversation.IsExpanded = !row.Conversation.IsExpanded;
        Rebuild();
    }

    private void OnMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (Lines.ContextMenu is null)
            return;
        Lines.ContextMenu.Items.Clear();
        if (Lines.SelectedItem is not ConversationRow row || row.Conversation.Receive is null)
        {
            e.Handled = true;
            return;
        }

        var answers = Answers(row.Conversation.Receive).ToList();
        if (answers.Count == 0)
        {
            e.Handled = true;
            return;
        }

        Lines.ContextMenu.Items.Add(Menu("Copy", answers, Copy));
        Lines.ContextMenu.Items.Add(Menu("Lookup", answers, Lookup));
        Lines.ContextMenu.Items.Add(Menu("Lookup + Probe", answers, Probe));
    }

    private static MenuItem Menu(string header, List<string> answers, Action<string> act)
    {
        var menu = new MenuItem { Header = header };
        if (header == "Copy")
            menu.Items.Add(Item("All", () => act(string.Join(Environment.NewLine, answers))));
        foreach (var answer in answers)
        {
            var value = answer;
            menu.Items.Add(Item(value, () => act(value)));
        }

        return menu;
    }

    private static MenuItem Item(string header, Action act)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => act();
        return item;
    }

    private static void Copy(string value)
        => Clipboard.SetText(value);

    private void Lookup(string value)
    {
        if (_host?.Lookup is null || string.IsNullOrWhiteSpace(value))
            return;
        _ = _host.Lookup(value);
    }

    private void Probe(string value)
    {
        if (_host?.LookupAndProbe is null || string.IsNullOrWhiteSpace(value))
            return;
        _ = _host.LookupAndProbe(value);
    }

    private static IEnumerable<string> Answers(MonitorLine line)
        => string.IsNullOrWhiteSpace(line.Answers)
            ? []
            : line.Answers.Split("; ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}

public sealed class Conversation
{
    public int Sequence { get; init; }
    public string Type { get; init; } = "";
    public MonitorLine? Sent { get; init; }
    public MonitorLine? Receive { get; set; }
    public bool IsExpanded { get; set; }
}

public sealed class ConversationRow
{
    public Conversation Conversation { get; init; } = null!;
    public string? Answer { get; init; }
    public string SequenceText { get; init; } = "";
    public string Type { get; init; } = "";
    public string TimeText { get; init; } = "";
    public string Status { get; init; } = "";
    public string Detail { get; init; } = "";
    public Thickness Indent { get; init; }
    public string Glyph { get; init; } = "";
    public Visibility ExpanderVisibility { get; init; }
    public bool IsExpanded { get; init; }
}
