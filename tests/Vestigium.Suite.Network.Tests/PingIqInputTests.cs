using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PingIqInputTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_target_rejects(string? target)
    {
        var ok = Try(out var query, out var reject, target: target);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Target is required.", reject);
    }

    [Fact]
    public void Loopback_count_four_delay_default_accepts()
    {
        var ok = Try(out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("127.0.0.1", query!.Target);
        Assert.Equal(4, query.Options.Count);
        Assert.Equal(TimeSpan.FromMilliseconds(PingIqInput.DefaultDelayMs), query.Options.Interval);
        Assert.Equal(TimeSpan.FromMilliseconds(PingIqInput.ReplyTimeoutMs), query.Options.Timeout);
        Assert.Equal(0, query.Options.InterfaceIndex);
        Assert.Null(query.Options.SourceAddress);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Count_outside_range_rejects(int count)
    {
        var ok = Try(out var query, out var reject, count: count);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Count must be 1–99.", reject);
    }

    [Fact]
    public void Count_one_accepts()
    {
        var ok = Try(out var query, out var reject, count: 1);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(1, query!.Options.Count);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(60001)]
    public void Delay_outside_range_rejects(int timeoutMs)
    {
        var ok = Try(out var query, out var reject, timeoutMs: timeoutMs);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Delay must be 10–60000 ms.", reject);
    }

    [Fact]
    public void Delay_ten_accepts()
    {
        var ok = Try(out var query, out var reject, timeoutMs: 10);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(TimeSpan.FromMilliseconds(10), query!.Options.Interval);
    }

    [Fact]
    public void Source_garbage_rejects()
    {
        var ok = Try(out var query, out var reject, source: "not-an-ip");
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Source must be an IPv4 or IPv6 address.", reject);
    }

    [Fact]
    public void Empty_source_accepts()
    {
        var ok = Try(out var query, out var reject, source: "");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Null(query!.Options.SourceAddress);
    }

    [Fact]
    public void Negative_interface_index_rejects()
    {
        var ok = Try(out var query, out var reject, index: -1);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Interface index cannot be negative.", reject);
    }

    [Fact]
    public void Zero_interface_index_accepts()
    {
        var ok = Try(out var query, out var reject, index: 0);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(0, query!.Options.InterfaceIndex);
    }

    [Fact]
    public void Count_ninety_nine_accepts()
    {
        var ok = Try(out var query, out var reject, count: 99);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(99, query!.Options.Count);
    }

    [Fact]
    public void Delay_sixty_seconds_accepts()
    {
        var ok = Try(out var query, out var reject, timeoutMs: 60000);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(TimeSpan.FromMilliseconds(60000), query!.Options.Interval);
    }

    [Fact]
    public void Source_ipv6_accepts()
    {
        var ok = Try(out var query, out var reject, source: "::1");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("::1", query!.Options.SourceAddress);
    }

    [Fact]
    public void Target_is_trimmed()
    {
        var ok = Try(out var query, out var reject, target: "  8.8.8.8  ");
        Assert.True(ok);
        Assert.Equal("8.8.8.8", query!.Target);
    }

    [Fact]
    public void Hostname_target_accepts()
    {
        var ok = Try(out var query, out var reject, target: "example.com");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("example.com", query!.Target);
    }

    private static bool Try(
        out PingIqQuery? query,
        out string? reject,
        string? target = "127.0.0.1",
        decimal count = 4,
        decimal timeoutMs = PingIqInput.DefaultDelayMs,
        int index = 0,
        string? source = null)
        => PingIqInput.TryCreate(target, count, timeoutMs, index, source, out query, out reject);
}
