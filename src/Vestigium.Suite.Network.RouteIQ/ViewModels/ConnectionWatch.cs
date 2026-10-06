using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private readonly Dictionary<string, WatchSlot> _slots = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _watch;
    private int _batch;

    public QuietCollection<ConnectionGridRow> Connections { get; } = [];
    public Action<int, int>? ReportWatch { get; set; }
    public ConnectionMarkPalette Marks { get; set; } = new();

    [ObservableProperty]
    private int _watchSeconds = 10;

    [ObservableProperty]
    private string _connectionSummary = "Connections.";

    partial void OnWatchSecondsChanged(int value)
    {
        if (value < 5)
            WatchSeconds = 5;
        else if (value > 180)
            WatchSeconds = 180;
        else
            QueryBook?.SetWatchSeconds(value);
    }

    [RelayCommand]
    private Task SnapshotConnections()
        => FillAsync();

    [RelayCommand]
    private async Task WatchConnections()
    {
        _watch?.Cancel();
        _watch = new CancellationTokenSource();
        var token = _watch.Token;
        var seconds = Math.Clamp(WatchSeconds, 5, 180);
        var until = DateTime.UtcNow.AddSeconds(seconds);
        var reason = "operator";
        RouteIqLog.WatchStarted(seconds);
        try
        {
            ReportWatch?.Invoke(seconds, seconds);
            while (DateTime.UtcNow < until && !token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(true);
                var left = Math.Max(0, (int)Math.Ceiling((until - DateTime.UtcNow).TotalSeconds));
                ReportWatch?.Invoke(left, seconds);
                var rows = await Task.Run(() => NetworkHelper.GetConnections(), token).ConfigureAwait(true);
                ApplyWatch(rows);
                await Paint().ConfigureAwait(true);
            }
        }
        catch (OperationCanceledException)
        {
            reason = "cancel";
        }
        catch (Exception ex)
        {
            Report(ex.Message);
            RouteIqLog.Fail(ex, RouteIqLog.WatchFailedId);
        }
        finally
        {
            RouteIqLog.WatchStopped(reason);
            ReportWatch?.Invoke(0, 0);
        }
    }

    [RelayCommand]
    private void CopyConnections()
    {
        try
        {
            var text = ConnectionSummary + Environment.NewLine
                + "Status     Protocol  Local                         Remote                        Service      State        Process          Time"
                + Environment.NewLine
                + string.Join(Environment.NewLine, Connections.Select(row =>
                    $"{row.Change,-10} {row.Protocol,-8} {row.LocalAddress}:{row.LocalPort,-16} {row.RemoteAddress}:{row.RemotePort,-16} {row.Service,-12} {row.State,-12} {row.Process,-16} {row.TimeSeconds,4}"));
            System.Windows.Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
            RouteIqLog.Fail(ex, RouteIqLog.ClipboardFailedId);
        }
    }

    private async Task LoadConnections(PrintScope scope, CancellationToken cancellationToken)
    {
        var batch = Interlocked.Increment(ref _batch);
        try
        {
            var rows = await Task.Run(() => NetworkHelper.GetConnections(), cancellationToken).ConfigureAwait(false);
            if (batch != _batch || !scope.IsCurrent)
                return;
            var now = DateTime.UtcNow;
            await OnUi(() =>
            {
                if (batch != _batch || !scope.IsCurrent)
                    return;
                TakeSnapshot(rows, now);
            });
            await Paint().ConfigureAwait(false);
            await OnUi(MarkConnectionsReady);
            RouteIqLog.PrintApplied("Connections", rows.Count, scope.Generation);
        }
        catch (Exception ex)
        {
            await OnUi(() => Report("Connections " + ex.Message));
            RouteIqLog.Fail(ex, RouteIqLog.PrintFailedId, SourceBag("Connections"));
        }
        finally
        {
            await OnUi(MarkConnectionsReady);
            MarkPrinted(scope, ConnectionBit);
        }
    }

    private async Task FillAsync()
    {
        var batch = Interlocked.Increment(ref _batch);
        ConnectionSummary = "Reading connections.";
        var rows = await Task.Run(() => NetworkHelper.GetConnections()).ConfigureAwait(true);
        if (batch != _batch)
            return;
        var now = DateTime.UtcNow;
        TakeSnapshot(rows, now);
        await Paint().ConfigureAwait(true);
        MarkConnectionsReady();
    }

    private void TakeSnapshot(IReadOnlyList<NetworkConnection> rows, DateTime now)
    {
        _slots.Clear();
        foreach (var row in rows)
            _slots[Key(row)] = new WatchSlot(row, now, now, "Open");
    }

    private void ApplyWatch(IReadOnlyList<NetworkConnection> rows)
    {
        var now = DateTime.UtcNow;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var key = Key(row);
            seen.Add(key);
            if (!_slots.TryGetValue(key, out var slot))
            {
                _slots[key] = new WatchSlot(row, now, now, "Added");
                continue;
            }

            var change = slot.Change == "Dropped" ? "Reopened" : slot.Change;
            var started = change == "Reopened" && slot.Change == "Dropped" ? now : slot.FirstSeen;
            _slots[key] = slot with { Row = row, FirstSeen = started, LastSeen = now, Change = change };
        }

        foreach (var key in _slots.Keys.ToArray())
        {
            if (seen.Contains(key) || _slots[key].Change == "Dropped")
                continue;
            var slot = _slots[key];
            _slots[key] = slot with { Change = "Dropped", LastSeen = now };
        }
    }

    private async Task Paint()
    {
        var now = DateTime.UtcNow;
        var slots = _slots.Values.ToArray();
        var rows = await Task.Run(() => Project(slots, now)).ConfigureAwait(false);
        await OnUi(() =>
        {
            Connections.Reset(rows);
            ConnectionSummary = Summarize(rows);
        });
    }

    private static ConnectionGridRow[] Project(IReadOnlyList<WatchSlot> slots, DateTime now)
        => slots
            .Select(slot => ToGrid(slot, now))
            .OrderBy(row => ConnectionAddressKey(row.LocalAddress), Comparer<byte[]>.Create(CompareConnectionAddress))
            .ThenBy(row => row.LocalPort)
            .ThenBy(row => Rank(row.Change))
            .ToArray();

    private static string Summarize(IReadOnlyList<ConnectionGridRow> rows)
    {
        var open = 0;
        var added = 0;
        var dropped = 0;
        var reopened = 0;
        foreach (var row in rows)
        {
            if (row.Change == "Open") open++;
            else if (row.Change == "Added") added++;
            else if (row.Change == "Dropped") dropped++;
            else if (row.Change == "Reopened") reopened++;
        }

        return $"Open {open}. Added {added}. Dropped {dropped}. Reopened {reopened}.";
    }

    private static ConnectionGridRow ToGrid(WatchSlot slot, DateTime now)
    {
        var end = slot.Change == "Dropped" ? slot.LastSeen : now;
        var time = (int)Math.Max(0, (end - slot.FirstSeen).TotalSeconds);
        var remote = string.IsNullOrWhiteSpace(slot.Row.RemoteAddress) ? "--" : slot.Row.RemoteAddress;
        var remotePort = slot.Row.RemotePort?.ToString() ?? "--";
        var process = string.IsNullOrWhiteSpace(slot.Row.ProcessName) ? slot.Row.ProcessId?.ToString() ?? "--" : slot.Row.ProcessName;
        var service = ConnectionServices.Label(slot.Row.Protocol.ToString(), slot.Row.RemotePort, slot.Row.LocalPort);
        return new ConnectionGridRow(slot.Change, Mark(slot.Change), slot.Row.Protocol.ToString(), slot.Row.LocalAddress, slot.Row.LocalPort, remote, remotePort, service, slot.Row.State ?? "--", process, time);
    }

    private static string Mark(string change) => change switch
    {
        "Added" => "\uE710",
        "Dropped" => "\uE738",
        "Reopened" => "\uE72C",
        _ => "\uEA3B"
    };

    private static int Rank(string change) => change switch { "Added" => 0, "Reopened" => 1, "Dropped" => 2, _ => 3 };

    private static byte[] ConnectionAddressKey(string? text)
    {
        if (!IPAddress.TryParse(text, out var address))
            return [0xFF];
        if (address.AddressFamily == AddressFamily.InterNetworkV6 && address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();
        return address.GetAddressBytes();
    }

    private static int CompareConnectionAddress(byte[] left, byte[] right)
    {
        var family = left.Length.CompareTo(right.Length);
        if (family != 0)
            return family;
        var count = Math.Min(left.Length, right.Length);
        for (var i = 0; i < count; i++)
        {
            var diff = left[i].CompareTo(right[i]);
            if (diff != 0)
                return diff;
        }

        return 0;
    }

    private static string Key(NetworkConnection row)
        => row.Protocol + "|" + row.LocalAddress + "|" + row.LocalPort + "|" + row.RemoteAddress + "|" + row.RemotePort;

    private sealed record WatchSlot(NetworkConnection Row, DateTime FirstSeen, DateTime LastSeen, string Change);
}
