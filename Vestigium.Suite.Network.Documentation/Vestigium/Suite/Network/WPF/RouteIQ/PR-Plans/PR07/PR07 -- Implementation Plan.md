# RouteIQ — PR07 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR07
**Version:** PR07
**Status:** Live. Step 5 done. Host cut is not started. Pin is `1.5.0`, not `1.4.6`.
**Date:** 6 October 2026
**Binding:** `PR07 -- Requirements.md`. This file wins on order. Requirements win on the cut. PR06 wins on the log. PR05 wins on the load path. PR04 wins on vendor lookup, ICMP, and the settings path.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

Two repos. The library commit lands and publishes before the host pin moves. Do not point RouteIQ at an unreleased package.

---

## 0. Done

Step 1 closed 6 October 2026. Requirements accepted. This plan is the paper you implement.
Step 2 closed 6 October 2026. `GetNeighbors(RouteFamily)` on Helpers `beede9f`. Windows reads one family. Name map built once. Parameterless call still means `All`. Route reads unchanged. Not published. Pin stays `1.4.5`.
Step 3 closed 6 October 2026. Protocol lookup on Helpers `2dd1e0e`. `TryByPort(443)` stays https. `TryService` does not log. 860 was not added. Not published.
Step 4 closed 6 October 2026. Async print doors on Helpers `791bc0d`. Six methods. Token before the hop throws. Not a `NetworkJob`. No second log line. `NetworkHelper.cs` was not reopened. Not published.
Step 5 closed 6 October 2026. Owner pushed `Vestigium.Helpers.Network` `1.5.0`. `1.4.6` is out. README surface is Helpers `8b49c35`. That commit does not republish the package.

The folder stays live until the owner accepts the slice.

---

## 0.1 Mind change

Old belief: the additive package is `1.4.6`.
What arrived: the owner pushed `Vestigium.Helpers.Network.1.5.0.nupkg`. NuGet returned Created.
New position: the pin is `1.5.0`. Do not publish `1.4.6`. Do not republish `1.5.0` to fix the README.
Out: a host cut against a version restore cannot see.

Old belief: RouteIQ needs its own neighbor reader because `GetNeighbors` is the wrong shape, and the host `Task.Run` is the async interface.
What arrived: the parsers match. The host skipped the library to avoid a per-row `GetAllNetworkInterfaces` and to split families. `ConnectionServices` is the same kind of fork against `NetworkPorts`, plus a protocol key the library dictionary cannot express. The `Task.Run` wrappers are the same hop copied across six loads and the watch tick. Probes already have `RunAsync`. Table prints do not, so the host invented the hop.
New position: fix the library call, add `*Async` as a thin hop over the sync methods, then delete both host types and the host hop.
Out: a public second parser. A host catalog that "stays small." A `NetworkJob` per table. A snapshot that merges the six sources.

---

## 1. Order

| Step | Closes | Repo / files | Exit |
|---|---|---|---|
| 1 | Paper | This folder. Keeper. | Done. |
| 2 | L1, L2, L3 | `Vestigium.Helpers` `NetworkWindowsTables.cs`, `NetworkLinuxTables.cs`, `NetworkStackEngine.cs`, `NetworkHelper.cs`. | Done. `beede9f`. Family filter. Name map once. Parameterless call unchanged. |
| 3 | L4–L8, T-A, T-B, T-C | `NetworkPorts.cs`, `NetworkHelper.Ports.cs`, `NetworkPortCatalogTests.cs`. | Done. `2dd1e0e`. Protocol lookup. `TryByPort(443)` still https. No new EVENTID. |
| 4 | L9–L12, T-F | `NetworkHelper.Async.cs`, `NetworkPrintAsyncTests.cs`. | Done. `791bc0d`. Six doors. Cancel before the hop throws. Not a `NetworkJob`. |
| 5 | Publish | Helpers package `1.5.0`. README surface. | Done. Owner push. README `8b49c35`. Package not republished for the README. |
| 6 | R1–R3, R5–R10, T-D, T-E | Suite: `MainViewModel.cs`, `ConnectionWatch.cs`, `NetBiosSummary.cs`, `HelpView.xaml.cs`. Delete `NeighborTables.cs`, `ConnectionServices.cs`. | Host awaits the library. Stamp hop gone. Grids and label behavior hold. |
| 7 | R4 | `Directory.Build.props` pin `1.5.0`. | Pin matches the package that step 6 compiled against. |

`NeighborGridRow.cs` is not edited. `PrintCoordinator.cs` is not edited. `RouteIqLog.cs` is not edited. Shell is not edited. PingIQ, DnsIQ, and NicIQ are not edited. `NetworkPortGuess.cs` was not edited. The record already had a transport.

---

## 2. Step 2 — neighbor read

Closed 6 October 2026. Helpers `beede9f`.

