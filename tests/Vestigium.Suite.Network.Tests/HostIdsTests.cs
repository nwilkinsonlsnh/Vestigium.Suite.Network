using Vestigium.Suite.Network.Shell;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class HostIdsTests
{
    [Fact]
    public void Host_appids_are_not_network()
    {
        Assert.Equal("PingIQ", HostIds.PingIQ);
        Assert.Equal("TraceIQ", HostIds.TraceIQ);
        Assert.Equal("DnsIQ", HostIds.DnsIQ);
        Assert.NotEqual("Network", HostIds.PingIQ);
    }
}
