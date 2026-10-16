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

    private static string Gb(ulong bytes)
        => bytes == 0 ? "" : (bytes / 1024d / 1024d / 1024d).ToString("F2", CultureInfo.InvariantCulture);

    private static string Percent(ulong free, ulong total)
        => total == 0 ? "" : (free * 100d / total).ToString("F1", CultureInfo.InvariantCulture);
}
