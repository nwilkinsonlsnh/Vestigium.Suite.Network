using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PingIqDashboardUnlockTests
{
    [Fact]
    public void Unlock_raises_availability()
    {
        var dash = new DashboardViewModel();
        bool? enabled = null;
        dash.DashboardAvailabilityChanged = value => enabled = value;
        dash.Unlock();
        Assert.True(enabled);
    }

    [Fact]
    public void ShowEcho_does_not_unlock()
    {
        var dash = new DashboardViewModel();
        var unlocked = false;
        dash.DashboardAvailabilityChanged = _ => unlocked = true;
        dash.ShowEcho([]);
        Assert.False(unlocked);
        Assert.False(dash.HasEchoData);
    }

    [Fact]
    public void ShowProbe_unlocks_even_when_samples_are_empty()
    {
        var dash = new DashboardViewModel();
        var unlocked = false;
        dash.DashboardAvailabilityChanged = _ => unlocked = true;
        dash.ShowProbe([]);
        Assert.True(unlocked);
        Assert.False(dash.HasProbeData);
    }
}
