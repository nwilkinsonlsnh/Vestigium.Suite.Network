using System.Globalization;
using System.Net.NetworkInformation;
using Vestigium.Helpers.SystemInfo;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

/// <summary>Maps library inventory records to host rows. Host owns display strings and caps.</summary>
public static class InventoryMap
{
    public static StorageSnapshot ToStorage(StorageInventory inventory)
    {
        var snap = new StorageSnapshot();
        if (inventory.DiskDrives.IsOk)
        {
            foreach (var d in inventory.DiskDrives.Value)
            {
                snap.DiskDrives.Add(new DiskDriveRow
                {
                    DeviceId = d.DeviceId,
                    Model = d.Model,
                    InterfaceType = d.InterfaceType,
                    SizeGb = Gb(d.SizeBytes),
                    SerialNumber = d.SerialNumber,
                    Status = d.Status,
                    Partitions = d.Partitions.ToString(CultureInfo.InvariantCulture),
                    MediaType = d.MediaType
                });
            }
        }
        else
            snap.DiskDrivesError = "Unavailable";

        if (inventory.LogicalDisks.IsOk)
        {
            foreach (var d in inventory.LogicalDisks.Value)
            {
                snap.LogicalDisks.Add(new LogicalDiskRow
                {
                    DeviceId = d.DeviceId,
                    VolumeName = d.VolumeName,
                    FileSystem = d.FileSystem,
                    DriveType = SizeFormat.DriveTypeName(d.DriveType),
                    SizeGb = Gb(d.SizeBytes),
                    FreeGb = Gb(d.FreeBytes),
                    FreePercent = Percent(d.FreeBytes, d.SizeBytes),
                    VolumeSerial = d.VolumeSerial
                });
            }
        }
        else
            snap.LogicalDisksError = "Unavailable";

        if (inventory.Partitions.IsOk)
        {
            foreach (var p in inventory.Partitions.Value)
            {
                snap.Partitions.Add(new PartitionRow
                {
                    DiskNumber = p.DiskNumber.ToString(CultureInfo.InvariantCulture),
                    PartitionNumber = p.PartitionNumber.ToString(CultureInfo.InvariantCulture),
                    Type = p.Type,
                    DriveLetter = p.DriveLetter,
                    SizeGb = Gb(p.SizeBytes),
                    OffsetGb = Gb(p.OffsetBytes),
                    IsBoot = p.IsBoot ? "Yes" : "",
                    IsSystem = p.IsSystem ? "Yes" : "",
                    IsHidden = p.IsHidden ? "Yes" : "",
                    Status = p.Status
                });
            }
        }
        else
            snap.PartitionsError = "Unavailable";

        if (inventory.Volumes.IsOk)
        {
            foreach (var v in inventory.Volumes.Value)
            {
                snap.Volumes.Add(new VolumeRow
                {
                    DriveLetter = v.DriveLetter,
                    Label = v.Label,
                    FileSystem = v.FileSystem,
                    DriveType = SizeFormat.DriveTypeName(v.DriveType),
                    SizeGb = Gb(v.SizeBytes),
                    FreeGb = Gb(v.FreeBytes),
                    Health = v.Health,
                    Status = v.Status,
                    Path = v.Path
                });
            }
        }
        else
            snap.VolumesError = "Unavailable";

        return snap;
    }

