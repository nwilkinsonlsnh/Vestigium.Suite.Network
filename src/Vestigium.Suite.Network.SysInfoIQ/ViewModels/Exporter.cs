using System.IO;
using System.Text;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class Exporter
{
    public static string Write(
        string outputRoot,
        string hostname,
        string summary,
        IReadOnlyList<InventorySection> sections,
        DateTime now)
    {
        var safeHost = string.IsNullOrWhiteSpace(hostname) ? Environment.MachineName : hostname;
        var folderName = $"{safeHost}_Inventory_{now:yyyyMMdd_HHmmss}";
        var folder = Path.Combine(outputRoot, folderName);
        Directory.CreateDirectory(folder);

        var written = new List<(string Name, long Length, DateTime Written)>();

        var summaryPath = Path.Combine(folder, $"{safeHost}_Summary.txt");
        File.WriteAllText(summaryPath, summary, Encoding.UTF8);
        written.Add((Path.GetFileName(summaryPath), new FileInfo(summaryPath).Length, File.GetLastWriteTime(summaryPath)));

        foreach (var section in sections)
        {
            var path = Path.Combine(folder, $"{safeHost}_{section.FileName}");
            File.WriteAllText(path, section.Content, Encoding.UTF8);
            written.Add((Path.GetFileName(path), new FileInfo(path).Length, File.GetLastWriteTime(path)));
        }

        var manifestPath = Path.Combine(folder, $"{safeHost}_InventoryManifest.txt");
        var manifest = BuildManifest(written);
        File.WriteAllText(manifestPath, manifest, Encoding.UTF8);

        return folder;
    }

    private static string BuildManifest(IReadOnlyList<(string Name, long Length, DateTime Written)> files)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Name".PadRight(48) + "Length".PadLeft(12) + "  LastWriteTime");
        sb.AppendLine(new string('-', 80));
        foreach (var f in files.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            sb.Append(f.Name.PadRight(48));
            sb.Append(f.Length.ToString().PadLeft(12));
            sb.Append("  ");
            sb.AppendLine(f.Written.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        return sb.ToString();
    }
}
