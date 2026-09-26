# PingIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.PingIQ`  
**APPID:** `PingIQ`  
**Status:** Live  
**Date:** 26 September 2026  
**Binding:** Requirements v1.1 wins on the window. Design v1.1 wins on types. Helpers.Network Requirements v1.6 and package **1.2.0** win on protocol. This file wins on the slice.

**Goal:** Ship the DnsIQ-shaped PingIQ window: chrome, four-echo grid, persist, Probe N over X, Dashboard charts.

**Not:** A protocol library. Not PathMtu. Not UdpProbe. Not `CreateEchoCampaign`. Not Count = 0. Not a wrap of `NetworkHelper`. Not a project reference to DnsIQ. Not a hop walk and not multipath — that is TraceIQ. Not `Process.Start("ping.exe")`.

---

## Starting point (repo as of this plan)

The exe already exists. It is not Rev 1.

| Piece | Today | Required |
|---|---|---|
| `App.OnStartup` | `HostLog.Initialize(HostIds.PingIQ)` then base | HostLog → Themes → DI → settings → window. |
| Project refs | Shell only | Keep. Packages stay on Shell. |
| Window | Target + Echo + Status + `Log` text box | `PingIqWindow` + three pages. No `Log`. |
| `MainViewModel` | Echo count hard-coded 4. No cancel. No busy lock. | Full VM per Design. |
| `ReplyRow` | Missing | Sequence, Status, Address, RttMs, Ttl, Detail. |
| Tests | `HostIds` only | Host reject + settings store. Not a protocol suite. |

Treat the current `Log` property and the read-only `TextBox` as debt. Replace them. Do not keep both surfaces.

---

## Decision

One PR. Six build slices plus owner gate. The stub is the start, not a second product.

Nuget is no longer the fight. Chrome copies DnsIQ Rev 1. Pulse copies DnsIQ `PulsePlan`. Charts use the pins already on Shell (Analytics 1.0.1, Charts 1.0.5).

| Call | Why |
|---|---|
| One in-flight token for Echo and Probe | P3. Second click is ignored, not queued. |
| Host rejects blank target, bad Count, bad Timeout **before** Status becomes Running | Acceptance. Library still runs `EgressBind.Validate` on send. |
| Interface and Source are closed combos | DnsIQ lock. Typing is how spoofing sneaks in. |
| Probe is a host loop of `IcmpEcho(Count=1)`, not `CreateEchoCampaign` | Campaign is recipe + JSONL + clock windows. Wrong shape. |
| Failed Echo clears the grid | P11, locked: clear. |
| Cancel → Status `Cancelled` | Cancel is not Failed. Prelude rows that landed stay. |
| No `IPingService` | Consume contract. Static `NetworkHelper` stays the door. |
| Host tests do not hit the wire | Suite tests are host contracts. Protocol tests live in Helpers. |
| Dashboard gated on Probe | Same as DnsIQ. Echo mix can still paint on the Echo tab once unlocked; unlock itself is Probe-only. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Keep the `Log` box “for now” | Design is a grid. Two surfaces is fog. |
| Wrap `NetworkHelper` so tests can mock send | Forbidden façade. Validate without sending. |
| Use `CreateEchoCampaign` for Probe | Campaign persist and windows are Out of v1.1. |
| Count = 0 on the form | Requirements Out. Needs MaxDuration UI. |
| TTL / Buffer / DF on the query row | Gold plate. Library defaults. |
| Call `EgressBind.Validate` from the host | Type is internal. Copy bind onto options; library validates. |
| Reference DnsIQ types from PingIQ | Hosts do not depend on each other. Copy the pattern, new namespace. |
| Bump package pins | Pins already match DnsIQ Rev 1. |
| Spawn `ping.exe` and scrape stdout | Second protocol stack. Door is `NetworkHelper.IcmpEcho`. |
| Hop list / multipath on this window | TraceIQ. PingIQ is echo + RTT. |

---

## Library doors this slice may call

Pin: `Vestigium.Helpers.Network` **1.2.0** (already in `Directory.Build.props`).

```
NetworkHelper.IcmpEcho(target, IcmpEchoOptions)
    → NetworkJob<IcmpEchoResult>

NetworkHelper.GetAdapters()
    → IReadOnlyList<NetworkAdapter>
```

`IcmpEchoOptions` fields this host sets: `Count`, `Timeout`, `InterfaceIndex`, `SourceAddress`.  
Host does not set `BufferSize`, `Ttl`, `DontFragment`, `Interval`, `MaxDuration`, `AllowBurst`, `StatsPath`.

---

## Implementation table

