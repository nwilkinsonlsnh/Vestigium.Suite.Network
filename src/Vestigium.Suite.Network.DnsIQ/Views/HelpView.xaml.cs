using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class HelpView : UserControl
{
    public HelpView()
    {
        Topics = new ObservableCollection<HelpTopic>(Build());
        InitializeComponent();
        DataContext = this;
        Selected = Topics[0];
        Selected.IsSelected = true;
        Show(Selected);
    }

    public ObservableCollection<HelpTopic> Topics { get; }

    public HelpTopic? Selected { get; private set; }

    private void Show(HelpTopic topic)
    {
        Selected = topic;
        if (topic.Sections.Count == 0)
        {
            PageSections.Visibility = Visibility.Collapsed;
            PageSections.ItemsSource = null;
            Bind(topic.Document);
            return;
        }

        PageSections.ItemsSource = topic.Sections;
        PageSections.Visibility = Visibility.Visible;
        var section = topic.Sections.FirstOrDefault(s => s.IsSelected) ?? topic.Sections[0];
        section.IsSelected = true;
        Bind(section.Document);
    }

    private void Bind(FlowDocument document)
    {
        document.SetResourceReference(FlowDocument.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        document.SetResourceReference(FlowDocument.BackgroundProperty, "Vestigium.Brushes.Surface.Window");
        Reader.Document = document;
    }

    private IEnumerable<HelpTopic> Build()
    {
        yield return Topic("Overview",
            "DnsIQ asks a resolver and prints the answer. It exists so an operator can see what a name resolves to, and whether that answer holds up over a short run, without opening a command window.",
            "The pages are DnsIQ, Capture, Dashboard, Exports, and Settings. Help is this page. DnsIQ is one name. Capture is the hosts in a file. Dashboard is a reading of the last run. Exports writes the prints already loaded. Settings is where the window is adjusted.",
            "DnsIQ does not change DNS, edit a zone, shell to nslookup.exe, or use HTTP DNS. If the print is wrong, the correction is made elsewhere. This host is the record of what the resolver said.",
            "A row that says Resolved is not Reachable. A name can answer and the site can still fail to load.");
        yield return Topic("Lookup",
            "The DnsIQ tab is one name. Lookup writes the answer grid. A blank Name means localhost. The name is not persisted.",
            "Server, Port, Type, and Interface are the query. Server is the resolver. An empty server is the first adapter DNS. Port defaults to 53. Type defaults to All. Interface is a closed list from the adapters on this machine, plus Any.",
            "Lookup writes Type, Name, Data, and Ttl. A failed Lookup clears the grid. One job in the process. Cancel stops it. Probe and Capture use that same job. A second click while one is running does not start another.",
            Head("When to use it"),
            "Use Lookup when the question is what this resolver says about one name. It is not a pulse. It does not open a capture.");
        yield return Topic("Probe",
            "Probe is that Lookup, then a pulse of N lookups over X seconds. The prelude fills the grid. The pulse does not append rows.",
            "Requests and Seconds live on Settings, on the Probe page. The defaults are 1000 and 60. The status bar during the pulse is sent/total. The center of the bar is the time rail.",
            "Dashboard stays disabled until a Probe finishes. Lookup alone does not unlock it. NxDomain counts as answered. Timeout and Refused are counts only.",
            Head("When to use it"),
            "Use Probe when one answer is not enough and you need the times. The charts are a reading of those times. They are not a second measurement.");
        yield return Topic("Capture",
            "Capture is the hosts in a file. Open capture reads a HAR, or a UTF-8 .txt, or a .har that is not JSON. A file that parses starts the DNS probe. The row menu is Lookup, or Lookup + Probe, for that host.",
            "Columns are Host, Ports, Hits, Sources, DNS, Error, and Answers. DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Skipped is an address and is not sent.",
            "Resolved is not Reachable. A name that answers is not a site that loaded. This page is not a HAR analyzer: no waterfall, no cookies, no header dump, no replay.",
            Head("When to use it"),
            "Use Capture when the names are already in a dump, a pasted email, or a sheet saved as text. The file is the intake. There is no paste box.");
        yield return Topic("Dashboard",
            "Dashboard is a reading of the last run. It is not a second measurement.",
            "The Lookup tab is the type mix of the last answer grid. The Probe tab is the RTT curve, the histogram, and the control chart when Analytics returns fences. The empty state points back to DnsIQ.",
            "The tab stays disabled until a Probe finishes. Lookup alone does not unlock it.");
        yield return Topic("Exports",
            "Exports writes the prints already loaded. It does not query again. It does not re-read the file. Cover is always written. A checked print with rows becomes a sheet.",
            "The workbook stays on this machine. It contains the name, the answers, the capture hosts, and the probe times. It is not uploaded.",
            Head("When to use it"),
            "Use the workbook when another desk needs the print, or when a change request needs the rows the resolver returned. The file is the record. This tab does not compare two files.");
        yield return Topic("Settings",
            "Settings holds the knobs that are not the name.",
            "Settings, on the DnsIQ page, holds the Port seed and Source. Source is the local bind address, not spoofing. Settings, on the Probe page, holds Requests and Seconds. Settings, on the Theme page, holds the palette and the status bar.",
            "The file is settings.json under ProgramData. Name is not stored. Pulse samples are not stored.");
        yield return GlossaryTopic();
        yield return AboutTopic();
        yield return Topic("License", LicenseLines());
    }

    private HelpTopic GlossaryTopic()
    {
        var topic = Topic("Glossary",
            "The words this window uses. A short meaning, in the sense of this print.");
        topic.Sections.Add(Section("Tabs",
            Term("Lookup", "One name, asked once. The answer grid is Type, Name, Data, and Ttl."),
            Term("Probe", "That lookup, then a pulse. The pulse does not append rows."),
            Term("Capture", "Hosts taken from a HAR or a text dump. Opening a file that parses starts the DNS probe."),
            Term("Export", "The checked prints, written to one workbook. Cover is always included."),
            Term("Export All", "Every print that has rows, written to one workbook. The checks are ignored."),
            Term("Cover", "The first sheet. Host, operator, time, the query on screen, and the count of each print that was written.")));
        topic.Sections.Add(Section("Lookup",
            Term("Prelude", "The lookup that runs before the pulse. It fills the answer grid."),
            Term("Pulse", "N lookups over X seconds. It does not append rows."),
            Term("Source", "The local bind address. It is not spoofing.")));
        topic.Sections.Add(Section("Capture",
            Term("Host", "A name taken from the file. An address is listed and then skipped."),
            Term("Hits", "How many times that host was seen."),
            Term("Sources", "Where the name was found: a request, a redirect, a page, or the text."),
            Term("Resolved", "A or AAAA returned an answer. Resolved is not Reachable."),
            Term("Skipped", "The host is already an address. It is not sent.")));
        topic.Sections.Add(Section("Probe",
            Term("Requests", "How many lookups the pulse will send. The default is 1000."),
            Term("Seconds", "How long the pulse runs. The default is 60.")));
        topic.Sections[0].IsSelected = true;
        Wire(topic);
        return topic;
    }

    private HelpTopic AboutTopic()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        var topic = Topic("About", "DnsIQ " + version + ".");
        topic.Sections.Add(Section("App",
            "DnsIQ " + version + ". One name, a pulse, and the hosts in a capture. Exports writes those prints to a workbook.",
            "Copyright (c) 2026 Nathaniel Wilkinson. MIT License. The notice is the License topic.",
            "The public source for this host is Vestigium.Suite.Network.",
            Link("https://github.com/nwilkinsonlsnh/Vestigium.Suite.Network", "Vestigium.Suite.Network")));
        topic.Sections[0].IsSelected = true;
        Wire(topic);
        return topic;
    }

    private static string[] LicenseLines()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "LICENSE");
        if (!File.Exists(path))
            return ["The license file was not next to the program."];
        var lines = File.ReadAllLines(path);
        return lines.Length == 0 ? ["The license file was empty."] : lines;
    }

    private void Wire(HelpTopic topic)
    {
        foreach (var section in topic.Sections)
        {
            section.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HelpTopic.IsSelected) && section.IsSelected && ReferenceEquals(Selected, topic))
                    Bind(section.Document);
            };
        }
    }

    private static HelpTopic Section(string title, params string[] lines) => new(title, lines);

    private static string Head(string line) => "\x1f" + line;

    private static string Link(string url, string? label = null) => "\x1c" + url + "|" + (label ?? url);

    private static string Term(string word, string means) => "\x1b" + word + "|" + means;

    private HelpTopic Topic(string title, params string[] lines)
    {
        var topic = new HelpTopic(title, lines);
        topic.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HelpTopic.IsSelected) && topic.IsSelected)
                Show(topic);
        };
        return topic;
    }
}

