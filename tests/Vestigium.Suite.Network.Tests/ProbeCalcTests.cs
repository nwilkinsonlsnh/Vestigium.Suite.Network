using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ProbeCalcTests
{
    [Fact]
    public void Default_300_over_30_is_about_a_tenth_second()
    {
        Assert.True(PulsePlan.TryCreate(300, 30, out var plan, out _));
        Assert.InRange(plan.Spacing.TotalSeconds, 0.10, 0.11);
        Assert.Equal(301, ProbeCalc.RequestsFromInterval(30, 0.1m));
        Assert.Equal(30, ProbeCalc.SecondsFromInterval(300, 0.1m));
    }
}
