namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeCalc
{
    public static int RequestsFromInterval(int seconds, int intervalMs)
    {
        if (seconds < PulsePlan.MinSeconds)
            seconds = PulsePlan.MinSeconds;
        if (intervalMs <= 0)
            return PulsePlan.MinRequests;

        var n = (int)Math.Round(seconds * 1000.0 / intervalMs, MidpointRounding.AwayFromZero);
        return Math.Clamp(n, PulsePlan.MinRequests, PulsePlan.MaxRequests);
    }

    public static int SecondsFromInterval(int requests, int intervalMs)
    {
        if (requests < PulsePlan.MinRequests)
            requests = PulsePlan.MinRequests;
        if (intervalMs <= 0)
            return PulsePlan.MinSeconds;

        var seconds = (int)Math.Round(requests * (intervalMs / 1000.0), MidpointRounding.AwayFromZero);
        return Math.Clamp(seconds, PulsePlan.MinSeconds, PulsePlan.MaxSeconds);
    }

    public static int IntervalMs(int requests, int seconds)
    {
        if (requests <= 0)
            return 1000;
        return Math.Clamp((int)Math.Round(seconds * 1000.0 / requests, MidpointRounding.AwayFromZero), 10, 60_000);
    }

    public static string Describe(int requests, int seconds)
    {
        if (!PulsePlan.TryCreate(requests, seconds, out var plan, out _))
            return string.Empty;

        var ms = plan.Spacing.TotalMilliseconds;
        var perSec = plan.Seconds == 0 ? 0 : plan.Requests / (double)plan.Seconds;
        return $"{plan.Requests} requests over {plan.Seconds}s → {ms:0.#} ms apart (≈{perSec:0.##}/s)";
    }
}
