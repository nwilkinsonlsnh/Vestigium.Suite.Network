using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ProbeCalcTests
{
    [Fact]
    public void Three_hundred_over_five_thousand_ms()
    {
        Assert.Contains("5000 ms", ProbeCalc.Describe(300, 5000));
        Assert.Contains("300 requests", ProbeCalc.Describe(300, 5000));
    }
}
