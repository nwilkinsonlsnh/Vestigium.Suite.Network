using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed class PingIqSettings
{
    public string? ThemeId { get; set; }
    public int Count { get; set; } = PingIqInput.DefaultCount;
    public int TimeoutMs { get; set; } = PingIqInput.DefaultTimeoutMs;
    public int InterfaceIndex { get; set; }
    public int Requests { get; set; } = 1000;
    public int Seconds { get; set; } = 60;
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public string? Source { get; set; }
    public bool ShowLegendEcho { get; set; } = true;
    public bool ShowLegendProbeRtt { get; set; } = true;
    public bool ShowLegendProbeDist { get; set; } = true;
    public bool ShowLegendProbeControl { get; set; } = true;
}

public sealed class PingIqSettingsStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public PingIqSettingsStore(string rootDirectory)
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
        "PingIQ");

    public PingIqSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new PingIqSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<PingIqSettings>(json, Json) ?? new PingIqSettings();
        }
        catch (Exception)
        {
            return new PingIqSettings();
        }
    }

    public void Save(PingIqSettings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
    }
}
