using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DetailRowsTests
{
    [Fact]
    public void Mx_splits_preference_and_exchanger()
    {
        var selected = new AnswerRow("MX", "lab.example", "10 mail.lab.example", 300);
        var lines = DetailRows.FromLookup(selected, [selected]);
        Assert.Contains(lines, line => line.Category == "Preference" && line.Answer == "10");
        Assert.Contains(lines, line => line.Category == "Exchanger" && line.Answer == "mail.lab.example");
    }

    [Fact]
    public void Soa_splits_the_seven_fields()
    {
        var selected = new AnswerRow("SOA", "lab.example", "ns1.lab.example host.lab.example 2026100701 7200 3600 1209600 3600", 300);
        var lines = DetailRows.FromLookup(selected, [selected]);
        Assert.Contains(lines, line => line.Category == "Primary" && line.Answer == "ns1.lab.example");
        Assert.Contains(lines, line => line.Category == "Contact" && line.Answer == "host.lab.example");
        Assert.Contains(lines, line => line.Category == "Serial" && line.Answer == "2026100701");
        Assert.Contains(lines, line => line.Category == "Minimum" && line.Answer == "3600");
    }

    [Fact]
    public void An_address_answer_stays_a_line_the_window_can_reverse()
    {
        var selected = new AnswerRow("AAAA", "edge.example", "2600:9000:27d1:4000:7:951d:7a80:93a1", 60);
        var lines = DetailRows.FromLookup(selected, [selected]);
        Assert.Contains(lines, line => line.Answer == "2600:9000:27d1:4000:7:951d:7a80:93a1");
        Assert.True(ReverseName.TryPtr("2600:9000:27d1:4000:7:951d:7a80:93a1", out _));
    }
}
