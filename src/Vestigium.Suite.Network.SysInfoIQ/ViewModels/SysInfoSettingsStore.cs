using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed class SysInfoSettings
{
    public string? ThemeId { get; set; }
    public bool StatusBarVisible { get; set; } = true;
    public string StatusBarDock { get; set; } = "Bottom";
    public string? OutputFolder { get; set; }
}

public sealed class SysInfoSettingsStore
{
    public const string AclOpenNote = "Settings folder is still writable by Users. The ACL was not replaced.";
    public const string AclFailedNote = "Settings folder ACL was not set.";

    private static readonly JsonSerializerOptions Json =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public SysInfoSettingsStore(string rootDirectory)
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
        "SysInfoIQ");

    public SysInfoSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new SysInfoSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<SysInfoSettings>(json, Json) ?? new SysInfoSettings();
        }
        catch (Exception)
        {
            return new SysInfoSettings();
        }
    }

    public void Save(SysInfoSettings settings)
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
