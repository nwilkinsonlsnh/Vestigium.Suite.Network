namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class DetailRows
{
    public static IReadOnlyList<CaptureLine> FromLookup(AnswerRow selected, IReadOnlyList<AnswerRow> answers)
    {
        var name = selected.Name;
        var rows = answers.Where(row => string.Equals(row.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (rows.Count == 0)
            rows.Add(selected);

        var lines = new List<CaptureLine>();
        foreach (var row in rows)
        {
            lines.Add(Line(name, row.Type, row.Data, row.Ttl));
            lines.AddRange(Fields(name, row));
        }

        return lines;
    }

    public static CaptureLine Lead(AnswerRow selected)
    {
        var category = CaptureLines.Category(selected.Name);
        if (category.Length == 0)
            category = selected.Type;
        return new CaptureLine(selected.Name, "", selected.Ttl, selected.Type, "", "", category, selected.Data);
    }

    private static IEnumerable<CaptureLine> Fields(string host, AnswerRow row)
    {
        var parts = row.Data.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (row.Type.Equals("MX", StringComparison.OrdinalIgnoreCase) && parts.Length >= 2)
        {
            yield return Line(host, "Preference", parts[0], row.Ttl);
            yield return Line(host, "Exchanger", string.Join(' ', parts.Skip(1)), row.Ttl);
            yield break;
        }

        if (row.Type.Equals("SOA", StringComparison.OrdinalIgnoreCase) && parts.Length >= 7)
        {
            yield return Line(host, "Primary", parts[0], row.Ttl);
            yield return Line(host, "Contact", parts[1], row.Ttl);
            yield return Line(host, "Serial", parts[2], row.Ttl);
            yield return Line(host, "Refresh", parts[3], row.Ttl);
            yield return Line(host, "Retry", parts[4], row.Ttl);
            yield return Line(host, "Expire", parts[5], row.Ttl);
            yield return Line(host, "Minimum", parts[6], row.Ttl);
        }
    }

    private static CaptureLine Line(string host, string category, string answer, int ttl)
        => new(host, "", ttl, "", "", "", category, answer);
}
