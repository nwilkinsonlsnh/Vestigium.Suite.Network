namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed record SaturationLevel(string Level, string Intent, string Feel, int IntervalMs)
{
    public static SaturationLevel Low { get; } = new("Low", "A fragile or production path", "One at a time, long gap", 2000);
    public static SaturationLevel Standard { get; } = new("Standard", "A normal diagnostic", "Steady, a few in flight", 1000);
    public static SaturationLevel Medium { get; } = new("Medium", "Enough samples to trust the loss", "Busier, still one hop at a time", 500);
    public static SaturationLevel High { get; } = new("High", "More samples in the same window", "Faster, capped, never a flood", 250);

    public static IReadOnlyList<SaturationLevel> All { get; } = [Low, Standard, Medium, High];

    public static SaturationLevel Find(string? level)
        => All.FirstOrDefault(x => x.Level.Equals(level, StringComparison.OrdinalIgnoreCase)) ?? Standard;

    public override string ToString() => Level;
}
