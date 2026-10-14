using System.Net.NetworkInformation;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>
/// Default monitor NIC.
/// Prefer Up, IP enabled, IPv4 address, and a usable gateway (not empty, not 0.0.0.0).
/// Among those, lowest IPv4 metric. Fall back to the metric rule when none qualify.
/// A saved Id wins when that adapter is still on the box.
/// </summary>
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
                string.Equals(a.Id, preferredId, StringComparison.OrdinalIgnoreCase));
            if (kept is not null)
                return kept;
        }

        var withGateway = rows
            .Where(a => a.Status == OperationalStatus.Up && a.IpEnabled && a.HasIpv4Unicast && HasUsableGateway(a))
            .OrderBy(a => a.Ipv4Metric ?? int.MaxValue)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (withGateway is not null)
            return withGateway;

        var primary = rows
            .Where(a => a.Status == OperationalStatus.Up && a.IpEnabled && a.HasIpv4Unicast)
            .OrderBy(a => a.Ipv4Metric ?? int.MaxValue)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (primary is not null)
            return primary;

        return rows
            .Where(a => a.Status == OperationalStatus.Up && a.IpEnabled)
            .OrderBy(a => a.Ipv4Metric ?? int.MaxValue)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static bool HasUsableGateway(NetworkAdapter adapter)
    {
        foreach (var gateway in adapter.Gateways)
        {
            if (string.IsNullOrWhiteSpace(gateway))
                continue;
            if (gateway is "0.0.0.0" or "::")
                continue;
            return true;
        }
        return false;
    }
}
