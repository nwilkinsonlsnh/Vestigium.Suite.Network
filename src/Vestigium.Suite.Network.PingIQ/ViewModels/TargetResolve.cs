using System.Net;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class TargetResolve
{
    public static bool IsAddress(string? target)
        => IPAddress.TryParse(target?.Trim(), out _);

    public static string? PickIp(DnsLookupResult? lookup)
    {
        if (lookup is null || lookup.Rcode != DnsRcode.NoError)
            return null;

        foreach (var answer in lookup.Answers)
        {
            if (answer.Type is not (DnsRecordType.A or DnsRecordType.Aaaa))
                continue;
            if (IPAddress.TryParse(answer.Data, out _))
                return answer.Data;
        }

        return null;
    }
}
