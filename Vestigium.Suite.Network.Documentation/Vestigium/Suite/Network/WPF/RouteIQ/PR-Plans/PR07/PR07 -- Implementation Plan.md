# RouteIQ — PR07 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR07
**Version:** PR07
**Status:** Live. Step 1 done. Library and host cuts are not started.
**Date:** 6 October 2026
**Binding:** `PR07 -- Requirements.md`. This file wins on order. Requirements win on the cut. PR06 wins on the log. PR05 wins on the load path. PR04 wins on vendor lookup, ICMP, and the settings path.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

Two repos. The library commit lands and publishes before the host pin moves. Do not point RouteIQ at an unreleased package.

---

## 0. Done

Step 1 closed 6 October 2026. Requirements accepted. This plan is the paper you implement.

The folder stays live until the owner accepts the slice.

---

## 0.1 Mind change

Old belief: RouteIQ needs its own neighbor reader because `GetNeighbors` is the wrong shape, and the host `Task.Run` is the async interface.
What arrived: the parsers match. The host skipped the library to avoid a per-row `GetAllNetworkInterfaces` and to split families. `ConnectionServices` is the same kind of fork against `NetworkPorts`, plus a protocol key the library dictionary cannot express. The `Task.Run` wrappers are the same hop copied across six loads and the watch tick. Probes already have `RunAsync`. Table prints do not, so the host invented the hop.
New position: fix the library call, add `*Async` as a thin hop over the sync methods, then delete both host types and the host hop.
Out: a public second parser. A host catalog that "stays small." A `NetworkJob` per table. A snapshot that merges the six sources.

---

## 1. Order

| Step | Closes | Repo / files | Exit |
|---|---|---|---|
| 1 | Paper | This folder. Keeper. | Done. |
| 2 | L1, L2, L3 | `Vestigium.Helpers` `NetworkWindowsTables.cs`, `NetworkLinuxTables.cs`, `NetworkStackEngine.cs`, `NetworkHelper.cs`. | `GetNeighbors(RouteFamily)` reads one family on Windows. Name map built once. Parameterless call unchanged. |
| 3 | L4–L8, T-A, T-B, T-C | `NetworkPorts.cs`, `NetworkPortGuess.cs` only if the record needs a second transport, `NetworkHelper.Ports.cs`, Helpers tests. | Protocol lookup matches the table in requirements. `TryByPort(443)` still https. No new EVENTID. |
| 4 | L9–L12, T-F | New `NetworkHelper.Async.cs`. One test file. | Six `*Async` doors. Token before the hop throws. No `NetworkJob`. No second log line. |
| 5 | Publish | Helpers package `1.4.6`. README surface row for `GetNeighbors(family)`, `TryService`, and the async doors. | Package on NuGet. Not a suite edit. |
| 6 | R1–R3, R5–R10, T-D, T-E | Suite: `MainViewModel.cs`, `ConnectionWatch.cs`, `NetBiosSummary.cs`, `HelpView.xaml.cs`. Delete `NeighborTables.cs`, `ConnectionServices.cs`. | Host awaits the library. Stamp hop gone. Grids and label behavior hold. |
| 7 | R4 | `Directory.Build.props` pin `1.4.6`. | Pin matches the package that step 6 compiled against. |

`NeighborGridRow.cs` is not edited. `PrintCoordinator.cs` is not edited. `RouteIqLog.cs` is not edited. Shell is not edited. PingIQ, DnsIQ, and NicIQ are not edited.

---

## 2. Step 2 — neighbor read

In `NetworkWindowsTables`, build the index-to-name map once at the start of `GetNeighbors`. Pass it into both readers. `InterfaceName(index)` becomes a dictionary lookup. Delete the per-row `GetAllNetworkInterfaces` call on this path. Route reads may keep their own lookup; do not refactor them in this step.

`GetNeighbors(RouteFamily family)` reads IPv4 only, IPv6 only, or both. `GetNeighbors()` calls `All`.

Linux filters the existing proc read. Do not add a new proc parser.

`NetworkStackEngine` and `NetworkHelper` grow the same argument, default `All`, so current callers compile. This is the only edit to `NetworkHelper.cs` in this plan. Async does not reopen it.

---

