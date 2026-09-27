using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ProbeCalcTests
{
    [Fact]
    public void Three_hundred_over_thirty_is_ten_per_second()
    {
        Assert.True(PulsePlan.TryCreate(300, 30, out var plan, out _));
        Assert.Equal(100, plan.Spacing.TotalMilliseconds, 3);
        Assert.Equal(300, ProbeCalc.RequestsFromInterval(30, 100));
        Assert.Equal(30, ProbeCalc.SecondsFromInterval(300, 100));
        Assert.Equal(100, ProbeCalc.IntervalMs(300, 30));
    }
}
