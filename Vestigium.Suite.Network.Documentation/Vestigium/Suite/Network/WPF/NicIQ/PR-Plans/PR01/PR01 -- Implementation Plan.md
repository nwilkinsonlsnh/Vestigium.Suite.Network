# NicIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 27 September 2026  
**Revised:** 27 September 2026 — owner: suite skeleton, not a bare grid  
**Binding:** Requirements v1.0 wins on the **job** (list + detail + watch). Helpers.Network 1.2.0 wins on protocol. DnsIQ / PingIQ Rev 1 wins on **chrome**. This file wins on the slice. Design v1.0 is the job map only; it does not describe chrome.

**Goal:** Stand NicIQ up as a Vestigium suite host (shell, theme, status bar, pages, package pins) and ship the first-and-ten job on the NicIQ page: which NIC, is it up, how fast.

**Not:** A protocol library. Not `SampleCounters`. Not a wrap of `NetworkHelper`. Not a project reference to DnsIQ or PingIQ. Not disable/rename/DHCP renew. Not `Get-NetAdapter`. Not live-updating charts during Watch. Not a pin bump.

---

## Owner lock (27 Sep 2026)

Old plan treated chrome as gold plate because Requirements v1.0 said “one window, no tabs, no chart.”

Owner: the application is a suite skeleton. Use the Vestigium packages already pinned in `Directory.Build.props` and already consumed by DnsIQ / PingIQ — Themes, Controls (Shell, StatusBar, NumericUpDown, UnderConstruction), Converters, Analytics, Charts, Network, Logging. Copy the host pattern. New namespace.

Requirements v1.0 is now stale on chrome. Do not rewrite that paper in this slice. Do not invent a NicIQ Requirements v1.1 to justify tabs. The job rules (N1–N10, acceptance 1–8 except “no chart control”) still stand.

---

## Starting point (repo as of this plan)

The exe already exists. It is a stub, not a suite host.

| Piece | Today | Required |
|---|---|---|
| `App.OnStartup` | `HostLog.Initialize(HostIds.NicIQ)` then base | HostLog → Themes → DI → settings → `NicIqWindow`. |
| Project refs | Shell only. No Themes / Controls / Charts pins on the exe. | Same pins as PingIQ / DnsIQ. Network still flows through Shell. |
| Window | `MainWindow` + Refresh + `Log` | `NicIqWindow` + `VestigiumShell`. Three pages. No `Log`. |
| `MainViewModel` | `GetAdapters()` dumped into `Log` | Job VM on the NicIQ page. |
| Tests | `HostIds` omits NicIQ | Host clamp + settings store. Not a protocol suite. |

Treat `Log` as debt. Replace it. Do not keep both surfaces.

---

## Decision

One PR. Six build slices plus owner gate. The stub is the start, not a second product.

Chrome copies DnsIQ Rev 1 / PingIQ PR01-01. Job stays Requirements first-and-ten. Dashboard is a hosted `ChartView` skeleton. It does not invent a counter engine.

| Call | Why |
|---|---|
| Copy chrome, new namespace | Owner. Hosts do not reference each other. |
| Pins match PingIQ csproj | Already in `Directory.Build.props`. Do not invent versions. |
| Startup order | HostLog (register Analytics + Charts catalogs) → ThemeCatalog → Themes.Initialize → DI → window. |
| Pages: NicIQ \| Dashboard (locked) \| Settings | Same shell as DnsIQ. Dashboard unlocks after a completed Watch, not after Refresh. |
| Duration is `VestigiumNumericUpDown` | Suite control. Default 10. Min 1. Max 60. |
| Status lives on `VestigiumStatusBar` | Not a homemade TextBlock under the grid. Page caption may echo the same line. |
| One in-flight watch | N6. Refresh disabled while Running. |
| Selection key is `Id`, fallback `Name` | N4. Index `0` is not a NIC. |
| Detail from the selected list row | `GetAdapter` only if stale. |
| Include down default **on** | `NetworkAdapterQuery.IncludeDown`. |
| Host duration clamp **1–60 s** | Form is tighter than the library 10 ms–1 h. |
| Do not set `AdapterWatchOptions.Interval` | Watch is one duration. |
| Failed inventory keeps the last list | N7. |
| Cancel → `Cancelled` | Cancel is not Failed. |
| No `INicService` | Static `NetworkHelper`. |
| Dashboard does not call `SampleCounters` | Out of first ten. Empty `ChartView` hosts + UnderConstruction until Watch produces samples. Then `NumericSeries` + `ChartView` + `ChartTheme.Paint`. No ScottPlot usings. |
| Host tests stay off the wire | Protocol tests live in Helpers. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Bare `MainWindow` grid with no shell | Owner rejected. Suite hosts look like suite hosts. |
| Keep `Log` “for now” | Two surfaces is fog. |
| Wrap `NetworkHelper` | Forbidden façade. |
| ProjectReference DnsIQ or PingIQ | Copy the pattern. New types. |
| Bump package pins | Pins already match the live hosts. |
| `SampleCounters` as the Dashboard series | Second job. Watch samples are enough to prove Charts + Analytics wire. |
| Live sparkline during Watch | DnsIQ lock: paint after the job. |
| Set `Interval` so the chart moves | Sampler. Out. |
| Spawn `Get-NetAdapter` | N8. |
| Unlock Dashboard on Refresh | Refresh is inventory. Unlock is Watch. |