    public static NetworkSnapshot ToNetwork(NetworkInventory inventory)
    {
        var snap = new NetworkSnapshot();
        if (inventory.Adapters.IsOk)
        {
            foreach (var a in inventory.Adapters.Value)
            {
                var addresses = string.Join(", ", a.Addresses);
                var dns = string.Join(", ", a.DnsServers);
                snap.Ip.Add(new IpConfigRow
                {
                    Name = a.Name,
                    Description = a.Description,
                    Status = a.Status,
                    Mac = a.Mac,
                    Addresses = addresses,
                    Gateways = string.Join(", ", a.Gateways),
                    Dns = dns,
                    Dhcp = a.DhcpEnabled ? "Yes" : ""
                });
                snap.Dns.Add(new DnsConfigRow { Name = a.Name, DnsServers = dns, Suffix = a.DnsSuffix });
                snap.Nics.Add(new NicRow
                {
                    Name = a.Name,
                    Description = a.Description,
                    Mac = a.Mac,
                    Status = a.Status,
                    SpeedMbps = a.SpeedBitsPerSecond > 0 ? (a.SpeedBitsPerSecond / 1_000_000).ToString(CultureInfo.InvariantCulture) : "",
                    Type = a.InterfaceType
                });
            }
        }

        if (inventory.Routes.IsOk)
        {
            foreach (var r in inventory.Routes.Value)
            {
                snap.Routes.Add(new RouteRow
                {
                    Destination = r.Destination,
                    Mask = r.Mask,
                    NextHop = r.NextHop,
                    InterfaceIndex = r.InterfaceIndex.ToString(CultureInfo.InvariantCulture),
                    Metric = r.Metric.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        if (inventory.Arp.IsOk)
        {
            var count = 0;
            foreach (var e in inventory.Arp.Value)
            {
                if (count >= NetworkSnapshot.ArpCap)
                {
                    snap.ArpTruncated = true;
                    break;
                }
                snap.Arp.Add(new ArpRow
                {
                    Address = e.Address,
                    Mac = e.Mac,
                    State = e.State.ToString(CultureInfo.InvariantCulture),
                    InterfaceIndex = e.InterfaceIndex.ToString(CultureInfo.InvariantCulture)
                });
                count++;
            }
        }

        if (inventory.Listeners.IsOk)
        {
            foreach (var l in inventory.Listeners.Value)
            {
                snap.Listeners.Add(new ListenerRow
                {
                    Protocol = l.Protocol,
                    LocalAddress = l.LocalAddress,
                    Port = l.Port.ToString(CultureInfo.InvariantCulture)
                });
            }
        }

        if (inventory.Tcp.IsOk)
        {
            var count = 0;
            foreach (var c in inventory.Tcp.Value)
            {
                if (count >= NetworkSnapshot.TcpCap)
                {
                    snap.TcpTruncated = true;
                    break;
                }
                snap.Tcp.Add(new TcpRow
                {
                    LocalAddress = c.LocalAddress,
                    LocalPort = c.LocalPort.ToString(CultureInfo.InvariantCulture),
                    RemoteAddress = c.RemoteAddress,
                    RemotePort = c.RemotePort.ToString(CultureInfo.InvariantCulture),
                    State = ((TcpState)c.State).ToString()
                });
                count++;
            }
        }

        return snap;
    }

    public static SecuritySnapshot ToSecurity(SecurityInventory inventory)
    {
        var snap = new SecuritySnapshot();
        if (inventory.Profiles.IsOk)
        {
            foreach (var p in inventory.Profiles.Value)
            {
                snap.Profiles.Add(new FirewallProfileRow
                {
                    Name = p.Name,
                    Enabled = EnabledLabel(p.Enabled),
                    DefaultInbound = ActionLabel(p.DefaultInboundAction),
                    DefaultOutbound = ActionLabel(p.DefaultOutboundAction),
                    NotifyOnListen = p.NotifyOnListen ? "Yes" : "",
                    LogAllowed = p.LogAllowed ? "Yes" : "",
                    LogBlocked = p.LogBlocked ? "Yes" : ""
                });
            }
        }
        else
            snap.ProfilesError = "Unavailable";

        if (inventory.Rules.IsOk)
        {
            var count = 0;
            foreach (var r in inventory.Rules.Value)
            {
                if (count >= SecuritySnapshot.RulesCap)
                {
                    snap.RulesTruncated = true;
                    break;
                }
                snap.Rules.Add(new FirewallRuleRow
                {
                    DisplayName = r.DisplayName,
                    Enabled = EnabledLabel(r.Enabled),
                    Direction = DirectionLabel(r.Direction),
                    Action = ActionLabel(r.Action),
                    Profile = ProfileLabel(r.Profiles),
                    Protocol = r.Protocol
                });
                count++;
            }
        }
        else
            snap.RulesError = "Unavailable";

        if (inventory.Certificates.IsOk)
        {
            foreach (var c in inventory.Certificates.Value)
            {
                snap.Certificates.Add(new CertificateRow
                {
                    Location = c.Location,
                    Store = c.Store,
                    Subject = c.Subject,
                    Issuer = c.Issuer,
                    Thumbprint = c.Thumbprint,
                    NotAfter = c.NotAfter.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    HasPrivateKey = c.HasPrivateKey ? "Yes" : ""
                });
            }
        }
        else
            snap.CertificatesError = "Unavailable";

        if (inventory.Drivers.IsOk)
        {
            var count = 0;
            foreach (var d in inventory.Drivers.Value)
            {
                if (count >= SecuritySnapshot.DriversCap)
                {
                    snap.DriversTruncated = true;
                    break;
                }
                snap.Drivers.Add(new SignedDriverRow
                {
                    Source = d.Source,
                    DeviceName = d.DeviceName,
                    DriverVersion = d.DriverVersion,
                    Manufacturer = d.Manufacturer,
                    IsSigned = d.IsSigned,
                    Signer = d.Signer,
                    InfName = d.InfName
                });
                count++;
            }
        }
        else
            snap.DriversError = "Unavailable";

        return snap;
    }

    public static SoftwareSnapshot ToSoftware(SoftwareInventory inventory)
    {
        var snap = new SoftwareSnapshot();
        if (inventory.Installed.IsOk)
        {
            foreach (var s in inventory.Installed.Value)
            {
                snap.Installed.Add(new InstalledSoftwareRow
                {
                    Location = s.Location,
                    Arch = s.Arch,
                    Name = s.Name,
                    Version = s.Version,
                    Publisher = s.Publisher,
                    InstallDate = s.InstallDate,
                    Hidden = s.Hidden ? "Yes" : "",
                    IsHidden = s.Hidden
                });
            }
        }
        else
            snap.InstalledError = "Unavailable";

        if (inventory.Hotfixes.IsOk)
        {
            foreach (var h in inventory.Hotfixes.Value)
            {
                snap.Hotfixes.Add(new HotfixRow
                {
                    HotFixId = h.HotFixId,
                    Description = h.Description,
                    InstalledOn = h.InstalledOn,
                    InstalledBy = h.InstalledBy
                });
            }
        }
        else
            snap.HotfixesError = "Unavailable";

        if (inventory.Features.IsOk)
        {
            var count = 0;
            foreach (var f in inventory.Features.Value)
            {
                if (count >= SoftwareSnapshot.FeaturesCap)
                {
                    snap.FeaturesTruncated = true;
                    break;
                }
                snap.Features.Add(new FeatureRow
                {
                    Name = f.Name,
                    State = FeatureState(f.InstallState)
                });
                count++;
            }
        }
        else
            snap.FeaturesError = "Unavailable";

        if (inventory.Odbc.IsOk)
        {
            foreach (var o in inventory.Odbc.Value)
            {
                snap.Odbc.Add(new OdbcRow
                {
                    Location = o.Location,
                    Arch = o.Arch,
                    Kind = o.Kind,
                    Name = o.Name,
                    Detail = o.Detail
                });
            }
        }
        else
            snap.OdbcError = "Unavailable";

        if (inventory.Environment.IsOk)
        {
            foreach (var e in inventory.Environment.Value)
            {
                snap.Environment.Add(new EnvironmentRow
                {
                    Scope = e.Scope,
                    Name = e.Name,
                    Value = e.Value
                });
            }
        }
        else
            snap.EnvironmentError = "Unavailable";

        if (inventory.Sql.IsOk)
        {
            foreach (var s in inventory.Sql.Value)
            {
                snap.Sql.Add(new SqlServiceRow
                {
                    Name = s.Name,
                    DisplayName = s.DisplayName,
                    Status = ((System.ServiceProcess.ServiceControllerStatus)s.Status).ToString(),
                    StartType = ((System.ServiceProcess.ServiceStartMode)s.StartType).ToString()
                });
            }
        }
        else
            snap.SqlError = "Unavailable";

        return snap;
    }

    private static string EnabledLabel(uint value)
        => value switch { 1 => "True", 2 => "False", _ => value.ToString(CultureInfo.InvariantCulture) };

    private static string DirectionLabel(uint value)
        => value switch { 1 => "Inbound", 2 => "Outbound", _ => value.ToString(CultureInfo.InvariantCulture) };

    private static string ActionLabel(uint value)
        => value switch { 2 => "Allow", 3 => "Block", 4 => "NotConfigured", _ => value.ToString(CultureInfo.InvariantCulture) };

    private static string ProfileLabel(uint bits)
    {
        if (bits == 0)
            return "";
        var names = new List<string>();
        if ((bits & 1) != 0) names.Add("Domain");
        if ((bits & 2) != 0) names.Add("Private");
        if ((bits & 4) != 0) names.Add("Public");
        return names.Count == 0 ? bits.ToString(CultureInfo.InvariantCulture) : string.Join(", ", names);
    }

    private static string FeatureState(uint value)
        => value switch { 1 => "Enabled", 2 => "Disabled", 3 => "Absent", _ => value.ToString(CultureInfo.InvariantCulture) };

    private static string Gb(ulong bytes)
        => bytes == 0 ? "" : (bytes / 1024d / 1024d / 1024d).ToString("F2", CultureInfo.InvariantCulture);

    private static string Percent(ulong free, ulong total)
        => total == 0 ? "" : (free * 100d / total).ToString("F1", CultureInfo.InvariantCulture);
}
