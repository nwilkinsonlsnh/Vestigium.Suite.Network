using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class NicIqSettings
{
    public string? ThemeId { get; set; }
    public int DurationSeconds { get; set; } = NicIqWatchInput.DefaultDurationSeconds;
    public bool IncludeDown { get; set; } = true;
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
}

public sealed class NicIqSettingsStore
{
    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public NicIqSettingsStore(string rootDirectory)
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
        "NicIQ");

    public NicIqSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new NicIqSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<NicIqSettings>(json, Json) ?? new NicIqSettings();
        }
        catch (Exception)
        {
            return new NicIqSettings();
        }
    }

    public void Save(NicIqSettings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
    }
}
