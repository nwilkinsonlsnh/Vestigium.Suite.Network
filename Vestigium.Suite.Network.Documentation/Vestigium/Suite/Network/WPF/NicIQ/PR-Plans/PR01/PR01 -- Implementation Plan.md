# NicIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 27 September 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. Helpers.Network Requirements v1.6 and package **1.2.0** win on protocol. This file wins on the slice.

**Goal:** Ship the first usable NicIQ window: workstation header, adapter list, selected detail, Include-down, Refresh, Watch 1–60 s, Cancel, status line.

**Not:** A protocol library. Not `SampleCounters`. Not a chart or sparkline. Not DnsIQ/PingIQ chrome, themes, persist, or dashboard. Not a wrap of `NetworkHelper`. Not disable/rename/DHCP renew. Not a bind-to-this-NIC helper for PingIQ. Not `Get-NetAdapter` / `ipconfig` / `ethtool`.

---

## Starting point (repo as of this plan)

The exe already exists. It is not first-and-ten.

| Piece | Today | Required |
|---|---|---|
| `App.OnStartup` | `HostLog.Initialize(HostIds.NicIQ)` | Keep. |
| Project refs | Shell only | Keep. Packages stay on Shell. |
| Window | Title + Refresh + `Log` text box | Header + toolbar + grid + detail + Status. No `Log`. |
| `MainViewModel` | `GetAdapters()` dumped into `Log`. No selection. No watch. No cancel. | Full VM per Design. |
| Tests | `HostIds` lists Ping/Trace/Dns only | Host clamp + reject tests. Not a protocol suite. |

Treat the current `Log` property and the read-only `TextBox` as debt. Replace them. Do not keep both surfaces.

---

## Decision

One PR. Four build slices plus owner gate. The stub is the start, not a second product.

Requirements first-and-ten is “which NIC, is it up, how fast.” That is one window. Copying DnsIQ Rev 1 chrome into this slice is a stunt.

| Call | Why |
|---|---|
| One in-flight watch | N6. Second Watch is ignored, not queued. Refresh disabled while Running. |
| Watch with no selection does not start | Acceptance 5. `CanExecute` false. Do not invent IfIndex `1`. |
| Selection key is `Id`, fallback `Name` | N4. Index `0` is not a NIC. |
| Detail comes from the selected list row | Design §Flow. `GetAdapter` only if that row is stale after a watch or the operator asks Refresh. |
| Include down default **on** | Requirements. Pass `NetworkAdapterQuery { IncludeDown = flag }`. Library default is already `true`. |
| Duration default **10 s**. Host clamp **1–60 s** | Requirements. Library floor is 10 ms and ceiling is 1 hour — host clamp is tighter and wins on the form. |
| Host does not set `AdapterWatchOptions.Interval` | Watch is one duration, not a sample engine. Null interval is the library start/end pair. |
| Failed inventory keeps the last list | N7, locked: keep. Status = `Failed` plus `ex.Message`. |
| Failed watch does not wipe the list | List is inventory, not a job grid. Status = `Failed`. Detail stays. |
| Cancel → Status `Cancelled` | Cancel is not Failed. |
| Status line after watch uses library fields | `LastStatus` + last sample `SpeedBitsPerSecond` when present. Do not remap `OperationalStatus` names. |
| Field names match the library | N3. `Status`, not `OperationalStatus`. Speed column is `SpeedBitsPerSecond`. |
| No `INicService` | Consume contract. Static `NetworkHelper` stays the door. |
| Host tests do not hit inventory or watch | Suite tests are host contracts. Protocol tests live in Helpers. |
| No chrome lift this slice | Requirements: one window, no tabs, no `ChartView`. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Keep the `Log` box “for now” | Design is a grid + detail. Two surfaces is fog. |
| Wrap `NetworkHelper` so tests can mock adapters | Forbidden façade. Clamp and reject without calling the library. |
| `SampleCounters` next to Watch | Requirements Out. Second job. |
| Sparkline / Charts | N9. After counters exist. |
| Port DnsIQ `PingIqWindow` chrome, ThemeCatalog, persist | Wrong first-and-ten. That is a later host-paper change, not this slice. |
| Call `GetAdapter` on every click | Design: same record unless stale. Extra round trip invents nothing. |
| Set `Interval` to 1 s “so the status moves” | That is a live sampler. Out of first ten. |
| Spawn `Get-NetAdapter` when the library list is empty | N8. Empty list is allowed. |
| Watch loopback by hard-coded index `1` when nothing is selected | N4. No selection means no watch. |
| Bill / P95 on speed samples | Watch does not bill. Analytics stays unused. |

---

## Library doors this slice may call

Pin: `Vestigium.Helpers.Network` **1.2.0** (already in `Directory.Build.props`).

