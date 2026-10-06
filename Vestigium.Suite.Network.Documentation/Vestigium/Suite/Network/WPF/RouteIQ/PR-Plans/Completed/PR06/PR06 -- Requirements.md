# RouteIQ — PR06 Requirements

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-PR06
**Version:** PR06
**Status:** Live. Binding for the log slice until an implementation plan closes it.
**Date:** 6 October 2026
**Project:** `Vestigium.Suite.Network.RouteIQ`
**Kind:** WPF exe, `net10.0-windows`, MVVM
**APPID:** `RouteIQ` (never `Network`)
**Library:** `Vestigium.Logging` 1.7.x (package already referenced through the shell)
**Binding:** This file wins on what the host writes to the log. PR04 still wins on protocol honesty. PR05 still wins on the load path. Helpers.Network owns the print APIs. Vestigium.Logging SRS 2.3 wins on the logger. A plan that adds a log viewer, a seal, or a row dump is wrong.

PR06 is the same host, telling what it did, and handing every trapped failure to the logger’s .NET exception catalog. It is not a second product.

---

## 0. Decision

| Call | Why |
|---|---|
| Story, not a table dump | The operator already has the grids. The log is what the process did: started, asked for a print, applied or failed a source, probed, exported, saved settings, started or stopped a watch, shut down. A route, a neighbor, a connection, a NetBIOS name, or an LMHOSTS line in the log is a second copy of the grid and a flood. |
| One story line, then `Thrown` on failure | `VestigiumLog.Information` / `Warning` carries the stable sentence. `VestigiumLog.Thrown(ex, status)` is the only error door. It resolves `exception.GetType().FullName` against the embedded catalog (EVENTID 100–4999). A miss writes EVENTID 3 and a DEBUG hint. Do not pick an exception EVENTID by hand. |
| Do not rethrow out of the host | “Throw it to the library” means hand the instance to `Thrown`. The library does not accept a thrown exception. The UI stays up. The last list stays. PR05 R4 still holds. |
| Counts and names in `PROPERTIES` | Flood identity is `(APPID, CATEGORY, LEVEL, MESSAGE)`. MESSAGE stays fixed. Source name, generation, count, and HResult go in properties. |
| Host story ids start at 10000 | 0–4999 is the library catalog. 5000–9999 is the operations log. Network / KQL / ClosedXml catalogs are already registered by `HostLog` and those packages. RouteIQ does not invent ids inside those ranges. |
| Status bar stays | `Report` is the operator line. The log is the record. One does not replace the other. |

Rejected: a live log tab, HMAC seal, archive, janitor, per-row logs, logging the KQL text, a second logger, catching inside Helpers.Network from this host.

---

## 1. What this version is