## 3. Step 3 — one port catalog

`NetworkPorts` keeps `ByPort` for `TryByPort`. Add a second index keyed by transport and port.

Lookup order: exact TCP or UDP row, then `Both`. `TryByPort` stays first-row-wins on the existing catalog order so 443 remains https.

Add the rows in L5. Do not add 860. Aliases go in the name index only.

`NetworkHelper.TryService` trims the protocol, treats blank as TCP, and returns the guess name. It does not log.

Tests are table locks. No socket.

---

## 4. Step 4 — async print door

New `NetworkHelper.Async.cs`. Six methods. Each checks the token, then `Task.Run` of the matching sync method. `ConfigureAwait(false)`. Do not catch and wrap. A throw from the sync method surfaces as the task exception.

`GetNeighborsAsync` takes `RouteFamily` and forwards it. `GetRoutesAsync` does the same. `GetConnectionsAsync` takes the existing query.

No `NetworkJob` constructor. No progress event. No log call in this file.

T-F cancels before the hop and expects `OperationCanceledException`. It does not need a live stack if the token is cancelled first.

---

## 5. Step 5 — package

Version `1.4.6`. README surface gains the family argument, `TryService`, and the six async doors. Say the token does not abort `iphlpapi`. Contract link stays the Helpers requirements document; this PR07 paper is the slice contract.

Do not republish `1.4.5`.

---

## 6. Step 6 — host cut

`MainViewModel` neighbor loads await `GetNeighborsAsync(RouteFamily.Pv4)` and `GetNeighborsAsync(RouteFamily.Pv6)`. Same generation check. Same replace. Same PR06 lines. Delete `StampInterfaceNames` and `ApplyNames`. The row already has the name.

`LoadIpv4` and `LoadIpv6` await `GetRoutesAsync`, then `ByAddress` in the host. `LoadConnections`, `FillAsync`, and the watch tick await `GetConnectionsAsync`. `LoadLmHosts` awaits `GetLmHostsAsync`. `LoadNetBios` awaits names and stats, sorts in the host, and `OnUi` only assigns. `ApplyNetBiosStats` takes the stats object. It does not call the library.

`ConnectionWatch` replaces `ConnectionServices.Label` with remote-then-local `TryService`. Miss stays `--`. The projection `Task.Run` in `Paint` stays. That is host sort, not a stack read.

`HelpView` enumerates `NetworkPorts.All`. Group by port the way it already groups the host catalog. Protocol comes from the guess transport.

Delete `NeighborTables.cs` and `ConnectionServices.cs` in this step, not before the call sites move.

---

## 7. Step 7 — pin

`Directory.Build.props` `VestigiumNetworkVersion` becomes `1.4.6`. No other pin moves.

---

## 8. Watch

| Watch | Failure |
|---|---|
| Second parser | `NeighborTables` renamed and kept. |
| Per-row NIC walk | Name lookup still calls `GetAllNetworkInterfaces` inside the row loop. |
| Index 0 | Rewriting interface index 0 to 1. |
| Port truth | 860 added as iSCSI. `TryByPort(443)` flipped to quic. |
| Async shape | A table read returned as `NetworkJob`. A second log line from `*Async`. Async reopening `NetworkHelper.cs`. |
| Cancel lie | Documenting the token as an abort of `GetIpNetTable`. |
| Log | A story id, or a line per port lookup. |
| Load path | Neighbors back on the UI thread. Refresh pings. One failed source clears another. Host `Task.Run` left around a print that now has `*Async`. |
| Stamp | `StampInterfaceNames` kept "just in case." |
| Grid row | Editing `NeighborGridRow` "while we are here." |
| Pin | Suite pin moved before the package exists. |
| Siblings | Editing PingIQ, DnsIQ, or NicIQ in this PR. |

---

## 9. Out of this plan

Route-table name-lookup cleanup. Grid-row multicast helper. Exe publish. A Helpers requirements clone of this paper. Other hosts adopting the async doors.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR07 | 6 Oct 2026 | Plan opened. Step 1 done. Library first, then host delete, then pin. |
| PR07 | 6 Oct 2026 | Step 4 is the async print door in a new file. Host step drops `Task.Run` on prints and the name stamp. |
