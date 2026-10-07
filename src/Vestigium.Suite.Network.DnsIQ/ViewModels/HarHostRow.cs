using Vestigium.Helpers.LogParser;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed class HarHostRow
{
    public HarHostRow(LogHost host)
    {
        Host = host.Host;
        Ports = host.Ports.Count == 0 ? "" : string.Join(", ", host.Ports.OrderBy(p => p));
        Hits = host.HitCount;
        Sources = host.Sources.ToString();
        IsAddress = host.IsAddress;
    }

    public string Host { get; }

    public string Ports { get; }

    public int Hits { get; }

    public string Sources { get; }

    public string Dns { get; set; } = "";

    public string Answers { get; set; } = "";

    public bool IsAddress { get; }
}
