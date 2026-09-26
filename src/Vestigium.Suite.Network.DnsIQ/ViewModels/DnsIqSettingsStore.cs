using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed class DnsIqSettings
{
    public string? ThemeId { get; set; }
    public string? Server { get; set; }
    public string Type { get; set; } = "All";
    public int InterfaceIndex { get; set; }
    public int Port { get; set; } = DnsIqInput.DefaultPort;
    public int Requests { get; set; } = 1000;
    public int Seconds { get; set; } = 60;
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public string? Source { get; set; }
}

public sealed class DnsIqSettingsStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public DnsIqSettingsStore(string rootDirectory)
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
        "DnsIQ");

    public DnsIqSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new DnsIqSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<DnsIqSettings>(json, Json) ?? new DnsIqSettings();
        }
        catch (Exception)
        {
            return new DnsIqSettings();
        }
    }

    public void Save(DnsIqSettings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
    }
}