`NetworkWindowsTables.GetNeighbors(RouteFamily)` builds one index-to-name map, then reads IPv4, IPv6, or both. Row index is the table index. A miss stays null. Index 0 is not rewritten to 1. Both v4 and v6 indexes are recorded when a NIC has them. Route reads still call `InterfaceName` per row. That cleanup is out.

Linux `GetNeighbors(Pv6)` returns empty. `/proc/net/arp` is IPv4. No new proc parser.

`NetworkStackEngine` and `NetworkHelper` take the same argument, default `All`. `GetSnapshot` still calls the parameterless form. This was the only edit to `NetworkHelper.cs`.

---

## 3. Step 3 — one port catalog

Closed 6 October 2026. Helpers `2dd1e0e`.

`ByPort` is first-row-wins, so `TryByPort(443)` stays `https`. `Try` looks up the exact transport, then `Both`. `Try("UDP", 443)` is `quic`. `Try("TCP", 514)` is `rsh`. `Try("UDP", 514)` is `syslog`.

Added rows: 443/UDP quic, 512 rexec, 513 rlogin, 514/TCP rsh, 524 ncp, 548 afp, 691 msexch-routing, 749 kerberos-adm. Aliases only: `dhcp-server`, `snmptrap`, `dns-over-tls`, `ike`, `portmap`, `svrloc`. 860 was not added. 3260 stays `iscsi`.

`NetworkHelper.TryService` trims the protocol, treats blank as TCP, and returns the guess name. It does not log. A miss is false and an empty name.

Locks are `NetworkPortCatalogTests`. No socket. No new EVENTID.

---

## 4. Step 4 — async print door

Closed 6 October 2026. Helpers `791bc0d`.

`NetworkHelper.Async.cs` has `GetRoutesAsync`, `GetNeighborsAsync`, `GetConnectionsAsync`, `GetLmHostsAsync`, `GetNetBiosNamesAsync`, and `GetNetBiosStatsAsync`. Each throws if the token is already cancelled, then `Task.Run` of the sync method with `ConfigureAwait(false)`. No catch. No log call. No `NetworkJob`.

The token does not abort `iphlpapi` once the call is inside. `GetNeighborsAsync` and `GetRoutesAsync` forward `RouteFamily`. `GetConnectionsAsync` forwards the query.

T-F is `NetworkPrintAsyncTests`. A cancelled token on `GetRoutesAsync` throws `OperationCanceledException`. The return type is `Task`, not `NetworkJob`.

---

## 5. Step 5 — package

Closed 6 October 2026. Owner pushed `1.5.0`. Gallery listing can lag the push. A restore that cannot see `1.5.0` is not a reason to pin it.

README on Helpers `8b49c35` names the family argument, `TryService`, and the six async doors, and says the token does not abort `iphlpapi`. That commit is source only. The pushed package keeps the README it was packed with.

Do not republish `1.4.5`. Do not publish `1.4.6`. Do not republish `1.5.0` for the README.

---

## 6. Step 6 — host cut

`MainViewModel` neighbor loads await `GetNeighborsAsync(RouteFamily.Pv4)` and `GetNeighborsAsync(RouteFamily.Pv6)`. Same generation check. Same replace. Same PR06 lines. Delete `StampInterfaceNames` and `ApplyNames`. The row already has the name.

`LoadIpv4` and `LoadIpv6` await `GetRoutesAsync`, then `ByAddress` in the host. `LoadConnections`, `FillAsync`, and the watch tick await `GetConnectionsAsync`. `LoadLmHosts` awaits `GetLmHostsAsync`. `LoadNetBios` awaits names and stats, sorts in the host, and `OnUi` only assigns. `ApplyNetBiosStats` takes the stats object. It does not call the library.

`ConnectionWatch` replaces `ConnectionServices.Label` with remote-then-local `TryService`. Miss stays `--`. The projection `Task.Run` in `Paint` stays. That is host sort, not a stack read.

`HelpView` enumerates `NetworkPorts.All`. Group by port the way it already groups the host catalog. Protocol comes from the guess transport.

Delete `NeighborTables.cs` and `ConnectionServices.cs` in this step, not before the call sites move.

---

## 7. Step 7 — pin

`Directory.Build.props` `VestigiumNetworkVersion` becomes `1.5.0`. No other pin moves. Not until step 6 compiled against a restore of `1.5.0`.

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
| PR07 | 6 Oct 2026 | Step 2. Family filter and one name map. Helpers `beede9f`. Not published. |
| PR07 | 6 Oct 2026 | Step 3. Protocol lookup and `TryService`. Helpers `2dd1e0e`. Not published. |
| PR07 | 6 Oct 2026 | Step 4. Six async print doors. Helpers `791bc0d`. Not published. |
| PR07 | 6 Oct 2026 | Step 5. Owner pushed `1.5.0`. `1.4.6` is out. README `8b49c35` is source only. |
