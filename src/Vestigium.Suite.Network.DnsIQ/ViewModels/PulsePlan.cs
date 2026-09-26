namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public readonly record struct PulsePlan(int Bursts, int Seconds, TimeSpan Spacing)
{
    public static bool TryCreate(decimal bursts, decimal seconds, out PulsePlan plan, out string? reject)
    {
        plan = default;
        var n = (int)decimal.Truncate(bursts);
        var x = (int)decimal.Truncate(seconds);
        if (n is < 1 or > 60)
        {
            reject = "Bursts must be 1–60.";
            return false;
        }

        if (x is < 1 or > 60)
        {
            reject = "Seconds must be 1–60.";
            return false;
        }

        var spacing = n == 1 ? TimeSpan.Zero : TimeSpan.FromSeconds(x / (double)(n - 1));
        plan = new PulsePlan(n, x, spacing);
        reject = null;
        return true;
    }

    public TimeSpan DueAt(int burstIndex)
    {
        if (Bursts <= 1 || burstIndex <= 1)
            return TimeSpan.Zero;
        return TimeSpan.FromTicks(Spacing.Ticks * (burstIndex - 1));
    }
}
