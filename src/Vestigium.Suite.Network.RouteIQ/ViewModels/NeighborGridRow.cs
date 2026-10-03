using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed record NeighborGridRow(NetworkNeighbor Source, string? VendorText)
{
    public string Address => Source.Address;
    public string? MacAddress => Source.MacAddress;
    public string? InterfaceName => Source.InterfaceName;
    public int? InterfaceIndex => Source.InterfaceIndex;
    public string State => Source.State;
    public bool? IsRouter => Source.IsRouter;
    public bool? IsUnreachable => Source.IsUnreachable;
    public long? LastReachable => Source.LastReachable;
    public string? Vendor => Source.Vendor;
}
