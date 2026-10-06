# RouteIQ — PR07 Requirements

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-PR07
**Version:** PR07
**Status:** Live. Binding for the offload until the implementation plan closes it.
**Date:** 6 October 2026
**Project:** `Vestigium.Suite.Network.RouteIQ`
**Kind:** WPF exe, `net10.0-windows`, MVVM
**APPID:** `RouteIQ` (never `Network`)
**Library:** `Vestigium.Helpers.Network` pin `1.5.0`. Suite `27320d8`.
**Binding:** This file wins on what leaves the host. Helpers.Network owns the protocol facts after `1.5.0`. PR04 still wins on vendor lookup, ICMP, and the settings path. PR05 still wins on the load path. PR06 still wins on the log. A plan that adds a neighbor protocol, a port sweep, a second catalog, or a `NetworkJob` for a table print is wrong.

PR07 deletes the two protocol implementations that still live in the host, and gives the host a print door it can await. It is not a new window.

---

## 0. Decision

| Call | Why |
|---|---|
| Delete `NeighborTables` | `GetIpNetTable` / `GetIpNetTable2` already live in `NetworkWindowsTables.GetNeighbors`. The host copy leaves `InterfaceName` null and builds a side map because the library resolves the name per row. Fix the library. Do not keep a second parser. |
| Family filter on `GetNeighbors` | The host prints IPv4 and IPv6 as separate grids. `GetNeighbors()` today always reads both. `GetNeighbors(RouteFamily)` is the cut. The parameterless call stays and means `All`. |
| One port catalog | `ConnectionServices` is a second well-known-port table. `NetworkPorts` already owns that table, but it keys by port only, so 443 and 514 cannot tell TCP from UDP. Add a protocol-aware lookup. Do not keep the host table. |
| Async print door, not a job | Probes already plug in through `NetworkJob.RunAsync`. Table prints do not. The host wraps `GetRoutes`, `GetNeighbors`, `GetConnections`, `GetNetBiosNames`, and `GetLmHosts` in `Task.Run` so the UI thread stays clear. `*Async` methods are that hop, owned once. A table read is not a `NetworkJob`. |
| Sync stays | The sync methods are the implementation and the test seam. Async calls them. Do not obsolete them. |
| `1.5.0`, not `1.4.6` | Owner pushed `1.5.0`. The contract is still additive. `GetNeighbors()` and `TryByPort` keep their current answers. No type rename. No event id. Do not hunt `1.4.6`. |
| Host stays a host | Grids, KQL, watch, export, settings, sort, and story logging stay here. Shell stays "not a protocol API." |

Rejected: moving `NeighborTables` into the library as a public type. Rejected: a RouteIQ-only port subset after the cut. Rejected: moving `NeighborGridRow` display text into the library. Rejected: wrapping a table print in `NetworkJob`. Rejected: one `GetSnapshotAsync` that collapses the six print sources. Rejected: a port sweep, a mutate form, or a ping on refresh.

---

## 1. What this version is

The same RouteIQ window, printing from awaited library calls on `Vestigium.Helpers.Network` `1.5.0`.

| Surface | Today | PR07 |
|---|---|---|
| IPv4 / IPv6 neighbors | `NeighborTables.ReadIpv4` / `ReadIpv6` in the host. `iphlpapi` P/Invoke. Interface name filled later by `StampInterfaceNames`. | `await NetworkHelper.GetNeighborsAsync(family, token)`. Name resolved once per call. Host file and the stamp hop deleted. |
| Routes, connections, NetBIOS names, LMHOSTS | `await Task.Run(() => NetworkHelper.Get…(), token)`. | `await NetworkHelper.GetRoutesAsync` / `GetConnectionsAsync` / `GetNetBiosNamesAsync` / `GetLmHostsAsync`. Sort stays in the host. |
| NetBIOS stats | `GetNetBiosStats()` inside `OnUi`. | Fetched with the names, off the UI thread, via `GetNetBiosStatsAsync`. The UI only sets the sentence. |
| Connection service label | `ConnectionServices.Label` prefers remote port, then local port, keyed `protocol\|port`. | `NetworkHelper.TryService(protocol, port)` with the same preference. Host file deleted. |
| Help port list | `ConnectionServices.Catalog`. | `NetworkPorts.All`. One catalog. The Help list gets longer. That is the point. |
| Probes | `LookupOuiAsync` and `IcmpEcho` / `RunAsync` already. | Unchanged. Do not add a second ping door. |
| Pin | `Directory.Build.props` `VestigiumNetworkVersion` = `1.5.0` on suite `27320d8`. | Held. No other pin moved. |

