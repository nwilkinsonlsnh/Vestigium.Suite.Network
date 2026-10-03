using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqInputTests
{
    [Theory]
    [InlineData("All", RouteFamily.All)]
    [InlineData("IPv4", RouteFamily.Pv4)]
    [InlineData("IPv6", RouteFamily.Pv6)]
    [InlineData("  IPv4  ", RouteFamily.Pv4)]
    public void Family_maps_to_library_enum(string label, RouteFamily expected)
    {
        var ok = RouteIqInput.TryMapFamily(label, out var family, out var reason);
        Assert.True(ok);
        Assert.Null(reason);
        Assert.Equal(expected, family);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Pv4")]
    [InlineData("ipv4")]
    public void Unknown_family_rejects(string? label)
    {
        var ok = RouteIqInput.TryMapFamily(label, out _, out var reason);
        Assert.False(ok);
        Assert.Equal("Family must be All, IPv4, or IPv6.", reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_probe_is_not_a_parse_success(string? text)
    {
        var ok = RouteIqInput.TryParseProbe(text, out var address, out var reason);
        Assert.False(ok);
        Assert.Null(address);
        Assert.Null(reason);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("::1")]
    [InlineData("  127.0.0.1  ")]
    public void Ip_probe_parses(string text)
    {
        var ok = RouteIqInput.TryParseProbe(text, out var address, out var reason);
        Assert.True(ok);
        Assert.Null(reason);
        Assert.NotNull(address);
        Assert.Equal(text.Trim(), address!.ToString());
    }

    [Theory]
    [InlineData("not-an-ip")]
    [InlineData("dns.google")]
    public void Non_ip_probe_rejects(string text)
    {
        var ok = RouteIqInput.TryParseProbe(text, out var address, out var reason);
        Assert.False(ok);
        Assert.Null(address);
        Assert.Equal("Address must be a single IPv4 or IPv6 address.", reason);
    }
}
