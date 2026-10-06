# RouteIQ — PR07 Requirements

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-PR07
**Version:** PR07
**Status:** Live. Binding for the offload until the implementation plan closes it.
**Date:** 6 October 2026
**Project:** `Vestigium.Suite.Network.RouteIQ`
**Kind:** WPF exe, `net10.0-windows`, MVVM
**APPID:** `RouteIQ` (never `Network`)
**Library:** `Vestigium.Helpers.Network` pin `1.4.5` today. This slice consumes `1.4.6`.
**Binding:** This file wins on what leaves the host. Helpers.Network owns the protocol facts after `1.4.6`. PR04 still wins on vendor lookup, ICMP, and the settings path. PR05 still wins on the load path. PR06 still wins on the log. A plan that adds a neighbor protocol, a port sweep, or a second catalog is wrong.

PR07 deletes the two protocol implementations that still live in the host. It is not a new window.

---

## 0. Decision

| Call | Why |
|---|---|
| Delete `NeighborTables` | `GetIpNetTable` / `GetIpNetTable2` already live in `NetworkWindowsTables.GetNeighbors`. The host copy leaves `InterfaceName` null and builds a side map because the library resolves the name per row. Fix the library. Do not keep a second parser. |
| Family filter on `GetNeighbors` | The host prints IPv4 and IPv6 as separate sources. `GetNeighbors()` today always reads both. `GetNeighbors(RouteFamily)` is the cut. The parameterless call stays and means `All`. |
| One port catalog | `ConnectionServices` is a second well-known-port table. `NetworkPorts` already owns that table, but it keys by port only, so 443 and 514 cannot tell TCP from UDP. Add a protocol-aware lookup. Do not keep the host table. |
| `1.4.6`, not `1.5.0` | Additive. `GetNeighbors()` and `TryByPort` keep their current answers. No type rename. No event id. |
| Host stays a host | Grids, KQL, watch, export, settings, and story logging stay here. Shell stays "not a protocol API." |

Rejected: moving `NeighborTables` into the library as a public type. Rejected: a RouteIQ-only port subset after the cut. Rejected: moving `NeighborGridRow` display text into the library. Rejected: a port sweep, a mutate form, or a ping on refresh.

---

## 1. What this version is

The same RouteIQ window, printing neighbors and labeling connections from `Vestigium.Helpers.Network` `1.4.6`.

| Surface | Today | PR07 |
|---|---|---|
| IPv4 / IPv6 neighbors | `NeighborTables.ReadIpv4` / `ReadIpv6` in the host. `iphlpapi` P/Invoke. Interface name filled later from `NeighborTables.Names()`. | `NetworkHelper.GetNeighbors(RouteFamily.Pv4)` and `GetNeighbors(RouteFamily.Pv6)`. Name resolved once per call. Host file deleted. |
| Connection service label | `ConnectionServices.Label` prefers remote port, then local port, keyed `protocol\|port`. | `NetworkHelper.TryService(protocol, port)` with the same preference. Host file deleted. |
| Help port list | `ConnectionServices.Catalog`. | `NetworkPorts.All`. One catalog. The Help list gets longer. That is the point. |
| Routes, connections, NetBIOS, LMHOSTS | Already `NetworkHelper`. | Unchanged. |
| Pin | `Directory.Build.props` `VestigiumNetworkVersion` = `1.4.5`. | `1.4.6` after the package exists. Not before. |

---

## 2. Evidence

