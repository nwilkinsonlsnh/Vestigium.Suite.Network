using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class SystemDetailForm
{
    public string PageFileTotal { get; private set; } = Dash;

    public string PageFileMinimum { get; private set; } = Dash;

    public string PageFileMaximum { get; private set; } = Dash;

    public string PageFileManaged { get; private set; } = Dash;

    private void ApplyPageFile()
    {
        var configured = ReadPageConfig();
        PageFile = configured.Location;
        PageFileMinimum = configured.Minimum;
        PageFileMaximum = configured.Maximum;
        PageFileManaged = configured.Managed;
        PageFileTotal = PageTotal();
    }

    private static string PageTotal()
    {
        var status = new MemoryStatusEx { Length = (uint)System.Runtime.InteropServices.Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status) || status.TotalPageFile == 0)
            return Dash;
        return Mb(status.TotalPageFile);
    }

    private static (string Location, string Minimum, string Maximum, string Managed) ReadPageConfig()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management");
        if (key?.GetValue("PagingFiles") is not string[] files || files.Length == 0)
            return (Dash, Dash, Dash, Dash);
        var locations = new List<string>();
        var minimum = new List<string>();
        var maximum = new List<string>();
        var managed = true;
        foreach (var row in files)
        {
            if (string.IsNullOrWhiteSpace(row))
                continue;
            var parts = row.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            locations.Add(SystemDrive(parts[0]));
            var min = parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var low) ? low : 0;
            var max = parts.Length > 2 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var high) ? high : 0;
            if (min != 0 || max != 0)
                managed = false;
            minimum.Add(min == 0 ? "System managed" : min.ToString(CultureInfo.InvariantCulture) + " MB");
            maximum.Add(max == 0 ? "System managed" : max.ToString(CultureInfo.InvariantCulture) + " MB");
        }

        return (
            locations.Count == 0 ? Dash : string.Join("; ", locations),
            minimum.Count == 0 ? Dash : string.Join("; ", minimum),
            maximum.Count == 0 ? Dash : string.Join("; ", maximum),
            managed ? "Yes" : "No");
    }

    private static string SystemDrive(string path)
    {
        if (!path.StartsWith("?", StringComparison.Ordinal))
            return path;
        var root = Path.GetPathRoot(Environment.SystemDirectory);
        var letter = string.IsNullOrWhiteSpace(root) ? "C:" : root.TrimEnd('\\');
        return letter + path[1..];
    }
}