Build order is the Order column.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01-01 | Chrome. `PingIqWindow`, ThemeCatalog, ThemeChrome, three pages, icon, primary-monitor center. Kill `MainWindow`. Stub Echo/`Log` still on PingIQ page. | Done |
| 2 | PR01-02 | `PingIqInput` + Echo path + `ReplyRow` grid + Cancel + one in-flight. | Done |
| 3 | PR01-03 | Settings + persist + Interface/Source combos. | Done |
| 4 | PR01-04 | Probe prelude + `PulsePlan` loop. Dashboard still locked. | Done |
| 5 | PR01-05 | Dashboard charts + Unlock + legend persist. | Open |
| 6 | PR01-06 | Host tests. No wire. | Open |
| 7 | PR01-07 | Owner runs the window against Requirements v1.1 §5. | Owner |

### PR01-01

Stand up the DnsIQ chrome skeleton under the PingIQ namespace. Dashboard and Settings may be empty shells this slice. View menu Theme check must work. `MainWindow.xaml` goes away when `PingIqWindow` is the `StartupUri` / show path.

### PR01-02

`PingIqInput.TryCreate`. Echo command is the only library call in this slice. Probe can stay disabled until PR01-04. `ConfigureAwait(true)` is acceptable on the VM if the command starts on the UI thread.

### PR01-03

`PingIqSettings` / `PingIqSettingsStore` / `PingIqSession`. Tests inject a temp root. Missing file = defaults. Dead Source → Any.

### PR01-04

Same options builder as Echo, then `Count = 1` per pulse shot. One `_busy` / `_cts` / optional `_echoJob` field. Do not queue a second job.

### PR01-05

No ScottPlot usings. `ChartView` + `NumericSeries` + `ChartTheme.Paint`. Unlock only after Probe completes.

### PR01-06

Add `tests/Vestigium.Suite.Network.Tests/PingIqInputTests.cs` (name may vary; behavior names, not release ids).

Must assert:

- blank / whitespace target rejects
- `127.0.0.1` count 4 timeout 4000 accepts
- count 0 rejects; count 61 rejects; count 1 accepts
- timeout 9 ms rejects; timeout 60001 rejects; timeout 10 accepts
- source `not-an-ip` rejects when the input path still sees a raw string (store/combo tests use Any / a parsed address)
- interface `-1` rejects on the input type if it still accepts an int; combo path cannot produce -1

Keep `HostIdsTests` (`PingIQ` is not `Network`).

Do **not** add tests that call `IcmpEcho`. Those belong in Helpers.

Settings store tests: save → load round trip on a temp directory; corrupt file → defaults.

### PR01-07

Owner on the clone: Requirements v1.1 §5.

This agent does not mark Rev 1 closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.PingIQ/App.xaml
src/Vestigium.Suite.Network.PingIQ/App.xaml.cs
src/Vestigium.Suite.Network.PingIQ/PingIqWindow.xaml                 [NEW]
src/Vestigium.Suite.Network.PingIQ/PingIqWindow.xaml.cs              [NEW]
src/Vestigium.Suite.Network.PingIQ/ThemeCatalog.cs                   [NEW]
src/Vestigium.Suite.Network.PingIQ/ThemeChrome.cs                    [NEW]
src/Vestigium.Suite.Network.PingIQ/Assets/PingIQ.ico                 [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.PingIQ/ViewModels/ReplyRow.cs            [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/PingIqInput.cs         [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/AdapterChoices.cs      [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/SourceChoices.cs       [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/PingIqSettings.cs      [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/PingIqSettingsStore.cs [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/PingIqSession.cs       [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/SettingsViewModel.cs   [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/DashboardViewModel.cs  [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/ChartTheme.cs          [NEW]
src/Vestigium.Suite.Network.PingIQ/ViewModels/PulsePlan.cs           [NEW]
src/Vestigium.Suite.Network.PingIQ/Views/PingIqView.xaml             [NEW]
src/Vestigium.Suite.Network.PingIQ/Views/DashboardView.xaml          [NEW]
src/Vestigium.Suite.Network.PingIQ/Views/SettingsView.xaml           [NEW]
src/Vestigium.Suite.Network.PingIQ/Views/PageViewport.cs             [NEW]
src/Vestigium.Suite.Network.PingIQ/MainWindow.xaml                   [DELETE after window swap]
src/Vestigium.Suite.Network.PingIQ/MainWindow.xaml.cs                [DELETE after window swap]
tests/Vestigium.Suite.Network.Tests/PingIqInputTests.cs              [NEW]
tests/Vestigium.Suite.Network.Tests/PingIqSettingsStoreTests.cs      [NEW]
```

Do not edit Shell except if an APPID test is missing. Do not bump package pins. Do not add a project reference from the exe to Helpers.

When this plan finishes, move the whole `PR01/` folder to `PR-Plans/Completed/PR01/` and idle the queue README.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No façade. No campaign door. No second window. Grid columns match `IcmpEchoReply`. Chrome is a copy, not a remix. |
| Theodore | Cancel and Failed do not throw out of the UI. CanExecute actually blocks the second click. Tests stay off the wire. |
| Simon | Probe is a host loop, not a campaign. Source list is `GetAdapters` unicast only. Count 0 stays rejected. No `ping.exe`. No hop list. |

---

## Next action

PR01-06. Host tests for the remaining seams.
