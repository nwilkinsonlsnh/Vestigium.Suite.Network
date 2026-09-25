using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DnsIqInputTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_name_rejects(string? name)
    {
        var ok = Try(name: name, out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Name is required.", reject);
    }

    [Fact]
    public void Localhost_with_empty_server_accepts()
    {
        var ok = Try(name: "localhost", server: "", out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("localhost", query!.Name);
        Assert.Null(query.Options.Server);
        Assert.Equal(DnsRecordType.A, query.Options.Type);
        Assert.False(query.AllTypes);
        Assert.Equal(0, query.Options.InterfaceIndex);
        Assert.Null(query.Options.SourceAddress);
    }

    [Fact]
    public void Server_ipv4_accepts()
    {
        var ok = Try(server: "8.8.8.8", out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("8.8.8.8", query!.Options.Server);
    }

    [Fact]
    public void Server_hostname_rejects()
    {
        var ok = Try(server: "dns.google", out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Server must be an IPv4 or IPv6 address.", reject);
    }

    [Fact]
    public void Source_garbage_rejects()
    {
        var ok = Try(source: "not-an-ip", out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Source must be an IPv4 or IPv6 address.", reject);
    }

    [Fact]
    public void Empty_source_accepts()
    {
        var ok = Try(source: "", out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Null(query!.Options.SourceAddress);
    }

    [Fact]
    public void Negative_interface_index_rejects()
    {
        var ok = Try(index: -1, out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Interface index cannot be negative.", reject);
    }

    [Fact]
    public void Zero_interface_index_accepts()
    {
        var ok = Try(index: 0, out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(0, query!.Options.InterfaceIndex);
    }

    [Fact]
    public void Type_a_and_aaaa_accept()
    {
        Assert.True(Try(type: "A", out var a, out _));
        Assert.Equal(DnsRecordType.A, a!.Options.Type);
        Assert.False(a.AllTypes);
        Assert.True(Try(type: "AAAA", out var aaaa, out _));
        Assert.Equal(DnsRecordType.Aaaa, aaaa!.Options.Type);
    }

    [Fact]
    public void Type_all_accepts()
    {
        var ok = Try(type: "All", out var query, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.True(query!.AllTypes);
    }

    [Fact]
    public void Type_outside_the_eight_rejects()
    {
        var ok = Try(type: "SRV", out var query, out var reject);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Type is not allowed.", reject);
    }

    private static bool Try(
        string? name = "localhost",
        string? server = "",
        string? type = "A",
        int index = 0,
        string? source = null,
        out DnsIqQuery? query,
        out string? reject)
        => DnsIqInput.TryCreate(name, server, type, index, source, out query, out reject);
}
