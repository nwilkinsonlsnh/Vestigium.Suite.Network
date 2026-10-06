using Vestigium.Logging;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public static class RouteIqCatalog
{
    public const string Category = "RouteIQ";
    public const string Subcategory = "Host";

    public static IReadOnlyList<VestigiumEventDefinition> Register(VestigiumLoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var taxonomy = new VestigiumTaxonomy();
        taxonomy.Register(Category, Subcategory);
        options.RegisterTaxonomy(taxonomy);

        var registered = new VestigiumEventDefinition[Rows.Length];
        for (var i = 0; i < Rows.Length; i++)
        {
            var row = Rows[i];
            registered[i] = options.RegisterEvent(
                row.Name,
                "Vestigium.Suite.Network.RouteIQ.Events." + row.Name,
                Category,
                Subcategory,
                row.EventId,
                row.Severity,
                row.Message);
        }

        return registered;
    }

    internal static readonly CatalogRow[] Rows =
    [
        Row(RouteIqLog.HostStartedId, "HostStarted", "Information", RouteIqLog.HostStartedMessage),
        Row(RouteIqLog.HostStoppedId, "HostStopped", "Information", RouteIqLog.HostStoppedMessage),
        Row(RouteIqLog.PrintRequestedId, "PrintRequested", "Information", RouteIqLog.PrintRequestedMessage),
        Row(RouteIqLog.PrintAppliedId, "PrintApplied", "Information", RouteIqLog.PrintAppliedMessage),
        Row(RouteIqLog.PrintFailedId, "PrintSourceFailed", "Warning", RouteIqLog.PrintFailedMessage),
        Row(RouteIqLog.ProbeFinishedId, "ProbeFinished", "Information", RouteIqLog.ProbeFinishedMessage),
        Row(RouteIqLog.ProbeFailedId, "ProbeFailed", "Warning", RouteIqLog.ProbeFailedMessage),
        Row(RouteIqLog.ExportFinishedId, "ExportFinished", "Information", RouteIqLog.ExportFinishedMessage),
        Row(RouteIqLog.ExportFailedId, "ExportFailed", "Warning", RouteIqLog.ExportFailedMessage),
        Row(RouteIqLog.SettingsLoadedId, "SettingsLoaded", "Information", RouteIqLog.SettingsLoadedMessage),
        Row(RouteIqLog.SettingsRejectedId, "SettingsRejected", "Warning", RouteIqLog.SettingsRejectedMessage),
        Row(RouteIqLog.WatchStartedId, "WatchStarted", "Information", RouteIqLog.WatchStartedMessage),
        Row(RouteIqLog.WatchStoppedId, "WatchStopped", "Information", RouteIqLog.WatchStoppedMessage),
        Row(RouteIqLog.WatchFailedId, "WatchTickFailed", "Warning", RouteIqLog.WatchFailedMessage),
        Row(RouteIqLog.FilterRejectedId, "FilterRejected", "Warning", RouteIqLog.FilterRejectedMessage),
        Row(RouteIqLog.ClipboardFailedId, "ClipboardCopyFailed", "Warning", RouteIqLog.ClipboardFailedMessage)
    ];

    private static CatalogRow Row(int eventId, string name, string severity, string message)
        => new(eventId, name, severity, message);

    internal readonly record struct CatalogRow(int EventId, string Name, string Severity, string Message);
}