| # | Fact | Where |
|---|---|---|
| E1 | Host neighbor reader duplicates the library reader: `GetIpNetTable`, 24-byte IPv4 row, `GetIpNetTable2` AF 23, 88-byte IPv6 row, same state and type names. Host passes `order: false` and leaves the interface name null. Library passes `order: true` and calls `NetworkInterface.GetAllNetworkInterfaces()` inside `InterfaceName` per row. | `RouteIQ/ViewModels/NeighborTables.cs`. `Vestigium.Helpers.Network/NetworkWindowsTables.cs`. |
| E2 | Call sites are `MainViewModel` neighbor loads only. No test references `NeighborTables`. | `MainViewModel.cs`. `tests/` search. |
| E3 | `ConnectionServices` is a static port table plus `Label`. Used by `ConnectionWatch` and `HelpView`. No test references it. | `ConnectionServices.cs`, `ConnectionWatch.cs`, `HelpView.xaml.cs`. |
| E4 | `NetworkPorts.TryByPort` is a port-only dictionary. 443 is `https` / Tcp. There is no 443/UDP `quic` row. 514 is `syslog` / Udp. Host rows that the library does not have: 512 rexec, 513 rlogin, 514/TCP rsh, 524 ncp, 548 afp, 691 msexch-routing, 749 kerberos-adm, 443/UDP quic. | `NetworkPorts.cs`. `ConnectionServices.cs`. |
| E5 | Host 860 is labeled `iscsi`. Library 3260 is `iscsi`. Those are not the same fact. 860 does not move as a second iSCSI. | Both catalogs. |
| E6 | `AddressKind.Multicast` and `AddressKind.Broadcast` already exist. `NeighborGridRow` still hand-checks 224–239 and `FF:FF:FF:FF:FF:FF`. That is display policy on a grid row, not a second stack reader. | `SubnetTypes.cs`. `NeighborGridRow.cs`. |
| E7 | Shell already references the package. RouteIQ does not reference `Vestigium.Helpers.Network` directly. Pin is `1.4.5`. | `Vestigium.Suite.Network.Shell.csproj`. `Directory.Build.props`. |
| E8 | Library EVENTID band is 14500–14999, used through 14560. A port lookup must not take an id. `GetNeighbors` already logs a count. | `Network` README. `NetworkHelper.GetNeighbors`. |

---

## 3. Library contract (`1.4.6`)

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

---

## 4. Host requirements

| # | Rule |
|---|---|
| R1 | `NeighborTables.cs` is deleted. Neighbor loads call `NetworkHelper.GetNeighbors` with `RouteFamily.Pv4` or `Pv6` inside the existing `Task.Run`. |
| R2 | `ConnectionServices.cs` is deleted. `ConnectionWatch` labels with `TryService`, remote port first, then local port, else `--`. |
| R3 | `HelpView` reads `NetworkPorts.All`. It does not keep a private short list. |
| R4 | Pin moves to `1.4.6` only after that package is on NuGet. A local project reference is not the ship. |
| R5 | PR05 load path holds: neighbors stay off the UI thread, IPv4 and IPv6 stay separate sources, one failure does not clear the other, refresh does not ping. |
| R6 | PR06 log holds. This slice adds no story id. A throw from `GetNeighbors` is still the existing neighbor-source failure. |
| R7 | `NeighborGridRow` stays. Class letter and broadcast flag may keep using `ClassifyAddress`. Do not move the MAC-broadcast check or the reachable-duration text. |
| R8 | Code-behind still does not call `NetworkHelper`, except `HelpView` reading `NetworkPorts.All`. That read is a catalog, not a stack print. |

---

## 5. Failure

| Case | Required behavior |
|---|---|
| `GetNeighbors(Pv4)` throws | IPv4 neighbor grid unchanged. IPv6 source still applies. Status and PR06 10020 still fire. |
| Adapter name lookup throws | The map is empty. Rows still return. Name is null. Not a failed print. |
| Unknown port | Label is `--`. Help omits nothing that is in `NetworkPorts.All`. |
| 443 TCP and 443 UDP | TCP label `https`. UDP label `quic`. `TryByPort(443)` stays `https`. |
| Package not yet `1.4.6` | Host cut does not land. Pin stays `1.4.5`. |

Tuesday-at-2am failure this slice exists to stop: a Windows neighbor-table layout change fixed in the library and still broken in RouteIQ, because the host kept its own parser.

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

No UI test. No P/Invoke test that needs a real ARP table.

---

## 7. Acceptance

1. `NeighborTables.cs` and `ConnectionServices.cs` are gone.
2. IPv4 and IPv6 neighbor grids still fill, with interface name when the index is known.
3. Connection service column still prefers the remote port.
4. Help lists the library catalog, including quic on 443/UDP.
5. Pin is `1.4.6`.
6. PR04, PR05, and PR06 acceptance still hold.

---

## 8. Out

| Item | Why |
|---|---|
| Public `NeighborTables` in the library | Duplicate of `NetworkWindowsTables`. |
| Moving `NeighborGridRow` | Display. `ClassifyAddress` already covers class and broadcast kind. |
| Host port subset | Second catalog with a new name. |
| 860 as iSCSI | Contradicts the library. E5. |
| Route mutation, probe, watch interval | Not this slice. |
| Shell protocol helpers | Shell is chrome. |
| Publishing the exe | Not this slice. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR07 | 6 Oct 2026 | Offload opened. Neighbor parser and port catalog leave the host. Library `1.4.6` grows a family filter and a protocol-aware port lookup. |
