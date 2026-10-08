using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ReverseNameTests
{
    [Fact]
    public void Ipv4_is_in_addr_arpa()
    {
        Assert.True(ReverseName.TryPtr("203.0.113.8", out var name));
        Assert.Equal("8.113.0.203.in-addr.arpa", name);
    }

    [Fact]
    public void Ipv6_sample_is_ip6_arpa()
    {
        Assert.True(ReverseName.TryPtr("2600:9000:27d1:4000:7:951d:7a80:93a1", out var name));
        Assert.Equal("1.a.3.9.0.8.a.7.d.1.5.9.7.0.0.0.0.0.0.4.1.d.7.2.0.0.0.9.0.0.6.2.ip6.arpa", name);
    }

    [Fact]
    public void A_name_is_not_a_reverse_question()
    {
        Assert.False(ReverseName.TryPtr("edge.example", out var name));
        Assert.Equal("", name);
    }
}
