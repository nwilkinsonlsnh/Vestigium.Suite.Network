using System.Net.NetworkInformation;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Lowest-metric Up adapter with an address. Preferred Id wins when it is still Up.</summary>
public static class NicPrimaryAdapter
{
    public static NetworkAdapter? Pick(IEnumerable<NetworkAdapter> adapters, string? preferredId = null)
    {
        ArgumentNullException.ThrowIfNull(adapters);
        var rows = adapters.ToArray();
        if (rows.Length == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(preferredId))
        {
            var kept = rows.FirstOrDefault(a =>
                a.Status == OperationalStatus.Up
                && string.Equals(a.Id, preferredId, StringComparison.OrdinalIgnoreCase));
            if (kept is not null)
                return kept;
        }

        var up = rows.Where(a => a.Status == OperationalStatus.Up).ToArray();
        var addressed = up.Where(a => a.HasIpv4Unicast || a.HasIpv6Unicast).ToArray();
        var pool = addressed.Length > 0 ? addressed : up;
        if (pool.Length == 0)
            return null;

        return pool
            .OrderBy(a => a.Ipv4Metric ?? int.MaxValue)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .First();
    }
}
