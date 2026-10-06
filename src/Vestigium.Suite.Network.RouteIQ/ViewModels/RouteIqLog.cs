using Vestigium.Logging;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public static class RouteIqLog
{
    public const int HostStartedId = 10000;
    public const int HostStoppedId = 10005;
    public const int PrintRequestedId = 10010;
    public const int PrintAppliedId = 10015;
    public const int PrintFailedId = 10020;
    public const int ProbeFinishedId = 10025;
    public const int ProbeFailedId = 10030;
    public const int ExportFinishedId = 10035;
    public const int ExportFailedId = 10040;
    public const int SettingsLoadedId = 10045;
    public const int SettingsRejectedId = 10050;
    public const int WatchStartedId = 10055;
    public const int WatchStoppedId = 10060;
    public const int WatchFailedId = 10065;
    public const int FilterRejectedId = 10070;
    public const int ClipboardFailedId = 10080;

    public const string HostStartedMessage = "Host started";
    public const string HostStoppedMessage = "Host stopped";
    public const string PrintRequestedMessage = "Print requested";
    public const string PrintAppliedMessage = "Print source applied";
    public const string PrintFailedMessage = "Print source failed";
    public const string ProbeFinishedMessage = "Probe finished";
    public const string ProbeFailedMessage = "Probe failed";
    public const string ExportFinishedMessage = "Export finished";
    public const string ExportFailedMessage = "Export failed";
    public const string SettingsLoadedMessage = "Settings loaded";
    public const string SettingsRejectedMessage = "Settings rejected";
    public const string WatchStartedMessage = "Watch started";
    public const string WatchStoppedMessage = "Watch stopped";
    public const string WatchFailedMessage = "Watch tick failed";
    public const string FilterRejectedMessage = "Filter rejected";
    public const string ClipboardFailedMessage = "Clipboard copy failed";

    public const string VendorSource = "vendor";

    internal static Action<RouteIqLogCall>? Sink { get; set; }

    public static void HostStarted()
        => Story(HostStartedId, VestigiumLogLevel.Information, VestigiumStatus.None, HostStartedMessage, null);

    public static void HostStopped()
        => Story(HostStoppedId, VestigiumLogLevel.Information, VestigiumStatus.None, HostStoppedMessage, null);

    public static void PrintRequested(int generation)
        => Story(PrintRequestedId, VestigiumLogLevel.Information, VestigiumStatus.None, PrintRequestedMessage, Generation(generation));

    public static void PrintApplied(string source, int count, int generation)
        => Story(PrintAppliedId, VestigiumLogLevel.Information, VestigiumStatus.None, PrintAppliedMessage, Applied(source, count, generation));

    public static void ProbeFinished(bool hit)
        => Story(ProbeFinishedId, VestigiumLogLevel.Information, VestigiumStatus.None, ProbeFinishedMessage, Bag("result", hit ? "hit" : "miss"));

    public static void ExportFinished(int sheets)
        => Story(ExportFinishedId, VestigiumLogLevel.Information, VestigiumStatus.None, ExportFinishedMessage, Bag("sheets", sheets.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    public static void SettingsLoaded(string fileName)
        => Story(SettingsLoadedId, VestigiumLogLevel.Information, VestigiumStatus.None, SettingsLoadedMessage, Bag("path", FileName(fileName)));

    public static void WatchStarted(int seconds)
        => Story(WatchStartedId, VestigiumLogLevel.Information, VestigiumStatus.None, WatchStartedMessage, Bag("seconds", seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    public static void WatchStopped(string reason)
        => Story(WatchStoppedId, VestigiumLogLevel.Information, VestigiumStatus.None, WatchStoppedMessage, Bag("reason", StopReason(reason)));

    public static void FilterRejected()
        => Story(FilterRejectedId, VestigiumLogLevel.Warning, VestigiumStatus.None, FilterRejectedMessage, null);

    public static void Fail(Exception exception)
    {
        RejectCancel(exception);
        WriteThrown(exception);
    }

    public static void Fail(Exception exception, int storyId, IReadOnlyDictionary<string, string?>? properties = null)
    {
        RejectCancel(exception);
        if (!IsFailureStory(storyId))
            throw new ArgumentOutOfRangeException(nameof(storyId), storyId, "Fail requires a failure story id.");

        WriteThrown(exception);
        Story(storyId, VestigiumLogLevel.Warning, VestigiumStatus.Failed, Message(storyId), properties);
    }

    internal static string Message(int storyId) => storyId switch
    {
        HostStartedId => HostStartedMessage,
        HostStoppedId => HostStoppedMessage,
        PrintRequestedId => PrintRequestedMessage,
        PrintAppliedId => PrintAppliedMessage,
        PrintFailedId => PrintFailedMessage,
        ProbeFinishedId => ProbeFinishedMessage,
        ProbeFailedId => ProbeFailedMessage,
        ExportFinishedId => ExportFinishedMessage,
        ExportFailedId => ExportFailedMessage,
        SettingsLoadedId => SettingsLoadedMessage,
        SettingsRejectedId => SettingsRejectedMessage,
        WatchStartedId => WatchStartedMessage,
        WatchStoppedId => WatchStoppedMessage,
        WatchFailedId => WatchFailedMessage,
        FilterRejectedId => FilterRejectedMessage,
        ClipboardFailedId => ClipboardFailedMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(storyId), storyId, "Story id is not in the RouteIQ catalog.")
    };

    private static void RejectCancel(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        if (exception is OperationCanceledException)
            throw new ArgumentException("Cancel is not a failure.", nameof(exception));
    }

    private static bool IsFailureStory(int storyId) => storyId is PrintFailedId or ProbeFailedId or ExportFailedId or SettingsRejectedId or WatchFailedId or ClipboardFailedId;

    private static void WriteThrown(Exception exception)
    {
        if (Sink is { } sink)
        {
            sink(new RouteIqLogCall("Thrown", 0, exception.Message, VestigiumStatus.Failed, exception, null));
            return;
        }

        VestigiumLog.Thrown(exception, VestigiumStatus.Failed);
    }

    private static void Story(int eventId, VestigiumLogLevel level, VestigiumStatus status, string message, IReadOnlyDictionary<string, string?>? properties)
    {
        if (Sink is { } sink)
        {
            sink(new RouteIqLogCall("Story", eventId, message, status, null, properties));
            return;
        }

        VestigiumLog.Write(eventId, level, status, RouteIqCatalog.Category, RouteIqCatalog.Subcategory, message, properties: properties);
    }

    private static Dictionary<string, string?> Generation(int generation)
        => Bag("generation", generation.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static Dictionary<string, string?> Applied(string source, int count, int generation)
    {
        var bag = Generation(generation);
        bag["source"] = Source(source);
        bag["count"] = count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return bag;
    }

    private static Dictionary<string, string?> Bag(string key, string value)
        => new(StringComparer.Ordinal) { [key] = value };

    private static string Source(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (string.Equals(source, VendorSource, StringComparison.Ordinal))
            return VendorSource;
        if (PrintCoordinator.Names.Contains(source, StringComparer.Ordinal))
            return source;
        throw new ArgumentOutOfRangeException(nameof(source), source, "Print source is not a story source.");
    }

    private static string FileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name) || name != fileName.Trim())
            throw new ArgumentException("Settings path must be the file name.", nameof(fileName));
        return name;
    }

    private static string StopReason(string reason)
    {
        if (reason is "operator" or "cancel")
            return reason;
        throw new ArgumentOutOfRangeException(nameof(reason), reason, "Watch stop reason must be operator or cancel.");
    }
}

internal readonly record struct RouteIqLogCall(
    string Kind,
    int EventId,
    string Message,
    VestigiumStatus Status,
    Exception? Exception,
    IReadOnlyDictionary<string, string?>? Properties);
