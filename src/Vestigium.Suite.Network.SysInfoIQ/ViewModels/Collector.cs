using System.Management;
using System.Text;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class Collector
{
    public static IReadOnlyList<InventorySection> CollectBase(CancellationToken ct)
    {
        var sections = new List<InventorySection>();
        sections.Add(Query("ComputerSystem", "ComputerSystem.txt", "SELECT * FROM Win32_ComputerSystem", ct));
        sections.Add(Query("OperatingSystem", "OperatingSystem.txt", "SELECT * FROM Win32_OperatingSystem", ct));
        sections.Add(Query("BIOS", "BIOS.txt", "SELECT * FROM Win32_BIOS", ct));
        sections.Add(Query("CPU", "CPU.txt", "SELECT * FROM Win32_Processor", ct));
        sections.Add(Query("Memory", "Memory.txt", "SELECT * FROM Win32_PhysicalMemory", ct));
        return sections;
    }

    public static string BuildSummary(IReadOnlyList<InventorySection> sections, string inventoryLocation, DateTime now)
    {
        var cs = Find(sections, "ComputerSystem");
        var os = Find(sections, "OperatingSystem");
        var bios = Find(sections, "BIOS");
        var cpu = Find(sections, "CPU");

        var hostname = Value(cs, "DNSHostName") ?? Value(cs, "Name") ?? Environment.MachineName;
        var domain = Value(cs, "Domain") ?? "";
        var role = Int(cs, "DomainRole");
        var productType = Int(os, "ProductType");
        var manufacturer = Value(cs, "Manufacturer") ?? "";
        var model = Value(cs, "Model") ?? "";
        var serial = Value(bios, "SerialNumber") ?? "";
        var caption = Value(os, "Caption") ?? "";
        var arch = Value(os, "OSArchitecture") ?? "";
        var version = Value(os, "Version") ?? "";
        var build = Value(os, "BuildNumber") ?? "";
        var install = Date(os, "InstallDate");
        var boot = Date(os, "LastBootUpTime");
        var processor = Value(cpu, "Name")?.Trim() ?? "";
        var sockets = Int(cs, "NumberOfProcessors");
        var logical = Int(cs, "NumberOfLogicalProcessors");
        var memBytes = Long(cs, "TotalPhysicalMemory");
        var memGb = memBytes / 1024d / 1024d / 1024d;
        var user = SummaryFormatter.ShortUser(Value(cs, "UserName"));

        return SummaryFormatter.Format(
            hostname, domain, role, productType, manufacturer, model, serial,
            caption, arch, version, build, install, boot, processor,
            sockets, logical, memGb, user, now, inventoryLocation);
    }

    private static InventorySection Query(string name, string file, string wql, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            using var searcher = new ManagementObjectSearcher(wql);
            using var results = searcher.Get();
            var sb = new StringBuilder();
            foreach (ManagementObject mo in results)
            {
                using (mo)
                {
                    foreach (var prop in mo.Properties)
                    {
                        var val = prop.Value;
                        var text = val switch
                        {
                            null => "",
                            Array arr => string.Join(", ", arr.Cast<object>()),
                            _ => val.ToString() ?? ""
                        };
                        sb.Append(prop.Name.PadRight(42));
                        sb.Append(": ");
                        sb.AppendLine(text);
                    }
                    sb.AppendLine();
                }
            }
            return new InventorySection { Name = name, FileName = file, Content = sb.ToString() };
        }
        catch (Exception ex)
        {
            return new InventorySection
            {
                Name = name,
                FileName = file.Replace(".txt", "_ERROR.txt"),
                Content = ex.Message,
                IsError = true
            };
        }
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
        try
        {
            return ManagementDateTimeConverter.ToDateTime(raw);
        }
        catch
        {
            return DateTime.TryParse(raw, out var d) ? d : null;
        }
    }
}
