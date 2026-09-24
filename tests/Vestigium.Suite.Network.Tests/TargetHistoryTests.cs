using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class TargetHistoryTests
{
    [Fact]
    public void Last_used_moves_to_front_and_dedupes()
    {
        var next = TargetHistory.Remember(["cnn.com", "127.0.0.1"], "CNN.com", 10);
        Assert.Equal(["CNN.com", "127.0.0.1"], next);
    }

    [Fact]
    public void Cap_trims_oldest()
    {
        var next = TargetHistory.Remember(["a", "b", "c"], "d", 3);
        Assert.Equal(["d", "a", "b"], next);
    }

    [Fact]
    public void Blank_does_not_change_order()
    {
        var next = TargetHistory.Remember(["127.0.0.1"], "  ", 10);
        Assert.Equal(["127.0.0.1"], next);
    }
}
