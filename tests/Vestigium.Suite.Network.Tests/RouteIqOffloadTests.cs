using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqOffloadTests
{
    [Fact]
    public void Host_does_not_own_the_neighbor_parser_or_port_catalog()
    {
        var assembly = typeof(MainViewModel).Assembly;
        Assert.Null(assembly.GetType("Vestigium.Suite.Network.RouteIQ.ViewModels.NeighborTables"));
        Assert.Null(assembly.GetType("Vestigium.Suite.Network.RouteIQ.ViewModels.ConnectionServices"));
    }
}
