# PingIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.PingIQ`

## Window

Top: Target, Count (default 4), Timeout, Interface index, Source, Echo, Cancel.  
Middle: Status + sent/recv/lost/min/max/avg.  
Bottom: `DataGrid` or `ListView` of replies (Sequence, Status, Address, RttMs, Detail).

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.PingIQ)` then base. |
| `MainWindow` | Sets `DataContext` to `MainViewModel`. No library calls. |
| `MainViewModel` | Target, Count, Timeout, Bind, Status, Summary, `ObservableCollection<ReplyRow>`, `RunEchoCommand`, `CancelCommand`. |
| `ReplyRow` | Small view row. Not a library type. Map from `IcmpEchoReply` when the result lands. |

## Flow

1. Validate Target not blank. Count ≥ 1. Index ≥ 0. Source parses or empty.
2. `cts = new CancellationTokenSource()`.
3. `NetworkHelper.IcmpEcho(Target, new IcmpEchoOptions { Count, Timeout, InterfaceIndex = Bind.InterfaceIndex, SourceAddress = Bind.SourceAddress })`.
4. `await job.RunAsync(cts.Token)`.
5. Map result + replies onto the grid. Status = result.Status.
6. Catch → Status Failed, message on the line. Clear the grid (Requirements P-side: clear).

One `_run` gate so Echo is disabled while running.

## Out of this design

PathMtu pane. UdpProbe pane. Campaign file picker. `ChartView`.