---

## Package pins this host must list

Copy `Vestigium.Suite.Network.PingIQ.csproj`. Do not invent a third set.

| Package | Pin (Directory.Build.props) | Role on this host |
|---|---|---|
| `Vestigium.Helpers.Network` | 1.2.0 | Through Shell. Job doors. |
| `Vestigium.Logging` | 1.7.1 | Through Shell. `HostLog.Initialize(HostIds.NicIQ)`. |
| `Vestigium.Helpers.Analytics` | 1.0.1 | `NumericSeries` after Watch. Register catalog at HostLog. |
| `Vestigium.Helpers.Charts` | 1.0.5 | `ChartView` on Dashboard. No ScottPlot usings. |
| `Vestigium.Themes` | 1.0.2 | `ThemeCatalog` + `Themes.Initialize`. |
| `Vestigium.Controls` | 1.0.0 | `VestigiumShell`, default window VM. |
| `Vestigium.Controls.StatusBar` | 1.0.0 | Status bar. |
| `Vestigium.Controls.NumericUpDown` | 1.0.1 | Duration. |
| `Vestigium.Controls.UnderConstruction` | 1.0.0 | Dashboard empty state. |
| `Vestigium.Converters` | 1.0.0 | DI register. |
| `CommunityToolkit.Mvvm` | 8.4.0 | Through Shell. |
| `Microsoft.Extensions.DependencyInjection` | 10.0.12 | Same as PingIQ csproj (not the props token). |

Network stays consumed through Shell. The exe does not add a second Network PackageReference.

---

## Library doors the job may call

```
NetworkHelper.GetWorkstation() → WorkstationNetwork
NetworkHelper.GetAdapters(NetworkAdapterQuery?) → IReadOnlyList<NetworkAdapter>
NetworkHelper.GetAdapter(string nameOrId) → NetworkAdapter   // stale row only
NetworkHelper.WatchAdapter(string nameOrId, AdapterWatchOptions?) → NetworkJob<AdapterWatchResult>
```

`AdapterWatchOptions.Duration` only. Do not set `Interval`.

`WatchAdapter` throws `ArgumentException` at create when the adapter is missing. That is Failed.

Do not call `SampleCounters`, `IcmpEcho`, `LookupAsync`, `GetRoutes`, `GetSnapshot`, or OUI lookup.

---

## Types to land

| Type | File | Role |
|---|---|---|
| `App` | `App.xaml` / `.cs` | HostLog → Themes → DI → session → `NicIqWindow`. `SetDashboardEnabled`. |
| `ThemeCatalog` | `ThemeCatalog.cs` | Same nine packs as DnsIQ. |
| `ThemeChrome` | `ThemeChrome.cs` | Bind status bar to theme brushes. |
| `NicIqWindow` | `NicIqWindow.xaml` / `.cs` | Chrome. Primary work area, centered, Manual startup. Builds View → Theme. No protocol. |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | Header, list, detail, Refresh / Watch / Cancel, status bar posts. |
| `SettingsViewModel` | `ViewModels/SettingsViewModel.cs` | Pages NicIQ / Theme. Duration default, IncludeDown default, palette, bar. |
| `DashboardViewModel` | `ViewModels/DashboardViewModel.cs` | Hosts `FrameworkElement` from `ChartView`. `Unlock` after Watch. |
| `ChartTheme` | `ViewModels/ChartTheme.cs` | Copy PingIQ. Slot colors from theme brushes. |
| `AdapterRow` | `ViewModels/AdapterRow.cs` | Grid projection. |
| `NicIqWatchInput` | `ViewModels/NicIqWatchInput.cs` | Clamp / reject. No sockets. |
| `NicIqSettings` / `NicIqSettingsStore` / `NicIqSession` | `ViewModels/` | Persist under `%ProgramData%\Vestigium\Settings\Diagnostics\NicIQ\`. |
| `PageViewport` | `Views/PageViewport.cs` | Copy. |
| Views | `Views/NicIqView.xaml`, `DashboardView.xaml`, `SettingsView.xaml` | Pages. |

No `INicService`. No ScottPlot usings.

---

## Window map

`NicIqWindow` + `VestigiumShell` (`ShowNav=False`)

```
menu  File (Exit)
      View (bar visible, bar Top/Bottom, Theme submenu — one check)
