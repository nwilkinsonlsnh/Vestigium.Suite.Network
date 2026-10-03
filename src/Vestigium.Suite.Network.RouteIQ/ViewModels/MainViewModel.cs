using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
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

    public ObservableCollection<NetworkRoute> Ipv4Routes { get; } = [];

    public ObservableCollection<NetworkRoute> Ipv6Routes { get; } = [];

    public ObservableCollection<NetworkNeighbor> Ipv4Neighbors { get; } = [];

    public ObservableCollection<NetworkNeighbor> Ipv6Neighbors { get; } = [];

    public Action<string>? ReportStatus { get; set; }

    public MainViewModel() => _ = Refresh();

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task Refresh()
    {
        if (_busy)
            return;

        _busy = true;
        RaiseCanExecute();
        Report(string.Empty);
        try
        {
            var snapshot = await Task.Run(Load).ConfigureAwait(true);
            Replace(Ipv4Routes, snapshot.Ipv4);
            Replace(Ipv6Routes, snapshot.Ipv6);
            Replace(Ipv4Neighbors, ApplyPacked(snapshot.Ipv4Neighbors));
            Replace(Ipv6Neighbors, ApplyPacked(snapshot.Ipv6Neighbors));
            await ResolveLiveVendors().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
        finally
        {
            _busy = false;
            RaiseCanExecute();
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

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyNeighbors()
    {
        try
        {
            Clipboard.SetText(FormatNeighbors());
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    private bool CanRefresh() => !_busy;

    private bool CanCopy() => !_busy;

    private void RaiseCanExecute()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        CopyNeighborsCommand.NotifyCanExecuteChanged();
    }

    private void Report(string text) => ReportStatus?.Invoke(text);

    private static (IReadOnlyList<NetworkRoute> Ipv4, IReadOnlyList<NetworkRoute> Ipv6, IReadOnlyList<NetworkNeighbor> Ipv4Neighbors, IReadOnlyList<NetworkNeighbor> Ipv6Neighbors) Load()
    {
        var ipv4 = ByAddress(NetworkHelper.GetRoutes(RouteFamily.Pv4), row => row.Destination);
        var ipv6 = ByAddress(NetworkHelper.GetRoutes(RouteFamily.Pv6), row => row.Destination);
        var neighbors = NetworkHelper.GetNeighbors();
        var v4 = ByAddress(neighbors.Where(row => row.Family == AddressFamily.InterNetwork), row => row.Address);
        var v6 = ByAddress(neighbors.Where(row => row.Family == AddressFamily.InterNetworkV6), row => row.Address);
        return (ipv4, ipv6, v4, v6);
    }

    private static IReadOnlyList<NetworkNeighbor> ApplyPacked(IReadOnlyList<NetworkNeighbor> rows)
    {
        return rows.Select(row =>
        {
            if (!string.IsNullOrWhiteSpace(row.Vendor) || !CanLookup(row.MacAddress))
                return row;
            var hit = NetworkHelper.LookupOuiPacked(row.MacAddress!);
            return string.IsNullOrWhiteSpace(hit.Vendor) ? row : row with { Vendor = hit.Vendor };
        }).ToArray();
    }

    private async Task ResolveLiveVendors()
    {
        var pending = Ipv4Neighbors.Concat(Ipv6Neighbors)
            .Where(row => string.IsNullOrWhiteSpace(row.Vendor) && CanLookup(row.MacAddress))
            .Select(row => Oui(row.MacAddress!))
            .Where(oui => oui.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (pending.Length == 0)
        {
            Report(string.Empty);
            return;
        }

        var misses = new List<string>();
        var options = new OuiLookupOptions { Timeout = TimeSpan.FromSeconds(8) };
        foreach (var oui in pending)
        {
            Report($"OUI {oui}");
            try
            {
                var hit = await NetworkHelper.LookupOuiAsync(oui, options).ConfigureAwait(true);
                if (string.IsNullOrWhiteSpace(hit.Vendor))
                    misses.Add(oui);
                else
                {
                    Stamp(Ipv4Neighbors, oui, hit.Vendor);
                    Stamp(Ipv6Neighbors, oui, hit.Vendor);
                }
            }
            catch (Exception ex)
            {
                misses.Add(oui + " " + ex.Message);
            }

            await Task.Delay(1100).ConfigureAwait(true);
        }

        Report(misses.Count == 0 ? string.Empty : "OUI miss: " + string.Join(", ", misses));
    }

    private static void Stamp(ObservableCollection<NetworkNeighbor> rows, string oui, string vendor)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!string.Equals(Oui(row.MacAddress), oui, StringComparison.OrdinalIgnoreCase))
                continue;
            rows[i] = row with { Vendor = vendor };
        }
    }

    private static bool CanLookup(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac))
            return false;
        var parts = mac.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !byte.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var first))
            return false;
        if ((first & 0x01) != 0)
            return false;
        return !mac.StartsWith("FF:FF:FF", StringComparison.OrdinalIgnoreCase);
    }

    private static string Oui(string? mac)
    {
        var parts = mac?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return parts.Length < 3 ? string.Empty : string.Join(':', parts[0], parts[1], parts[2]);
    }

    private static IReadOnlyList<T> ByAddress<T>(IEnumerable<T> rows, Func<T, string?> address)
    {
        return rows
            .OrderBy(row => AddressKey(address(row)), Comparer<byte[]>.Create(CompareBytes))
            .ToArray();
    }

    private static byte[] AddressKey(string? text)
    {
        if (IPAddress.TryParse(text, out var address))
            return address.GetAddressBytes();
        return [0xFF];
    }

    private static int CompareBytes(byte[] left, byte[] right)
    {
        var count = Math.Min(left.Length, right.Length);
        for (var i = 0; i < count; i++)
        {
            var diff = left[i].CompareTo(right[i]);
            if (diff != 0)
                return diff;
        }

        return left.Length.CompareTo(right.Length);
    }

    private string FormatTables()
    {
        var text = new StringBuilder();
        AppendRoutes(text, "IPv4 Route Table", Ipv4Routes);
        AppendRoutes(text, "IPv6 Route Table", Ipv6Routes);
        return text.ToString();
    }

    private string FormatNeighbors()
    {
        var text = new StringBuilder();
        AppendNeighbors(text, "IPv4 Neighbor Cache", Ipv4Neighbors);
        AppendNeighbors(text, "IPv6 Neighbor Cache", Ipv6Neighbors);
        return text.ToString();
    }

    private static void AppendNeighbors(StringBuilder text, string title, IReadOnlyList<NetworkNeighbor> rows)
    {
        if (text.Length > 0)
            text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Address              MAC                Interface            State            Vendor");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Address, 21));
            text.Append(Pad(row.MacAddress, 19));
            text.Append(Pad(row.InterfaceName, 21));
            text.Append(Pad(row.State, 17));
            text.AppendLine(row.Vendor);
        }

        if (rows.Count == 0)
            text.AppendLine("None");
    }

    private static void AppendRoutes(StringBuilder text, string title, IReadOnlyList<NetworkRoute> rows)
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

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
