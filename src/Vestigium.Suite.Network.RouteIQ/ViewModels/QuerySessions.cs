using Vestigium.Helpers.Kql;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    public KqlSession RouteSession => _routeSession;

    public KqlSession NeighborSession => _neighborSession;

    public KqlSession ConnectionSession => _connectionSession;
}
