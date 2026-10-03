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
    private const int OuiGapMs = 1200;
    private bool _busy;
    private readonly SemaphoreSlim _ouiPace = new(1, 1);
    private DateTime _ouiSentAt = DateTime.UtcNow.AddSeconds(-2);

    public ObservableCollection<NetworkRoute> Ipv4Routes { get; } = [];
    public ObservableCollection<NetworkRoute> Ipv6Routes { get; } = [];
    public ObservableCollection<NeighborGridRow> Ipv4Neighbors { get; } = [];
    public ObservableCollection<NeighborGridRow> Ipv6Neighbors { get; } = [];
    public Action<string>? ReportStatus { get; set; }
    public Func<int>? OuiPoolSize { get; set; }

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
            var vendors = ResolveLiveVendors();
            var probes = ProbeNeighbors();
            var vendorLine = await vendors.ConfigureAwait(true);
            Report(vendorLine);
            await probes.ConfigureAwait(true);
            Report(vendorLine);
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
        try { Clipboard.SetText(FormatTables()); }
        catch (Exception ex) { Report(ex.Message); }
    }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyNeighbors()
    {
        try { Clipboard.SetText(FormatNeighbors()); }
        catch (Exception ex) { Report(ex.Message); }
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

    private static IReadOnlyList<NeighborGridRow> ApplyPacked(IReadOnlyList<NetworkNeighbor> rows)
    {
        return rows.Select(row =>
        {
            if (!CanLookup(row.MacAddress))
                return new NeighborGridRow(row, null);
            var hit = NetworkHelper.LookupOuiPacked(row.MacAddress!);
            if (string.IsNullOrWhiteSpace(hit.Vendor))
                return new NeighborGridRow(row, null);
            return new NeighborGridRow(row with { Vendor = hit.Vendor }, hit.Vendor);
        }).ToArray();
    }

    private async Task<string> ResolveLiveVendors()
    {
        var pending = Ipv4Neighbors.Concat(Ipv6Neighbors)
            .Where(row => string.IsNullOrWhiteSpace(row.VendorText) && CanLookup(row.MacAddress))
            .GroupBy(row => Oui(row.MacAddress!), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        if (pending.Length == 0)
            return "Vendor lookups complete. None to ask.";

        var pool = OuiPoolSize?.Invoke() ?? 10;
        if (pool < 1) pool = 1;
        if (pool > 20) pool = 20;
        var misses = 0;
        var filled = 0;
        var options = new OuiLookupOptions { Timeout = TimeSpan.FromSeconds(8) };
        using var slots = new SemaphoreSlim(pool, pool);
        var tasks = pending.Select(async sample =>
        {
            await slots.WaitAsync().ConfigureAwait(false);
            try
            {
                var mac = sample.MacAddress!;
                var oui = Oui(mac);
                await PaceOuiSend().ConfigureAwait(false);
                var hit = await NetworkHelper.LookupOuiAsync(mac, options).ConfigureAwait(false);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (string.IsNullOrWhiteSpace(hit.Vendor))
                    {
                        misses++;
                        StampVendor(Ipv4Neighbors, oui, null);
                        StampVendor(Ipv6Neighbors, oui, null);
                    }
                    else
                    {
                        filled++;
                        StampVendor(Ipv4Neighbors, oui, hit.Vendor);
                        StampVendor(Ipv6Neighbors, oui, hit.Vendor);
                    }
                });
            }
            catch (Exception)
            {
                var oui = Oui(sample.MacAddress);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    misses++;
                    StampVendor(Ipv4Neighbors, oui, null);
                    StampVendor(Ipv6Neighbors, oui, null);
                });
            }
            finally
            {
                slots.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(true);
        return $"Vendor lookups complete. {filled} found, {misses} missed.";
    }

    private async Task ProbeNeighbors()
    {
        var pending = Ipv4Neighbors.Concat(Ipv6Neighbors)
            .Where(row => CanPing(row.Address))
            .GroupBy(row => row.Address, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        var options = new IcmpEchoOptions
        {
            Count = 1,
            Timeout = TimeSpan.FromSeconds(1),
            Interval = TimeSpan.FromMilliseconds(200),
            Ttl = 128
        };
        using var slots = new SemaphoreSlim(4, 4);
        var tasks = pending.Select(async row =>
        {
            await slots.WaitAsync().ConfigureAwait(false);
            try
            {
                var target = EchoTarget(row);
                var result = await NetworkHelper.Ping(target, options).RunAsync().ConfigureAwait(false);
                var reply = result.Replies.FirstOrDefault();
                var rtt = result.Received > 0 && result.AverageMs is not null
                    ? Math.Round(result.AverageMs.Value).ToString(CultureInfo.InvariantCulture)
                    : "--";
                var hops = reply is null ? "--" : EstimateHops(reply.Ttl).ToString(CultureInfo.InvariantCulture);
                await Application.Current.Dispatcher.InvokeAsync(() => StampProbe(row.Address, rtt, hops));
            }
            catch (Exception)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => StampProbe(row.Address, "--", "--"));
            }
            finally
            {
                slots.Release();
            }
        });
        await Task.WhenAll(tasks).ConfigureAwait(true);
    }

    private static string EchoTarget(NeighborGridRow row)
    {
        if (IPAddress.TryParse(row.Address, out var ip) && ip.IsIPv6LinkLocal && row.InterfaceIndex is > 0)
            return row.Address + "%" + row.InterfaceIndex.Value.ToString(CultureInfo.InvariantCulture);
        return row.Address;
    }

    private static int EstimateHops(int ttl)
    {
        if (ttl <= 0)
            return 0;
        var origin = ttl <= 64 ? 64 : ttl <= 128 ? 128 : 255;
        return Math.Max(0, origin - ttl);
    }

    private async Task PaceOuiSend()
    {
        await _ouiPace.WaitAsync().ConfigureAwait(false);
        try
        {
            var wait = OuiGapMs - (int)(DateTime.UtcNow - _ouiSentAt).TotalMilliseconds;
            if (wait > 0)
                await Task.Delay(wait).ConfigureAwait(false);
            _ouiSentAt = DateTime.UtcNow;
        }
        finally
        {
            _ouiPace.Release();
        }
    }

    private static void StampVendor(ObservableCollection<NeighborGridRow> rows, string oui, string? vendor)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!string.Equals(Oui(row.MacAddress), oui, StringComparison.OrdinalIgnoreCase))
                continue;
            var source = string.IsNullOrWhiteSpace(vendor) ? row.Source with { Vendor = null } : row.Source with { Vendor = vendor };
            rows[i] = new NeighborGridRow(source, string.IsNullOrWhiteSpace(vendor) ? "--" : vendor, row.RttMs, row.Hops);
        }
    }

    private void StampProbe(string address, string rtt, string hops)
    {
        StampProbe(Ipv4Neighbors, address, rtt, hops);
        StampProbe(Ipv6Neighbors, address, rtt, hops);
    }

    private static void StampProbe(ObservableCollection<NeighborGridRow> rows, string address, string rtt, string hops)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!string.Equals(row.Address, address, StringComparison.OrdinalIgnoreCase))
                continue;
            rows[i] = row with { RttMs = rtt, Hops = hops };
        }
    }

    private static bool CanLookup(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac))
            return false;
        var parts = mac.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 6 || !byte.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var first))
            return false;
        if ((first & 0x01) != 0)
            return false;
        return !mac.StartsWith("FF:FF:FF", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanPing(string? address)
    {
        if (!IPAddress.TryParse(address, out var ip))
            return false;
        if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.IPv6Any) || ip.Equals(IPAddress.Any))
            return false;
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var first = ip.GetAddressBytes()[0];
            return first is < 224 and not 0;
        }

        return !ip.IsIPv6Multicast;
    }

    private static string Oui(string? mac)
    {
        var parts = mac?.Split(':', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return parts.Length < 3 ? string.Empty : string.Join(':', parts[0], parts[1], parts[2]);
    }

    private static IReadOnlyList<T> ByAddress<T>(IEnumerable<T> rows, Func<T, string?> address)
        => rows.OrderBy(row => AddressKey(address(row)), Comparer<byte[]>.Create(CompareBytes)).ToArray();

    private static byte[] AddressKey(string? text)
        => IPAddress.TryParse(text, out var address) ? address.GetAddressBytes() : [0xFF];

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

    private static void AppendNeighbors(StringBuilder text, string title, IReadOnlyList<NeighborGridRow> rows)
    {
        if (text.Length > 0)
            text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Address              MAC                Interface            State            Multicast  Vendor               RTT   Hops");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Address, 21));
            text.Append(Pad(row.MacAddress, 19));
            text.Append(Pad(row.InterfaceName, 21));
            text.Append(Pad(row.State, 17));
            text.Append(Pad(row.IsMulticast ? "True" : "False", 11));
            text.Append(Pad(row.VendorText, 21));
            text.Append(Pad(row.RttMs, 6));
            text.AppendLine(row.Hops);
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
