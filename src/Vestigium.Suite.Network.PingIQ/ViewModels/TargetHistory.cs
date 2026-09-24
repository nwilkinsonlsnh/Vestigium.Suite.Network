namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class TargetHistory
{
    public const int DefaultSize = 10;
    public const int MinSize = 1;
    public const int MaxSize = 25;

    public static int ClampSize(int size)
        => size is < MinSize or > MaxSize ? DefaultSize : size;

    public static IReadOnlyList<string> Remember(IEnumerable<string>? current, string? target, int size)
    {
        var cap = ClampSize(size);
        var trimmed = target?.Trim() ?? string.Empty;
        var next = new List<string>(cap);
        if (trimmed.Length > 0)
            next.Add(trimmed);

        if (current is not null)
        {
            foreach (var item in current)
            {
                var value = item?.Trim() ?? string.Empty;
                if (value.Length == 0)
                    continue;
                if (next.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase)))
                    continue;
                next.Add(value);
                if (next.Count >= cap)
                    break;
            }
        }

        return next;
    }
}
