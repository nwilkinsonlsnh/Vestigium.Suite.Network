using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DetailRowTests
{
    [Fact]
    public void An_address_starts_pending_with_the_arpa_question()
    {
        var row = new DetailRow("IPv4", "203.0.113.8");

        Assert.Equal("Pending", row.Check);
        Assert.Equal("8.113.0.203.in-addr.arpa", row.Question);
    }

    [Fact]
    public void A_name_starts_with_no_question()
    {
        var row = new DetailRow("FQDN", "edge.example");

        Assert.Equal("", row.Check);
        Assert.Equal("", row.Question);
    }
}
