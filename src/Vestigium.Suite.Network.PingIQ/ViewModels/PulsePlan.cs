namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public readonly record struct PulsePlan(int Requests, int Seconds, TimeSpan Spacing)
{
    public const int MinRequests = 1;
    public const int MaxRequests = 10_000;
    public const int MinSeconds = 1;
    public const int MaxSeconds = 600;

    public static bool TryCreate(decimal requests, decimal seconds, out PulsePlan plan, out string? reject)
    {
        plan = default;
        var n = (int)decimal.Truncate(requests);
        var x = (int)decimal.Truncate(seconds);
        if (n is < MinRequests or > MaxRequests)
        {
            reject = $"Requests must be {MinRequests}–{MaxRequests}.";
            return false;
        }

        if (x is < MinSeconds or > MaxSeconds)
        {
            reject = $"Seconds must be {MinSeconds}–{MaxSeconds}.";
            return false;
        }

        var spacing = TimeSpan.FromMilliseconds(x * 1000.0 / n);
        plan = new PulsePlan(n, x, spacing);
        reject = null;
        return true;
    }

    public TimeSpan DueAt(int requestIndex)
    {
        if (requestIndex <= 1)
            return TimeSpan.Zero;
        if (requestIndex >= Requests)
            return Spacing * (Requests - 1);
        return Spacing * (requestIndex - 1);
    }
}
