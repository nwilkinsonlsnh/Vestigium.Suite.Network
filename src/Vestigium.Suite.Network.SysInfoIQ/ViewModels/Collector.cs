using System.Text;
using Vestigium.Helpers.SystemInfo;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class Collector
{
    public static IReadOnlyList<InventorySection> CollectBase(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var inventory = SystemInventory.ReadBase();
        var sections = new List<InventorySection>
        {
            Section("ComputerSystem", "ComputerSystem.txt", inventory.ComputerSystem),
            Section("OperatingSystem", "OperatingSystem.txt", inventory.OperatingSystem),
            Section("BIOS", "BIOS.txt", inventory.Bios),
            Section("CPU", "CPU.txt", inventory.Processor)
        };
        return sections;
    }

    public static string BuildSummary(IReadOnlyList<InventorySection> sections, string inventoryLocation, DateTime now)
    {
        var cs = Find(sections, "ComputerSystem");
        var os = Find(sections, "OperatingSystem");
        var bios = Find(sections, "BIOS");
        var cpu = Find(sections, "CPU");

        var hostname = Value(cs, "Hostname") ?? Environment.MachineName;
        var domain = Value(cs, "Domain") ?? "";
        var role = Int(cs, "DomainRole");
        var productType = Int(os, "ProductType");
        var manufacturer = Value(cs, "Manufacturer") ?? "";
        var model = Value(cs, "Model") ?? "";
        var serial = Value(bios, "SerialNumber") ?? "";
        var caption = Value(os, "Caption") ?? "";
        var arch = Value(os, "Architecture") ?? "";
        var version = Value(os, "Version") ?? "";
        var build = Value(os, "BuildNumber") ?? "";
        var install = Date(os, "InstallDate");
        var boot = Date(os, "LastBootUpTime");
        var processor = Value(cpu, "Name")?.Trim() ?? "";
        var sockets = Int(cs, "Sockets");
        var logical = Int(cs, "LogicalProcessors");
        var memBytes = Long(cs, "TotalPhysicalMemoryBytes");
        var memGb = memBytes / 1024d / 1024d / 1024d;
        var user = SummaryFormatter.ShortUser(Value(cs, "UserName"));

        return SummaryFormatter.Format(
            hostname, domain, role, productType, manufacturer, model, serial,
            caption, arch, version, build, install, boot, processor,
            sockets, logical, memGb, user, now, inventoryLocation);
    }

    private static InventorySection Section<T>(string name, string file, Fact<T> fact)
    {
        if (!fact.IsOk)
            return new InventorySection { Name = name, FileName = file.Replace(".txt", "_ERROR.txt"), Content = "Unavailable", IsError = true };
        return new InventorySection { Name = name, FileName = file, Content = Format(fact.Value) };
    }

    private static string Format(object value)
    {
        var sb = new StringBuilder();
        foreach (var prop in value.GetType().GetProperties())
        {
            var raw = prop.GetValue(value);
            var text = raw switch
            {
                null => "",
                DateTime dt => dt.ToString("yyyyMMddHHmmss.000000-000", System.Globalization.CultureInfo.InvariantCulture),
                _ => raw.ToString() ?? ""
            };
            sb.Append(prop.Name.PadRight(42));
            sb.Append(": ");
            sb.AppendLine(text);
        }
        return sb.ToString();
    }

    private static InventorySection? Find(IReadOnlyList<InventorySection> sections, string name)
        => sections.FirstOrDefault(s => s.Name == name && !s.IsError);

    private static string? Value(InventorySection? section, string key)
    {
        if (section is null)
            return null;
        foreach (var line in section.Content.Split('\n'))
        {
            var idx = line.IndexOf(':');
            if (idx < 0)
                continue;
            var k = line[..idx].Trim();
            if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                return line[(idx + 1)..].Trim();
        }
        return null;
    }

    private static int Int(InventorySection? section, string key)
        => int.TryParse(Value(section, key), out var n) ? n : 0;

    private static long Long(InventorySection? section, string key)
        => long.TryParse(Value(section, key), out var n) ? n : 0;

    private static DateTime? Date(InventorySection? section, string key)
    {
        var raw = Value(section, key);
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        return DateTime.TryParseExact(raw, "yyyyMMddHHmmss.000000-000", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d)
            ? d
            : DateTime.TryParse(raw, out var parsed) ? parsed : null;
    }
}
