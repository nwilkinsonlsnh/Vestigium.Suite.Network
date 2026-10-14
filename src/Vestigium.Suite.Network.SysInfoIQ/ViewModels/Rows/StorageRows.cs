namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

public sealed class DiskDriveRow
{
    public string DeviceId { get; init; } = "";
    public string Model { get; init; } = "";
    public string InterfaceType { get; init; } = "";
    public string SizeGb { get; init; } = "";
    public string SerialNumber { get; init; } = "";
    public string Status { get; init; } = "";
    public string Partitions { get; init; } = "";
    public string MediaType { get; init; } = "";
}

public sealed class LogicalDiskRow
{
    public string DeviceId { get; init; } = "";
    public string VolumeName { get; init; } = "";
    public string FileSystem { get; init; } = "";
    public string DriveType { get; init; } = "";
    public string SizeGb { get; init; } = "";
    public string FreeGb { get; init; } = "";
    public string FreePercent { get; init; } = "";
    public string VolumeSerial { get; init; } = "";
}

public sealed class PartitionRow
{
    public string DiskNumber { get; init; } = "";
    public string PartitionNumber { get; init; } = "";
    public string Type { get; init; } = "";
    public string DriveLetter { get; init; } = "";
    public string SizeGb { get; init; } = "";
    public string OffsetGb { get; init; } = "";
    public string IsBoot { get; init; } = "";
    public string IsSystem { get; init; } = "";
    public string IsHidden { get; init; } = "";
    public string Status { get; init; } = "";
}

public sealed class VolumeRow
{
    public string DriveLetter { get; init; } = "";
    public string Label { get; init; } = "";
    public string FileSystem { get; init; } = "";
    public string DriveType { get; init; } = "";
    public string SizeGb { get; init; } = "";
    public string FreeGb { get; init; } = "";
    public string Health { get; init; } = "";
    public string Status { get; init; } = "";
    public string Path { get; init; } = "";
}

public sealed class StorageSnapshot
{
    public List<DiskDriveRow> DiskDrives { get; } = new();
    public List<LogicalDiskRow> LogicalDisks { get; } = new();
    public List<PartitionRow> Partitions { get; } = new();
    public List<VolumeRow> Volumes { get; } = new();
    public string? DiskDrivesError { get; set; }
    public string? LogicalDisksError { get; set; }
    public string? PartitionsError { get; set; }
    public string? VolumesError { get; set; }
}