icon  Assets/NicIQ.ico
place primary WorkArea, centered, Manual startup
nav   HorizontalTab  NicIQ | Dashboard (disabled until Watch) | Settings
client
  NicIQ
    caption = live status
    header  workstation HostName / DomainName
    Refresh, Include down, Duration (NumericUpDown 1–60, default 10), Watch, Cancel
    left   adapter grid  Name, Status, Type, MacAddress, SpeedBitsPerSecond, Id
    right  detail  Description, unicasts, gateways, DNS, DHCP, NetBIOS
  Dashboard
    empty + Open NicIQ until unlocked
    after Watch: speed series via NumericSeries + ChartView (line). Optional status text.
  Settings
    indented HorizontalTab  NicIQ | Theme
    NicIQ:  default Duration, default Include down
    Theme:  palette, bar visible, dock
status bar    message | progress | detail | clock
```

NumericUpDown host style: centered text, theme card/stroke — copy `App.xaml` from DnsIQ.

---

## Behavior

### Input (`NicIqWatchInput.TryCreate`)

| Input | Rule |
|---|---|
| Adapter key | Trim. Blank → reject (“No adapter selected.”). |
| Duration seconds | `< 1` or `> 60` → reject. `1..60` → `TimeSpan.FromSeconds(n)`. |

### Inventory

On construct and Refresh: `GetWorkstation()` for the header, `GetAdapters(new NetworkAdapterQuery { IncludeDown = IncludeDown })` for the list. Preserve selection by `Id`. Selection fills detail from that row. Inventory exception keeps the last list and posts `Failed` + `ex.Message` on the status bar.

### Watch

1. Busy → command no-ops.
2. Validate. Reject does not set Running.
3. Status bar Message = `Running`. New CTS.
4. `WatchAdapter(key, new AdapterWatchOptions { Duration = duration })` then `RunAsync(token)`.
5. Hold the job so Cancel calls `job.Cancel()` and the token.
6. Success: Message = `LastStatus` + last sample speed (bits/s). Push samples to Dashboard as a speed series. Unlock Dashboard.
7. Cancel → `Cancelled`. Do not unlock.
8. Exception including create-time `ArgumentException` → `Failed` + `ex.Message`. List stays.
9. `finally`: dispose token, clear busy, raise CanExecute.

Refresh `CanExecute`: not busy. Watch: not busy and selection present. Cancel: busy.

### Dashboard

`ChartView` + `NumericSeries.From` watch speeds (or elapsed-indexed speed samples). `ChartTheme.Paint`. No UCL/LCL invention in the host — if `ControlLimits` fails, skip the control chart. Empty state is UnderConstruction + “Open NicIQ”, not a fake series.

### Persist

`%ProgramData%\Vestigium\Settings\Diagnostics\NicIQ\settings.json`

Load after Themes.Initialize. Save on change. Tests inject a temp root. Missing / corrupt file → defaults (Duration 10, IncludeDown true, LightBlue).

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01-01 | Skeleton. `NicIqWindow`, ThemeCatalog, ThemeChrome, three pages, icon, primary-monitor center, package pins. Kill `MainWindow`. NicIQ page may still stub the job. Dashboard locked. | Done |
| 2 | PR01-02 | Inventory on NicIQ page. Header, grid, detail, Include down, Refresh, keep-last-list. | Done |
| 3 | PR01-03 | `NicIqWatchInput` + Watch + Cancel + one in-flight. Status bar posts. | Done |
| 4 | PR01-04 | Settings persist + Theme page + Duration / IncludeDown defaults. | Open |
| 5 | PR01-05 | Dashboard `ChartView` + Unlock after Watch. No `SampleCounters`. | Open |
| 6 | PR01-06 | Host tests. No inventory call. No watch call. | Open |
| 7 | PR01-07 | Owner gate. Requirements §4 job + chrome smoke. | Owner |

### PR01-01

Stand the DnsIQ chrome skeleton under the NicIQ namespace. View → Theme check must work. `MainWindow.xaml` goes away when `NicIqWindow` is the show path. Icon: add `Assets/NicIQ.ico` (owner asset or a copy of an existing suite icon renamed — do not block the slice on art).

`App.xaml` copies DnsIQ NumericUpDown + UnderConstruction styles. `csproj` copies PingIQ package list. Assembly name `NicIQ`.

### PR01-02

Refresh is the only library traffic. Watch can stay disabled.

### PR01-03

One `_busy` / `_cts` / optional `_watchJob`. Do not set `Interval`.

### PR01-04

`NicIqSettings` / store / session. Tests inject a temp root.

### PR01-05

No ScottPlot usings. Unlock only after Watch completes.

### PR01-06

`NicIqWatchInputTests`:

- blank / whitespace key rejects
- `Ethernet` + duration `10` accepts → `TimeSpan.FromSeconds(10)`
- `0`, `61`, `-1` reject; `1` and `60` accept

`NicIqSettingsStoreTests`: save → load on a temp directory; corrupt file → defaults.

`HostIdsTests`: add `NicIQ` is not `Network`.

ProjectReference the NicIQ exe from the test project.

Do **not** call `GetAdapters` / `WatchAdapter` from tests.

### PR01-07

Owner on the clone:

1. Window opens as suite chrome. `%ProgramData%\Vestigium\Logs\NicIQ\` exists after first run.
2. Theme menu switches palette. Status bar dock works.
3. Refresh fills a list without throwing (empty list allowed).
4. Header shows a host name from `GetWorkstation`.
5. Selecting a row fills detail.
6. Watch with no selection does not start.
7. Watch 1–10 s on a selected up adapter returns a status line.
8. Cancel stops a running watch.
9. Dashboard stays locked until Watch completes; then a `ChartView` is present.
10. No counter button. No send-packet button. No `SampleCounters`.

This agent does not mark first-and-ten closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.NicIQ/App.xaml
src/Vestigium.Suite.Network.NicIQ/App.xaml.cs
src/Vestigium.Suite.Network.NicIQ/Vestigium.Suite.Network.NicIQ.csproj
src/Vestigium.Suite.Network.NicIQ/NicIqWindow.xaml                 [NEW]
src/Vestigium.Suite.Network.NicIQ/NicIqWindow.xaml.cs              [NEW]
src/Vestigium.Suite.Network.NicIQ/ThemeCatalog.cs                   [NEW]
src/Vestigium.Suite.Network.NicIQ/ThemeChrome.cs                    [NEW]
src/Vestigium.Suite.Network.NicIQ/Assets/NicIQ.ico                  [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.NicIQ/ViewModels/AdapterRow.cs          [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/NicIqWatchInput.cs     [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/NicIqSettings.cs       [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/NicIqSettingsStore.cs  [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/NicIqSession.cs        [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/SettingsViewModel.cs   [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/DashboardViewModel.cs  [NEW]
src/Vestigium.Suite.Network.NicIQ/ViewModels/ChartTheme.cs          [NEW]
src/Vestigium.Suite.Network.NicIQ/Views/NicIqView.xaml              [NEW]
src/Vestigium.Suite.Network.NicIQ/Views/DashboardView.xaml          [NEW]
src/Vestigium.Suite.Network.NicIQ/Views/SettingsView.xaml           [NEW]
src/Vestigium.Suite.Network.NicIQ/Views/PageViewport.cs             [NEW]
src/Vestigium.Suite.Network.NicIQ/MainWindow.xaml                   [DELETE after window swap]
src/Vestigium.Suite.Network.NicIQ/MainWindow.xaml.cs                [DELETE after window swap]
tests/Vestigium.Suite.Network.Tests/NicIqWatchInputTests.cs         [NEW]
tests/Vestigium.Suite.Network.Tests/NicIqSettingsStoreTests.cs      [NEW]
tests/Vestigium.Suite.Network.Tests/HostIdsTests.cs
tests/Vestigium.Suite.Network.Tests/Vestigium.Suite.Network.Tests.csproj
```

Do not edit Shell except the APPID assert. Do not bump pins in `Directory.Build.props`. Do not add a Network PackageReference on the exe.

When this plan finishes, move the whole `PR01/` folder to `PR-Plans/Completed/PR01/` and idle the queue README.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | Chrome is a copy, not a remix. No façade. No `SampleCounters`. Grid columns match `NetworkAdapter`. |
| Theodore | Cancel and Failed stay on the status bar. CanExecute blocks the second Watch. Tests stay off inventory. |
| Simon | Host clamp 1–60 s. `Interval` unset. Dashboard unlocks on Watch, not Refresh. Status names from the library. |

---

## Next action

PR01-04. Settings persist + Theme page + Duration / IncludeDown defaults.
