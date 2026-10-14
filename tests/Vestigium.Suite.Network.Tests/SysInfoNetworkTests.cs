using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoNetworkTests
{
    [Fact]
    public void Arp_cap_is_500()
    {
        Assert.Equal(500, NetworkSnapshot.ArpCap);
    }

    [Fact]
    public void Tcp_cap_is_1000()
    {
        Assert.Equal(1000, NetworkSnapshot.TcpCap);
    }

    [Fact]
    public void Snapshot_starts_clean()
    {
        var snap = new NetworkSnapshot();
        Assert.Empty(snap.Ip);
        Assert.Empty(snap.Dns);
        Assert.Empty(snap.Nics);
        Assert.Empty(snap.Routes);
        Assert.Empty(snap.Arp);
        Assert.Empty(snap.Listeners);
        Assert.Empty(snap.Tcp);
        Assert.False(snap.ArpTruncated);
        Assert.False(snap.TcpTruncated);
        Assert.Null(snap.Error);
    }

    [Fact]
    public void Row_shapes_hold_the_sample_fields()
    {
        var ip = new IpConfigRow { Name = "Ethernet", Addresses = "172.16.0.61", Status = "Up" };
        var route = new RouteRow { Destination = "0.0.0.0", Mask = "0.0.0.0", NextHop = "172.16.0.1", Metric = "25" };
        var arp = new ArpRow { Address = "172.16.0.1", Mac = "BC-24-11-FE-3D-5E", State = "Reachable" };
        var listener = new ListenerRow { Protocol = "TCP", LocalAddress = "0.0.0.0", Port = "135" };
        var tcp = new TcpRow { LocalAddress = "0.0.0.0", LocalPort = "135", RemoteAddress = "0.0.0.0", RemotePort = "0", State = "Listen" };

        Assert.Equal("Ethernet", ip.Name);
        Assert.Equal("0.0.0.0", route.Destination);
        Assert.Equal("BC-24-11-FE-3D-5E", arp.Mac);
        Assert.Equal("135", listener.Port);
        Assert.Equal("Listen", tcp.State);
    }
}
