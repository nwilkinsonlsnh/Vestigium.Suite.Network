using System.Collections.Concurrent;
using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqPrintCoordinatorTests
{
    [Fact]
    public async Task Request_starts_every_source_before_any_completes()
    {
        var started = 0;
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var progress = new ConcurrentBag<PrintProgress>();
        PrintSource source = async (_, _) =>
        {
            if (Interlocked.Increment(ref started) == PrintCoordinator.SourceCount)
                opened.TrySetResult();
            await release.Task;
        };

        var coordinator = Create(source, progress.Add);
        var run = coordinator.Request();
        await opened.Task;

        Assert.Equal(PrintCoordinator.SourceCount, Volatile.Read(ref started));
        Assert.False(run.IsCompleted);
        release.TrySetResult();
        await run;
        Assert.Equal(PrintCoordinator.SourceCount, progress.Count);
        Assert.All(progress, item => Assert.Equal(PrintCoordinator.SourceCount, item.Started));
        Assert.Contains(progress, item => item.Finished == PrintCoordinator.SourceCount);
    }

    [Fact]
    public async Task Faulted_source_does_not_cancel_the_others()
    {
        var applied = new ConcurrentBag<string>();
        var faults = new ConcurrentBag<string>();
        PrintSource Ok(string name) => (_, _) =>
        {
            applied.Add(name);
            return Task.CompletedTask;
        };

        var coordinator = new PrintCoordinator(
            Ok("IPv4 routes"),
            Ok("IPv6 routes"),
            (_, _) => throw new InvalidOperationException("neighbors down"),
            Ok("Connections"),
            Ok("NetBIOS"),
            Ok("LMHOSTS"),
            fault: (name, _) => faults.Add(name));

        await coordinator.Request();

        Assert.Equal(5, applied.Count);
        Assert.DoesNotContain("Neighbors", applied);
        Assert.Equal("Neighbors", Assert.Single(faults));
    }

    [Fact]
    public async Task Second_request_during_a_run_arms_one_follow_up()
    {
        var started = 0;
        var live = 0;
        var maxLive = 0;
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PrintSource source = async (_, _) =>
        {
            var now = Interlocked.Increment(ref live);
            int seen;
            do
            {
                seen = Volatile.Read(ref maxLive);
                if (now <= seen)
                    break;
            }
            while (Interlocked.CompareExchange(ref maxLive, now, seen) != seen);

            if (Interlocked.Increment(ref started) == PrintCoordinator.SourceCount)
                opened.TrySetResult();
            await release.Task;
            Interlocked.Decrement(ref live);
        };

        var coordinator = Create(source);
        var first = coordinator.Request();
        await opened.Task;
        var second = coordinator.Request();
        var third = coordinator.Request();

        Assert.Same(first, second);
        Assert.Same(first, third);
        release.TrySetResult();
        await first;

        Assert.Equal(PrintCoordinator.SourceCount * 2, started);
        Assert.Equal(PrintCoordinator.SourceCount, maxLive);
        Assert.Equal(2, coordinator.Generation);
    }

    [Fact]
    public async Task Stale_generation_is_not_current_after_the_next_run_starts()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PrintScope first = default;
        PrintSource slow = async (scope, _) =>
        {
            first = scope;
            entered.TrySetResult();
            await release.Task;
        };
        PrintSource done = (_, _) => Task.CompletedTask;

        var coordinator = new PrintCoordinator(slow, done, done, done, done, done);
        var run = coordinator.Request();
        await entered.Task;
        Assert.True(first.IsCurrent);
        release.TrySetResult();
        await run;

        await coordinator.Request();

        Assert.False(first.IsCurrent);
        Assert.False(coordinator.IsCurrent(first.Generation));
        Assert.True(coordinator.IsCurrent(coordinator.Generation));
        Assert.Equal(2, coordinator.Generation);
    }

    [Fact]
    public void Gated_sources_do_not_include_packed_oui()
    {
        Assert.Equal(6, PrintCoordinator.SourceCount);
        Assert.Equal(
            ["IPv4 routes", "IPv6 routes", "Neighbors", "Connections", "NetBIOS", "LMHOSTS"],
            PrintCoordinator.Names);
        Assert.DoesNotContain(PrintCoordinator.Names, name =>
            name.Contains("oui", StringComparison.OrdinalIgnoreCase)
            || name.Contains("vendor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Run_reports_one_start_and_the_follow_up_reports_another()
    {
        var starts = new ConcurrentBag<int>();
        var started = 0;
        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        PrintSource source = async (_, _) =>
        {
            if (Interlocked.Increment(ref started) == PrintCoordinator.SourceCount)
                opened.TrySetResult();
            await release.Task;
        };

        var coordinator = new PrintCoordinator(source, source, source, source, source, source, started: starts.Add);
        var run = coordinator.Request();
        await opened.Task;
        var follow = coordinator.Request();
        Assert.Same(run, follow);
        release.TrySetResult();
        await run;

        Assert.Equal(PrintCoordinator.SourceCount * 2, started);
        Assert.Equal([1, 2], starts.OrderBy(value => value));
    }


    private static PrintCoordinator Create(PrintSource source, Action<PrintProgress>? progress = null)
        => new(source, source, source, source, source, source, progress);
}
