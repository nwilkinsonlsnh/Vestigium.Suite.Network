using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class SoftwareCollector
{
    private static readonly (RegistryHive Hive, string View, string Location, string Arch)[] Opens =
    [
        (RegistryHive.LocalMachine, "", "LM", "x64"),
        (RegistryHive.LocalMachine, "WOW6432Node\\", "LM", "x86"),
        (RegistryHive.CurrentUser, "", "CU", "x64"),
        (RegistryHive.CurrentUser, "WOW6432Node\\", "CU", "x86")
    ];

    public static SoftwareSnapshot Collect(CancellationToken ct, Action<string>? progress = null)
    {
        var snap = new SoftwareSnapshot();
        Run(progress, "Installed", ct, () => CollectInstalled(snap, ct), msg => snap.InstalledError = msg);
        Run(progress, "Hotfixes", ct, () => CollectHotfixes(snap, ct), msg => snap.HotfixesError = msg);
        Run(progress, "Features", ct, () => CollectFeatures(snap, ct), msg => snap.FeaturesError = msg);
        Run(progress, "ODBC", ct, () => CollectOdbc(snap, ct), msg => snap.OdbcError = msg);
        Run(progress, "Environment", ct, () => CollectEnvironment(snap, ct), msg => snap.EnvironmentError = msg);
        Run(progress, "SQL", ct, () => CollectSql(snap, ct), msg => snap.SqlError = msg);
        return snap;
    }

    public static IReadOnlyList<string> Failures(SoftwareSnapshot snap)
    {
        var list = new List<string>();
        if (!string.IsNullOrWhiteSpace(snap.InstalledError)) list.Add("Installed");
        if (!string.IsNullOrWhiteSpace(snap.HotfixesError)) list.Add("Hotfixes");
        if (!string.IsNullOrWhiteSpace(snap.FeaturesError)) list.Add("Features");
        if (!string.IsNullOrWhiteSpace(snap.OdbcError)) list.Add("ODBC");
        if (!string.IsNullOrWhiteSpace(snap.EnvironmentError)) list.Add("Environment");
        if (!string.IsNullOrWhiteSpace(snap.SqlError)) list.Add("SQL");
        return list;
    }

    public static (string Location, string Arch) MapHive(RegistryHive hive, bool wow)
        => hive == RegistryHive.LocalMachine
            ? ("LM", wow ? "x86" : "x64")
            : ("CU", wow ? "x86" : "x64");

    private static void Run(Action<string>? progress, string name, CancellationToken ct, Action body, Action<string> onError)
    {
        progress?.Invoke(name);
        try
        {
            ct.ThrowIfCancellationRequested();
            body();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            onError(ex.Message);
        }
    }

    private static void CollectInstalled(SoftwareSnapshot snap, CancellationToken ct)
    {
        foreach (var (hive, view, location, arch) in Opens)
        {
            ct.ThrowIfCancellationRequested();
            var path = $"SOFTWARE\\{view}Microsoft\\Windows\\CurrentVersion\\Uninstall";
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = root.OpenSubKey(path);
            if (key is null)
                continue;
            foreach (var sub in key.GetSubKeyNames())
            {
                using var child = key.OpenSubKey(sub);
                if (child is null)
                    continue;
                var name = child.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                snap.Installed.Add(new InstalledSoftwareRow
                {
                    Location = location,
                    Arch = arch,
                    Name = name,
                    Version = child.GetValue("DisplayVersion")?.ToString() ?? "",
                    Publisher = child.GetValue("Publisher")?.ToString() ?? "",
                    InstallDate = child.GetValue("InstallDate")?.ToString() ?? ""
                });
            }
        }
    }

    private static void CollectHotfixes(SoftwareSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        foreach (var mo in Query("SELECT HotFixID, Description, InstalledOn, InstalledBy FROM Win32_QuickFixEngineering"))
        {
            using (mo)
            {
                snap.Hotfixes.Add(new HotfixRow
                {
                    HotFixId = Str(mo, "HotFixID"),
                    Description = Str(mo, "Description"),
                    InstalledOn = Str(mo, "InstalledOn"),
                    InstalledBy = Str(mo, "InstalledBy")
                });
            }
        }
    }

    private static void CollectFeatures(SoftwareSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var count = 0;
        foreach (var mo in Query("SELECT Name, InstallState FROM Win32_OptionalFeature"))
        {
            using (mo)
            {
                if (count >= SoftwareSnapshot.FeaturesCap)
                {
                    snap.FeaturesTruncated = true;
                    break;
                }
                snap.Features.Add(new FeatureRow
                {
                    Name = Str(mo, "Name"),
                    State = FeatureState(mo["InstallState"])
                });
                count++;
            }
        }
    }

    private static void CollectOdbc(SoftwareSnapshot snap, CancellationToken ct)
    {
        foreach (var (hive, view, location, arch) in Opens)
        {
            ct.ThrowIfCancellationRequested();
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            AddOdbc(snap, root, $"SOFTWARE\\{view}ODBC\\ODBCINST.INI", location, arch, "Driver");
            AddOdbc(snap, root, $"SOFTWARE\\{view}ODBC\\ODBC.INI", location, arch, "DSN");
        }
    }

    private static void AddOdbc(SoftwareSnapshot snap, RegistryKey root, string path, string location, string arch, string kind)
    {
        using var key = root.OpenSubKey(path);
        if (key is null)
            return;
        foreach (var name in key.GetSubKeyNames())
        {
            if (string.Equals(name, "ODBC Drivers", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "ODBC Data Sources", StringComparison.OrdinalIgnoreCase))
                continue;
            using var child = key.OpenSubKey(name);
            var detail = kind == "Driver"
                ? child?.GetValue("Driver")?.ToString() ?? ""
                : child?.GetValue("Driver")?.ToString() ?? "";
            snap.Odbc.Add(new OdbcRow
            {
                Location = location,
                Arch = arch,
                Kind = kind,
                Name = name,
                Detail = detail
            });
        }
    }

    private static void CollectEnvironment(SoftwareSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        AddEnv(snap, "Process", Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process));
        AddEnv(snap, "User", Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User));
        AddEnv(snap, "Machine", Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine));
    }

    private static void AddEnv(SoftwareSnapshot snap, string scope, System.Collections.IDictionary vars)
    {
        foreach (System.Collections.DictionaryEntry entry in vars)
        {
            snap.Environment.Add(new EnvironmentRow
            {
                Scope = scope,
                Name = entry.Key?.ToString() ?? "",
                Value = entry.Value?.ToString() ?? ""
            });
        }
    }

    private static void CollectSql(SoftwareSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        foreach (var svc in ServiceController.GetServices())
        {
            using (svc)
            {
                var name = svc.ServiceName;
                if (!name.StartsWith("MSSQL", StringComparison.OrdinalIgnoreCase) &&
                    !name.StartsWith("SQL", StringComparison.OrdinalIgnoreCase))
                    continue;
                snap.Sql.Add(new SqlServiceRow
                {
                    Name = name,
                    DisplayName = svc.DisplayName,
                    Status = svc.Status.ToString(),
                    StartType = svc.StartType.ToString()
                });
            }
        }
    }

    private static string FeatureState(object? value)
        => int.TryParse(value?.ToString(), out var n)
            ? n switch { 1 => "Enabled", 2 => "Disabled", 3 => "Absent", _ => n.ToString() }
            : value?.ToString() ?? "";

    private static IEnumerable<ManagementObject> Query(string wql)
    {
        using var searcher = new ManagementObjectSearcher(wql);
        using var results = searcher.Get();
        foreach (ManagementObject mo in results)
            yield return mo;
    }

    private static string Str(ManagementObject mo, string name)
    {
        try
        {
            return mo[name]?.ToString() ?? "";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
