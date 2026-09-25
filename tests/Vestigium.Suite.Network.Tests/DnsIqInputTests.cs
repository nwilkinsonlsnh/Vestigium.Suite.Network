using System.Net;
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
        var ok = Try(out var query, out var reject, name: name);
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Name is required.", reject);
    }

    [Fact]
    public void Localhost_with_empty_server_accepts()
    {
        var ok = Try(out var query, out var reject, name: "localhost", server: "");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("localhost", query!.Name);
        Assert.True(query.Options.Server is null || IPAddress.TryParse(query.Options.Server, out _));
        Assert.Equal(DnsRecordType.A, query.Options.Type);
        Assert.False(query.AllTypes);
        Assert.Equal(0, query.Options.InterfaceIndex);
        Assert.Null(query.Options.SourceAddress);
    }

    [Fact]
    public void Server_ipv4_accepts()
    {
        var ok = Try(out var query, out var reject, server: "8.8.8.8");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal("8.8.8.8", query!.Options.Server);
    }

    [Fact]
    public void Server_hostname_rejects()
    {
        var ok = Try(out var query, out var reject, server: "dns.google");
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Server must be an IPv4 or IPv6 address.", reject);
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
    public void Type_a_and_aaaa_accept()
    {
        Assert.True(Try(out var a, out _, type: "A"));
        Assert.Equal(DnsRecordType.A, a!.Options.Type);
        Assert.False(a.AllTypes);
        Assert.True(Try(out var aaaa, out _, type: "AAAA"));
        Assert.Equal(DnsRecordType.Aaaa, aaaa!.Options.Type);
    }

    [Fact]
    public void Type_all_accepts()
    {
        var ok = Try(out var query, out var reject, type: "All");
        Assert.True(ok);
        Assert.Null(reject);
        Assert.True(query!.AllTypes);
    }

    [Fact]
    public void Type_outside_the_eight_rejects()
    {
        var ok = Try(out var query, out var reject, type: "SRV");
        Assert.False(ok);
        Assert.Null(query);
        Assert.Equal("Type is not allowed.", reject);
    }

    private static bool Try(
        out DnsIqQuery? query,
        out string? reject,
        string? name = "localhost",
        string? server = "",
        string? type = "A",
        int index = 0,
        string? source = null)
        => DnsIqInput.TryCreate(name, server, type, index, source, out query, out reject);
}
