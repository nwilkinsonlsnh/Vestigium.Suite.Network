using System.Net.NetworkInformation;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

internal static class AdapterListFilter
{
    public static IReadOnlyList<AdapterRow> Apply(
        IEnumerable<NetworkAdapter> source,
        bool showUp,
        bool showDown,
        bool showIpv4,
        bool showIpv6)
    {
        ArgumentNullException.ThrowIfNull(source);
        var rows = new List<AdapterRow>();
        foreach (var adapter in source)
        {
            var up = adapter.Status == OperationalStatus.Up;
            if (up && !showUp)
                continue;
            if (!up && !showDown)
                continue;
            if (showIpv4 || showIpv6)
            {
                var v4 = showIpv4 && adapter.IpEnabled && (adapter.SupportsIpv4 || adapter.HasIpv4Unicast);
                var v6 = showIpv6 && adapter.IpEnabled && (adapter.SupportsIpv6 || adapter.HasIpv6Unicast);
                if (!v4 && !v6)
                    continue;
            }

            rows.Add(new AdapterRow(adapter));
        }

        return rows;
    }
}
