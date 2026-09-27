using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class HopEstimateTests
{
    [Fact]
    public void Ttl_57_from_64_is_seven_hops()
    {
        Assert.Equal(7, HopEstimate.FromTtl(57));
    }

    [Fact]
    public void Ttl_128_is_same_subnet()
    {
        Assert.Equal(0, HopEstimate.FromTtl(128));
    }

    [Fact]
    public void Ttl_zero_is_unknown()
    {
        Assert.Null(HopEstimate.FromTtl(0));
    }
}
