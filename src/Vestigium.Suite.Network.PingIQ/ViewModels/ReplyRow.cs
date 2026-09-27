namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed record ReplyRow(
    int Sequence,
    string Status,
    string? Address,
    long RttMs,
    int Ttl,
    int? Hops,
    string? Detail);

public static class HopEstimate
{
    public static int? FromTtl(int ttl)
    {
        if (ttl <= 0)
            return null;

        var initial = ttl <= 64 ? 64 : ttl <= 128 ? 128 : 255;
        return Math.Max(0, initial - ttl);
    }
}
