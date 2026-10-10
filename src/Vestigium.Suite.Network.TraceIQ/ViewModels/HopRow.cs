namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed class HopRow
{
    public int Ttl { get; init; }
    public string Address { get; init; } = "*";
    public string Name { get; init; } = "";
    public string Probes { get; init; } = "";

    public static HopRow From(Vestigium.Helpers.Network.IcmpTraceHop hop)
        => new()
        {
            Ttl = hop.Ttl,
            Address = string.IsNullOrWhiteSpace(hop.Address) ? "*" : hop.Address,
            Name = hop.Name ?? "",
            Probes = string.Join("  ", hop.Probes.Select(Probe))
        };

    private static string Probe(Vestigium.Helpers.Network.IcmpTraceProbe probe)
    {
        if (probe.Status is Vestigium.Helpers.Network.IcmpEchoStatus.TimedOut
            or Vestigium.Helpers.Network.IcmpEchoStatus.Failed
            && probe.Address is null)
            return "*";

        if (probe.RoundtripTimeMs > 0)
            return $"{probe.RoundtripTimeMs} ms";

        return probe.Status == Vestigium.Helpers.Network.IcmpEchoStatus.Success ? "<1 ms" : "reply";
    }
}
