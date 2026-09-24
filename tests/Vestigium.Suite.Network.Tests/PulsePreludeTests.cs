using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PulsePreludeTests
{
    [Fact]
    public void Failed_lookup_does_not_start_pulse()
    {
        Assert.False(PulsePrelude.MayStartPulse(lookupSucceeded: false));
    }

    [Fact]
    public void Successful_lookup_may_start_pulse()
    {
        Assert.True(PulsePrelude.MayStartPulse(lookupSucceeded: true));
    }
}