```
NetworkHelper.GetWorkstation()
    → WorkstationNetwork          // HostName, DomainName, CapturedUtc

NetworkHelper.GetAdapters(NetworkAdapterQuery?)
    → IReadOnlyList<NetworkAdapter>
      NetworkAdapterQuery.IncludeDown   // default true

NetworkHelper.GetAdapter(string nameOrId)
    → NetworkAdapter              // only when the selected row is stale

NetworkHelper.WatchAdapter(string nameOrId, AdapterWatchOptions?)
    → NetworkJob<AdapterWatchResult>
      RunAsync(token) / Cancel()
      AdapterWatchOptions.Duration      // host sets 1–60 s; do not set Interval
      AdapterWatchResult.FirstStatus, LastStatus, Samples[], Elapsed
```

`NetworkAdapter` fields the window binds:

| Grid | Detail |
|---|---|
| `Name`, `Status`, `Type`, `MacAddress`, `SpeedBitsPerSecond`, `Id` | `Description`, `UnicastAddresses`, `Gateways`, `DnsServers`, `Dhcp`, `NetbiosOverTcp` |

Do not call `SampleCounters`, `IcmpEcho`, `LookupAsync`, `GetRoutes`, `GetSnapshot`, or OUI lookup.

`WatchAdapter` throws `ArgumentException` at create when the adapter is missing. That is Failed, not a crash.

---

## Types to land

| Type | File | Role |
|---|---|---|
| `App` | `App.xaml.cs` | Already correct. Do not add protocol calls. |
| `MainWindow` | `MainWindow.xaml` / `.cs` | DataContext only. No `NetworkHelper`. |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | Header, list, detail, Status, Refresh/Watch/Cancel. |
| `AdapterRow` | `ViewModels/AdapterRow.cs` | Grid projection of `NetworkAdapter`. Holds the source record or the six grid fields plus a detail payload. No protocol. |
| `NicIqWatchInput` | `ViewModels/NicIqWatchInput.cs` | Pure clamp / reject. No sockets. No `NetworkInterface`. |

`NicIqWatchInput` is the cheapest seam that is not a second Network API. Tests call it. The VM calls it, then the library.

---

## Window map

Header: workstation `HostName` / `DomainName` from `GetWorkstation`. `CapturedUtc` may sit on the same line. It is optional.

Toolbar: Refresh, Include down (checked), Duration (default 10), Watch, Cancel.

Left: read-only `DataGrid` bound to `Adapters` — columns Name, Status, Type, MacAddress, SpeedBitsPerSecond, Id.

Right: selected detail — Description, unicast addresses, gateways, DNS servers, DHCP server/lease when present, NetBIOS-over-TCP enum. A read-only `TextBox` or stacked `TextBlock`s is enough. Do not invent a second grid of counters.

Bottom: Status line. Idle / Running / Failed / Cancelled plus last watch outcome.

No tab control. No chart host. No counter button. No send-packet button.

Speed cell: number as bits/s when `SpeedBitsPerSecond` has a value; blank when null.

---

## Behavior

### Input (`NicIqWatchInput.TryCreate`)

Reject and return a message. Do not call the library.

| Input | Rule |
|---|---|
| Adapter key | Trim. Blank → reject (“No adapter selected.”). |
| Duration seconds | Not a number → reject. `< 1` → reject. `> 60` → reject. `1..60` → `TimeSpan.FromSeconds(n)`. |

Refresh has no watch input. Include-down is a bool on the VM, not a reject rule.

### Inventory

1. On construct and Refresh: `GetWorkstation()` for the header. `GetAdapters(new NetworkAdapterQuery { IncludeDown = IncludeDown })` for the list.
2. Map each `NetworkAdapter` to `AdapterRow`. Preserve selection by `Id` when the id still exists after reload.
3. Selection → Detail from that row. Do not call `GetAdapter` unless Refresh left the selected id in the list but the row looks incomplete, or Watch just finished and the operator still has that id selected — then one `GetAdapter(id)` is allowed to refresh detail.
4. Inventory exception: keep the current `Adapters` collection, Status = `Failed`, error text = `ex.Message`. Do not throw out of the command.
5. Empty list is legal. Status may stay `Idle`.

### Watch

1. If busy, command does nothing (`CanExecute` false).
2. Validate via `NicIqWatchInput`. On reject: Status = the message (or `Failed` plus the message). Do not set Running.
3. Status = `Running`. New `CancellationTokenSource`.
4. `var job = NetworkHelper.WatchAdapter(key, new AdapterWatchOptions { Duration = duration }); await job.RunAsync(token)`.
5. Hold the job so Cancel can call `job.Cancel()` as well as cancel the token.
6. Success: Status = `Idle` is wrong. Status line = `LastStatus` plus speed from the last sample when `Samples` is not empty (format bits/s). Prefix with `Cancelled` only on cancel. A completed watch is not Failed; put the outcome on the line (`Up 1000000000` / `Down` / library status name + speed).
7. `OperationCanceledException` / `TaskCanceledException` → Status = `Cancelled`.
8. Any other exception, including create-time `ArgumentException` → Status = `Failed`. Error text = `ex.Message`. List stays.
9. `finally`: dispose token, clear busy, raise CanExecute.

