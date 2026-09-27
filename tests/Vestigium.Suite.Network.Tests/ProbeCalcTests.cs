using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ProbeCalcTests
{
    [Fact]
    public void Default_300_over_30_is_about_100ms()
    {
        Assert.True(PulsePlan.TryCreate(300, 30, out var plan, out _));
        Assert.InRange(plan.Spacing.TotalMilliseconds, 100.0, 101.0);
        Assert.Equal(301, ProbeCalc.RequestsFromInterval(30, 100));
        Assert.Equal(30, ProbeCalc.SecondsFromInterval(300, 100));
    }
}
