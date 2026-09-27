namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeCalc
{
    public static int RequestsFromInterval(int seconds, int intervalMs)
    {
        if (seconds < PulsePlan.MinSeconds)
            seconds = PulsePlan.MinSeconds;
        if (intervalMs <= 0)
            return PulsePlan.MinRequests;

        var n = 1 + (int)Math.Round(seconds * 1000.0 / intervalMs, MidpointRounding.AwayFromZero);
        return Math.Clamp(n, PulsePlan.MinRequests, PulsePlan.MaxRequests);
    }

    public static int SecondsFromInterval(int requests, int intervalMs)
    {
        if (requests <= 1)
            return PulsePlan.MinSeconds;
        if (intervalMs <= 0)
            return PulsePlan.MinSeconds;

        var seconds = (int)Math.Round((requests - 1) * (intervalMs / 1000.0), MidpointRounding.AwayFromZero);
        return Math.Clamp(seconds, PulsePlan.MinSeconds, PulsePlan.MaxSeconds);
    }

    public static decimal HzFromInterval(int intervalMs)
        => intervalMs <= 0 ? 0 : Math.Round(1000m / intervalMs, 2, MidpointRounding.AwayFromZero);

    public static int IntervalFromHz(decimal hz)
    {
        if (hz <= 0)
            return 1000;
        var ms = (int)Math.Round(1000m / hz, MidpointRounding.AwayFromZero);
        return Math.Clamp(ms, 10, 60_000);
    }

    public static string Describe(int requests, int seconds)
    {
        if (!PulsePlan.TryCreate(requests, seconds, out var plan, out _))
            return string.Empty;

        if (plan.Requests <= 1)
            return "One request. No spacing.";

        var ms = plan.Spacing.TotalMilliseconds;
        var perSec = plan.Seconds == 0 ? 0 : (plan.Requests - 1) / (double)plan.Seconds;
        return $"{plan.Requests} requests over {plan.Seconds}s → {ms:0.#} ms apart (≈{perSec:0.##}/s)";
    }
}
