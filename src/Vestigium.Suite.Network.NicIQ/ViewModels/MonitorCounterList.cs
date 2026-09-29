using Vestigium.Helpers.PerfMon.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Selected Network Interface counters. Bool knobs from v1.0 seed the first list.</summary>
public static class MonitorCounterList
{
    public static readonly string[] SeedReceiveSend =
    [
        NetworkInterface.BytesReceivedPerSec,
        NetworkInterface.BytesSentPerSec,
        NetworkInterface.BytesTotalPerSec
    ];

    public static IReadOnlyList<string> FromSettings(NicIqSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.MonitorCounters is { Count: > 0 })
            return Sanitize(settings.MonitorCounters);

        var rows = new List<string>();
        if (settings.MonitorReceive || settings.MonitorSend)
            rows.AddRange(SeedReceiveSend);
        else
        {
            if (settings.MonitorReceive)
                rows.Add(NetworkInterface.BytesReceivedPerSec);
            if (settings.MonitorSend)
                rows.Add(NetworkInterface.BytesSentPerSec);
        }

        if (settings.MonitorErrors)
        {
            rows.Add(NetworkInterface.PacketsReceivedErrors);
            rows.Add(NetworkInterface.PacketsOutboundErrors);
        }

        if (settings.MonitorDiscards)
        {
            rows.Add(NetworkInterface.PacketsReceivedDiscarded);
            rows.Add(NetworkInterface.PacketsOutboundDiscarded);
        }

        var clean = Sanitize(rows);
        return clean.Count > 0 ? clean : Sanitize(SeedReceiveSend);
    }

    public static IReadOnlyList<string> Sanitize(IEnumerable<string?> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        var known = NetworkInterface.Counters;
        var rows = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in names)
        {
            var text = name?.Trim() ?? string.Empty;
            if (text.Length == 0)
                continue;
            var match = known.FirstOrDefault(k => k.Equals(text, StringComparison.OrdinalIgnoreCase));
            if (match is null || !seen.Add(match))
                continue;
            rows.Add(match);
        }

        return rows;
    }

    public static IReadOnlyList<string> Available(IEnumerable<string> selected)
    {
        var taken = new HashSet<string>(Sanitize(selected), StringComparer.OrdinalIgnoreCase);
        return NetworkInterface.Counters.Where(c => !taken.Contains(c)).ToArray();
    }
}
