namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed class HopRow
{
    public int Ttl { get; init; }
    public string Address { get; init; } = "*";
    public string Name { get; init; } = "";
    public string Probes { get; init; } = "";
    public bool Reached { get; init; }

    public static HopRow From(Vestigium.Helpers.Network.IcmpTraceHop hop)
        => new()
        {
            Ttl = hop.Ttl,
            Address = string.IsNullOrWhiteSpace(hop.Address) ? "*" : hop.Address,
            Name = hop.Name ?? "",
            Probes = string.Join("  ", hop.Probes.Select(Probe)),
            Reached = hop.Probes.Any(p => p.Status == Vestigium.Helpers.Network.IcmpEchoStatus.Success)
        };

    private static string Probe(Vestigium.Helpers.Network.IcmpTraceProbe probe)
    {
        if (probe.Address is null && probe.Status is Vestigium.Helpers.Network.IcmpEchoStatus.TimedOut
            or Vestigium.Helpers.Network.IcmpEchoStatus.Failed)
            return "*";

        if (probe.RoundtripTimeMs > 0)
            return $"{probe.RoundtripTimeMs} ms";

        return "<1 ms";
    }
}
