using Vestigium.Suite.Network.NicIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class NicPdhInstanceTests
{
    [Fact]
    public void Resolve_prefers_exact_description()
    {
        var hit = NicPdhInstance.Resolve("Ethernet", "Intel(R) Ethernet", ["Intel(R) Ethernet", "Wi-Fi"]);
        Assert.Equal("Intel(R) Ethernet", hit);
    }

    [Fact]
    public void Resolve_normalizes_slash_to_underscore()
    {
        var hit = NicPdhInstance.Resolve("Local Area Connection", "Intel / I225", ["Intel _ I225"]);
        Assert.Equal("Intel _ I225", hit);
    }

    [Fact]
    public void Resolve_skips_total_and_empty()
    {
        Assert.Null(NicPdhInstance.Resolve("Ethernet", "Ethernet", ["_Total"]));
        Assert.Null(NicPdhInstance.Resolve("Ethernet", "Ethernet", []));
    }
}

public sealed class NicPrimaryAdapterTests
{
    [Fact]
    public void Pick_uses_lowest_metric_up_addressed_adapter()
    {
        var slow = NicAdapterFactory.Up("slow", metric: 50, ipv4: true);
        var fast = NicAdapterFactory.Up("fast", metric: 10, ipv4: true);
        var down = NicAdapterFactory.Down("down", metric: 1);
        var picked = NicPrimaryAdapter.Pick([slow, fast, down]);
        Assert.Equal("fast", picked?.Id);
    }

    [Fact]
    public void Pick_keeps_preferred_when_still_up()
    {
        var a = NicAdapterFactory.Up("a", metric: 5, ipv4: true);
        var b = NicAdapterFactory.Up("b", metric: 25, ipv4: true);
        var picked = NicPrimaryAdapter.Pick([a, b], preferredId: "b");
        Assert.Equal("b", picked?.Id);
    }

    [Fact]
    public void Pick_keeps_preferred_when_still_on_the_box()
    {
        var up = NicAdapterFactory.Up("up", metric: 20, ipv4: true);
        var down = NicAdapterFactory.Down("down", metric: 1);
        var picked = NicPrimaryAdapter.Pick([up, down], preferredId: "down");
        Assert.Equal("down", picked?.Id);
    }
}

public sealed class MonitorCounterListTests
{
    [Fact]
    public void FromSettings_seeds_receive_send_when_list_empty()
    {
        var rows = MonitorCounterList.FromSettings(new NicIqSettings());
        Assert.Contains("Bytes Received/sec", rows);
        Assert.Contains("Bytes Sent/sec", rows);
        Assert.Contains("Bytes Total/sec", rows);
    }

    [Fact]
    public void FromSettings_uses_chart_need()
    {
        var rows = MonitorCounterList.FromSettings(new NicIqSettings
        {
            MonitorCounters = ["Bytes Sent/sec", "not-a-counter", "Bytes Sent/sec"]
        });
        Assert.Equal(MonitorCounterList.ChartNeed, rows);
    }

    [Fact]
    public void Sanitize_drops_unknown_and_dedupes()
    {
        var rows = MonitorCounterList.Sanitize(["Bytes Sent/sec", "not-a-counter", "Bytes Sent/sec"]);
        Assert.Equal(["Bytes Sent/sec"], rows);
    }

    [Fact]
    public void Available_omits_selected()
    {
        var left = MonitorCounterList.Available(["Bytes Sent/sec"]);
        Assert.DoesNotContain("Bytes Sent/sec", left);
        Assert.Contains("Current Bandwidth", left);
    }
}
