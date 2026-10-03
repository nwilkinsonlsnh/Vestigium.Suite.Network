using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed class RouteIqSettings
{
    public string? ThemeId { get; set; }
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public int OuiPoolSize { get; set; } = 10;
}

public sealed class RouteIqSettingsStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public RouteIqSettingsStore(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        FilePath = Path.Combine(rootDirectory, "settings.json");
    }

    public string RootDirectory { get; }

    public string FilePath { get; }

    public static string DefaultRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Vestigium",
        "Settings",
        "Diagnostics",
        "RouteIQ");

    public RouteIqSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new RouteIqSettings();
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<RouteIqSettings>(json, Json) ?? new RouteIqSettings();
        }
        catch (Exception)
        {
            return new RouteIqSettings();
        }
    }

    public void Save(RouteIqSettings settings)
    {
        try
        {
            Directory.CreateDirectory(RootDirectory);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
        }
        catch (Exception)
        {
        }
    }
}
