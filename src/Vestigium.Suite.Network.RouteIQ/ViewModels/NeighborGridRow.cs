using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed record NeighborGridRow(NetworkNeighbor Source, string? VendorText, string? PingText = null)
{
    public string Address => Source.Address;
    public string? MacAddress => Source.MacAddress;
    public string? InterfaceName => Source.InterfaceName;
    public int? InterfaceIndex => Source.InterfaceIndex;
    public string State => Source.State;
    public bool? IsRouter => Source.IsRouter;
    public string? LastReachableText => FormatReachable(Source.LastReachable);
    public string? Vendor => Source.Vendor;

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
