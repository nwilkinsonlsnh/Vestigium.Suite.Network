using System.Net;
using System.Net.Sockets;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed record NeighborGridRow(NetworkNeighbor Source, string? VendorText = "--", string? RttMs = "--", string? Hops = null)
{
    public string Address => Source.Address;
    public string? ClassText { get; } = Letter(Source.Address);
    public string? MacAddress => Source.MacAddress;
    public string? InterfaceName => Source.InterfaceName;
    public int? InterfaceIndex => Source.InterfaceIndex;
    public string State => Source.State;
    public bool? IsRouter => Source.IsRouter;
    public bool IsMulticast => Multicast(Source.Address);
    public bool IsBroadcast => Broadcast(Source.Address, Source.MacAddress);
    public string? LastReachableText => FormatReachable(Source.LastReachable);
    public string? Vendor => Source.Vendor;

    private static string? Letter(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;
        try
        {
            var traditional = NetworkHelper.ClassifyAddress(address).TraditionalClass;
            return traditional == TraditionalClass.None ? null : traditional.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool Broadcast(string? address, string? mac)
    {
        if (string.Equals(mac, "FF:FF:FF:FF:FF:FF", StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(address, "255.255.255.255", StringComparison.Ordinal))
            return true;
        if (string.IsNullOrWhiteSpace(address))
            return false;
        try
        {
            return NetworkHelper.ClassifyAddress(address).Kind.HasFlag(AddressKind.Broadcast);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool Multicast(string? address)
    {
        if (!IPAddress.TryParse(address, out var ip))
            return false;
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var first = ip.GetAddressBytes()[0];
            return first is >= 224 and <= 239;
        }

        return ip.IsIPv6Multicast;
    }

    private static string? FormatReachable(long? milliseconds)
    {
        if (milliseconds is null or < 0)
            return null;
        var span = TimeSpan.FromMilliseconds(milliseconds.Value);
        if (span.TotalDays >= 1)
            return $"{(int)span.TotalDays}d {span.Hours}h";
        if (span.TotalHours >= 1)
            return $"{(int)span.TotalHours}h {span.Minutes}m";
        if (span.TotalMinutes >= 1)
            return $"{(int)span.TotalMinutes}m {span.Seconds}s";
        return $"{Math.Max(0, (int)span.TotalSeconds)}s";
    }
}
