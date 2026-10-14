using System.Management;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class StorageCollector
{
    public static StorageSnapshot Collect(CancellationToken ct, Action<string>? progress = null)
    {
        var snap = new StorageSnapshot();
        Run(progress, "DiskDrives", ct, () => CollectDiskDrives(snap, ct), msg => snap.DiskDrivesError = msg);
        Run(progress, "LogicalDisks", ct, () => CollectLogicalDisks(snap, ct), msg => snap.LogicalDisksError = msg);
        Run(progress, "Partitions", ct, () => CollectPartitions(snap, ct), msg => snap.PartitionsError = msg);
        Run(progress, "Volumes", ct, () => CollectVolumes(snap, ct), msg => snap.VolumesError = msg);
        return snap;
    }

    public static IReadOnlyList<string> Failures(StorageSnapshot snap)
    {
        var list = new List<string>();
        if (!string.IsNullOrWhiteSpace(snap.DiskDrivesError)) list.Add("DiskDrives");
        if (!string.IsNullOrWhiteSpace(snap.LogicalDisksError)) list.Add("LogicalDisks");
        if (!string.IsNullOrWhiteSpace(snap.PartitionsError)) list.Add("Partitions");
        if (!string.IsNullOrWhiteSpace(snap.VolumesError)) list.Add("Volumes");
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

    private static void CollectDiskDrives(StorageSnapshot snap, CancellationToken ct)
    {
        foreach (var mo in Query("SELECT * FROM Win32_DiskDrive"))
        {
            ct.ThrowIfCancellationRequested();
            using (mo)
            {
                snap.DiskDrives.Add(new DiskDriveRow
                {
                    DeviceId = Str(mo, "DeviceID"),
                    Model = Str(mo, "Caption"),
                    InterfaceType = Str(mo, "InterfaceType"),
                    SizeGb = SizeFormat.Gb(mo["Size"]),
                    SerialNumber = Str(mo, "SerialNumber").Trim(),
                    Status = Str(mo, "Status"),
                    Partitions = Str(mo, "Partitions"),
                    MediaType = Str(mo, "MediaType")
                });
            }
        }
    }

    private static void CollectLogicalDisks(StorageSnapshot snap, CancellationToken ct)
    {
        foreach (var mo in Query("SELECT * FROM Win32_LogicalDisk"))
        {
            ct.ThrowIfCancellationRequested();
            using (mo)
            {
                snap.LogicalDisks.Add(new LogicalDiskRow
                {
                    DeviceId = Str(mo, "DeviceID"),
                    VolumeName = Str(mo, "VolumeName"),
                    FileSystem = Str(mo, "FileSystem"),
                    DriveType = SizeFormat.DriveTypeName(mo["DriveType"]),
                    SizeGb = SizeFormat.Gb(mo["Size"]),
                    FreeGb = SizeFormat.Gb(mo["FreeSpace"]),
                    FreePercent = SizeFormat.Percent(mo["FreeSpace"], mo["Size"]),
                    VolumeSerial = Str(mo, "VolumeSerialNumber")
                });
            }
        }
    }

    private static void CollectPartitions(StorageSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rows = TryStorageNamespace(
            "SELECT DiskNumber, PartitionNumber, DriveLetter, Size, Offset, GptType, IsBoot, IsSystem, IsHidden, OperationalStatus FROM MSFT_Partition",
            MapPartition);
        if (rows is null)
        {
            rows = new List<PartitionRow>();
            foreach (var mo in Query("SELECT * FROM Win32_DiskPartition"))
            {
                using (mo)
                {
                    rows.Add(new PartitionRow
                    {
                        DiskNumber = Str(mo, "DiskIndex"),
                        PartitionNumber = Str(mo, "Index"),
                        Type = Str(mo, "Type"),
                        SizeGb = SizeFormat.Gb(mo["Size"]),
                        OffsetGb = SizeFormat.Gb(mo["StartingOffset"]),
                        IsBoot = Str(mo, "BootPartition"),
                        IsSystem = Str(mo, "PrimaryPartition"),
                        Status = Str(mo, "Status")
                    });
                }
            }
        }
        snap.Partitions.AddRange(rows);
    }

    private static void CollectVolumes(StorageSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rows = TryStorageNamespace(
            "SELECT DriveLetter, FileSystemLabel, FileSystem, DriveType, Size, SizeRemaining, HealthStatus, OperationalStatus, Path FROM MSFT_Volume",
            MapVolume);
        if (rows is null)
        {
            rows = new List<VolumeRow>();
            foreach (var mo in Query("SELECT * FROM Win32_Volume"))
            {
                using (mo)
                {
                    rows.Add(new VolumeRow
                    {
                        DriveLetter = Str(mo, "DriveLetter"),
                        Label = Str(mo, "Label"),
                        FileSystem = Str(mo, "FileSystem"),
                        DriveType = Str(mo, "DriveType"),
                        SizeGb = SizeFormat.Gb(mo["Capacity"]),
                        FreeGb = SizeFormat.Gb(mo["FreeSpace"]),
                        Path = Str(mo, "DeviceID")
                    });
                }
            }
        }
        snap.Volumes.AddRange(rows);
    }

    private static PartitionRow MapPartition(ManagementObject mo) => new()
    {
        DiskNumber = Str(mo, "DiskNumber"),
        PartitionNumber = Str(mo, "PartitionNumber"),
        DriveLetter = Str(mo, "DriveLetter"),
        Type = Str(mo, "GptType"),
        SizeGb = SizeFormat.Gb(mo["Size"]),
        OffsetGb = SizeFormat.Gb(mo["Offset"]),
        IsBoot = Str(mo, "IsBoot"),
        IsSystem = Str(mo, "IsSystem"),
        IsHidden = Str(mo, "IsHidden"),
        Status = Str(mo, "OperationalStatus")
    };

    private static VolumeRow MapVolume(ManagementObject mo) => new()
    {
        DriveLetter = Str(mo, "DriveLetter"),
        Label = Str(mo, "FileSystemLabel"),
        FileSystem = Str(mo, "FileSystem"),
        DriveType = Str(mo, "DriveType"),
        SizeGb = SizeFormat.Gb(mo["Size"]),
        FreeGb = SizeFormat.Gb(mo["SizeRemaining"]),
        Health = Str(mo, "HealthStatus"),
        Status = Str(mo, "OperationalStatus"),
        Path = Str(mo, "Path")
    };

    private static List<T>? TryStorageNamespace<T>(string wql, Func<ManagementObject, T> map)
    {
        try
        {
            var scope = new ManagementScope(@"\\.\root\Microsoft\Windows\Storage");
            scope.Connect();
            using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(wql));
            using var results = searcher.Get();
            var list = new List<T>();
            foreach (ManagementObject mo in results)
            {
                using (mo)
                    list.Add(map(mo));
            }
            return list;
        }
        catch (Exception)
        {
            return null;
        }
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
