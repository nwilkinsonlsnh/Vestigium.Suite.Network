using Vestigium.Helpers.LogParser;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class CaptureLinesTests
{
    [Fact]
    public void A_host_with_three_answers_is_three_lines()
    {
        var host = Row("edge.example", address: false, "Resolved", "1.1.1.1", "2600:9000:27d1:4000:7:951d:7a80:93a1", "8.8.8.8");
        var lines = CaptureLines.From(host);
        Assert.Equal(3, lines.Count);
        Assert.Equal(["IPv4", "IPv6", "IPv4"], lines.Select(line => line.Category).ToArray());
        Assert.Equal("2600:9000:27d1:4000:7:951d:7a80:93a1", lines[1].Answer);
        Assert.All(lines, line => Assert.Equal("edge.example", line.Host));
    }

    [Fact]
    public void A_host_with_no_answers_keeps_one_line()
    {
        var lines = CaptureLines.From(Row("missing.example", address: false, "NxDomain"));
        Assert.Single(lines);
        Assert.Equal("", lines[0].Category);
        Assert.Equal("", lines[0].Answer);
        Assert.Equal("NxDomain", lines[0].Dns);
    }

    [Fact]
    public void An_address_host_is_one_address_line()
    {
        var lines = CaptureLines.From(Row("203.0.113.8", address: true, "Skipped"));
        Assert.Single(lines);
        Assert.Equal("Address", lines[0].Category);
        Assert.Equal("203.0.113.8", lines[0].Answer);
        Assert.Equal("Skipped", lines[0].Dns);
    }

    private static HarHostRow Row(string host, bool address, string dns, params string[] answers)
    {
        var row = new HarHostRow(new LogHost(host, [], 1, default, address));
        row.Dns = dns;
        row.SetAnswerItems(answers);
        return row;
    }
}
