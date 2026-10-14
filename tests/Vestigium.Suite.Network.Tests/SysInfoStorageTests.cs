using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoStorageTests
{
    [Fact]
    public void Gb_formats_two_decimals()
    {
        Assert.Equal("1.00", SizeFormat.Gb(1024L * 1024 * 1024));
        Assert.Equal("512.00", SizeFormat.Gb(512L * 1024 * 1024 * 1024));
        Assert.Equal("0.50", SizeFormat.Gb(512L * 1024 * 1024));
    }

    [Fact]
    public void Gb_blank_on_missing_or_bad()
    {
        Assert.Equal("", SizeFormat.Gb(null));
        Assert.Equal("", SizeFormat.Gb("not-a-number"));
        Assert.Equal("", SizeFormat.Gb(-1));
    }

    [Fact]
    public void Percent_is_one_decimal()
    {
        Assert.Equal("50.0", SizeFormat.Percent(50, 100));
        Assert.Equal("38.4", SizeFormat.Percent(195754385408L, 510363955200L));
    }

    [Fact]
    public void Percent_blank_when_total_missing()
    {
        Assert.Equal("", SizeFormat.Percent(10, null));
        Assert.Equal("", SizeFormat.Percent(10, 0));
    }

    [Fact]
    public void Drive_type_3_is_fixed()
    {
        Assert.Equal("Fixed", SizeFormat.DriveTypeName(3));
        Assert.Equal("Removable", SizeFormat.DriveTypeName(2));
        Assert.Equal("Network", SizeFormat.DriveTypeName(4));
        Assert.Equal("Unknown", SizeFormat.DriveTypeName(0));
    }

    [Fact]
    public void Failures_lists_only_errored_sections()
    {
        var snap = new StorageSnapshot
        {
            DiskDrivesError = "access denied",
            PartitionsError = "namespace missing"
        };

        var failed = StorageCollector.Failures(snap);

        Assert.Equal(new[] { "DiskDrives", "Partitions" }, failed);
    }

    [Fact]
    public void Failures_empty_when_clean()
    {
        Assert.Empty(StorageCollector.Failures(new StorageSnapshot()));
    }
}
