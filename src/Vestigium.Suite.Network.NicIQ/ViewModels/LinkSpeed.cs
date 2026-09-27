using System.Globalization;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public readonly record struct LinkSpeedScale(string Unit, double Divisor, string SeriesName);

public static class LinkSpeed
{
    public static LinkSpeedScale ScaleFor(IReadOnlyList<long> bitsPerSecond)
    {
        var max = 0L;
        foreach (var value in bitsPerSecond)
        {
            if (value > max)
                max = value;
        }

        return ScaleFor(max);
    }

    public static LinkSpeedScale ScaleFor(long bitsPerSecond)
    {
        if (bitsPerSecond >= 1_000_000_000)
            return new LinkSpeedScale("Gbps", 1_000_000_000d, "nic-link-gbps");
        if (bitsPerSecond >= 1_000_000)
            return new LinkSpeedScale("Mbps", 1_000_000d, "nic-link-mbps");
        if (bitsPerSecond >= 1_000)
            return new LinkSpeedScale("kbps", 1_000d, "nic-link-kbps");
        return new LinkSpeedScale("bps", 1d, "nic-link-bps");
    }

    public static double ToUnit(long bitsPerSecond, double divisor)
        => bitsPerSecond / divisor;

    public static string Format(long bitsPerSecond)
    {
        var scale = ScaleFor(bitsPerSecond);
        var value = ToUnit(bitsPerSecond, scale.Divisor);
        return string.Create(CultureInfo.InvariantCulture, $"{value:0.###} {scale.Unit}");
    }
}