The host after PR05, with a story in `%ProgramData%\Vestigium\Logs\RouteIQ\` and every trapped failure mapped through `VestigiumLog.Thrown`.

| Surface | Today | PR06 |
|---|---|---|
| Initialize | `HostLog.Initialize(HostIds.RouteIQ)` registers Network, KQL, ClosedXml. No host story catalog. | Same initialize. Host story catalog registered in that callback. APPID stays `RouteIQ`. |
| Prints | `PrintCoordinator` catches, calls `_fault`, which only `Report`s. | Story line for the request, for each apply, and for each fail. Fail also `Thrown`. |
| Settings | Load / save / legacy copy catch and return a reason string. | Same result object. Also `Thrown`. |
| Export, copy, probe, watch | Catch and `Report(ex.Message)`, or swallow. | `Report` stays. `Thrown` added. Cancel is not a failure. |
| Empty catch | `NeighborGridRow` swallows. Coordinator swallows a fault-handler throw and a progress throw. | Per-row parse miss stays quiet. A fault handler that throws is a bug, logged once. Progress throw stays quiet. |
| Unhandled | No dispatcher or domain hook in this host. | Dispatcher and domain unhandled call `Thrown`, then the dispatcher marks the print fault handled. |

---

## 2. Evidence

| # | Fact | Where |
|---|---|---|
| E1 | Logger is initialized before the window. Directory is `%ProgramData%\Vestigium\Logs\RouteIQ`. | `App.OnStartup`, `HostLog.Initialize`. |
| E2 | RouteIQ source has no `VestigiumLog` call. | `src/Vestigium.Suite.Network.RouteIQ`. |
| E3 | A print source throw is caught. The fault callback is status text. A throw inside that callback is swallowed so the join still finishes. | `PrintCoordinator.RunSource`. |
| E4 | Settings load, save, and legacy copy catch `Exception` and return a reason. They do not log. | `RouteIqSettingsStore`. |
| E5 | Export and clipboard copy catch and `Report(ex.Message)`. | `RouteIqExport`, `MainViewModel.Copy`. |
| E6 | Watch cancel is `OperationCanceledException` and is not a failure. A different exception is reported. | `ConnectionWatch`. |
| E7 | `Thrown` without an id walks the exception type, then base types, against the embedded catalog. Unknown type writes EVENTID 3 plus a DEBUG hint. After 20 distinct unknown types, a second DEBUG line says to add a custom catalog row. | `VestigiumLog.Thrown`, SRS 2.3. |
| E8 | Embedded .NET exceptions are EVENTID 100–4999. Host custom ids are ≥ 10000. | Developers Guide 2.2 §11. |
| E9 | Types this host can actually meet are already in the shards: `NetworkInformationException` 1260, `SocketException` 1280, `PingException` 1265, plus IO, unauthorized, JSON, and Win32 rows in `system.io.json` / `system.json` / `system.runtime.json`. | `EventCatalog/Shards`. |

---

## 3. Story

One sentence per action. No destination, gateway, MAC, 5-tuple, NetBIOS name, or LMHOSTS line. No query text.

| EVENTID | Level | STATUS | MESSAGE | PROPERTIES |
|---|---|---|---|---|
| 10000 | Information | None | Host started | — |
| 10005 | Information | None | Host stopped | — |
| 10010 | Information | None | Print requested | `generation` |
| 10015 | Information | None | Print source applied | `source`, `count`, `generation` |
| 10020 | Warning | Failed | Print source failed | `source`, `generation` |
| 10025 | Information | None | Probe finished | `result` = hit or miss. No address. |
| 10030 | Warning | Failed | Probe failed | — |
| 10035 | Information | None | Export finished | `sheets` |
| 10040 | Warning | Failed | Export failed | — |
| 10045 | Information | None | Settings loaded | `path` = file name only, not a roaming profile dump |
| 10050 | Warning | Failed | Settings rejected | `op` = load, save, or copy |
| 10055 | Information | None | Watch started | `seconds` |
| 10060 | Information | None | Watch stopped | `reason` = operator or cancel |
| 10065 | Warning | Failed | Watch tick failed | — |
| 10070 | Warning | None | Filter rejected | — |
| 10075 | Information | None | Clipboard copy failed is not a story. Use 10080. | — |
| 10080 | Warning | Failed | Clipboard copy failed | — |

10075 is unused. Do not write it.

Packed OUI is not its own story. It is part of the neighbor source. A vendor-lookup failure after opt-in is `Print source failed` only if the neighbor list did not apply. If the list applied and the stamp failed, one Warning 10020 with `source=vendor` is enough. Do not log each lookup.

Refresh coalesced is not a line. PR05 already coalesces. A second request is still one `Print requested` when the follow-up starts.

---

## 4. Requirements

| # | Rule |
|---|---|
| R1 | `HostLog.Initialize` remains the only initialize. Register the RouteIQ story catalog in the existing callback. Do not call `VestigiumLogger.Initialize` again. |
| R2 | Story writes use the table in section 3. MESSAGE is the constant in that table. Varying values go in `PROPERTIES`. |
| R3 | A caught failure calls `VestigiumLog.Thrown(ex, status)` with no event id. The catalog picks the .NET type. Then the matching story warning. Order: `Thrown`, then the story line. |
| R4 | `Thrown` is not used for a non-exception: blank probe, bad KQL compile, empty print. Those are story or status only. |
| R5 | `OperationCanceledException` on watch stop or a superseded print is `Watch stopped` or silence. It is not `Thrown` and not Failed. |
| R6 | No log line contains a route row, neighbor row, connection row, NetBIOS name, LMHOSTS line, MAC, gateway, or the query text. Counts are allowed. |
| R7 | Status text stays. A log failure must not replace `Report`, and `Report` must not be the only record. |
| R8 | Empty `catch` is illegal except the coordinator progress callback. A fault-handler throw is logged with `Thrown` once, then swallowed so the join still finishes. |
| R9 | `DispatcherUnhandledException` and `AppDomain.CurrentDomain.UnhandledException` call `Thrown`. A dispatcher exception from a print or command is marked handled. Do not mark a settings-store or export exception unhandled if it was already caught. |
| R10 | Code-behind still does not call `NetworkHelper`. This slice does not add a log viewer, a seal, or a janitor. |
| R11 | Library packages keep their own catalogs. RouteIQ does not wrap `NetworkHelper` to re-log what the package already logs. Host story is the host’s action, not a second copy of the library line. |

---

## 5. Traps

These are the doors that can throw on this host. Each one is caught at the host boundary. None of them are caught per row.

| Door | Catch | Report |
|---|---|---|
| IPv4 routes, IPv6 routes, neighbors, connections, NetBIOS, LMHOSTS | Already in `PrintCoordinator.RunSource` | `Thrown` + 10020. Other sources still apply. |
| Packed OUI / opt-in vendor stamp | Catch at the stamp task, not per address | `Thrown`. List already applied stays. |
| Probe one address | Catch in the command | `Thrown` + 10030. |
| Settings load, save, legacy copy | Already caught | `Thrown` + 10050. Result object unchanged. |
| Workbook export, open-after, open-folder | Already caught | `Thrown` + 10040. |
| Clipboard | Already caught | `Thrown` + 10080. |
| Watch tick | Already caught, after the cancel filter | `Thrown` + 10065. |
| KQL apply | Catch only if it throws | `Thrown` + 10070. A compile miss that already reports is not an exception. |
| Dispatcher / domain | New hooks in `App` | `Thrown` only. No story id. |

`NeighborGridRow` parse misses stay quiet. A bad color string is a settings problem and is already a `FormatException` at the palette. That one `Thrown` is enough. Do not log it per row.

---

## 6. Failure

| Case | Required behavior |
|---|---|
| `GetNeighbors` throws `NetworkInformationException` | Grid unchanged. Status names the source. Log has `Thrown` at 1260 and story 10020. Other sources still apply. |
| Unknown exception type | `Thrown` writes EVENTID 3 and the DEBUG hint. Story 10020 still written. Do not add a custom exception row in this slice. |
| Settings file locked | Load returns defaults plus a reason. `Thrown` fires. Host still opens. |
| Export path denied | Workbook not written. Status shows the reason. `Thrown` fires. Grids unchanged. |
| Watch stopped by the operator | No `Thrown`. One `Watch stopped`. |
| Fault callback throws | Join still completes. That second exception is `Thrown` once. |
| Logger not initialized | Must not happen. `OnStartup` initializes before any print. A write before initialize is a bug, not a swallowed catch. |
| Operator hits Refresh during a print | PR05 coalescing. One new `Print requested` when the follow-up starts. No per-row noise. |

Tuesday-at-2am failure this slice exists to stop: a print throw that exists only as status-bar text, gone on the next refresh, with no EVENTID in the RouteIQ log.

---

## 7. Tests

In `Vestigium.Suite.Network.Tests`. No network. No stack. No real `%ProgramData%` write.

| # | Lock |
|---|---|
| T-A | Story MESSAGE constants match section 3. A test that formats a line cannot append a destination or a MAC. |
| T-B | A source fault invokes `Thrown` and the 10020 story. The other sources still complete. |
| T-C | `OperationCanceledException` does not call `Thrown`. |
| T-D | Settings load failure returns defaults and records a rejected story. |
| T-E | PR05 locks still hold: refresh does not ping, one failed source does not clear the others. |

The static logger is not the test seam. A small host log type owns the story methods and the `Thrown` call. Tests substitute that type. Do not wrap `VestigiumLogger`.

No UI test. No JSONL file test.

---

## 8. Acceptance

1. Cold start writes `Host started` under APPID `RouteIQ`.
2. A print writes `Print requested` and one `Print source applied` per source that returned, with a count, and no row text.
3. A thrown print source writes `Thrown` at the catalog id for that CLR type, plus `Print source failed`.
4. Settings, export, probe, clipboard, and watch failures do the same. Cancel does not.
5. Dispatcher unhandled from a command is logged and does not tear the process down.
6. PR04 acceptance and PR05 acceptance still hold.
7. `dotnet test` for the new locks passes without a network.

---

## 9. Out

| Item | Why |
|---|---|
| Log viewer tab | No buyer. The file is the record. |
| Seal, archive, janitor | Host has not opted in. Not this slice. |
| Per-row or per-lookup lines | Flood. Contradicts section 3. |
| Query text in the log | It can carry an address. Status already shows the compile miss. |
| Custom exception rows for this host | Embedded catalog is the point. Add a row only after the DEBUG hint fires for a type we keep meeting. |
| Logging inside Helpers.Network | That package has its own catalog. This paper is the host. |
| Changing print overlap, splash, or ICMP | PR05 and PR04. |
| Publishing the exe | Not this slice. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR06 | 6 Oct 2026 | Log slice open. Host story at EVENTID 10000+. Failures go to `VestigiumLog.Thrown` and the embedded .NET catalog. No topology in the log. |
