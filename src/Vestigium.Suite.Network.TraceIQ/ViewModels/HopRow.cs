namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed class HopRow
{
    public const int MaxColumns = 10;

    public int Ttl { get; init; }
    public string Address { get; init; } = "*";
    public string Name { get; init; } = "";
    public string Probes { get; init; } = "";
    public bool Reached { get; init; }
    public string P1 { get; init; } = "";
    public string P2 { get; init; } = "";
    public string P3 { get; init; } = "";
    public string P4 { get; init; } = "";
    public string P5 { get; init; } = "";
    public string P6 { get; init; } = "";
    public string P7 { get; init; } = "";
    public string P8 { get; init; } = "";
    public string P9 { get; init; } = "";
    public string P10 { get; init; } = "";

    public static HopRow From(Vestigium.Helpers.Network.IcmpTraceHop hop)
    {
        var values = hop.Probes.Take(MaxColumns).Select(Probe).ToArray();
        return new HopRow
        {
            Ttl = hop.Ttl,
            Address = string.IsNullOrWhiteSpace(hop.Address) ? "*" : hop.Address,
            Name = hop.Name ?? "",
            Probes = string.Join("  ", values),
            Reached = hop.Probes.Any(p => p.Status == Vestigium.Helpers.Network.IcmpEchoStatus.Success),
            P1 = At(values, 0),
            P2 = At(values, 1),
            P3 = At(values, 2),
            P4 = At(values, 3),
            P5 = At(values, 4),
            P6 = At(values, 5),
            P7 = At(values, 6),
            P8 = At(values, 7),
            P9 = At(values, 8),
            P10 = At(values, 9)
        };
    }

    public HopRow WithTimes(IReadOnlyList<string> times)
    {
        string At(int i) => i < times.Count ? times[i] : "";
        return new HopRow
        {
            Ttl = Ttl,
            Address = Address,
            Name = Name,
            Probes = string.Join("  ", times),
            Reached = Reached,
            P1 = At(0),
            P2 = At(1),
            P3 = At(2),
            P4 = At(3),
            P5 = At(4),
            P6 = At(5),
            P7 = At(6),
            P8 = At(7),
            P9 = At(8),
            P10 = At(9)
        };
    }

    public static HopRow Gap(int ttl, string probes = "Waiting")
        => new() { Ttl = ttl, Address = "*", Name = "", Probes = probes, P1 = probes };

    private static string At(string[] values, int index)
        => index < values.Length ? values[index] : "";

    private static string Probe(Vestigium.Helpers.Network.IcmpTraceProbe probe)
    {
        if (probe.Address is null && probe.Status is Vestigium.Helpers.Network.IcmpEchoStatus.TimedOut
            or Vestigium.Helpers.Network.IcmpEchoStatus.Failed)
            return "*";

        if (probe.RoundtripTimeMs > 0)
            return probe.RoundtripTimeMs.ToString();

        return "<1 ms";
    }
}
