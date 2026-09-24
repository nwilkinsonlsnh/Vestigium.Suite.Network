namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public readonly record struct PulsePlan(int Requests, int DurationMs, TimeSpan Spacing)
{
    public const int MinRequests = 1;
    public const int MaxRequests = 10_000;
    public const int MinDurationMs = 100;
    public const int MaxDurationMs = 600_000;

    public int Seconds => Math.Max(1, (int)Math.Round(DurationMs / 1000.0, MidpointRounding.AwayFromZero));

    public static bool TryCreate(decimal requests, decimal durationMs, out PulsePlan plan, out string? reject)
    {
        plan = default;
        var n = (int)decimal.Truncate(requests);
        var ms = (int)decimal.Truncate(durationMs);
        if (n is < MinRequests or > MaxRequests)
        {
            reject = $"Requests must be {MinRequests}–{MaxRequests}.";
            return false;
        }

        if (ms is < MinDurationMs or > MaxDurationMs)
        {
            reject = $"Milliseconds must be {MinDurationMs}–{MaxDurationMs}.";
            return false;
        }

        plan = new PulsePlan(n, ms, TimeSpan.FromMilliseconds(ms / (double)n));
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
