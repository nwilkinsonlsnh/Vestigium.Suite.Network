using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const string NoValue = "--";
    private const int OuiGapMs = 1200;
    private const int Ipv4Bit = 1;
    private const int Ipv6Bit = 2;
    private const int NeighborBit = 4;
    private const int ConnectionBit = 8;
    private const int NetBiosBit = 16;
    private const int LmHostBit = 32;
    private const int AllPrints = 63;
    private bool _busy;
    private CancellationTokenSource? _rtt;
    private bool _connectionsReady;
    private int _printedGen;
    private int _printedMask;
    private readonly Lock _printedGate = new();
    private readonly SemaphoreSlim _ouiPace = new(1, 1);
    private readonly PrintCoordinator _prints;
    private DateTime _ouiSentAt = DateTime.UtcNow.AddSeconds(-2);

    public QuietCollection<NetworkRoute> Ipv4Routes { get; } = [];
    public QuietCollection<NetworkRoute> Ipv6Routes { get; } = [];
    public QuietCollection<NeighborGridRow> Ipv4Neighbors { get; } = [];
    public QuietCollection<NeighborGridRow> Ipv6Neighbors { get; } = [];
    public QuietCollection<NetworkNetBiosName> NetBiosNames { get; } = [];
    public QuietCollection<NetworkLmHostEntry> LmHosts { get; } = [];
    public Action<string>? ReportStatus { get; set; }
    public Action<string, double>? ReportSplash { get; set; }
    public bool PrintsReady => _connectionsReady && _printedMask == AllPrints && _printedGen == _prints.Generation;
    public Func<int>? OuiPoolSize { get; set; }
    public Func<bool>? LiveVendorLookup { get; set; }
    public const string LiveVendorUrl = "https://api.macvendors.com/{oui}";

    public MainViewModel()
    {
        _prints = new PrintCoordinator(
            LoadIpv4,
            LoadIpv6,
            LoadNeighbors,
            LoadConnections,
            LoadNetBios,
            LoadLmHosts,
            OnPrintProgress,
            OnPrintFault,
            RouteIqLog.PrintRequested);
    }

    public Task BeginPrints()
        => _prints.Request();

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshRoutes() => RefreshPage(PrintCoordinator.RouteMask, vendors: false);

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshNeighbors() => RefreshPage(PrintCoordinator.NeighborMask, vendors: true);

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshNetBios() => RefreshPage(PrintCoordinator.NetBiosMask, vendors: false);

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private Task RefreshLmHosts() => RefreshPage(PrintCoordinator.LmHostMask, vendors: false);

    private async Task RefreshPage(int mask, bool vendors)
    {
        if (_busy)
        {
            _prints.Request(mask);
            return;
        }

        _busy = true;
        RaiseCanExecute();
        Report(string.Empty);
        try
        {
            await _prints.Request(mask).ConfigureAwait(true);
            if (vendors && LiveVendorLookup?.Invoke() == true)
                Report(await ResolveLiveVendors().ConfigureAwait(true));
        }
        catch (Exception ex)
        {
            Report(ex.Message);
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId);
        }
        finally
        {
            _busy = false;
            RaiseCanExecute();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void Copy() { try { Clipboard.SetText(FormatTables()); } catch (Exception ex) { Report(ex.Message); RouteIqLog.Fail(ex, RouteIqLog.ClipboardFailedId); } }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyNeighbors() { try { Clipboard.SetText(FormatNeighbors()); } catch (Exception ex) { Report(ex.Message); RouteIqLog.Fail(ex, RouteIqLog.ClipboardFailedId); } }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyNetBios() { try { Clipboard.SetText(FormatNetBios()); } catch (Exception ex) { Report(ex.Message); RouteIqLog.Fail(ex, RouteIqLog.ClipboardFailedId); } }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyLmHosts() { try { Clipboard.SetText(FormatLmHosts()); } catch (Exception ex) { Report(ex.Message); RouteIqLog.Fail(ex, RouteIqLog.ClipboardFailedId); } }

    private bool CanRefresh() => !_busy;
    private bool CanCopy() => !_busy;

    private void RaiseCanExecute()
    {
        RefreshRoutesCommand.NotifyCanExecuteChanged();
        RefreshNeighborsCommand.NotifyCanExecuteChanged();
        RefreshNetBiosCommand.NotifyCanExecuteChanged();
        RefreshLmHostsCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        CopyNeighborsCommand.NotifyCanExecuteChanged();
        CopyNetBiosCommand.NotifyCanExecuteChanged();
        CopyLmHostsCommand.NotifyCanExecuteChanged();
        ExportSelectedCommand.NotifyCanExecuteChanged();
        ExportAllCommand.NotifyCanExecuteChanged();
    }

    private void Report(string text) => ReportStatus?.Invoke(text);

    private void OnPrintProgress(PrintProgress progress)
    {
        var percent = progress.Started == 0 ? 0d : progress.Finished * 100d / progress.Started;
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            ReportSplash?.Invoke(progress.Source, percent);
            return;
        }

        dispatcher.BeginInvoke(() => ReportSplash?.Invoke(progress.Source, percent));
    }

    private void OnPrintFault(string source, Exception ex)
    {
        try
        {
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag(source));
            OnUi(() => Report(source + " " + ex.Message));
        }
        catch (Exception second)
        {
            RouteIqLog.Fail(second);
        }
    }

    private static Dictionary<string, string?> SourceBag(string source)
        => new(StringComparer.Ordinal) { ["source"] = source };

    private async Task LoadIpv4(PrintScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await Task.Run(() => ByAddress(NetworkHelper.GetRoutes(RouteFamily.Pv4), row => row.Destination), cancellationToken).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() => Replace(Ipv4Routes, rows));
            RouteIqLog.PrintApplied("IPv4 routes", rows.Count, scope.Generation);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("IPv4 routes " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("IPv4 routes"));
        }
        finally
        {
            MarkPrinted(scope, Ipv4Bit);
        }
    }

    private async Task LoadIpv6(PrintScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await Task.Run(() => ByAddress(NetworkHelper.GetRoutes(RouteFamily.Pv6), row => row.Destination), cancellationToken).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() => Replace(Ipv6Routes, rows));
            RouteIqLog.PrintApplied("IPv6 routes", rows.Count, scope.Generation);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("IPv6 routes " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("IPv6 routes"));
        }
        finally
        {
            MarkPrinted(scope, Ipv6Bit);
        }
    }

    private async Task LoadNeighbors(PrintScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await Task.Run(() =>
            {
                var neighbors = NetworkHelper.GetNeighbors();
                var v4 = ByAddress(neighbors.Where(row => row.Family == AddressFamily.InterNetwork), row => row.Address);
                var v6 = ByAddress(neighbors.Where(row => row.Family == AddressFamily.InterNetworkV6), row => row.Address);
                return (v4, v6);
            }, cancellationToken).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() =>
            {
                Replace(Ipv4Neighbors, Blank(rows.v4));
                Replace(Ipv6Neighbors, Blank(rows.v6));
            });
            var packed4 = await Task.Run(() => ApplyPacked(rows.v4)).ConfigureAwait(false);
            var packed6 = await Task.Run(() => ApplyPacked(rows.v6)).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() =>
            {
                Replace(Ipv4Neighbors, packed4);
                Replace(Ipv6Neighbors, packed6);
            });
            RouteIqLog.PrintApplied("Neighbors", packed4.Count + packed6.Count, scope.Generation);
            StartRtt(scope, packed4, packed6);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("Neighbors " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("Neighbors"));
        }
        finally
        {
            MarkPrinted(scope, NeighborBit);
        }
    }

    private async Task LoadNetBios(PrintScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var names = await Task.Run(
                () => NetworkHelper.GetNetBiosNames().OrderBy(row => row.IsCache).ThenBy(row => row.Name, StringComparer.OrdinalIgnoreCase).ToArray(),
                cancellationToken).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() =>
            {
                Replace(NetBiosNames, names);
                ApplyNetBiosStats();
            });
            RouteIqLog.PrintApplied("NetBIOS", names.Length, scope.Generation);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("NetBIOS " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("NetBIOS"));
        }
        finally
        {
            MarkPrinted(scope, NetBiosBit);
        }
    }

    private async Task LoadLmHosts(PrintScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var hosts = await Task.Run(() => NetworkHelper.GetLmHosts(), cancellationToken).ConfigureAwait(false);
            if (!scope.IsCurrent)
                return;
            await OnUi(() =>
            {
                Replace(LmHosts, hosts);
                ApplyLmHostSummary(hosts.Count);
            });
            RouteIqLog.PrintApplied("LMHOSTS", hosts.Count, scope.Generation);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("LMHOSTS " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("LMHOSTS"));
        }
        finally
        {
            MarkPrinted(scope, LmHostBit);
        }
    }

    private void MarkPrinted(PrintScope scope, int bit)
    {
        if (!scope.IsCurrent)
            return;
        lock (_printedGate)
        {
            if (scope.Generation != _printedGen)
            {
                _printedGen = scope.Generation;
                _printedMask = 0;
            }

            _printedMask |= bit;
        }
    }

    private static NeighborGridRow[] Blank(IReadOnlyList<NetworkNeighbor> rows)
        => rows.Select(row => new NeighborGridRow(row, NoValue, NoValue)).ToArray();

    private static IReadOnlyList<NeighborGridRow> ApplyPacked(IReadOnlyList<NetworkNeighbor> rows)
    {
        return rows.Select(row =>
        {
            if (!CanLookup(row.MacAddress))
                return new NeighborGridRow(row, NoValue, NoValue);
            var hit = NetworkHelper.LookupOuiPacked(row.MacAddress!);
            if (hit is null || string.IsNullOrWhiteSpace(hit.Vendor))
                return new NeighborGridRow(row, NoValue, NoValue);
            return new NeighborGridRow(row with { Vendor = hit.Vendor }, hit.Vendor, NoValue);
        }).ToArray();
    }

    private async Task<string> ResolveLiveVendors()
    {
        var pending = Ipv4Neighbors.Concat(Ipv6Neighbors).Where(row => row.VendorText == NoValue && CanLookup(row.MacAddress)).GroupBy(row => Oui(row.MacAddress!), StringComparer.OrdinalIgnoreCase).Select(group => group.First()).ToArray();
        if (pending.Length == 0)
            return "Vendor lookups complete. None to ask.";
        var pool = OuiPoolSize?.Invoke() ?? 10;
        if (pool < 1) pool = 1;
        if (pool > 20) pool = 20;
        var misses = 0;
        var filled = 0;
        Exception? vendorFault = null;
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
                var vendor = hit?.Vendor;
                await OnUi(() =>
                {
                    if (string.IsNullOrWhiteSpace(vendor)) { misses++; StampVendor(Ipv4Neighbors, oui, null); StampVendor(Ipv6Neighbors, oui, null); }
                    else { filled++; StampVendor(Ipv4Neighbors, oui, vendor); StampVendor(Ipv6Neighbors, oui, vendor); }
                });
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref vendorFault, ex, null);
                var oui = Oui(sample.MacAddress);
                await OnUi(() => { misses++; StampVendor(Ipv4Neighbors, oui, null); StampVendor(Ipv6Neighbors, oui, null); });
            }
            finally { slots.Release(); }
        });
        await Task.WhenAll(tasks).ConfigureAwait(true);
        if (vendorFault is not null)
            RouteIqLog.Fail(vendorFault, RouteIqLog.PrintFailedId, SourceBag(RouteIqLog.VendorSource));
        return $"Vendor lookups complete. {filled} found, {misses} missed.";
    }

    private static Task OnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null)
            return Task.CompletedTask;
        return dispatcher.InvokeAsync(action).Task;
    }

    private void StartRtt(PrintScope scope, IReadOnlyList<NeighborGridRow> v4, IReadOnlyList<NeighborGridRow> v6)
    {
        _rtt?.Cancel();
        _rtt = new CancellationTokenSource();
        var targets = v4.Concat(v6)
            .Where(row => CanPing(row.Address))
            .Select(row => (row.Address, EchoTarget(row)))
            .ToArray();
        _ = FillRtt(scope, targets, _rtt.Token);
    }

    private async Task FillRtt(PrintScope scope, (string Address, string Target)[] targets, CancellationToken token)
    {
        Exception? first = null;
        using var slots = new SemaphoreSlim(4, 4);
        var tasks = targets.Select(async target =>
        {
            await slots.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var options = new IcmpEchoOptions { Count = 1, Timeout = TimeSpan.FromSeconds(1), Interval = TimeSpan.FromMilliseconds(200), Ttl = 128 };
                var result = await NetworkHelper.Ping(target.Target, options).RunAsync().ConfigureAwait(false);
                if (token.IsCancellationRequested || !scope.IsCurrent)
                    return;
                var rtt = result.Received > 0 && result.AverageMs is not null
                    ? Math.Round(result.AverageMs.Value).ToString(CultureInfo.InvariantCulture)
                    : NoValue;
                await OnUi(() =>
                {
                    if (scope.IsCurrent)
                        StampProbe(target.Address, rtt);
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Interlocked.CompareExchange(ref first, ex, null);
                if (!token.IsCancellationRequested && scope.IsCurrent)
                    await OnUi(() =>
                    {
                        if (scope.IsCurrent)
                            StampProbe(target.Address, NoValue);
                    });
            }
            finally
            {
                slots.Release();
            }
        });
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (first is not null && scope.IsCurrent)
            RouteIqLog.Fail(first, RouteIqLog.ProbeFailedId);
    }

    private static string EchoTarget(NeighborGridRow row)
    {
        if (IPAddress.TryParse(row.Address, out var ip) && ip.IsIPv6LinkLocal && row.InterfaceIndex is > 0)
            return row.Address + "%" + row.InterfaceIndex.Value.ToString(CultureInfo.InvariantCulture);
        return row.Address;
    }

    private async Task PaceOuiSend()
    {
        await _ouiPace.WaitAsync().ConfigureAwait(false);
        try
        {
            var wait = OuiGapMs - (int)(DateTime.UtcNow - _ouiSentAt).TotalMilliseconds;
            if (wait > 0) await Task.Delay(wait).ConfigureAwait(false);
            _ouiSentAt = DateTime.UtcNow;
        }
        finally { _ouiPace.Release(); }
    }

    private static void StampVendor(QuietCollection<NeighborGridRow> rows, string oui, string? vendor)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!string.Equals(Oui(row.MacAddress), oui, StringComparison.OrdinalIgnoreCase)) continue;
            var source = string.IsNullOrWhiteSpace(vendor) ? row.Source with { Vendor = null } : row.Source with { Vendor = vendor };
            rows[i] = new NeighborGridRow(source, string.IsNullOrWhiteSpace(vendor) ? NoValue : vendor, row.RttMs, row.Hops);
        }
    }

    private void StampProbe(string address, string rtt)
    {
        StampProbe(Ipv4Neighbors, address, rtt);
        StampProbe(Ipv6Neighbors, address, rtt);
    }

    private static void StampProbe(ObservableCollection<NeighborGridRow> rows, string address, string rtt)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!string.Equals(row.Address, address, StringComparison.OrdinalIgnoreCase)) continue;
            rows[i] = row with { RttMs = string.IsNullOrWhiteSpace(rtt) ? NoValue : rtt };
        }
    }

    private static bool CanLookup(string? mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return false;
        var parts = mac.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 6 || !byte.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var first)) return false;
        if ((first & 0x01) != 0) return false;
        return !mac.StartsWith("FF:FF:FF", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanPing(string? address)
    {
        if (!IPAddress.TryParse(address, out var ip)) return false;
        if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.IPv6Any) || ip.Equals(IPAddress.Any)) return false;
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

    private static byte[] AddressKey(string? text) => IPAddress.TryParse(text, out var address) ? address.GetAddressBytes() : [0xFF];

    private static int CompareBytes(byte[] left, byte[] right)
    {
        var count = Math.Min(left.Length, right.Length);
        for (var i = 0; i < count; i++)
        {
            var diff = left[i].CompareTo(right[i]);
            if (diff != 0) return diff;
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

    private string FormatNetBios()
    {
        var text = new StringBuilder();
        text.AppendLine(NetBiosSummary);
        text.AppendLine("Cache  Adapter              Node             Name                 Suffix  Meaning            Type      Status           Address          Life");
        foreach (var row in NetBiosNames)
        {
            text.Append(Pad(row.IsCache ? "True" : "False", 7));
            text.Append(Pad(row.Adapter, 21));
            text.Append(Pad(row.NodeAddress, 17));
            text.Append(Pad(row.Name, 21));
            text.Append(Pad(row.Suffix, 8));
            text.Append(Pad(row.SuffixName, 19));
            text.Append(Pad(row.Type, 10));
            text.Append(Pad(row.Status, 17));
            text.Append(Pad(row.Address, 17));
            text.AppendLine(row.LifeSeconds?.ToString(CultureInfo.InvariantCulture));
        }
        if (NetBiosNames.Count == 0) text.AppendLine("None");
        return text.ToString();
    }

    private string FormatLmHosts()
    {
        var text = new StringBuilder();
        text.AppendLine(LmHostSummary);
        text.AppendLine("Address          Name                 Preload  Domain           MultiHome  Include");
        foreach (var row in LmHosts)
        {
            text.Append(Pad(row.Address, 17));
            text.Append(Pad(row.Name, 21));
            text.Append(Pad(row.Preload ? "True" : "False", 9));
            text.Append(Pad(row.Domain, 17));
            text.Append(Pad(row.MultiHome ? "True" : "False", 11));
            text.AppendLine(row.IncludePath ?? NoValue);
        }
        if (LmHosts.Count == 0) text.AppendLine("None");
        return text.ToString();
    }

    private static void AppendNeighbors(StringBuilder text, string title, IReadOnlyList<NeighborGridRow> rows)
    {
        if (text.Length > 0) text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Address              MAC                Interface            State            Multicast  Vendor               RTT");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Address, 21));
            text.Append(Pad(row.MacAddress, 19));
            text.Append(Pad(row.InterfaceName, 21));
            text.Append(Pad(row.State, 17));
            text.Append(Pad(row.IsMulticast ? "True" : "False", 11));
            text.Append(Pad(row.VendorText, 21));
            text.AppendLine(row.RttMs);
        }
        if (rows.Count == 0) text.AppendLine("None");
    }

    private static void AppendRoutes(StringBuilder text, string title, IReadOnlyList<NetworkRoute> rows)
    {
        if (text.Length > 0) text.AppendLine();
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
        if (rows.Count == 0) text.AppendLine("None");
    }

    private static string Pad(string? value, int width)
    {
        var text = value ?? NoValue;
        return text.Length >= width ? text + " " : text.PadRight(width);
    }

    private static void Replace<T>(QuietCollection<T> target, IReadOnlyList<T> source)
        => target.Reset(source);
}