---

## 2. Evidence

| # | Fact | Where |
|---|---|---|
| E1 | Host neighbor reader duplicates the library reader: `GetIpNetTable`, 24-byte IPv4 row, `GetIpNetTable2` AF 23, 88-byte IPv6 row, same state and type names. Host passes `order: false` and leaves the interface name null. Library passes `order: true` and calls `NetworkInterface.GetAllNetworkInterfaces()` inside `InterfaceName` per row. | `RouteIQ/ViewModels/NeighborTables.cs`. `Vestigium.Helpers.Network/NetworkWindowsTables.cs`. |
| E2 | Call sites are `MainViewModel` neighbor loads, plus `StampInterfaceNames`, which is a second `Task.Run` only because the name is missing. No test references `NeighborTables`. | `MainViewModel.cs`. `tests/` search. |
| E3 | `ConnectionServices` is a static port table plus `Label`. Used by `ConnectionWatch` and `HelpView`. No test references it. | `ConnectionServices.cs`, `ConnectionWatch.cs`, `HelpView.xaml.cs`. |
| E4 | `NetworkPorts.TryByPort` is a port-only dictionary. 443 is `https` / Tcp. There is no 443/UDP `quic` row. 514 is `syslog` / Udp. Host rows that the library does not have: 512 rexec, 513 rlogin, 514/TCP rsh, 524 ncp, 548 afp, 691 msexch-routing, 749 kerberos-adm, 443/UDP quic. | `NetworkPorts.cs`. `ConnectionServices.cs`. |
| E5 | Host 860 is labeled `iscsi`. Library 3260 is `iscsi`. Those are not the same fact. 860 does not move as a second iSCSI. | Both catalogs. |
| E6 | `AddressKind.Multicast` and `AddressKind.Broadcast` already exist. `NeighborGridRow` still hand-checks 224–239 and `FF:FF:FF:FF:FF:FF`. That is display policy on a grid row, not a second stack reader. | `SubnetTypes.cs`. `NeighborGridRow.cs`. |
| E7 | Shell already references the package. RouteIQ does not reference `Vestigium.Helpers.Network` directly. Pin is `1.4.5`. | `Vestigium.Suite.Network.Shell.csproj`. `Directory.Build.props`. |
| E8 | Library EVENTID band is 14500–14999, used through 14560. A port lookup must not take an id. `GetNeighbors` already logs a count. | `Network` README. `NetworkHelper.GetNeighbors`. |
| E9 | Print loads and the watch tick are `Task.Run` around a sync library call. Probe and live OUI already await library tasks. `GetNetBiosStats` runs inside `OnUi` after the name load. | `MainViewModel.LoadIpv4`, `LoadIpv6`, `LoadNeighbors`, `LoadNetBios`, `LoadLmHosts`. `ConnectionWatch`. `NetBiosSummary.ApplyNetBiosStats`. `NetworkJob.RunAsync`. |

---

## 3. Library contract (`1.5.0`)

Owned by `nwilkinsonlsnh/Vestigium.Helpers`, consumed here. This paper is the contract. The Helpers repo does not get a second requirements file for this slice.

