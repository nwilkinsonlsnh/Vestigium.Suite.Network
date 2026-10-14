using Microsoft.Win32;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoSoftwareTests
{
    [Fact]
    public void Features_cap_is_500()
    {
        Assert.Equal(500, SoftwareSnapshot.FeaturesCap);
    }

    [Theory]
    [InlineData(RegistryHive.LocalMachine, false, "LM", "x64")]
    [InlineData(RegistryHive.LocalMachine, true, "LM", "x86")]
    [InlineData(RegistryHive.CurrentUser, false, "CU", "x64")]
    [InlineData(RegistryHive.CurrentUser, true, "CU", "x86")]
    public void Hive_map_stamps_location_and_arch(RegistryHive hive, bool wow, string location, string arch)
    {
        var mapped = SoftwareCollector.MapHive(hive, wow);
        Assert.Equal(location, mapped.Location);
        Assert.Equal(arch, mapped.Arch);
    }

    [Fact]
    public void Snapshot_starts_clean()
    {
        var snap = new SoftwareSnapshot();
        Assert.Empty(snap.Installed);
        Assert.Empty(snap.Hotfixes);
        Assert.Empty(snap.Features);
        Assert.Empty(snap.Odbc);
        Assert.Empty(snap.Environment);
        Assert.Empty(snap.Sql);
        Assert.False(snap.FeaturesTruncated);
        Assert.Null(snap.InstalledError);
        Assert.Null(snap.OdbcError);
        Assert.Null(snap.SqlError);
    }

    [Fact]
    public void Failures_lists_only_errored_sections()
    {
        var snap = new SoftwareSnapshot
        {
            InstalledError = "access denied",
            OdbcError = "key missing"
        };

        var failed = SoftwareCollector.Failures(snap);

        Assert.Equal(new[] { "Installed", "ODBC" }, failed);
    }

    [Fact]
    public void Failures_empty_when_clean()
    {
        Assert.Empty(SoftwareCollector.Failures(new SoftwareSnapshot()));
    }

    [Fact]
    public void Row_shapes_hold_the_sample_fields()
    {
        var installed = new InstalledSoftwareRow { Location = "LM", Arch = "x64", Name = "SysInfoIQ", Version = "1.0" };
        var odbc = new OdbcRow { Location = "CU", Arch = "x86", Kind = "Driver", Name = "SQL Server" };
        var env = new EnvironmentRow { Scope = "Machine", Name = "Path", Value = "C:\\Windows" };
        var sql = new SqlServiceRow { Name = "MSSQLSERVER", Status = "Running", StartType = "Automatic" };

        Assert.Equal("LM", installed.Location);
        Assert.Equal("x64", installed.Arch);
        Assert.Equal("CU", odbc.Location);
        Assert.Equal("x86", odbc.Arch);
        Assert.Equal("Driver", odbc.Kind);
        Assert.Equal("Machine", env.Scope);
        Assert.Equal("MSSQLSERVER", sql.Name);
    }
}
