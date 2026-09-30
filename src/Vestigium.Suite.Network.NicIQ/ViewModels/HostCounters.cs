using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Machine-wide CPU and Memory PDH paths. Independent of the selected NIC.</summary>
internal static class HostCounters
{
    public const string ProcessorTime = "% Processor Time";
    public const string UserTime = "% User Time";
    public const string PrivilegedTime = "% Privileged Time";
    public const string AvailableMBytes = "Available MBytes";
    public const string CommittedBytes = "Committed Bytes";
    public const string CommittedPct = "% Committed Bytes In Use";
    public const string CommitLimit = "Commit Limit";
    public const string CacheBytes = "Cache Bytes";

    public static IReadOnlyList<CounterPath> Preferred { get; } =
    [
        new CounterPath("Processor Information", ProcessorTime, "_Total", "%"),
        new CounterPath("Processor Information", UserTime, "_Total", "%"),
        new CounterPath("Processor Information", PrivilegedTime, "_Total", "%"),
        new CounterPath("Processor", ProcessorTime, "_Total", "%"),
        new CounterPath("Processor", UserTime, "_Total", "%"),
        new CounterPath("Processor", PrivilegedTime, "_Total", "%"),
        new CounterPath("Memory", AvailableMBytes, string.Empty, "MB"),
        new CounterPath("Memory", CommittedBytes, string.Empty, "bytes"),
        new CounterPath("Memory", CommittedPct, string.Empty, "%"),
        new CounterPath("Memory", CommitLimit, string.Empty, "bytes"),
        new CounterPath("Memory", CacheBytes, string.Empty, "bytes")
    ];

    public static IReadOnlyList<CounterPath> ProcessorFallback { get; } =
    [
        new CounterPath("Processor", ProcessorTime, "_Total", "%"),
        new CounterPath("Processor", UserTime, "_Total", "%"),
        new CounterPath("Processor", PrivilegedTime, "_Total", "%")
    ];
}
