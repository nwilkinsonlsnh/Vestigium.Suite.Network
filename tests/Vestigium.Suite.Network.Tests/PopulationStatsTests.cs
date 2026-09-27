using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PopulationStatsTests
{
    [Fact]
    public void Format_uses_population_descriptors_from_full_series()
    {
        var rows = new[]
        {
            new ReplyRow(1, "Success", "127.0.0.1", 10, 64, 0, null),
            new ReplyRow(2, "Success", "127.0.0.1", 12, 64, 0, null),
            new ReplyRow(3, "TimedOut", null, 0, 0, null, null),
        };

        var rtts = PopulationStats.Rtts(rows);
        Assert.Equal([10m, 12m], rtts);
        var text = PopulationStats.Format(rtts);
        Assert.Contains("n=2", text);
        Assert.Contains("σp=", text);
        Assert.Contains("varp=", text);
        Assert.DoesNotContain("sent=", text);
    }
}