Cancel command: if a `NetworkJob` is live, `job.Cancel()`. Always cancel the token.

Refresh `CanExecute`: not busy. Watch `CanExecute`: not busy and selection present. Cancel `CanExecute`: busy.

`ConfigureAwait(true)` is acceptable on the VM if the command starts on the UI thread; do not marshal by hand.

---

## Implementation table

Build order is the Order column.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01-01 | Window + types. Replace `Log`. Header, Include down, Duration, Watch, Cancel, adapter grid, detail pane, `AdapterRow`. | Open |
| 2 | PR01-02 | Inventory path. `GetWorkstation` + `GetAdapters` + IncludeDown + keep-last-list on fail + selection detail. | Open |
| 3 | PR01-03 | `NicIqWatchInput` + Watch + one in-flight + Cancel. Refresh disabled while Running. | Open |
| 4 | PR01-04 | Host tests for duration clamp, blank key, APPID. No inventory call. No watch call. | Open |
| 5 | PR01-05 | Owner runs the window against Requirements §4. | Owner |

### PR01-01

Rewrite `MainWindow.xaml` to the Design layout. Delete the `Log` text box and the `Log` property. `MainWindow.xaml.cs` stays DataContext-only. `AdapterRow` is a plain record or small class — no logic. Commands may be stubs that set Status.

### PR01-02

Refresh is the only library traffic in this slice. Watch/Cancel can stay disabled or no-op. Constructor still loads once.

### PR01-03

`NicIqWatchInput.TryCreate`. One `_busy` / `_cts` / optional `_watchJob` field. `RelayCommand` `CanExecute` tied to busy + selection. Do not queue a second job. Do not set `Interval`.

### PR01-04

Add `tests/Vestigium.Suite.Network.Tests/NicIqWatchInputTests.cs` (name may vary; behavior names, not release ids).

Must assert:

- blank / whitespace adapter key rejects
- `Ethernet` (any non-blank key) with duration `10` accepts and yields `TimeSpan.FromSeconds(10)`
- duration `0` rejects; duration `61` rejects; duration `1` accepts; duration `60` accepts
- duration `-1` rejects

Add `HostIds.NicIQ` to `HostIdsTests` (`NicIQ` is not `Network`).

Add a project reference from the test project to `Vestigium.Suite.Network.NicIQ` so the input type compiles. Do not reference Helpers from the test project for this slice.

Do **not** add tests that call `GetAdapters`, `GetAdapter`, `GetWorkstation`, or `WatchAdapter`. Those belong in Helpers.

### PR01-05

Owner on the clone:

1. Window opens. `%ProgramData%\Vestigium\Logs\NicIQ\` exists after first run.
2. Refresh fills a list without throwing out of the UI (empty list is allowed).
3. Header shows a host name from `GetWorkstation`.
4. Selecting a row fills detail (addresses may be empty).
5. Watch with no selection does not start.
6. Watch for 1–10 s on a selected up adapter returns a status line (up/down/speed or Failed).
7. Cancel stops a running watch.
8. No counter button, no chart, no send-packet button.

This agent does not mark first-and-ten closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.NicIQ/MainWindow.xaml
src/Vestigium.Suite.Network.NicIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.NicIQ/ViewModels/AdapterRow.cs              [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/NicIqWatchInput.cs         [NEW]
tests/Vestigium.Suite.Network.Tests/NicIqWatchInputTests.cs             [NEW]
tests/Vestigium.Suite.Network.Tests/HostIdsTests.cs
tests/Vestigium.Suite.Network.Tests/Vestigium.Suite.Network.Tests.csproj
```

Do not edit Shell except the APPID assert already listed. Do not bump package pins. Do not add a project reference from the exe to Helpers — Shell already flows Network through. Do not add Themes / Controls / Charts usings to NicIQ.

When this plan finishes, move the whole `PR01/` folder to `PR-Plans/Completed/PR01/` and idle the queue README.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No façade. No chrome lift. No `SampleCounters`. Grid columns match `NetworkAdapter`. Detail is the same record. |
| Theodore | Cancel and Failed do not throw out of the UI. CanExecute blocks the second Watch. Refresh stays off during watch. Tests stay off inventory. |
| Simon | Host duration clamp is 1–60 s, not the library 10 ms–1 h. `Interval` stays unset. Status names come from the library. Index `0` is never a selection key. |

---

## Next action

PR01-01. Kill `Log`. Land the Design window and `AdapterRow`. Inventory and Watch stay the next two slices.
