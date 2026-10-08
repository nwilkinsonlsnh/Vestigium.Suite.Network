using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class ReverseNameMappedTests
{
    [Fact]
    public void Mapped_ipv6_asks_the_ipv4_question()
    {
        Assert.True(ReverseName.TryPtr("::ffff:1.2.3.4", out var name));
        Assert.Equal("4.3.2.1.in-addr.arpa", name);
    }
}
