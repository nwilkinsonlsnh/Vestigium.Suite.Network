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

        var candidates = Candidates(name, description).ToArray();

        foreach (var candidate in candidates)
        {
            var hit = live.FirstOrDefault(i => NormalizedEquals(i, candidate));
            if (hit is not null)
                return hit;
        }

        foreach (var candidate in candidates)
        {
            var hit = live.FirstOrDefault(i => NormalizedContains(i, candidate));
            if (hit is not null)
                return hit;
        }

        return null;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return value.Trim()
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace('(', '[')
            .Replace(')', ']');
    }

    private static bool NormalizedEquals(string live, string candidate)
        => string.Equals(Normalize(live), Normalize(candidate), StringComparison.OrdinalIgnoreCase);

    private static bool NormalizedContains(string live, string candidate)
    {
        var left = Normalize(live);
        var right = Normalize(candidate);
        if (left.Length == 0 || right.Length == 0)
            return false;
        return left.Contains(right, StringComparison.OrdinalIgnoreCase)
            || right.Contains(left, StringComparison.OrdinalIgnoreCase);
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
