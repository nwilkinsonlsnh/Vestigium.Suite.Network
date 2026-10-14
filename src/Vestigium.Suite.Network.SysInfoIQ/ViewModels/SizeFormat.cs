using System.Globalization;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class SizeFormat
{
    public static string Gb(object? bytes)
    {
        if (bytes is null)
            return "";
        if (!long.TryParse(bytes.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < 0)
            return "";
        var gb = n / 1024d / 1024d / 1024d;
        return gb.ToString("F2", CultureInfo.InvariantCulture);
    }

    public static string Percent(object? free, object? total)
    {
        if (!long.TryParse(free?.ToString(), out var f) || !long.TryParse(total?.ToString(), out var t) || t <= 0)
            return "";
        return (f * 100d / t).ToString("F1", CultureInfo.InvariantCulture);
    }

    public static string DriveTypeName(object? code)
    {
        if (!int.TryParse(code?.ToString(), out var n))
            return code?.ToString() ?? "";
        return n switch
        {
            0 => "Unknown",
            1 => "No Root",
            2 => "Removable",
            3 => "Fixed",
            4 => "Network",
            5 => "CD-ROM",
            6 => "RAM Disk",
            _ => n.ToString(CultureInfo.InvariantCulture)
        };
    }
}
