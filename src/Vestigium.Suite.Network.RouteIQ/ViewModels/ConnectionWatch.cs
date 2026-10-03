using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private readonly Dictionary<string, WatchSlot> _slots = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _watch;

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

    public void LoadConnections(IReadOnlyList<NetworkConnection> rows)
    {
        var now = DateTime.UtcNow;
        _slots.Clear();
        foreach (var row in rows)
            _slots[Key(row)] = new WatchSlot(row, now, now, false, 0);
        Publish(now);
    }

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
                var rows = await Task.Run(() => NetworkHelper.GetConnections()).ConfigureAwait(true);
                ApplyWatch(rows);
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
                + "Change    Protocol  Local                         Remote                        State        Process          Time  Returns"
                + Environment.NewLine
                + string.Join(Environment.NewLine, Connections.Select(row =>
                    $"{row.Change,-9} {row.Protocol,-8} {row.LocalAddress}:{row.LocalPort,-16} {row.RemoteAddress}:{row.RemotePort,-16} {row.State,-12} {row.Process,-16} {row.TimeSeconds,4}  {row.Returns}"));
            System.Windows.Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    private void ApplyWatch(IReadOnlyList<NetworkConnection> rows)
    {
        var now = DateTime.UtcNow;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var key = Key(row);
            seen.Add(key);
            if (_slots.TryGetValue(key, out var slot))
            {
                var returns = slot.Dropped ? slot.Returns + 1 : slot.Returns;
                _slots[key] = slot with { Row = row, LastSeen = now, Dropped = false, Returns = returns };
            }
            else
            {
                _slots[key] = new WatchSlot(row, now, now, false, 0);
            }
        }

        foreach (var key in _slots.Keys.ToArray())
        {
            if (!seen.Contains(key))
                _slots[key] = _slots[key] with { Dropped = true, LastSeen = now };
        }

        Publish(now);
    }

    private void Publish(DateTime now)
    {
        var rows = _slots.Values
            .Select(slot => ToGrid(slot, now))
            .OrderBy(row => Rank(row.Change))
            .ThenBy(row => row.LocalPort)
            .ToArray();
        Replace(Connections, rows);
        var added = rows.Count(row => row.Change == "Added");
        var dropped = rows.Count(row => row.Change == "Dropped");
        var returned = rows.Count(row => row.Change == "Returned");
        var open = rows.Length - dropped;
        ConnectionSummary = $"Open {open}. Added {added}. Dropped {dropped}. Returned {returned}.";
    }

    private static ConnectionGridRow ToGrid(WatchSlot slot, DateTime now)
    {
        var fresh = (now - slot.FirstSeen).TotalSeconds < 2;
        var change = slot.Dropped ? "Dropped" : slot.Returns > 0 ? "Returned" : fresh ? "Added" : "Open";
        var end = slot.Dropped ? slot.LastSeen : now;
        var time = (int)Math.Max(0, (end - slot.FirstSeen).TotalSeconds);
        var remote = string.IsNullOrWhiteSpace(slot.Row.RemoteAddress) ? "--" : slot.Row.RemoteAddress;
        var remotePort = slot.Row.RemotePort?.ToString() ?? "--";
        var process = string.IsNullOrWhiteSpace(slot.Row.ProcessName) ? slot.Row.ProcessId?.ToString() ?? "--" : slot.Row.ProcessName;
        return new ConnectionGridRow(change, slot.Row.Protocol.ToString(), slot.Row.LocalAddress, slot.Row.LocalPort, remote, remotePort, slot.Row.State ?? "--", process, time, slot.Returns);
    }

    private static int Rank(string change) => change switch { "Added" => 0, "Returned" => 1, "Dropped" => 2, _ => 3 };

    private static string Key(NetworkConnection row)
        => row.Protocol + "|" + row.LocalAddress + "|" + row.LocalPort + "|" + row.RemoteAddress + "|" + row.RemotePort + "|" + row.ProcessId;

    private sealed record WatchSlot(NetworkConnection Row, DateTime FirstSeen, DateTime LastSeen, bool Dropped, int Returns);
}
