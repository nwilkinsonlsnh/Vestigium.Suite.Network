using System.Globalization;
using System.Text;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class SummaryFormatter
{
    public static string Format(
        string hostname,
        string domain,
        int domainRole,
        int productType,
        string manufacturer,
        string model,
        string serial,
        string osCaption,
        string osArchitecture,
        string version,
        string buildNumber,
        DateTime? installDate,
        DateTime? lastBoot,
        string processor,
        int sockets,
        int logical,
        double memoryGb,
        string currentUser,
        DateTime collectionTime,
        string inventoryLocation)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WINDOWS SERVER INVENTORY SUMMARY");
        sb.AppendLine("================================");
        Line(sb, "Hostname", hostname);
        Line(sb, "Domain", domain);
        Line(sb, "Domain Role", $"{domainRole} ({DomainRoleName(domainRole)})");
        Line(sb, "OS Product Type", $"{productType} ({ProductTypeName(productType)})");
        Line(sb, "Manufacturer", manufacturer);
        Line(sb, "Model", model);
        Line(sb, "Serial Number", serial);
        Line(sb, "Operating System", osCaption);
        Line(sb, "OS Architecture", osArchitecture);
        Line(sb, "Version", version);
        Line(sb, "Build Number", buildNumber);
        Line(sb, "Install Date", installDate?.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) ?? "");
        Line(sb, "Last Boot Time", lastBoot?.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) ?? "");
        Line(sb, "Current Uptime", Uptime(lastBoot, collectionTime));
        Line(sb, "Processor", processor);
        Line(sb, "Processor Sockets", sockets.ToString(CultureInfo.InvariantCulture));
        Line(sb, "Logical Processors", logical.ToString(CultureInfo.InvariantCulture));
        Line(sb, "Installed Memory (GB)", memoryGb.ToString("F2", CultureInfo.InvariantCulture));
        Line(sb, "Current User", currentUser);
        Line(sb, "Collection Time", collectionTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        Line(sb, "Inventory Location", inventoryLocation);
        return sb.ToString();
    }

    public static string DomainRoleName(int role) => role switch
    {
        0 => "Standalone Workstation",
        1 => "Member Workstation",
        2 => "Standalone Server",
        3 => "Member Server",
        4 => "Backup Domain Controller",
        5 => "Primary Domain Controller",
        _ => "Unknown"
    };

    public static string ProductTypeName(int type) => type switch
    {
        1 => "Work Station",
        2 => "Domain Controller",
        3 => "Server",
        _ => "Unknown"
    };

    public static string Uptime(DateTime? lastBoot, DateTime now)
    {
        if (lastBoot is null)
            return "";
        var span = now - lastBoot.Value;
        if (span < TimeSpan.Zero)
            span = TimeSpan.Zero;
        return $"{(int)span.TotalDays} days, {span.Hours} hours, {span.Minutes} minutes";
    }

    public static string ShortUser(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return "";
        var idx = userName.LastIndexOf('\\');
        return idx >= 0 ? userName[(idx + 1)..] : userName;
    }

    private static void Line(StringBuilder sb, string label, string value)
    {
        sb.Append(label.PadRight(26));
        sb.Append(": ");
        sb.AppendLine(value ?? "");
    }
}
