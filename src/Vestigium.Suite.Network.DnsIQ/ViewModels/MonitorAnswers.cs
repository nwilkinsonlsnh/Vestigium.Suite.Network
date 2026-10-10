using System.Text.RegularExpressions;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static partial class MonitorAnswers
{
    public static IReadOnlyList<string> Values(string? answers)
    {
        if (string.IsNullOrWhiteSpace(answers))
            return [];

        var list = new List<string>();
        foreach (var part in answers.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (var value in Expand(part))
            {
                if (value.Length > 0)
                    list.Add(value);
            }
        }

        return list;
    }

    private static IEnumerable<string> Expand(string part)
    {
        var text = part.Trim().TrimEnd(';').Trim();
        if (text.Length == 0)
            yield break;

        var matches = TypePrefix().Matches(text);
        if (matches.Count == 0)
        {
            yield return text;
            yield break;
        }

        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : text.Length;
            var value = text[start..end].Trim().TrimEnd(';').Trim();
            if (value.Length > 0)
                yield return value;
        }
    }

    [GeneratedRegex(@"type:\s*\d+\s+", RegexOptions.IgnoreCase)]
    private static partial Regex TypePrefix();
}
