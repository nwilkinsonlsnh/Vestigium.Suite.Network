using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed class TraceIqSettings
{
    public string? ThemeId { get; set; }
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public int MaxHops { get; set; } = 30;
    public int Parallel { get; set; } = 10;
    public int Probes { get; set; } = 5;
    public string InterfaceId { get; set; } = "";
    public string Source { get; set; } = "";
    public string Family { get; set; } = "All";
    public int MruMax { get; set; } = 10;
    public int StickyMax { get; set; } = 3;
    public List<MruEntry> Mru { get; set; } = [];
}

public sealed class MruEntry
{
    public string Target { get; set; } = "";
    public bool Sticky { get; set; }
}

public static class TraceIqSettingsStore
{
    public static string Path => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Vestigium", "TraceIQ", "settings.json");

    public static TraceIqSettings Load()
    {
        try
        {
            if (!File.Exists(Path))
                return new TraceIqSettings();
            return JsonSerializer.Deserialize<TraceIqSettings>(File.ReadAllText(Path)) ?? new TraceIqSettings();
        }
        catch (Exception)
        {
            return new TraceIqSettings();
        }
    }

    public static void Save(TraceIqSettings data)
    {
        var dir = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(Path, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }
}
