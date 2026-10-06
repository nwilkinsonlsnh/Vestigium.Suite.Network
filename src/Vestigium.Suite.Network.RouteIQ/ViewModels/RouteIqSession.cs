using System.Collections.ObjectModel;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed class RouteIqSession
{
    public const string RouteTab = "RouteIQ";
    public const string NeighborTab = "Neighbors";
    public const string ConnectionTab = "Connections";

    private readonly RouteIqSettingsStore _store;
    private readonly ThemeManager _themes;
    private readonly VestigiumDefaultWindowViewModel _chrome;
    private SettingsViewModel? _settings;
    private bool _ready;

    public RouteIqSession(RouteIqSettingsStore store, ThemeManager themes, VestigiumDefaultWindowViewModel chrome)
    {
        _store = store;
        _themes = themes;
        _chrome = chrome;
        var loaded = store.Load();
        Current = loaded.Settings;
        Report(loaded.Reason);
        QueryMruLimit = Clamp(Current.QueryMruLimit);
        WatchSeconds = Current.WatchSeconds is >= 5 and <= 180 ? Current.WatchSeconds : 10;
        ExportRoutes = Current.ExportRoutes;
        ExportNeighbors = Current.ExportNeighbors;
        ExportConnections = Current.ExportConnections;
        ExportNetBios = Current.ExportNetBios;
        ExportLmHosts = Current.ExportLmHosts;
        ExportOpenAfter = Current.ExportOpenAfter;
        ExportOpenFolder = Current.ExportOpenFolder;
        foreach (var entry in Current.Queries)
            Queries.Add(entry);
        Trim();
        ApplyThemeAndBar();
    }

    public RouteIqSettings Current { get; private set; }

    public ObservableCollection<RouteIqQueryEntry> Queries { get; } = [];

    public int QueryMruLimit { get; private set; }

    public int WatchSeconds { get; private set; }

    public bool ExportRoutes { get; private set; }

    public bool ExportNeighbors { get; private set; }

    public bool ExportConnections { get; private set; }

    public bool ExportNetBios { get; private set; }

    public bool ExportLmHosts { get; private set; }

    public bool ExportOpenAfter { get; private set; }

    public bool ExportOpenFolder { get; private set; }

    public bool QueryMruEnabled => QueryMruLimit > 0;

    public int OuiPoolSize => Current.OuiPoolSize is >= 1 and <= 20 ? Current.OuiPoolSize : 10;

    public void Attach(SettingsViewModel settings)
    {
        _settings = settings;
        settings.Session = this;
        settings.BindQueries();
        settings.BeginLoad();
        settings.LoadFrom(Current);
        settings.EndLoad();
        _ready = true;
    }

    public void SetQueryMruLimit(int limit)
    {
        QueryMruLimit = Clamp(limit);
        Trim();
        Save();
    }

    public void SetWatchSeconds(int seconds)
    {
        var clamped = Math.Clamp(seconds, 5, 180);
        if (WatchSeconds == clamped)
            return;
        WatchSeconds = clamped;
        Save();
    }

    public void SetExports(bool routes, bool neighbors, bool connections, bool netbios, bool lmhosts, bool openAfter, bool openFolder)
    {
        if (ExportRoutes == routes && ExportNeighbors == neighbors && ExportConnections == connections && ExportNetBios == netbios && ExportLmHosts == lmhosts && ExportOpenAfter == openAfter && ExportOpenFolder == openFolder)
            return;
        ExportRoutes = routes;
        ExportNeighbors = neighbors;
        ExportConnections = connections;
        ExportNetBios = netbios;
        ExportLmHosts = lmhosts;
        ExportOpenAfter = openAfter;
        ExportOpenFolder = openFolder;
        Save();
    }

    public void Remember(string tab, string text)
    {
        text = text.Trim();
        if (text.Length == 0 || QueryMruLimit == 0)
            return;

        var existing = Queries.FirstOrDefault(entry =>
            string.Equals(entry.Tab, tab, StringComparison.Ordinal)
            && string.Equals(entry.Text, text, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            Queries.Remove(existing);
            Queries.Insert(0, existing);
        }
        else
        {
            Queries.Insert(0, new RouteIqQueryEntry { Tab = tab, Text = text });
        }

        Trim();
        Save();
    }

    public void SetSticky(RouteIqQueryEntry entry, bool sticky)
    {
        entry.Sticky = sticky;
        Trim();
        Save();
    }

    public void Remove(RouteIqQueryEntry entry)
    {
        Queries.Remove(entry);
        Save();
    }

    public void Save()
    {
        if (!_ready || _settings is null)
            return;

        Current = new RouteIqSettings
        {
            ThemeId = _settings.SelectedThemeId,
            StatusBarVisible = _settings.BarVisible,
            StatusBarDock = _settings.BarPosition == VestigiumStatusBarPosition.Top ? "Top" : "Bottom",
            OuiPoolSize = _settings.OuiPoolSize is >= 1 and <= 20 ? _settings.OuiPoolSize : 10,
            QueryMruLimit = QueryMruLimit,
            WatchSeconds = WatchSeconds,
            CloseMruOnApply = _settings.CloseMruOnApply,
            ExportRoutes = ExportRoutes,
            ExportNeighbors = ExportNeighbors,
            ExportConnections = ExportConnections,
            ExportNetBios = ExportNetBios,
            ExportLmHosts = ExportLmHosts,
            ExportOpenAfter = ExportOpenAfter,
            ExportOpenFolder = ExportOpenFolder,
            ConnectionMarks = _settings.Marks.ToSettings(),
            LiveVendorLookup = _settings.LiveVendorLookup,
            Queries = Queries.ToList()
        };
        Report(_store.Save(Current).Reason);
    }

    private void Report(string? reason)
    {
        if (!string.IsNullOrWhiteSpace(reason))
            _chrome.Status.Message = reason;
    }

    private void Trim()
    {
        if (QueryMruLimit == 0)
            return;

        foreach (var tab in new[] { RouteTab, NeighborTab, ConnectionTab })
        {
            var rolling = Queries.Where(entry => entry.Tab == tab && !entry.Sticky).Skip(QueryMruLimit).ToArray();
            foreach (var entry in rolling)
                Queries.Remove(entry);
        }
    }

    private static int Clamp(int limit) => Math.Clamp(limit, 0, 30);

    private void ApplyThemeAndBar()
    {
        if (!string.IsNullOrWhiteSpace(Current.ThemeId))
            _themes.SwitchTheme(Current.ThemeId);

        _chrome.ShowStatusBar = Current.StatusBarVisible;
        if (string.Equals(Current.StatusBarDock, "Top", StringComparison.OrdinalIgnoreCase))
            _chrome.DockStatusBarTopCommand.Execute(null);
        else
            _chrome.DockStatusBarBottomCommand.Execute(null);
    }
}
