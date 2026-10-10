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
        var ms = probe.RoundtripTimeMs > 0 ? $" {probe.RoundtripTimeMs} ms" : "";
        return $"{probe.Status}{ms}";
    }
}
