using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private readonly Dictionary<string, List<WatchSlot>> _slots = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _watch;
    private int _batch;

    public ObservableCollection<ConnectionGridRow> Connections { get; } = [];

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
        try
        {
            while (DateTime.UtcNow < until && !token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(true);
                var rows = await Task.Run(() => NetworkHelper.GetConnections(), token).ConfigureAwait(true);
                ApplyWatch(rows);
                await Paint().ConfigureAwait(true);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    [RelayCommand]
    private void CopyConnections()
    {
        try
        {
            var text = ConnectionSummary + Environment.NewLine
                + "Change     Protocol  Local                         Remote                        State        Process          Time"
                + Environment.NewLine
                + string.Join(Environment.NewLine, Connections.Select(row =>
                    $"{row.Change,-10} {row.Protocol,-8} {row.LocalAddress}:{row.LocalPort,-16} {row.RemoteAddress}:{row.RemotePort,-16} {row.State,-12} {row.Process,-16} {row.TimeSeconds,4}"));
            System.Windows.Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
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
        _slots.Clear();
        foreach (var row in rows)
            _slots[Key(row)] = [new WatchSlot(row, now, now, "Open")];
        await Paint().ConfigureAwait(true);
    }

    private void ApplyWatch(IReadOnlyList<NetworkConnection> rows)
    {
        var now = DateTime.UtcNow;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var key = Key(row);
            seen.Add(key);
            if (!_slots.TryGetValue(key, out var list))
            {
                _slots[key] = [new WatchSlot(row, now, now, "Added")];
                continue;
            }

            var active = list.FindIndex(slot => slot.Change != "Dropped");
            if (active >= 0)
            {
                var slot = list[active];
                list[active] = slot with { Row = row, LastSeen = now };
                continue;
            }

            list.Add(new WatchSlot(row, now, now, "Returned"));
        }

        foreach (var key in _slots.Keys)
        {
            if (seen.Contains(key))
                continue;
            var list = _slots[key];
            var active = list.FindIndex(slot => slot.Change != "Dropped");
            if (active < 0)
                continue;
            var slot = list[active];
            list[active] = slot with { Change = "Dropped", LastSeen = now };
        }
    }

    private async Task Paint()
    {
        var now = DateTime.UtcNow;
        var rows = _slots.Values
            .SelectMany(list => list)
            .Select(slot => ToGrid(slot, now))
            .OrderBy(row => ConnectionAddressKey(row.LocalAddress), Comparer<byte[]>.Create(CompareConnectionAddress))
            .ThenBy(row => row.LocalPort)
            .ThenBy(row => Rank(row.Change))
            .ToArray();
        Connections.Clear();
        for (var i = 0; i < rows.Length; i++)
        {
            Connections.Add(rows[i]);
            if (i > 0 && i % 40 == 0)
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
        }

        var open = 0;
        var added = 0;
        var dropped = 0;
        var returned = 0;
        foreach (var row in rows)
        {
            if (row.Change == "Open") open++;
            else if (row.Change == "Added") added++;
            else if (row.Change == "Dropped") dropped++;
            else if (row.Change == "Returned") returned++;
        }

        ConnectionSummary = $"Open {open}. Added {added}. Dropped {dropped}. Returned {returned}.";
    }

    private static ConnectionGridRow ToGrid(WatchSlot slot, DateTime now)
    {
        var end = slot.Change == "Dropped" ? slot.LastSeen : now;
        var time = (int)Math.Max(0, (end - slot.FirstSeen).TotalSeconds);
        var remote = string.IsNullOrWhiteSpace(slot.Row.RemoteAddress) ? "--" : slot.Row.RemoteAddress;
        var remotePort = slot.Row.RemotePort?.ToString() ?? "--";
        var process = string.IsNullOrWhiteSpace(slot.Row.ProcessName) ? slot.Row.ProcessId?.ToString() ?? "--" : slot.Row.ProcessName;
        return new ConnectionGridRow(slot.Change, Mark(slot.Change), slot.Row.Protocol.ToString(), slot.Row.LocalAddress, slot.Row.LocalPort, remote, remotePort, slot.Row.State ?? "--", process, time);
    }

    private static string Mark(string change) => change switch
    {
        "Added" => "\uE710",
        "Dropped" => "\uE738",
        "Returned" => "\uE72C",
        _ => "\uEA3B"
    };

    private static int Rank(string change) => change switch { "Open" => 0, "Added" => 1, "Returned" => 2, "Dropped" => 3, _ => 4 };

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
        => row.Protocol + "|" + row.LocalAddress + "|" + row.LocalPort + "|" + row.RemoteAddress + "|" + row.RemotePort + "|" + row.ProcessId;

    private sealed record WatchSlot(NetworkConnection Row, DateTime FirstSeen, DateTime LastSeen, string Change);
}