| # | Rule |
|---|---|
| L1 | `GetNeighbors()` remains `GetNeighbors(RouteFamily.All)`. New optional argument. Windows reads only the requested family. Linux may filter the proc table. |
| L2 | Interface name is resolved from one index-to-name map per call. A row whose index misses the map keeps a null name. Index 0 is not rewritten. |
| L3 | IPv4 state names stay `Dynamic` / `Static` / `Invalid` / `Other`. IPv6 state names stay the ND set already returned. MAC format stays colon, uppercase, six octets. |
| L4 | `NetworkPorts.Try(string transport, int port, out NetworkPortGuess)` matches a specific TCP or UDP row before a `Both` row. `TryByPort(443)` still returns `https`. `Try("UDP", 443)` returns `quic`. `Try("TCP", 514)` returns `rsh`. `Try("UDP", 514)` returns `syslog`. |
| L5 | Add the non-colliding host rows as library rows: 512 rexec, 513 rlogin, 524 ncp, 548 afp, 691 msexch-routing, 749 kerberos-adm, 443/UDP quic, 514/TCP rsh. Aliases only where the name differs and the port does not: `dhcp-server`→dhcp, `snmptrap`→snmp-trap, `dns-over-tls`→dot, `ike`→isakmp, `portmap`→rpcbind, `svrloc`→slp. |
| L6 | Do not add 860 as iSCSI. Do not change 3260. |
| L7 | `NetworkHelper.TryService(string? protocol, int port, out string name)` is the host door. Blank protocol is TCP. Miss returns false. No log line. |
| L8 | No new EVENTID. No plot API. No port sweep. No `ping.exe`. |
| L9 | New file `NetworkHelper.Async.cs`. Doors: `GetRoutesAsync`, `GetNeighborsAsync`, `GetConnectionsAsync`, `GetLmHostsAsync`, `GetNetBiosNamesAsync`, `GetNetBiosStatsAsync`. Each takes a `CancellationToken` and returns the same type as the sync method. Implementation is a token check, then `Task.Run` of the sync method. `ConfigureAwait(false)` inside. |
| L10 | The token cancels the wait. It does not abort a P/Invoke already inside `iphlpapi`. A cancelled token before the hop throws `OperationCanceledException`. After the hop, the caller checks the token. Do not document this as abort. |
| L11 | These doors are not `NetworkJob`. No `JobId`, no progress, no single-start. Probes stay on `RunAsync`. |
| L12 | Async methods do not log a second line. The sync method already logs. |

---

## 4. Host requirements

| # | Rule |
|---|---|
| R1 | `NeighborTables.cs` is deleted. Neighbor loads await `GetNeighborsAsync` with `RouteFamily.Pv4` or `Pv6`. `StampInterfaceNames` is deleted. The name is on the row. |
| R2 | `ConnectionServices.cs` is deleted. `ConnectionWatch` labels with `TryService`, remote port first, then local port, else `--`. |
| R3 | `HelpView` reads `NetworkPorts.All`. It does not keep a private short list. |
| R4 | Pin moves to `1.5.0` only after a restore can see that package. A local project reference is not the ship. |
| R5 | PR05 load path holds: prints stay off the UI thread, IPv4 and IPv6 stay separate grids, one failure does not clear the other, refresh does not ping. The off-thread hop is the library async method, not a host `Task.Run` around a sync print. |
| R6 | PR06 log holds. This slice adds no story id. A throw from `GetNeighborsAsync` is still the existing neighbor-source failure. `OperationCanceledException` on a superseded print stays silence. |
| R7 | `NeighborGridRow` stays. Class letter and broadcast flag may keep using `ClassifyAddress`. Do not move the MAC-broadcast check or the reachable-duration text. Sort stays in the host. |
| R8 | Code-behind still does not call `NetworkHelper`, except `HelpView` reading `NetworkPorts.All`. That read is a catalog, not a stack print. |
| R9 | `LoadIpv4`, `LoadIpv6`, `LoadConnections`, `LoadNetBios`, `LoadLmHosts`, `FillAsync`, and the watch tick await the matching `*Async` method. No `Task.Run` around those calls. `ByAddress` and the connection projection stay in the host. |
| R10 | `ApplyNetBiosStats` does not call `GetNetBiosStats` on the UI thread. The load fetches stats off-thread and the UI sets the sentence. |

---

## 5. Failure

