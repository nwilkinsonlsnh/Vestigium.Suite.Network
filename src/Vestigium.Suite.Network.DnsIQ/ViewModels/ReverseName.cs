using System.Net;
using System.Net.Sockets;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class ReverseName
{
    public static bool TryPtr(string? value, out string name)
    {
        name = "";
        if (string.IsNullOrWhiteSpace(value) || !IPAddress.TryParse(value.Trim(), out var address))
            return false;

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            name = $"{bytes[3]}.{bytes[2]}.{bytes[1]}.{bytes[0]}.in-addr.arpa";
            return true;
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return false;

        var raw = address.GetAddressBytes();
        var nibbles = new string[32];
        var at = 0;
        for (var i = raw.Length - 1; i >= 0; i--)
        {
            nibbles[at++] = (raw[i] & 0x0F).ToString("x");
            nibbles[at++] = ((raw[i] >> 4) & 0x0F).ToString("x");
        }

        name = string.Join(".", nibbles) + ".ip6.arpa";
        return true;
    }
}
