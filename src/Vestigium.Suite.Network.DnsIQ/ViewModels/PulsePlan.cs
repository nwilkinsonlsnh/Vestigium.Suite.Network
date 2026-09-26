namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

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

        var spacing = n == 1 ? TimeSpan.Zero : TimeSpan.FromSeconds(x / (double)(n - 1));
        plan = new PulsePlan(n, x, spacing);
        reject = null;
        return true;
    }

    public TimeSpan DueAt(int requestIndex)
    {
        if (Requests <= 1 || requestIndex <= 1)
            return TimeSpan.Zero;
        if (requestIndex >= Requests)
            return TimeSpan.FromSeconds(Seconds);
        return TimeSpan.FromSeconds(Seconds * (requestIndex - 1) / (double)(Requests - 1));
    }
}
