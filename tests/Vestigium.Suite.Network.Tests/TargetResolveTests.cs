using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class TargetResolveTests
{
    [Fact]
    public void Loopback_is_an_address()
    {
        Assert.True(TargetResolve.IsAddress("127.0.0.1"));
        Assert.True(TargetResolve.IsAddress("::1"));
        Assert.False(TargetResolve.IsAddress("cnn.com"));
    }

    [Fact]
    public void PickIp_takes_first_A()
    {
        var lookup = new DnsLookupResult(
            "cnn.com",
            DnsRecordType.A,
            null,
            DnsRcode.NoError,
            false,
            false,
            TimeSpan.Zero,
            [new DnsRecord(DnsRecordType.A, "cnn.com", 60, "151.101.1.67")]);
        Assert.Equal("151.101.1.67", TargetResolve.PickIp(lookup));
    }

    [Fact]
    public void PickIp_rejects_nxdomain()
    {
        var lookup = new DnsLookupResult(
            "nope.invalid",
            DnsRecordType.A,
            null,
            DnsRcode.NxDomain,
            false,
            false,
            TimeSpan.Zero,
            []);
        Assert.Null(TargetResolve.PickIp(lookup));
    }
}
