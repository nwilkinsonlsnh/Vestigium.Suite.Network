using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PulsePlanTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    [InlineData(-1)]
    public void Burst_count_outside_1_to_60_rejects(int bursts)
    {
        var ok = PulsePlan.TryCreate(bursts, 10, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Bursts must be 1–60.", reject);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void Seconds_outside_1_to_60_rejects(int seconds)
    {
        var ok = PulsePlan.TryCreate(10, seconds, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Seconds must be 1–60.", reject);
    }

    [Fact]
    public void One_burst_has_zero_spacing()
    {
        Assert.True(PulsePlan.TryCreate(1, 10, out var plan, out var reject));
        Assert.Null(reject);
        Assert.Equal(1, plan.Bursts);
        Assert.Equal(10, plan.Seconds);
        Assert.Equal(TimeSpan.Zero, plan.Spacing);
        Assert.Equal(TimeSpan.Zero, plan.DueAt(1));
    }

    [Fact]
    public void Ten_bursts_over_ten_seconds_spaces_by_ten_ninths()
    {
        Assert.True(PulsePlan.TryCreate(10, 10, out var plan, out _));
        var expected = TimeSpan.FromSeconds(10d / 9d);
        Assert.Equal(expected, plan.Spacing);
        Assert.Equal(TimeSpan.Zero, plan.DueAt(1));
        Assert.Equal(expected, plan.DueAt(2));
        Assert.Equal(TimeSpan.FromSeconds(10), plan.DueAt(10));
    }

    [Fact]
    public void Truncates_decimal_inputs()
    {
        Assert.True(PulsePlan.TryCreate(10.9m, 5.2m, out var plan, out _));
        Assert.Equal(10, plan.Bursts);
        Assert.Equal(5, plan.Seconds);
    }
}
