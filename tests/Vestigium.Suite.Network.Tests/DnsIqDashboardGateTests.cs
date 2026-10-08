using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DnsIqDashboardGateTests
{
    [Fact]
    public void Lookup_chart_does_not_unlock()
    {
        var dash = new DashboardViewModel();
        var unlocked = false;
        dash.DashboardAvailabilityChanged = _ => unlocked = true;

        dash.ShowLookup([]);

        Assert.False(unlocked);
    }

    [Fact]
    public void Probe_chart_unlocks()
    {
        var dash = new DashboardViewModel();
        var unlocked = false;
        dash.DashboardAvailabilityChanged = enabled => unlocked = enabled;

        dash.ShowProbe([]);

        Assert.True(unlocked);
    }
}