| Case | Required behavior |
|---|---|
| `GetNeighborsAsync(Pv4)` throws | IPv4 neighbor grid unchanged. IPv6 source still applies. Status and PR06 10020 still fire. |
| Adapter name lookup throws | The map is empty. Rows still return. Name is null. Not a failed print. |
| Token cancelled before the hop | `OperationCanceledException`. Not a story failure. Grid unchanged. |
| Token cancelled during the P/Invoke | The call finishes. The host generation check drops the result. No abort claim. |
| Unknown port | Label is `--`. Help omits nothing that is in `NetworkPorts.All`. |
| 443 TCP and 443 UDP | TCP label `https`. UDP label `quic`. `TryByPort(443)` stays `https`. |
| Package not restorable as `1.5.0` | Host cut does not land. Pin stays `1.4.5`. A push acknowledgement is not a restore. |

Tuesday-at-2am failure this slice exists to stop: a Windows neighbor-table layout change fixed in the library and still broken in RouteIQ, because the host kept its own parser. The async door exists so the host cannot grow a third `Task.Run` around the same call.

---

## 6. Tests

Library tests live in `Vestigium.Helpers.Tests`. Host tests live in `Vestigium.Suite.Network.Tests`. No live stack required for the port locks.

| # | Lock |
|---|---|
| T-A | `Try("UDP", 443)` is quic. `Try("TCP", 443)` is https. `TryByPort(443)` is still https. |
| T-B | `Try("TCP", 514)` is rsh. `Try("UDP", 514)` is syslog. |
| T-C | `TryService` with a blank protocol uses TCP. A miss is false. |
| T-D | Host has no `NeighborTables` and no `ConnectionServices` type. |
| T-E | PR05 locks still hold. A neighbor source fault does not clear routes. |
| T-F | A cancelled token passed to `GetRoutesAsync` before the hop throws `OperationCanceledException`. The method is not a `NetworkJob`. |

No UI test. No P/Invoke test that needs a real ARP table.

---

## 7. Acceptance

1. `NeighborTables.cs` and `ConnectionServices.cs` are gone. `StampInterfaceNames` is gone.
2. IPv4 and IPv6 neighbor grids still fill, with interface name when the index is known, from one awaited call each.
3. Route, connection, NetBIOS, and LMHOSTS loads await the library. No `Task.Run` around those prints.
4. NetBIOS stats are not read on the UI thread.
5. Connection service column still prefers the remote port.
6. Help lists the library catalog, including quic on 443/UDP.
7. Pin is `1.5.0`.
8. PR04, PR05, and PR06 acceptance still hold.

---

## 8. Out

| Item | Why |
|---|---|
| Public `NeighborTables` in the library | Duplicate of `NetworkWindowsTables`. |
| Moving `NeighborGridRow` | Display. `ClassifyAddress` already covers class and broadcast kind. |
| Host port subset | Second catalog with a new name. |
| 860 as iSCSI | Contradicts the library. E5. |
| `NetworkJob` for a table print | Probes already have that shape. A list has no progress and no single-start. |
| `GetSnapshotAsync` | Collapses sources. Fights PR06. |
| Async `TryService` | In-memory. No hop. |
| Route mutation, probe, watch interval | Not this slice. |
| Shell protocol helpers | Shell is chrome. |
| PingIQ, DnsIQ, NicIQ adopting `*Async` | Same package, later slice. Not this PR. |
| Publishing the exe | Not this slice. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR07 | 6 Oct 2026 | Offload opened. Neighbor parser and port catalog leave the host. Library grows a family filter and a protocol-aware port lookup. |
| PR07 | 6 Oct 2026 | Async print door added. Table reads get `*Async`. They do not become `NetworkJob`. Host drops `Task.Run` on those prints and the interface-name stamp. |
| PR07 | 6 Oct 2026 | Owner pushed `1.5.0`. `1.4.6` is out. Pin moves only after restore can see `1.5.0`. |
| PR07 | 6 Oct 2026 | Pin is `1.5.0`. Suite `27320d8`. |
