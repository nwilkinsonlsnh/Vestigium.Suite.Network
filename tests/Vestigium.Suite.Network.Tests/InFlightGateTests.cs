using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class InFlightGateTests
{
    [Fact]
    public void Caps_are_the_pr07_ceilings()
    {
        Assert.Equal(32, MainViewModel.PulseInFlightCap);
        Assert.Equal(8, DetailsCheck.MaxInFlight);
    }

    [Fact]
    public async Task Wait_drops_completed_work_until_under_the_cap()
    {
        var inflight = new List<Task>
        {
            Task.CompletedTask,
            Task.CompletedTask,
            Task.CompletedTask
        };

        await InFlightGate.WaitAsync(inflight, 2, CancellationToken.None);

        Assert.Equal(1, inflight.Count);
    }

    [Fact]
    public async Task A_cancelled_wait_throws_and_does_not_fault_the_row()
    {
        var pending = new TaskCompletionSource();
        var inflight = new List<Task> { pending.Task };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => InFlightGate.WaitAsync(inflight, 1, cts.Token));

        Assert.Single(inflight);
        Assert.False(pending.Task.IsFaulted);
    }
}
