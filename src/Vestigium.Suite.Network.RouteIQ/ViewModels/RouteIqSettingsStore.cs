using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed class RouteIqQueryEntry
{
    public string Tab { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Sticky { get; set; }
}

public sealed class ConnectionMarkSettings
{
    public bool ColorChangedRows { get; set; } = true;
    public string AddedBackground { get; set; } = ConnectionMarkPalette.DefaultAddedBackground;
    public string AddedForeground { get; set; } = ConnectionMarkPalette.DefaultAddedForeground;
    public string DroppedBackground { get; set; } = ConnectionMarkPalette.DefaultDroppedBackground;
    public string DroppedForeground { get; set; } = ConnectionMarkPalette.DefaultDroppedForeground;
    public string ReopenedBackground { get; set; } = ConnectionMarkPalette.DefaultReopenedBackground;
    public string ReopenedForeground { get; set; } = ConnectionMarkPalette.DefaultReopenedForeground;
}

public sealed class RouteIqSettings
{
    public string? ThemeId { get; set; }
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public int OuiPoolSize { get; set; } = 10;
    public int QueryMruLimit { get; set; } = 10;
    public int WatchSeconds { get; set; } = 10;
    public bool CloseMruOnApply { get; set; } = true;
    public bool ExportRoutes { get; set; } = true;
    public bool ExportNeighbors { get; set; } = true;
    public bool ExportConnections { get; set; } = true;
    public bool ExportNetBios { get; set; } = true;
    public bool ExportLmHosts { get; set; } = true;
    public bool ExportOpenAfter { get; set; }
    public bool ExportOpenFolder { get; set; }
    public bool LiveVendorLookup { get; set; }
    public ConnectionMarkSettings ConnectionMarks { get; set; } = new();
    public List<RouteIqQueryEntry> Queries { get; set; } = [];
}

public sealed record RouteIqSettingsResult(RouteIqSettings Settings, string? Reason)
{
    public bool Ok => Reason is null;
}

public sealed class RouteIqSettingsStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    private readonly string? _legacyFilePath;

    public RouteIqSettingsStore(string rootDirectory, string? legacyFilePath = null)
    {
        RootDirectory = rootDirectory;
        FilePath = Path.Combine(rootDirectory, "settings.json");
        _legacyFilePath = legacyFilePath;
    }

    public string RootDirectory { get; }

    public string FilePath { get; }

    public static string DefaultRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Vestigium",
        "Settings",
        "RouteIQ");

    public static string LegacyFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Vestigium",
        "Settings",
        "Diagnostics",
        "RouteIQ",
        "settings.json");

    public RouteIqSettingsResult Load()
    {
        if (!File.Exists(FilePath))
        {
            var copied = CopyLegacyOnce();
            if (copied is not null)
                return new RouteIqSettingsResult(new RouteIqSettings(), copied);
        }

        if (!File.Exists(FilePath))
            return new RouteIqSettingsResult(new RouteIqSettings(), null);

        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<RouteIqSettings>(json, Json) ?? new RouteIqSettings();
            return new RouteIqSettingsResult(settings, null);
        }
        catch (Exception ex)
        {
            return new RouteIqSettingsResult(new RouteIqSettings(), "Settings file could not be read. " + ex.Message);
        }
    }

    public RouteIqSettingsResult Save(RouteIqSettings settings)
    {
        try
        {
            Directory.CreateDirectory(RootDirectory);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
            return new RouteIqSettingsResult(settings, null);
        }
        catch (Exception ex)
        {
            return new RouteIqSettingsResult(settings, "Settings were not saved. " + ex.Message);
        }
    }

    private string? CopyLegacyOnce()
    {
        var legacy = _legacyFilePath ?? LegacyFilePath;
        if (!File.Exists(legacy))
            return null;

        try
        {
            Directory.CreateDirectory(RootDirectory);
            File.Copy(legacy, FilePath, overwrite: false);
            return null;
        }
        catch (Exception ex)
        {
            return "Settings were not copied from the old folder. " + ex.Message;
        }
    }
}
