using System.Globalization;
using System.Text;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class AdapterRow
{
    public AdapterRow(NetworkAdapter source)
    {
        Source = source;
    }

    public NetworkAdapter Source { get; }

    public string Id => Source.Id;

    public string Name => Source.Name;

    public string Status => Source.Status.ToString();

    public string Type => Source.Type.ToString();

    public string? MacAddress => Source.MacAddress;

    public long? SpeedBitsPerSecond => Source.SpeedBitsPerSecond;

    public string Speed => Source.SpeedBitsPerSecond is null
        ? string.Empty
        : LinkSpeed.Format(Source.SpeedBitsPerSecond.Value);

    public string Detail => FormatDetail(Source);

    public static string FormatDetail(NetworkAdapter adapter)
    {
        var text = new StringBuilder();
        text.AppendLine("Description");
        text.AppendLine(string.IsNullOrWhiteSpace(adapter.Description) ? "—" : adapter.Description.Trim());
        text.AppendLine();
        text.AppendLine("Unicast addresses");
        if (adapter.UnicastAddresses.Count == 0)
            text.AppendLine("—");
        else
        {
            foreach (var address in adapter.UnicastAddresses)
            {
                var mask = string.IsNullOrWhiteSpace(address.SubnetMask) ? string.Empty : $"  mask {address.SubnetMask}";
                var dhcp = address.IsDhcpAssigned ? "  dhcp" : string.Empty;
                text.AppendLine($"{address.Address}/{address.PrefixLength}  {address.Family}{mask}{dhcp}");
            }
        }

        text.AppendLine();
        text.AppendLine("Gateways");
        AppendList(text, adapter.Gateways);
        text.AppendLine();
        text.AppendLine("DNS servers");
        AppendList(text, adapter.DnsServers);
        text.AppendLine();
        text.AppendLine("DHCP");
        if (adapter.Dhcp.IsEnabled is null && string.IsNullOrWhiteSpace(adapter.Dhcp.Server)
            && adapter.Dhcp.LeaseObtained is null && adapter.Dhcp.LeaseExpires is null)
        {
            text.AppendLine("—");
        }
        else
        {
            text.AppendLine($"Enabled  {FormatOptional(adapter.Dhcp.IsEnabled)}");
            text.AppendLine($"Server   {adapter.Dhcp.Server ?? "—"}");
            text.AppendLine($"Lease    {FormatLease(adapter.Dhcp)}");
        }

        text.AppendLine();
        text.AppendLine("NetBIOS-over-TCP");
        text.Append(adapter.NetbiosOverTcp.ToString());
        return text.ToString();
    }

    private static void AppendList(StringBuilder text, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            text.AppendLine("—");
            return;
        }

        foreach (var value in values)
            text.AppendLine(value);
    }

    private static string FormatOptional(bool? value)
        => value is null ? "—" : value.Value ? "Yes" : "No";

    private static string FormatLease(DhcpInfo dhcp)
    {
        if (dhcp.LeaseObtained is null && dhcp.LeaseExpires is null)
            return "Not reported";
        if (dhcp.LeaseObtained is not null && dhcp.LeaseExpires is not null)
            return $"obtained {FormatTime(dhcp.LeaseObtained)}  expires {FormatTime(dhcp.LeaseExpires)}";
        if (dhcp.LeaseObtained is not null)
            return $"obtained {FormatTime(dhcp.LeaseObtained)}";
        return $"expires {FormatTime(dhcp.LeaseExpires)}";
    }

    private static string FormatTime(DateTimeOffset? value)
        => value is null ? "—" : value.Value.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
}
