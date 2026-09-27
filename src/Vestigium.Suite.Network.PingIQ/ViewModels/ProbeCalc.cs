namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeCalc
{
    public static string Describe(int requests, int durationMs)
    {
        if (!PulsePlan.TryCreate(requests, durationMs, out var plan, out _))
            return string.Empty;

        var spacing = plan.Spacing.TotalMilliseconds;
        var perSec = plan.DurationMs == 0 ? 0 : plan.Requests / (plan.DurationMs / 1000.0);
        return $"{plan.Requests} requests over {plan.DurationMs} ms → {spacing:0.#} ms apart (≈{perSec:0.##}/s)";
    }
}
