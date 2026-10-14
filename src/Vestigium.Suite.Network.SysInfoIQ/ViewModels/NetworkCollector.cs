using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public static class NetworkCollector
{
    public static NetworkSnapshot Collect(CancellationToken ct)
    {
        var snap = new NetworkSnapshot();
        try
        {
            CollectAdapters(snap, ct);
            CollectRoutes(snap, ct);
            CollectArp(snap, ct);
            CollectListeners(snap, ct);
            CollectTcp(snap, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            snap.Error = ex.Message;
        }
        return snap;
    }

    private static void CollectAdapters(NetworkSnapshot snap, CancellationToken ct)
    {
        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            ct.ThrowIfCancellationRequested();
            var props = nic.GetIPProperties();
            var addresses = string.Join(", ", props.UnicastAddresses.Select(a => a.Address.ToString()));
            var gateways = string.Join(", ", props.GatewayAddresses.Select(g => g.Address.ToString()));
            var dns = string.Join(", ", props.DnsAddresses.Select(d => d.ToString()));
            var speed = nic.Speed > 0 ? (nic.Speed / 1_000_000).ToString() : "";

            snap.Ip.Add(new IpConfigRow
            {
                Name = nic.Name,
                Description = nic.Description,
                Status = nic.OperationalStatus.ToString(),
                Mac = FormatMac(nic.GetPhysicalAddress()),
                Addresses = addresses,
                Gateways = gateways,
                Dns = dns,
                Dhcp = props.GetIPv4Properties()?.IsDhcpEnabled == true ? "Yes" : ""
            });

            snap.Dns.Add(new DnsConfigRow
            {
                Name = nic.Name,
                DnsServers = dns,
                Suffix = props.DnsSuffix
            });

            snap.Nics.Add(new NicRow
            {
                Name = nic.Name,
                Description = nic.Description,
                Mac = FormatMac(nic.GetPhysicalAddress()),
                Status = nic.OperationalStatus.ToString(),
                SpeedMbps = speed,
                Type = nic.NetworkInterfaceType.ToString()
            });
        }
    }

    private static void CollectRoutes(NetworkSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_IP4RouteTable");
            using var results = searcher.Get();
            foreach (ManagementObject mo in results)
            {
                using (mo)
                {
                    snap.Routes.Add(new RouteRow
                    {
                        Destination = Str(mo, "Destination"),
                        Mask = Str(mo, "Mask"),
                        NextHop = Str(mo, "NextHop"),
                        InterfaceIndex = Str(mo, "InterfaceIndex"),
                        Metric = Str(mo, "Metric1")
                    });
                }
            }
        }
        catch (Exception)
        {
            // routes stay empty
        }
    }

    private static void CollectArp(NetworkSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var scope = new ManagementScope(@"\\.\root\StandardCimv2");
            scope.Connect();
            using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT * FROM MSFT_NetNeighbor"));
            using var results = searcher.Get();
            var count = 0;
            foreach (ManagementObject mo in results)
            {
                using (mo)
                {
                    if (count >= NetworkSnapshot.ArpCap)
                    {
                        snap.ArpTruncated = true;
                        break;
                    }
                    snap.Arp.Add(new ArpRow
                    {
                        Address = Str(mo, "IPAddress"),
                        Mac = Str(mo, "LinkLayerAddress"),
                        State = Str(mo, "State"),
                        InterfaceIndex = Str(mo, "InterfaceIndex")
                    });
                    count++;
                }
            }
        }
        catch (Exception)
        {
            // ARP stays empty
        }
    }

    private static void CollectListeners(NetworkSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var props = IPGlobalProperties.GetIPGlobalProperties();
        foreach (var ep in props.GetActiveTcpListeners())
        {
            snap.Listeners.Add(new ListenerRow
            {
                Protocol = "TCP",
                LocalAddress = ep.Address.ToString(),
                Port = ep.Port.ToString()
            });
        }
        foreach (var ep in props.GetActiveUdpListeners())
        {
            snap.Listeners.Add(new ListenerRow
            {
                Protocol = "UDP",
                LocalAddress = ep.Address.ToString(),
                Port = ep.Port.ToString()
            });
        }
    }

    private static void CollectTcp(NetworkSnapshot snap, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var props = IPGlobalProperties.GetIPGlobalProperties();
        var count = 0;
        foreach (var c in props.GetActiveTcpConnections())
        {
            if (count >= NetworkSnapshot.TcpCap)
            {
                snap.TcpTruncated = true;
                break;
            }
            snap.Tcp.Add(new TcpRow
            {
                LocalAddress = c.LocalEndPoint.Address.ToString(),
                LocalPort = c.LocalEndPoint.Port.ToString(),
                RemoteAddress = c.RemoteEndPoint.Address.ToString(),
                RemotePort = c.RemoteEndPoint.Port.ToString(),
                State = c.State.ToString()
            });
            count++;
        }
    }

    private static string FormatMac(PhysicalAddress? address)
    {
        if (address is null)
            return "";
        var bytes = address.GetAddressBytes();
        return bytes.Length == 0 ? "" : string.Join("-", bytes.Select(b => b.ToString("X2")));
    }

    private static string Str(ManagementObject mo, string name)
    {
        try
        {
            return mo[name]?.ToString() ?? "";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
