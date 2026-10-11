using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public const int MruMaxLimit = 30;
    public const int StickyMaxLimit = 10;

    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private readonly IReadOnlyList<NetworkAdapter> _adapters;
    public IReadOnlyList<NetworkAdapter> Adapters => _adapters;
    private bool _loading;

    public SettingsViewModel(ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _themes = themes;
        _chrome = chrome;
        try
        {
            _adapters = NetworkHelper.GetAdapters();
        }
        catch (Exception)
        {
            _adapters = [];
        }

        Interfaces = AdapterChoices.From(_adapters);
        Families = ["All", "IPv4", "IPv6"];
        Sources = SourceChoices.From(_adapters, SelectedInterfaceId);
        _selectedThemeId = themes.Current?.Id ?? themes.AvailableThemes.FirstOrDefault()?.Id;
        _barPosition = chrome.Status.Position;
        _barVisible = chrome.ShowStatusBar;
    }

    public IReadOnlyList<ThemeDefinition> Themes => _themes.AvailableThemes;
    public IReadOnlyList<AdapterChoice> Interfaces { get; }
    public IReadOnlyList<SourceChoice> Sources { get; private set; }
    public IReadOnlyList<string> Protocols { get; } = ["ICMP", "UDP", "TCP", "Hybrid"];
    public IReadOnlyList<string> Families { get; }
    public IReadOnlyList<VestigiumStatusBarPosition> BarPositions { get; } =
    [
        VestigiumStatusBarPosition.Bottom,
        VestigiumStatusBarPosition.Top
    ];

    public ObservableCollection<MruEntry> Mru { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TracePageOpen))]
    [NotifyPropertyChangedFor(nameof(AdvancedPageOpen))]
    [NotifyPropertyChangedFor(nameof(MruPageOpen))]
    [NotifyPropertyChangedFor(nameof(ThemePageOpen))]
    private string _settingsPage = "TraceIQ";

    public bool TracePageOpen
    {
        get => SettingsPage == "TraceIQ";
        set { if (value) SettingsPage = "TraceIQ"; }
    }

    public bool AdvancedPageOpen
    {
        get => SettingsPage == "Advanced";
        set { if (value) SettingsPage = "Advanced"; }
    }

    public bool MruPageOpen
    {
        get => SettingsPage == "MRU";
        set { if (value) SettingsPage = "MRU"; }
    }

    public bool ThemePageOpen
    {
        get => SettingsPage == "Theme";
        set { if (value) SettingsPage = "Theme"; }
    }

    [ObservableProperty] private string? _selectedThemeId;
    [ObservableProperty] private bool _barVisible;
    [ObservableProperty] private VestigiumStatusBarPosition _barPosition;
    [ObservableProperty] private decimal _maxHops = 30;
    [ObservableProperty] private decimal _parallel = 10;
    [ObservableProperty] private decimal _probes = 5;
    [ObservableProperty] private string _selectedInterfaceId = "";
    [ObservableProperty] private string _source = "";
    [ObservableProperty] private string _family = "All";
    [ObservableProperty] private string _protocolChoice = "Hybrid";
    [ObservableProperty] private bool _secondPass;
    [ObservableProperty] private decimal _mruMax = 10;
    [ObservableProperty] private decimal _stickyMax = 3;

    public void Load(TraceIqSettings data)
    {
        _loading = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(data.ThemeId))
                SelectedThemeId = data.ThemeId;
            BarVisible = data.StatusBarVisible;
            BarPosition = string.Equals(data.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase)
                ? VestigiumStatusBarPosition.Top
                : VestigiumStatusBarPosition.Bottom;
            MaxHops = data.MaxHops;
            Parallel = data.Parallel;
            Probes = data.Probes;
            SelectedInterfaceId = data.InterfaceId ?? "";
            Source = data.Source ?? "";
            Family = string.IsNullOrWhiteSpace(data.Family) ? "All" : data.Family;
            ProtocolChoice = string.IsNullOrWhiteSpace(data.Protocol) ? "Hybrid" : data.Protocol;
            SecondPass = data.SecondPass;
            MruMax = Math.Clamp(data.MruMax, 1, MruMaxLimit);
            StickyMax = Math.Clamp(data.StickyMax, 0, StickyMaxLimit);
            Mru.Clear();
            foreach (var entry in data.Mru)
                Mru.Add(entry);
            Trim();
        }
        finally
        {
            _loading = false;
        }
    }

    public TraceIqSettings Capture()
        => new()
        {
            ThemeId = SelectedThemeId,
            StatusBarVisible = BarVisible,
            StatusBarDock = BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom",
            MaxHops = (int)MaxHops,
            Parallel = (int)Parallel,
            Probes = (int)Probes,
            InterfaceId = SelectedInterfaceId,
            Source = Source,
            Family = Family,
            Protocol = ProtocolChoice,
            SecondPass = SecondPass,
            MruMax = (int)MruMax,
            StickyMax = (int)StickyMax,
            Mru = Mru.ToList()
        };

    public void Remember(string target)
    {
        var name = target.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;
        var existing = Mru.FirstOrDefault(m => m.Target.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            var index = Mru.Count(m => m.Sticky);
            Mru.Insert(index, new MruEntry { Target = name });
        }
        else if (!existing.Sticky)
        {
            var index = Mru.Count(m => m.Sticky);
            var current = Mru.IndexOf(existing);
            if (current != index)
                Mru.Move(current, index);
        }

        Trim();
        Persist();
    }

    public bool IsSticky(string target)
        => Mru.Any(m => m.Sticky && m.Target.Equals(target.Trim(), StringComparison.OrdinalIgnoreCase));

    public bool CanPin(string target)
    {
        var name = target.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (IsSticky(name))
            return true;
        return Mru.Count(m => m.Sticky) < (int)StickyMax;
    }

    public void Pin(string target)
    {
        var name = target.Trim();
        if (!CanPin(name))
            return;
        Remember(name);
        var entry = Mru.First(m => m.Target.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (!entry.Sticky && Mru.Count(m => m.Sticky) >= (int)StickyMax)
            return;
        entry.Sticky = !entry.Sticky;
        var ordered = Mru.OrderByDescending(m => m.Sticky).ThenBy(m => Mru.IndexOf(m)).ToList();
        Mru.Clear();
        foreach (var row in ordered)
            Mru.Add(row);
        Persist();
    }

    [RelayCommand]
    private void ToggleSticky(MruEntry? entry)
    {
        if (entry is null)
            return;
        if (!entry.Sticky && Mru.Count(m => m.Sticky) >= (int)StickyMax)
            return;
        entry.Sticky = !entry.Sticky;
        var ordered = Mru.OrderByDescending(m => m.Sticky).ThenBy(m => Mru.IndexOf(m)).ToList();
        Mru.Clear();
        foreach (var row in ordered)
            Mru.Add(row);
        OnPropertyChanged(nameof(Mru));
        Persist();
    }

    private void Trim()
    {
        var stickies = Mru.Where(m => m.Sticky).Take((int)StickyMax).ToList();
        var rest = Mru.Where(m => !m.Sticky).Take((int)MruMax).ToList();
        if (stickies.Count + rest.Count == Mru.Count && stickies.Count == Mru.Count(m => m.Sticky))
            return;
        Mru.Clear();
        foreach (var row in stickies.Concat(rest))
            Mru.Add(row);
    }

    partial void OnSelectedThemeIdChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || _loading)
            return;
        if (_themes.Current?.Id != value)
            _themes.SwitchTheme(value);
        Persist();
    }

    partial void OnBarVisibleChanged(bool value)
    {
        if (_loading) return;
        _chrome.ShowStatusBar = value;
        Persist();
    }

    partial void OnBarPositionChanged(VestigiumStatusBarPosition value)
    {
        if (_loading) return;
        _chrome.Status.Position = value;
        if (value == VestigiumStatusBarPosition.Bottom)
            _chrome.DockStatusBarBottomCommand.Execute(null);
        else
            _chrome.DockStatusBarTopCommand.Execute(null);
        Persist();
    }

    partial void OnMaxHopsChanged(decimal value) => Persist();
    partial void OnParallelChanged(decimal value) => Persist();
    partial void OnProbesChanged(decimal value) => Persist();
    partial void OnSelectedInterfaceIdChanged(string value)
    {
        var keep = Source;
        Sources = SourceChoices.From(_adapters, value);
        OnPropertyChanged(nameof(Sources));
        if (Sources.All(s => s.Address != keep))
            Source = "";
        Persist();
    }
    partial void OnSourceChanged(string value) => Persist();
    partial void OnFamilyChanged(string value) => Persist();
    partial void OnProtocolChoiceChanged(string value)
    {
        OnPropertyChanged(nameof(SecondPassEnabled));
        Persist();
    }

    public bool SecondPassEnabled => ProtocolChoice == "Hybrid";
    partial void OnSecondPassChanged(bool value) => Persist();
    partial void OnMruMaxChanged(decimal value) { Trim(); Persist(); }
    partial void OnStickyMaxChanged(decimal value) { Trim(); Persist(); }

    private void Persist()
    {
        if (_loading)
            return;
        TraceIqSettingsStore.Save(Capture());
    }
}
