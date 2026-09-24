using System.Net.NetworkInformation;
using System.Net.Sockets;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class AdapterChoiceTests
{
    [Fact]
    public void Adapter_list_starts_with_any()
    {
        var list = AdapterChoices.From([]);
        Assert.Equal(AdapterChoices.Any, list[0]);
        Assert.Equal(0, list[0].Index);
    }

    [Fact]
    public void Adapter_list_numbers_nics_from_one()
    {
        var list = AdapterChoices.From([Nic("eth0", "10.0.0.5"), Nic("wlan0", "10.0.0.8")]);
        Assert.Equal(3, list.Count);
        Assert.Equal(1, list[1].Index);
        Assert.Equal("eth0  (1)", list[1].Label);
        Assert.Equal(2, list[2].Index);
        Assert.Equal("wlan0  (2)", list[2].Label);
    }

    [Fact]
    public void Source_list_starts_with_any_and_lists_unicast()
    {
        var list = SourceChoices.From([Nic("eth0", "10.0.0.5"), Nic("wlan0", "10.0.0.8")]);
        Assert.Equal(SourceChoices.Any, list[0]);
        Assert.Equal("10.0.0.5", list[1].Address);
        Assert.Contains("eth0", list[1].Label);
        Assert.Equal("10.0.0.8", list[2].Address);
    }

    [Fact]
    public void Source_list_narrows_when_interface_is_pinned()
    {
        var adapters = new[] { Nic("eth0", "10.0.0.5"), Nic("wlan0", "10.0.0.8") };
        var list = SourceChoices.From(adapters, interfaceIndex: 2);
        Assert.Equal(2, list.Count);
        Assert.Equal(SourceChoices.Any, list[0]);
        Assert.Equal("10.0.0.8", list[1].Address);
        Assert.DoesNotContain(list, s => s.Address == "10.0.0.5");
    }

    [Fact]
    public void Dead_persisted_source_becomes_any()
    {
        var list = SourceChoices.From([Nic("eth0", "10.0.0.5")]);
        Assert.Equal(SourceChoices.Any, SourceChoices.Resolve(list, "9.9.9.9"));
        Assert.Equal("10.0.0.5", SourceChoices.Resolve(list, "10.0.0.5").Address);
        Assert.Equal(SourceChoices.Any, SourceChoices.Resolve(list, null));
    }

    private static NetworkAdapter Nic(string name, string address) =>
        new(
            Id: name,
            Name: name,
            Description: name,
            Type: NetworkInterfaceType.Ethernet,
            Status: OperationalStatus.Up,
            MacAddress: null,
            SpeedBitsPerSecond: null,
            SupportsMulticast: true,
            UnicastAddresses:
            [
                new UnicastAddress(AddressFamily.InterNetwork, address, 24, "255.255.255.0", false)
            ],
            Gateways: [],
            DnsServers: [],
            Dhcp: new DhcpInfo(null, null, null, null),
            NetbiosOverTcp: NetbiosOverTcp.Unknown);
}
