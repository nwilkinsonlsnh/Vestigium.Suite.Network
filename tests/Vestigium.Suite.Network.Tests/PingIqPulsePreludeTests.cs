using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PingIqPulsePreludeTests
{
    [Fact]
    public void Failed_echo_does_not_start_pulse()
    {
        Assert.False(PulsePrelude.MayStartPulse(echoSucceeded: false));
    }

    [Fact]
    public void Successful_echo_may_start_pulse()
    {
        Assert.True(PulsePrelude.MayStartPulse(echoSucceeded: true));
    }
}
