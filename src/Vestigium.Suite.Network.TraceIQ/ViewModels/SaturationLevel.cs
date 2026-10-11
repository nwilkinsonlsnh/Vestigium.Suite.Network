namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed record SaturationLevel(string Level, string Intent, string Feel, int IntervalMs)
{
    public static SaturationLevel Low { get; } = new("Low", "A fragile or production path", "2 seconds between pings", 2000);
    public static SaturationLevel Standard { get; } = new("Standard", "A normal diagnostic", "1 second between pings", 1000);
    public static SaturationLevel Medium { get; } = new("Medium", "Enough samples to trust the loss", "Half a second between pings", 500);
    public static SaturationLevel High { get; } = new("High", "More samples in the same window", "Quarter second between pings, still capped", 250);
    public static SaturationLevel Flood { get; } = new("Flood", "As many samples as the window allows", "100 pings a second", 10);
    public static SaturationLevel Custom { get; } = new("Custom", "You set the rate", "Pings per second", 0);

    public static IReadOnlyList<SaturationLevel> All { get; } = [Low, Standard, Medium, High, Flood, Custom];

    public static SaturationLevel Find(string? level)
        => All.FirstOrDefault(x => x.Level.Equals(level, StringComparison.OrdinalIgnoreCase)) ?? Standard;

    public override string ToString() => Level;
}
