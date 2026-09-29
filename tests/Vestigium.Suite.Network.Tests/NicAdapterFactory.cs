using System.Net.NetworkInformation;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.Tests;

internal static class NicAdapterFactory
{
    public static NetworkAdapter Up(string id, int metric, bool ipv4)
        => Make(id, OperationalStatus.Up, metric, ipv4);

    public static NetworkAdapter Down(string id, int metric)
        => Make(id, OperationalStatus.Down, metric, ipv4: false);

    private static NetworkAdapter Make(string id, OperationalStatus status, int metric, bool ipv4)
    {
        IReadOnlyList<UnicastAddress> addresses = ipv4
            ? [new UnicastAddress(System.Net.Sockets.AddressFamily.InterNetwork, "192.0.2.10", 24, "255.255.255.0", false)]
            : [];

        return new NetworkAdapter(
            id,
            id,
            id,
            NetworkInterfaceType.Ethernet,
            status,
            "00-00-00-00-00-00",
            1_000_000_000,
            true,
            addresses,
            [],
            [],
            new DhcpInfo(null, null, null, null),
            NetbiosOverTcp.Unknown,
            1,
            metric,
            true,
            true,
            1500,
            null,
            [],
            true,
            true,
            null,
            ipv4,
            false);
    }
}
