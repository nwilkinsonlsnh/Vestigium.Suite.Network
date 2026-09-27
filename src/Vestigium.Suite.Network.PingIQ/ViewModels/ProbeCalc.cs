namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeCalc
{
    public static int RequestsFromInterval(int seconds, decimal intervalSeconds)
    {
        if (seconds < PulsePlan.MinSeconds)
            seconds = PulsePlan.MinSeconds;
        if (intervalSeconds <= 0)
            return PulsePlan.MinRequests;

        var n = 1 + (int)Math.Round((decimal)seconds / intervalSeconds, MidpointRounding.AwayFromZero);
        return Math.Clamp(n, PulsePlan.MinRequests, PulsePlan.MaxRequests);
    }

    public static int SecondsFromInterval(int requests, decimal intervalSeconds)
    {
        if (requests <= 1)
            return PulsePlan.MinSeconds;
        if (intervalSeconds <= 0)
            return PulsePlan.MinSeconds;

        var seconds = (int)Math.Round((requests - 1) * intervalSeconds, MidpointRounding.AwayFromZero);
        return Math.Clamp(seconds, PulsePlan.MinSeconds, PulsePlan.MaxSeconds);
    }

    public static string Describe(int requests, int seconds)
    {
        if (!PulsePlan.TryCreate(requests, seconds, out var plan, out _))
            return string.Empty;

        if (plan.Requests <= 1)
            return "One request. No spacing.";

        var spacing = plan.Spacing.TotalSeconds;
        return $"{plan.Requests} requests over {plan.Seconds}s → {spacing:0.###} s apart";
    }
}
