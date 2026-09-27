using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Media;
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

    public string StatusGlyph => Source.Status == OperationalStatus.Up ? "▲" : "▼";

    public Brush StatusFill => Source.Status == OperationalStatus.Up
        ? UpFill
        : Source.Status == OperationalStatus.Down
            ? DownFill
            : OtherFill;

    public bool StatusChanged { get; set; }

    private static readonly Brush UpFill = BrushFrom("#2E7D32");
    private static readonly Brush DownFill = BrushFrom("#C62828");
    private static readonly Brush OtherFill = BrushFrom("#546E7A");

    private static Brush BrushFrom(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }

    public string Type => Source.Type.ToString();

    public string? MacAddress => Source.MacAddress;

    public long? SpeedBitsPerSecond => Source.SpeedBitsPerSecond;

    public string Speed => Source.SpeedBitsPerSecond is null
        ? string.Empty
        : LinkSpeed.Format(Source.SpeedBitsPerSecond.Value);

    public string Metric
    {
        get
        {
            if (Source.Ipv4Metric is int metric)
                return Source.Ipv4MetricIsAutomatic == true
                    ? $"{metric}  OS"
                    : metric.ToString(CultureInfo.InvariantCulture);
            return "OS";
        }
    }

    public string Ipv4 => Source.HasIpv4Unicast ? "Yes" : Source.SupportsIpv4 ? "bound" : string.Empty;

    public string Ipv6 => Source.HasIpv6Unicast ? "Yes" : Source.SupportsIpv6 ? "bound" : string.Empty;

    public string Detail => FormatDetail(Source);

    public static string FormatDetail(NetworkAdapter adapter)
    {
        var text = new StringBuilder();
        text.AppendLine("Description");
        text.AppendLine(string.IsNullOrWhiteSpace(adapter.Description) ? "—" : adapter.Description.Trim());
        text.AppendLine($"IP enabled       {FormatOptional(adapter.IpEnabled)}");
        text.AppendLine($"IPv4             bind {Yes(adapter.SupportsIpv4)}  address {Yes(adapter.HasIpv4Unicast)}");
        text.AppendLine($"IPv6             bind {Yes(adapter.SupportsIpv6)}  address {Yes(adapter.HasIpv6Unicast)}");
        if (adapter.InterfaceIndex is int index)
        {
            text.AppendLine();
            text.AppendLine($"Interface index  {index}");
        }

        text.AppendLine($"IPv4 metric      {FormatMetric(adapter)}");

        if (adapter.Mtu is int mtu)
            text.AppendLine($"MTU              {mtu}");

        text.AppendLine($"Autoconfig       {FormatOptional(adapter.Ipv4AutoconfigEnabled)}");
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
        text.AppendLine("DNS suffix");
        text.AppendLine(string.IsNullOrWhiteSpace(adapter.DnsSuffix) ? "—" : adapter.DnsSuffix.Trim());
        text.AppendLine($"Register in DNS  {FormatOptional(adapter.DnsRegistrationEnabled)}");
        text.AppendLine();
        text.AppendLine("WINS servers");
        AppendList(text, adapter.WinsServers);
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
        text.AppendLine(adapter.NetbiosOverTcp.ToString());
        if (adapter.Driver is { } driver)
        {
            text.AppendLine();
            text.AppendLine("Driver");
            text.AppendLine(driver.Description ?? "—");
            text.AppendLine($"Provider   {driver.Provider ?? "—"}");
            text.AppendLine($"Version    {driver.Version ?? "—"}");
            text.AppendLine($"Date       {FormatTime(driver.Date)}");
            text.AppendLine($"INF        {driver.Inf ?? "—"}");
            text.AppendLine($"Hardware   {driver.HardwareId ?? "—"}");
            text.AppendLine($"Service    {driver.Service ?? "—"}");
        }

        if (adapter.PhysicalAdapter is not null)
        {
            text.AppendLine();
            text.AppendLine($"Physical adapter  {FormatOptional(adapter.PhysicalAdapter)}");
        }

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

    private static string FormatMetric(NetworkAdapter adapter)
    {
        if (adapter.Ipv4Metric is int metric)
            return adapter.Ipv4MetricIsAutomatic == true ? $"{metric}  OS" : metric.ToString(CultureInfo.InvariantCulture);
        return "OS";
    }

    private static string Yes(bool value) => value ? "Yes" : "No";

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
