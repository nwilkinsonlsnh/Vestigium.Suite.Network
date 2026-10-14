using System.Globalization;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class SecurityCollector
{
    private static readonly string[] CertStores = ["My", "Root", "CA", "TrustedPublisher"];

    public static SecuritySnapshot Collect(CancellationToken ct, Action<string>? progress = null)
    {
        var snap = new SecuritySnapshot();
        Run(progress, "Profiles", ct, () => CollectProfiles(snap, ct), msg => snap.ProfilesError = msg);
        Run(progress, "Rules", ct, () => CollectRules(snap, ct), msg => snap.RulesError = msg);
        Run(progress, "Certificates", ct, () => CollectCertificates(snap, ct), msg => snap.CertificatesError = msg);
        Run(progress, "Drivers", ct, () => CollectDrivers(snap, ct), msg => snap.DriversError = msg);
        return snap;
    }

    public static IReadOnlyList<string> Failures(SecuritySnapshot snap)
    {
        var list = new List<string>();
        if (!string.IsNullOrWhiteSpace(snap.ProfilesError)) list.Add("Profiles");
        if (!string.IsNullOrWhiteSpace(snap.RulesError)) list.Add("Rules");
        if (!string.IsNullOrWhiteSpace(snap.CertificatesError)) list.Add("Certificates");
        if (!string.IsNullOrWhiteSpace(snap.DriversError)) list.Add("Drivers");
        return list;
    }

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

    private static void CollectProfiles(SecuritySnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        foreach (var mo in QueryCim("SELECT * FROM MSFT_NetFirewallProfile"))
        {
            using (mo)
            {
                snap.Profiles.Add(new FirewallProfileRow
                {
                    Name = Str(mo, "Name"),
                    Enabled = Str(mo, "Enabled"),
                    DefaultInbound = Str(mo, "DefaultInboundAction"),
                    DefaultOutbound = Str(mo, "DefaultOutboundAction"),
                    NotifyOnListen = Str(mo, "NotifyOnListen"),
                    LogAllowed = Str(mo, "LogAllowed"),
                    LogBlocked = Str(mo, "LogBlocked")
                });
            }
        }
    }

    private static void CollectRules(SecuritySnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var count = 0;
        foreach (var mo in QueryCim("SELECT DisplayName, Enabled, Direction, Action, Profiles, Protocol FROM MSFT_NetFirewallRule"))
        {
            using (mo)
            {
                var enabled = Str(mo, "Enabled");
                if (!string.Equals(enabled, "1", StringComparison.Ordinal) && !string.Equals(enabled, "True", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (count >= SecuritySnapshot.RulesCap)
                {
                    snap.RulesTruncated = true;
                    break;
                }
                snap.Rules.Add(new FirewallRuleRow
                {
                    DisplayName = Str(mo, "DisplayName"),
                    Enabled = Str(mo, "Enabled"),
                    Direction = Str(mo, "Direction"),
                    Action = Str(mo, "Action"),
                    Profile = Str(mo, "Profiles"),
                    Protocol = Str(mo, "Protocol")
                });
                count++;
            }
        }
    }

    private static void CollectCertificates(SecuritySnapshot snap, CancellationToken ct)
    {
        var closed = new List<string>();
        foreach (var name in CertStores)
        {
            ct.ThrowIfCancellationRequested();
            using var store = new X509Store(name, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
            }
            catch (Exception)
            {
                closed.Add(name);
                continue;
            }

            foreach (var cert in store.Certificates)
            {
                snap.Certificates.Add(new CertificateRow
                {
                    Store = name,
                    Subject = cert.Subject,
                    Issuer = cert.Issuer,
                    Thumbprint = cert.Thumbprint,
                    NotAfter = cert.NotAfter.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    HasPrivateKey = cert.HasPrivateKey ? "Yes" : ""
                });
            }
        }

        if (closed.Count == CertStores.Length)
            throw new InvalidOperationException("all certificate stores closed: " + string.Join(", ", closed));
    }

    private static void CollectDrivers(SecuritySnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var count = 0;
        foreach (var mo in Query("SELECT DeviceName, DriverVersion, Manufacturer, IsSigned, Signer, InfName FROM Win32_PnPSignedDriver"))
        {
            using (mo)
            {
                if (count >= SecuritySnapshot.DriversCap)
                {
                    snap.DriversTruncated = true;
                    break;
                }
                snap.Drivers.Add(new SignedDriverRow
                {
                    DeviceName = Str(mo, "DeviceName"),
                    DriverVersion = Str(mo, "DriverVersion"),
                    Manufacturer = Str(mo, "Manufacturer"),
                    IsSigned = Str(mo, "IsSigned"),
                    Signer = Str(mo, "Signer"),
                    InfName = Str(mo, "InfName")
                });
                count++;
            }
        }
    }

    private static IEnumerable<ManagementObject> QueryCim(string wql)
    {
        var scope = new ManagementScope(@"\\.\root\StandardCimv2");
        scope.Connect();
        using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(wql));
        using var results = searcher.Get();
        foreach (ManagementObject mo in results)
            yield return mo;
    }

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
            var val = mo[name];
            return val switch
            {
                null => "",
                Array arr => string.Join(", ", arr.Cast<object>()),
                _ => val.ToString() ?? ""
            };
        }
        catch (Exception)
        {
            return "";
        }
    }
}
