using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PingIqPulsePlanTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    [InlineData(-1)]
    public void Request_count_outside_range_rejects(int requests)
    {
        var ok = PulsePlan.TryCreate(requests, 5000, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Requests must be 1–10000.", reject);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(600001)]
    public void Duration_outside_range_rejects(int ms)
    {
        var ok = PulsePlan.TryCreate(300, ms, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Milliseconds must be 100–600000.", reject);
    }

    [Fact]
    public void Three_hundred_over_five_seconds_is_even()
    {
        Assert.True(PulsePlan.TryCreate(300, 5000, out var plan, out var reject));
        Assert.Null(reject);
        Assert.Equal(300, plan.Requests);
        Assert.Equal(5000, plan.DurationMs);
        Assert.Equal(TimeSpan.Zero, plan.DueAt(1));
        Assert.InRange(plan.DueAt(300).TotalMilliseconds, 4980, 5000);
    }
}
