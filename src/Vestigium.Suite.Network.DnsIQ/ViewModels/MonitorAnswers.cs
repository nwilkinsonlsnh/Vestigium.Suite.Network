namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class MonitorAnswers
{
    public static IReadOnlyList<string> Values(string? answers)
    {
        if (string.IsNullOrWhiteSpace(answers))
            return [];

        var list = new List<string>();
        foreach (var part in answers.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var value = StripType(part);
            if (value.Length > 0)
                list.Add(value);
        }

        return list;
    }

    private static string StripType(string part)
    {
        var text = part.Trim();
        if (!text.StartsWith("type:", StringComparison.OrdinalIgnoreCase))
            return text;

        var rest = text[5..].TrimStart();
        var space = rest.IndexOf(' ');
        if (space < 0 || !int.TryParse(rest[..space], out _))
            return text;

        return rest[(space + 1)..].Trim();
    }
}
