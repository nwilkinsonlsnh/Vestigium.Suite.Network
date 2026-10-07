using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class CaptureProbe
{
    public static (string Dns, string Answers) Map(
        bool isAddress,
        string host,
        DnsLookupResult? a,
        DnsLookupResult? aaaa)
    {
        if (isAddress)
            return ("Skipped", host);

        var answers = new List<string>();
        if (a is not null)
            answers.AddRange(a.Answers.Select(record => record.Data));
        if (aaaa is not null)
            answers.AddRange(aaaa.Answers.Select(record => record.Data));
        if (answers.Count > 0)
            return ("Resolved", string.Join(", ", answers.Distinct(StringComparer.OrdinalIgnoreCase)));

        if (TimedOut(a) || TimedOut(aaaa))
            return ("TimedOut", "");
        if (a?.Rcode == DnsRcode.Refused || aaaa?.Rcode == DnsRcode.Refused)
            return ("Refused", "");
        if (Nx(a) && Nx(aaaa))
            return ("NxDomain", "");
        return ("Failed", "");
    }

    private static bool TimedOut(DnsLookupResult? result)
        => result?.Rcode is DnsRcode.Timeout;

    private static bool Nx(DnsLookupResult? result)
        => result?.Rcode == DnsRcode.NxDomain
           || result is { Rcode: DnsRcode.NoError, Answers.Count: 0 };
}
