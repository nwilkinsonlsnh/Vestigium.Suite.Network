namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeCalc
{
    public static int RequestsFromInterval(int seconds, int intervalMs)
        => RequestsFromDuration(seconds * 1000, intervalMs);

    public static int RequestsFromDuration(int durationMs, int intervalMs)
    {
        if (durationMs <= 0 || intervalMs <= 0)
            return PulsePlan.MinRequests;
        var n = (int)Math.Round(durationMs / (double)intervalMs, MidpointRounding.AwayFromZero);
        return Math.Clamp(n, PulsePlan.MinRequests, PulsePlan.MaxRequests);
    }

    public static int SecondsFromInterval(int requests, int intervalMs)
    {
        var duration = DurationMs(requests, intervalMs);
        var seconds = (int)Math.Round(duration / 1000.0, MidpointRounding.AwayFromZero);
        return Math.Clamp(seconds, PulsePlan.MinSeconds, PulsePlan.MaxSeconds);
    }

    public static int IntervalMs(int requests, int seconds)
        => IntervalFromDuration(seconds * 1000, requests);

    public static int IntervalFromDuration(int durationMs, int requests)
    {
        if (requests <= 0 || durationMs <= 0)
            return 100;
        return Math.Clamp((int)Math.Round(durationMs / (double)requests, MidpointRounding.AwayFromZero), 10, 60_000);
    }

    public static int DurationMs(int requests, int intervalMs)
    {
        if (requests <= 0 || intervalMs <= 0)
            return 1000;
        return Math.Clamp(requests * intervalMs, 10, PulsePlan.MaxSeconds * 1000);
    }

    public static string Describe(int requests, int seconds)
    {
        if (!PulsePlan.TryCreate(requests, seconds, out var plan, out _))
            return string.Empty;

        var ms = plan.Spacing.TotalMilliseconds;
        var perSec = plan.Seconds == 0 ? 0 : plan.Requests / (double)plan.Seconds;
        return $"{plan.Requests} requests over {plan.Seconds * 1000} ms → {ms:0.#} ms apart (≈{perSec:0.##}/s)";
    }
}
