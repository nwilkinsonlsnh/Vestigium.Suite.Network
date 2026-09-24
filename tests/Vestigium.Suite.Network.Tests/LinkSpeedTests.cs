using Vestigium.Suite.Network.NicIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class LinkSpeedTests
{
    [Fact]
    public void Gigabit_formats_as_gbps()
    {
        Assert.Equal("1.201 Gbps", LinkSpeed.Format(1_201_000_000));
        Assert.Equal("Gbps", LinkSpeed.ScaleFor(1_201_000_000).Unit);
    }

    [Fact]
    public void Megabit_formats_as_mbps()
        => Assert.Equal("100 Mbps", LinkSpeed.Format(100_000_000));

    [Fact]
    public void Series_picks_unit_from_the_fastest_sample()
    {
        var scale = LinkSpeed.ScaleFor([100_000_000, 1_201_000_000]);
        Assert.Equal("Gbps", scale.Unit);
        Assert.Equal(1.201d, LinkSpeed.ToUnit(1_201_000_000, scale.Divisor), 3);
    }
}
