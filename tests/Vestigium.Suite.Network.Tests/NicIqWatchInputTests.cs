using Vestigium.Suite.Network.NicIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class NicIqWatchInputTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_adapter_rejects(string? key)
    {
        var ok = NicIqWatchInput.TryCreate(key, 10, out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("No adapter selected.", reject);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.4)]
    [InlineData(61)]
    [InlineData(90)]
    public void Duration_outside_range_rejects(decimal seconds)
    {
        var ok = NicIqWatchInput.TryCreate("{id}", seconds, out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Duration must be 1–60 seconds.", reject);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(60)]
    public void Duration_in_range_accepts(decimal seconds)
    {
        var ok = NicIqWatchInput.TryCreate("  {id}  ", seconds, out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("{id}", query!.AdapterKey);
        Assert.Equal(TimeSpan.FromSeconds((int)seconds), query.Duration);
    }
}
