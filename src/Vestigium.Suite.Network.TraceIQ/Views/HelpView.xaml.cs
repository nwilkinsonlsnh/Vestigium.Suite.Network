using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Vestigium.Suite.Network.TraceIQ.Views;

public partial class HelpView : UserControl
{
    public HelpView()
    {
        Topics = new ObservableCollection<HelpTopic>(
        [
            new("TraceIQ", "One target. One walk. The path, then a ping to each hop that answered."),
            new("Settings", "Max hops, parallel, probes, interface, source, and family live here. So does the MRU."),
            new("MRU", "Recent targets. A sticky stays and does not count against the MRU max.")
        ]);
        InitializeComponent();
        DataContext = this;
        Selected = Topics[0];
        Selected.IsSelected = true;
    }

    public ObservableCollection<HelpTopic> Topics { get; }
    public HelpTopic Selected { get; private set; }
    public string Body => Selected.Body;
}

public sealed class HelpTopic : INotifyPropertyChanged
{
    public HelpTopic(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public string Title { get; }
    public string Body { get; }
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected))); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
