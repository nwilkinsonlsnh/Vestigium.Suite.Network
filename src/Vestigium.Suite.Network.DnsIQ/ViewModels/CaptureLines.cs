using System.Net;
using System.Net.Sockets;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class CaptureLines
{
    public const string Ipv4 = "IPv4";
    public const string Ipv6 = "IPv6";
    public const string Address = "Address";

    public static IReadOnlyList<CaptureLine> From(IEnumerable<HarHostRow> hosts)
    {
        var lines = new List<CaptureLine>();
        foreach (var host in hosts)
            lines.AddRange(From(host));
        return lines;
    }

    public static IReadOnlyList<CaptureLine> From(HarHostRow host)
    {
        if (host.IsAddress)
            return [Line(host, Address, host.Host)];

        if (host.AnswerItems.Count == 0)
            return [Line(host, "", "")];

        return host.AnswerItems
            .Select(answer => Line(host, Category(answer), answer))
            .ToList();
    }

    public static string Category(string answer)
    {
        if (!IPAddress.TryParse(answer, out var address))
            return "";
        return address.AddressFamily == AddressFamily.InterNetwork ? Ipv4 : Ipv6;
    }

    private static CaptureLine Line(HarHostRow host, string category, string answer)
        => new(host.Host, host.Ports, host.Hits, host.Sources, host.Dns, host.Error, category, answer);
}
