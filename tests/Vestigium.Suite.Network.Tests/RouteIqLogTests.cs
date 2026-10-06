using Vestigium.Logging;
using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqLogTests
{
    [Fact]
    public void Catalog_rows_match_the_story_and_skip_10075()
    {
        var options = new VestigiumLoggerOptions();
        var registered = RouteIqCatalog.Register(options);

        Assert.Equal(16, registered.Count);
        Assert.DoesNotContain(registered, row => row.EventId == 10075);
        Assert.DoesNotContain(registered, row => row.EventId < 10000);
        Assert.Equal(
            [
                RouteIqLog.HostStartedId,
                RouteIqLog.HostStoppedId,
                RouteIqLog.PrintRequestedId,
                RouteIqLog.PrintAppliedId,
                RouteIqLog.PrintFailedId,
                RouteIqLog.ProbeFinishedId,
                RouteIqLog.ProbeFailedId,
                RouteIqLog.ExportFinishedId,
                RouteIqLog.ExportFailedId,
                RouteIqLog.SettingsLoadedId,
                RouteIqLog.SettingsRejectedId,
                RouteIqLog.WatchStartedId,
                RouteIqLog.WatchStoppedId,
                RouteIqLog.WatchFailedId,
                RouteIqLog.FilterRejectedId,
                RouteIqLog.ClipboardFailedId
            ],
            registered.Select(row => row.EventId));
        Assert.All(registered, row =>
        {
            Assert.Equal(RouteIqCatalog.Category, row.Category);
            Assert.Equal(RouteIqCatalog.Subcategory, row.Subcategory);
            Assert.Equal("Vestigium.Suite.Network.RouteIQ.Events." + row.EventName, row.FullName);
            Assert.Equal(RouteIqLog.Message(row.EventId), row.Description);
            Assert.DoesNotContain('{', RouteIqLog.Message(row.EventId));
        });
    }

    [Fact]
    public void Applied_story_keeps_the_message_fixed()
    {
        var seen = Capture();
        try
        {
            RouteIqLog.PrintApplied("Neighbors", 4, 2);
        }
        finally
        {
            RouteIqLog.Sink = null;
        }

        var call = Assert.Single(seen);
        Assert.Equal("Story", call.Kind);
        Assert.Equal(RouteIqLog.PrintAppliedId, call.EventId);
        Assert.Equal(RouteIqLog.PrintAppliedMessage, call.Message);
        Assert.Equal("Neighbors", call.Properties!["source"]);
        Assert.Equal("4", call.Properties["count"]);
        Assert.Equal("2", call.Properties["generation"]);
        Assert.DoesNotContain(call.Properties.Values, value => value?.Contains(':') == true);
    }

    [Fact]
    public void Fail_writes_thrown_then_the_story_once()
    {
        var seen = Capture();
        try
        {
            RouteIqLog.Fail(new InvalidOperationException("neighbors down"), RouteIqLog.PrintFailedId, new Dictionary<string, string?> { ["source"] = "Neighbors" });
        }
        finally
        {
            RouteIqLog.Sink = null;
        }

        Assert.Equal(2, seen.Count);
        Assert.Equal("Thrown", seen[0].Kind);
        Assert.Equal(VestigiumStatus.Failed, seen[0].Status);
        Assert.IsType<InvalidOperationException>(seen[0].Exception);
        Assert.Equal("Story", seen[1].Kind);
        Assert.Equal(RouteIqLog.PrintFailedId, seen[1].EventId);
        Assert.Equal(RouteIqLog.PrintFailedMessage, seen[1].Message);
        Assert.Equal(VestigiumStatus.Failed, seen[1].Status);
    }

    [Fact]
    public void Cancel_is_not_a_failure()
    {
        var seen = Capture();
        try
        {
            var error = Assert.Throws<ArgumentException>(() => RouteIqLog.Fail(new OperationCanceledException(), RouteIqLog.WatchFailedId));
            Assert.Equal("exception", error.ParamName);
        }
        finally
        {
            RouteIqLog.Sink = null;
        }

        Assert.Empty(seen);
    }

    [Fact]
    public void Fail_rejects_a_null_exception_and_a_story_id()
    {
        Assert.Throws<ArgumentNullException>(() => RouteIqLog.Fail(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteIqLog.Fail(new InvalidOperationException("x"), RouteIqLog.FilterRejectedId));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteIqLog.PrintApplied("00:11:22:33:44:55", 1, 1));
        Assert.Throws<ArgumentException>(() => RouteIqLog.SettingsLoaded(@"C:\ProgramData\Vestigium\Settings\RouteIQ\settings.json"));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteIqLog.WatchStopped("done"));
    }

    private static List<RouteIqLogCall> Capture()
    {
        var seen = new List<RouteIqLogCall>();
        RouteIqLog.Sink = seen.Add;
        return seen;
    }
}
