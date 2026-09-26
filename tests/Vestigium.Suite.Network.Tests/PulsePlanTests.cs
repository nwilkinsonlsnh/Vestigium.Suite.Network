using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PulsePlanTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    [InlineData(-1)]
    public void Request_count_outside_range_rejects(int requests)
    {
        var ok = PulsePlan.TryCreate(requests, 60, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Requests must be 1–10000.", reject);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(601)]
    public void Seconds_outside_range_rejects(int seconds)
    {
        var ok = PulsePlan.TryCreate(1000, seconds, out var plan, out var reject);
        Assert.False(ok);
        Assert.Equal(default, plan);
        Assert.Equal("Seconds must be 1–600.", reject);
    }

    [Fact]
    public void One_request_has_zero_spacing()
    {
        Assert.True(PulsePlan.TryCreate(1, 60, out var plan, out var reject));
        Assert.Null(reject);
        Assert.Equal(1, plan.Requests);
        Assert.Equal(60, plan.Seconds);
        Assert.Equal(TimeSpan.Zero, plan.Spacing);
        Assert.Equal(TimeSpan.Zero, plan.DueAt(1));
    }

    [Fact]
    public void Thousand_requests_over_sixty_seconds_spaces_evenly()
    {
        Assert.True(PulsePlan.TryCreate(1000, 60, out var plan, out _));
        Assert.Equal(TimeSpan.Zero, plan.DueAt(1));
        Assert.Equal(60, plan.DueAt(1000).TotalSeconds);
        Assert.True(plan.DueAt(2) > TimeSpan.Zero);
        Assert.True(plan.DueAt(2) < plan.DueAt(1000));
    }

    [Fact]
    public void Truncates_decimal_inputs()
    {
        Assert.True(PulsePlan.TryCreate(1000.9m, 180.2m, out var plan, out _));
        Assert.Equal(1000, plan.Requests);
        Assert.Equal(180, plan.Seconds);
    }
}