public sealed class HelpTopic : INotifyPropertyChanged
{
    private bool _isSelected;

    public HelpTopic(string title, IReadOnlyList<string> lines)
    {
        Title = title;
        Lines = lines;
        Document = new FlowDocument { PagePadding = new Thickness(24) };
        Document.Blocks.Add(new Paragraph(new Run(title))
        {
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 12)
        });
        foreach (var line in lines)
        {
            if (line.StartsWith("\x1c", StringComparison.Ordinal))
            {
                var parts = line[1..].Split('|', 2);
                Document.Blocks.Add(LinkParagraph(parts[0], parts.Length == 2 ? parts[1] : parts[0]));
                continue;
            }

            if (line.StartsWith("\x1b", StringComparison.Ordinal))
            {
                var parts = line[1..].Split('|', 2);
                Document.Blocks.Add(new Paragraph(new Run(parts[0]))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 2)
                });
                if (parts.Length == 2)
                {
                    Document.Blocks.Add(new Paragraph(new Run(parts[1]))
                    {
                        FontSize = 15,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }
                continue;
            }

            var heading = line.StartsWith("\x1f", StringComparison.Ordinal);
            var body = heading ? line[1..] : line;
            Document.Blocks.Add(new Paragraph(new Run(body))
            {
                FontSize = heading ? 16 : 15,
                FontWeight = heading ? FontWeights.SemiBold : FontWeights.Normal,
                Margin = new Thickness(0, heading ? 14 : 0, 0, 8)
            });
        }
    }

    public string Title { get; }

    public IReadOnlyList<string> Lines { get; }

    public FlowDocument Document { get; }

    public List<HelpTopic> Sections { get; } = new();

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private static Paragraph LinkParagraph(string url, string label)
    {
        var link = new Hyperlink(new Run(label))
        {
            NavigateUri = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null
        };
        link.RequestNavigate += OnNavigate;
        return new Paragraph(link) { Margin = new Thickness(0, 0, 0, 8) };
    }

    private static void OnNavigate(object sender, RequestNavigateEventArgs e)
    {
        if (e.Uri is null || e.Uri.Scheme != Uri.UriSchemeHttps)
            return;
        e.Handled = true;
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
    }
}
