using System.Windows.Threading;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private readonly DispatcherTimer _routeDelay = Delay();
    private readonly DispatcherTimer _neighborDelay = Delay();
    private readonly DispatcherTimer _connectionDelay = Delay();

    private void ArmFilters()
    {
        _routeDelay.Tick += (_, _) => ApplyRoute();
        _neighborDelay.Tick += (_, _) => ApplyNeighbor();
        _connectionDelay.Tick += (_, _) => ApplyConnection();
    }

    private void ApplyRoute()
    {
        _routeDelay.Stop();
        _routeBound = Compile(RouteQuery, _routeSession);
        Refresh(Ipv4Routes, RouteFilter);
        Refresh(Ipv6Routes, RouteFilter);
    }

    private void ApplyNeighbor()
    {
        _neighborDelay.Stop();
        _neighborBound = Compile(NeighborQuery, _neighborSession);
        Refresh(Ipv4Neighbors, NeighborFilter);
        Refresh(Ipv6Neighbors, NeighborFilter);
    }

    private void ApplyConnection()
    {
        _connectionDelay.Stop();
        _connectionBound = Compile(ConnectionQuery, _connectionSession);
        Refresh(Connections, ConnectionFilter);
    }

    private static DispatcherTimer Delay() => new() { Interval = TimeSpan.FromMilliseconds(180) };
}
