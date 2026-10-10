using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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
    public bool ShowLegendLookup { get; set; } = true;
    public bool ShowLegendProbeRtt { get; set; } = true;
    public bool ShowLegendProbeDist { get; set; } = true;
    public bool ShowLegendProbeControl { get; set; } = true;
    public bool ExportLookup { get; set; } = true;
    public bool ExportCapture { get; set; } = true;
    public bool ExportProbe { get; set; } = true;
    public bool ExportMonitoring { get; set; } = true;
    public bool ExportOpenAfter { get; set; }
    public bool ExportOpenFolder { get; set; }
}

public sealed class DnsIqSettingsStore
{
    public const string AclOpenNote = "Settings folder is still writable by Users. The ACL was not replaced.";
    public const string AclFailedNote = "Settings folder ACL was not set.";

    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public DnsIqSettingsStore(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        FilePath = Path.Combine(rootDirectory, "settings.json");
    }

    public string RootDirectory { get; }

    public string FilePath { get; }

    public string? LastAclNote { get; private set; }

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
        LastAclNote = Protect(RootDirectory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Json));
    }

    public static string? Protect(string directory)
    {
        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            var info = new DirectoryInfo(directory);
            var security = info.GetAccessControl();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            var inherit = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                FileSystemRights.FullControl,
                inherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            var user = WindowsIdentity.GetCurrent().User;
            if (user is not null)
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    user,
                    FileSystemRights.Modify,
                    inherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
            }

            info.SetAccessControl(security);
            return GrantsUsersModify(directory) ? AclOpenNote : null;
        }
        catch (Exception)
        {
            return AclFailedNote;
        }
    }

    public static bool GrantsUsersModify(string directory)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        var security = new DirectoryInfo(directory).GetAccessControl();
        var users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
        foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
        {
            if (rule.AccessControlType != AccessControlType.Allow)
                continue;
            if (rule.IdentityReference is not SecurityIdentifier sid || sid != users)
                continue;
            if ((rule.FileSystemRights & (FileSystemRights.Modify | FileSystemRights.Write)) != 0)
                return true;
        }

        return false;
    }
}
