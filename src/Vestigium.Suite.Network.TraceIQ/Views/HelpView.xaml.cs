using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Vestigium.Suite.Network.TraceIQ.Views;

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
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        foreach (var draft in HelpCopy.Topics(version, LicenseLines()))
        {
            var topic = Topic(draft.Title, draft.Lines.ToArray());
            foreach (var section in draft.Sections)
                topic.Sections.Add(Section(section.Title, section.Lines.ToArray()));
            if (topic.Sections.Count > 0)
            {
                topic.Sections[0].IsSelected = true;
                Wire(topic);
            }
            yield return topic;
        }
    }

    private static string[] LicenseLines()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "LICENSE");
        if (!File.Exists(path))
            return ["The license file was not next to the program."];
        return File.ReadAllLines(path);
    }

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

    private HelpTopic Section(string title, params string[] lines) => new(title, lines);

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
}

public sealed class HelpTopic : INotifyPropertyChanged
{
    public HelpTopic(string title, params string[] lines)
    {
        Title = title;
        Lines = lines;
        Document = new FlowDocument { PagePadding = new Thickness(0), FontSize = 15 };
        foreach (var line in lines)
        {
            if (line.StartsWith("https://", StringComparison.Ordinal))
            {
                var parts = line.Split('|', 2);
                Document.Blocks.Add(Link(parts[0], parts.Length == 2 ? parts[1] : parts[0]));
                continue;
            }
            if (line.StartsWith("\x1c", StringComparison.Ordinal))
            {
                Document.Blocks.Add(Picture(line[1..]));
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
                    Document.Blocks.Add(new Paragraph(new Run(parts[1])) { FontSize = 15, Margin = new Thickness(0, 0, 0, 8) });
                continue;
            }
            var heading = line.StartsWith("\x1f", StringComparison.Ordinal);
            Document.Blocks.Add(new Paragraph(new Run(heading ? line[1..] : line))
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

    private bool _isSelected;
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

    private static Block Picture(string name)
    {
        var image = new System.Windows.Controls.Image
        {
            Source = new BitmapImage(new Uri($"pack://application:,,,/Help/{name}", UriKind.Absolute)),
            MaxWidth = 640,
            Stretch = System.Windows.Media.Stretch.Uniform
        };
        return new BlockUIContainer(image) { Margin = new Thickness(0, 4, 0, 12) };
    }

    private static Paragraph Link(string url, string label)
    {
        var link = new Hyperlink(new Run(label))
        {
            NavigateUri = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null
        };
        link.RequestNavigate += (_, e) =>
        {
            if (e.Uri is null || e.Uri.Scheme != Uri.UriSchemeHttps)
                return;
            e.Handled = true;
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        };
        return new Paragraph(link) { Margin = new Thickness(0, 0, 0, 8) };
    }
}
