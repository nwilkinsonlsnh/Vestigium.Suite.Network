using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private bool _busy;

    public IReadOnlyList<string> Families { get; } = ["All", "IPv4", "IPv6"];

    public ObservableCollection<NetworkRoute> Ipv4Routes { get; } = [];

    public ObservableCollection<NetworkRoute> Ipv6Routes { get; } = [];

    public ObservableCollection<NetworkNeighbor> Neighbors { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowIpv4))]
    [NotifyPropertyChangedFor(nameof(ShowIpv6))]
    private string _family = "All";

    public bool ShowIpv4 => Family is "All" or "IPv4";

    public bool ShowIpv6 => Family is "All" or "IPv6";

    public Action<string>? ReportStatus { get; set; }

    public MainViewModel() => _ = Refresh();

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task Refresh()
    {
        if (_busy)
            return;

        if (!RouteIqInput.TryMapFamily(Family, out var family, out var reason))
        {
            Report(reason ?? "Family must be All, IPv4, or IPv6.");
            return;
        }

        _busy = true;
        RefreshCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        Report(string.Empty);
        try
        {
            var snapshot = await Task.Run(() => Load(family)).ConfigureAwait(true);
            Replace(Ipv4Routes, snapshot.Ipv4);
            Replace(Ipv6Routes, snapshot.Ipv6);
            Replace(Neighbors, snapshot.Neighbors);
            Report(string.Empty);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
        finally
        {
            _busy = false;
            RefreshCommand.NotifyCanExecuteChanged();
            CopyCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void Copy()
    {
        try
        {
            Clipboard.SetText(FormatTables());
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    private bool CanRefresh() => !_busy;

    private bool CanCopy() => !_busy;

    private void Report(string text) => ReportStatus?.Invoke(text);

    private static (IReadOnlyList<NetworkRoute> Ipv4, IReadOnlyList<NetworkRoute> Ipv6, IReadOnlyList<NetworkNeighbor> Neighbors) Load(RouteFamily family)
    {
        var ipv4 = family == RouteFamily.Pv6
            ? Array.Empty<NetworkRoute>()
            : NetworkHelper.GetRoutes(RouteFamily.Pv4);
        var ipv6 = family == RouteFamily.Pv4
            ? Array.Empty<NetworkRoute>()
            : NetworkHelper.GetRoutes(RouteFamily.Pv6);
        var neighbors = Filter(NetworkHelper.GetNeighbors(), family);
        return (ipv4, ipv6, neighbors);
    }

    private string FormatTables()
    {
        var text = new StringBuilder();
        if (ShowIpv4)
            Append(text, "IPv4 Route Table", Ipv4Routes);
        if (ShowIpv6)
            Append(text, "IPv6 Route Table", Ipv6Routes);
        return text.ToString();
    }

    private static void Append(StringBuilder text, string title, IReadOnlyList<NetworkRoute> rows)
    {
        if (text.Length > 0)
            text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Destination          Prefix  Mask             Gateway            Interface           Index  Metric  Protocol");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Destination, 21));
            text.Append(Pad(row.PrefixLength.ToString(), 8));
            text.Append(Pad(row.Mask, 17));
            text.Append(Pad(row.Gateway, 19));
            text.Append(Pad(row.InterfaceName, 20));
            text.Append(Pad(row.InterfaceIndex.ToString(), 7));
            text.Append(Pad(row.Metric.ToString(), 8));
            text.AppendLine(row.Protocol);
        }

        if (rows.Count == 0)
            text.AppendLine("None");
    }

    private static string Pad(string? value, int width)
    {
        var text = value ?? string.Empty;
        return text.Length >= width ? text + " " : text.PadRight(width);
    }

    private static IReadOnlyList<NetworkNeighbor> Filter(IReadOnlyList<NetworkNeighbor> rows, RouteFamily family)
    {
        if (family == RouteFamily.All)
            return rows;

        var want = family == RouteFamily.Pv4
            ? AddressFamily.InterNetwork
            : AddressFamily.InterNetworkV6;
        return rows.Where(row => row.Family == want).ToArray();
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
