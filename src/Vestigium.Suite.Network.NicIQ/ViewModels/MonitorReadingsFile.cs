using System.IO;
using System.Text.Json;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class MonitorReadingsFile
{
    public string Kind { get; set; } = "NicIQ.Readings";
    public int Version { get; set; } = 1;
    public DateTimeOffset ExportedUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? NicName { get; set; }
    public string? PdhInstance { get; set; }
    public List<MonitorReadingSeries> Series { get; set; } = [];
}

public sealed class MonitorReadingSeries
{
    public MonitorReadingSeries()
    {
    }

    public MonitorReadingSeries(string counter, IReadOnlyList<MonitorReadingPoint> points)
    {
        Counter = counter;
        Points = [.. points];
    }

    public string Counter { get; set; } = string.Empty;
    public List<MonitorReadingPoint> Points { get; set; } = [];
}

public sealed class MonitorReadingPoint
{
    public MonitorReadingPoint()
    {
    }

    public MonitorReadingPoint(decimal value, DateTimeOffset? at)
    {
        Value = value;
        At = at;
    }

    public decimal Value { get; set; }
    public DateTimeOffset? At { get; set; }
}

internal static class MonitorReadingsIo
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string DefaultFolder
    {
        get
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "Vestigium",
                "Exports");
            Directory.CreateDirectory(root);
            return root;
        }
    }

    public static void Write(string path, MonitorReadingsFile file)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(file);
        File.WriteAllText(path, JsonSerializer.Serialize(file, Json));
    }

    public static MonitorReadingsFile? Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            return null;
        return JsonSerializer.Deserialize<MonitorReadingsFile>(File.ReadAllText(path), Json);
    }
}
