using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoSecurityTests
{
    [Fact]
    public void Rules_cap_is_1000()
    {
        Assert.Equal(1000, SecuritySnapshot.RulesCap);
    }

    [Fact]
    public void Drivers_cap_is_500()
    {
        Assert.Equal(5000, SecuritySnapshot.DriversCap);
    }

    [Fact]
    public void Snapshot_starts_clean()
    {
        var snap = new SecuritySnapshot();
        Assert.Empty(snap.Profiles);
        Assert.Empty(snap.Rules);
        Assert.Empty(snap.Certificates);
        Assert.Empty(snap.Drivers);
        Assert.False(snap.RulesTruncated);
        Assert.False(snap.DriversTruncated);
        Assert.Null(snap.ProfilesError);
        Assert.Null(snap.RulesError);
        Assert.Null(snap.CertificatesError);
        Assert.Null(snap.DriversError);
    }

    [Fact]
    public void Failures_lists_only_errored_sections()
    {
        var snap = new SecuritySnapshot
        {
            ProfilesError = "access denied",
            DriversError = "namespace missing"
        };

        var failed = SecurityCollector.Failures(snap);

        Assert.Equal(new[] { "Profiles", "Drivers" }, failed);
    }

    [Fact]
    public void Failures_empty_when_clean()
    {
        Assert.Empty(SecurityCollector.Failures(new SecuritySnapshot()));
    }

    [Fact]
    public void Row_shapes_hold_the_sample_fields()
    {
        var profile = new FirewallProfileRow { Name = "Domain", Enabled = "True", DefaultInbound = "NotConfigured" };
        var rule = new FirewallRuleRow { DisplayName = "Core Networking", Enabled = "True", Direction = "Inbound", Action = "Allow" };
        var cert = new CertificateRow { Location = "LM", Store = "My", Subject = "CN=device", Thumbprint = "744629296696D0ADF96CF8507EFE0306EF50A226", HasPrivateKey = "Yes" };
        var driver = new SignedDriverRow { DeviceName = "Audio Endpoint", DriverVersion = "10.0.26100.1", IsSigned = "True", Signer = "Microsoft" };

        Assert.Equal("Domain", profile.Name);
        Assert.Equal("Allow", rule.Action);
        Assert.Equal("LM", cert.Location);
        Assert.Equal("My", cert.Store);
        Assert.Equal("Microsoft", driver.Signer);
    }
}
