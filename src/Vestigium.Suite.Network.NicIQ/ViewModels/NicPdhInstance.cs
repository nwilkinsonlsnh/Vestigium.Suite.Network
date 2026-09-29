namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Map a Helpers.Network adapter onto a PDH Network Interface instance name.</summary>
public static class NicPdhInstance
{
    public static string? Resolve(string? name, string? description, IEnumerable<string> liveInstances)
    {
        ArgumentNullException.ThrowIfNull(liveInstances);
        var live = liveInstances
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Where(s => !s.Equals("_Total", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (live.Length == 0)
            return null;

        foreach (var candidate in Candidates(name, description))
        {
            var hit = live.FirstOrDefault(i => i.Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
                return hit;
        }

        foreach (var candidate in Candidates(name, description))
        {
            var hit = live.FirstOrDefault(i =>
                i.Contains(candidate, StringComparison.OrdinalIgnoreCase)
                || candidate.Contains(i, StringComparison.OrdinalIgnoreCase));
            if (hit is not null)
                return hit;
        }

        return null;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return value.Trim().Replace('/', '_');
    }

    private static IEnumerable<string> Candidates(string? name, string? description)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in new[] { description, name })
        {
            var text = raw?.Trim() ?? string.Empty;
            if (text.Length == 0)
                continue;
            if (seen.Add(text))
                yield return text;
            var normalized = Normalize(text);
            if (normalized.Length > 0 && seen.Add(normalized))
                yield return normalized;
        }
    }
}
