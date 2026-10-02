using System.Globalization;
using System.Net.Sockets;
using System.Text;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class AdapterDetailForm
{
    private const string Dash = "\u2014";

    private AdapterDetailForm(string title, string copyText)
    {
        Title = title;
        CopyText = copyText;
    }

    public string Title { get; }

    public string CopyText { get; }

    public string Name { get; private init; } = Dash;

    public string Description { get; private init; } = Dash;

    public string Type { get; private init; } = Dash;

    public string Status { get; private init; } = Dash;

    public string Mac { get; private init; } = Dash;

    public string InterfaceIndex { get; private init; } = Dash;

    public string Physical { get; private init; } = Dash;

    public string Speed { get; private init; } = Dash;

    public string Mtu { get; private init; } = Dash;

    public string Assignment { get; private init; } = Dash;

    public string Metric { get; private init; } = Dash;

    public string Ipv4 { get; private init; } = Dash;

    public string Prefix { get; private init; } = Dash;

    public string Gateway { get; private init; } = Dash;

    public string Dns { get; private init; } = Dash;

    public string Ipv6 { get; private init; } = Dash;

    public string Suffix { get; private init; } = Dash;

    public string DriverProvider { get; private init; } = Dash;

    public string DriverVersion { get; private init; } = Dash;

    public string DriverDate { get; private init; } = Dash;

    public string DriverService { get; private init; } = Dash;

    public static AdapterDetailForm From(AdapterRow? row)
    {
        if (row is null)
            return new AdapterDetailForm("Adapter", "No adapter selected.");

        var adapter = row.Source;
        var ipv4 = adapter.UnicastAddresses.FirstOrDefault(a => a.Family == AddressFamily.InterNetwork);
        var ipv6 = adapter.UnicastAddresses.FirstOrDefault(a => a.Family == AddressFamily.InterNetworkV6 && !a.Address.StartsWith("fe80", StringComparison.OrdinalIgnoreCase));
        var form = new AdapterDetailForm(Text(row.Name), string.Empty)
        {
            Name = Text(row.Name),
            Description = Text(adapter.Description),
            Type = Text(row.Type),
            Status = Text(row.Status),
            Mac = Text(row.MacAddress),
            InterfaceIndex = adapter.InterfaceIndex?.ToString(CultureInfo.InvariantCulture) ?? Dash,
            Physical = Yes(adapter.PhysicalAdapter),
            Speed = Text(row.Speed),
            Mtu = adapter.Mtu?.ToString(CultureInfo.InvariantCulture) ?? Dash,
            Assignment = ipv4 is null ? Dash : ipv4.IsDhcpAssigned ? "DHCP" : "Static",
            Metric = Text(row.Metric),
            Ipv4 = Text(ipv4?.Address),
            Prefix = ipv4 is null ? Dash : ipv4.PrefixLength.ToString(CultureInfo.InvariantCulture),
            Gateway = Text(adapter.Gateways.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))),
            Dns = Text(adapter.DnsServers.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value))),
            Ipv6 = Text(ipv6?.Address),
            Suffix = Text(adapter.DnsSuffix),
            DriverProvider = Text(adapter.Driver?.Provider),
            DriverVersion = Text(adapter.Driver?.Version),
            DriverDate = adapter.Driver?.Date is DateTimeOffset date ? date.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) : Dash,
            DriverService = Text(adapter.Driver?.Service)
        };
        return form with { CopyText = form.Format() };
    }

    private string Format()
    {
        var text = new StringBuilder();
        text.AppendLine(Title);
        text.AppendLine();
        text.AppendLine("Identity");
        Line(text, "Name", Name);
        Line(text, "Description", Description);
        Line(text, "Type", Type);
        Line(text, "Status", Status);
        Line(text, "MAC", Mac);
        Line(text, "Interface index", InterfaceIndex);
        Line(text, "Physical", Physical);
        text.AppendLine();
        text.AppendLine("Link");
        Line(text, "Speed", Speed);
        Line(text, "MTU", Mtu);
        Line(text, "Assignment", Assignment);
        Line(text, "Metric", Metric);
        text.AppendLine();
        text.AppendLine("Addresses");
        Line(text, "IPv4", Ipv4);
        Line(text, "Prefix", Prefix);
        Line(text, "Gateway", Gateway);
        Line(text, "DNS", Dns);
        Line(text, "IPv6", Ipv6);
        Line(text, "Suffix", Suffix);
        text.AppendLine();
        text.AppendLine("Driver");
        Line(text, "Provider", DriverProvider);
        Line(text, "Version", DriverVersion);
        Line(text, "Date", DriverDate);
        Line(text, "Service", DriverService);
        return text.ToString();
    }

    private static void Line(StringBuilder text, string label, string value)
        => text.AppendLine($"{label}  {value}");

    private static string Text(string? value)
        => string.IsNullOrWhiteSpace(value) ? Dash : value.Trim();

    private static string Yes(bool? value)
        => value is null ? Dash : value.Value ? "Yes" : "No";
}
