# RouteIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.RouteIQ`  
**APPID:** `RouteIQ`  
**Status:** Live  
**Date:** 2 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types and columns. Helpers.Network Requirements v1.6 wins on protocol. This file wins on the slice.

**Goal:** Replace the log stub with the first shippable window: family, refresh, route grid, neighbor grid, one probe address, status line.

**Not:** Option C add/change/remove. Not a default-route button. Not neighbor flush. Not a saved route set. Not `route.exe` / `netsh` / `arp`. Not a chart. Not a library change. Not a second window.

Path note: the request named `WPF/TraceIQ`. That folder has no design or requirements. This plan implements `WPF/RouteIQ`. TraceIQ is a different host.

---

## Starting point (repo as of this plan)

The exe exists. It is not first-and-ten.

| Piece | Today | Required |
|---|---|---|
| `App.OnStartup` | `HostLog.Initialize(HostIds.RouteIQ)` | Keep. |
| Project refs | Shell only | Keep. Packages stay on Shell. Do not add a Helpers reference. |
| Window | Title, `Print routes`, read-only `Log` box | Family, Refresh, probe box, Probe, two grids, status. No `Log`. No write controls. |
| `MainViewModel` | `GetRoutes()` into a string. Ctor refreshes. Failure overwrites `Log`. | Family, two lists, probe, status. Failure keeps the lists. |
| Neighbors / Probe | Missing | Required. |
| Tests | `HostIds` only | Host reject + family map. Not a stack suite. |

Treat `Log` as debt. Delete it. Do not keep a text dump beside the grids.

Package drift: Requirements v1.0 says Helpers.Network **1.2.0**. `Directory.Build.props` pins **1.3.6**. Do not downgrade. Do not bump. The doors this slice needs are on the pin.

---

## Decision

One PR. Four build slices plus owner gate. The stub is the start, not a second product.

| Call | Why |
|---|---|
| Pass `RouteFamily` into `GetRoutes` | The door accepts it (`All` default). Do not fetch All and filter routes in the host. |
| Filter neighbors in the VM | `GetNeighbors()` has no family argument. |
| Window labels are All / IPv4 / IPv6 | Library enum is `RouteFamily.All`, `Pv4`, `Pv6`. Map in the host. Do not show `Pv4` on the window. Do not rename the library in this PR. |
| Bind `NetworkRoute` and `NetworkNeighbor` directly | Columns already match Design. A host row type is a copy with no buyer. |
| One busy flag for Refresh and Probe | Second click does nothing. Do not queue. |
| Print and probe run off the UI thread | Both doors are synchronous. `ProbeNeighbor` can ARP and nudge. A hung stack must not freeze the window. |
| Blank probe does not start | Acceptance 3. `CanExecute` false when the box is blank or busy. |
| Garbage probe address rejects in the host | Library throws `ArgumentException` on a non-IP. Host parses first so Status is Failed and the library is not the validator. |
| `Found == false` is Failed, probe line is the address and no MAC | Requirements: address + MAC, or Failed. A miss is not a crash and not a success line. |
| Failed print keeps the last lists | R5. Status carries the exception or `NetworkRouteDenied` text. |
| No `IRouteService` | Static `NetworkHelper` stays the door. |
| Host tests do not call `GetRoutes`, `GetNeighbors`, or `ProbeNeighbor` | Those hit the stack. Protocol tests live in Helpers. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Keep the `Log` box “for now” | Design is two lists. Two surfaces is fog. |
| Add `IsPersistent` column | Not in Design. Record has it. Window does not. |
| Fetch All and filter routes in the VM | Door already takes `RouteFamily`. Extra filter is a second policy. |
| Rename `RouteFamily.Pv4` / `Pv6` in Helpers | Wrong repo. Wrong slice. Map at the combo. |
| Write form, collapsed, “for later” | Design forbids leftover write fields. |
| Default-route checkbox | Library lock 34. Permanent non-goal. |
| `Task.Run` per row or a progress channel | Gold plate. One off-thread call per button. |
| Suite shell / theme chrome in PR01-01 | Design is a window. DnsIQ chrome is a later host. Not this slice. |

---

## Library doors this slice may call

Pin: `Vestigium.Helpers.Network` **1.3.6** (suite `Directory.Build.props`). Protocol still from Requirements v1.6.

```
NetworkHelper.GetRoutes(RouteFamily family = RouteFamily.All)
    → IReadOnlyList<NetworkRoute>
      Family, Destination, PrefixLength, Mask, Gateway,
      InterfaceName, InterfaceIndex, Metric, IsPersistent, Protocol

NetworkHelper.GetNeighbors()
    → IReadOnlyList<NetworkNeighbor>
      Family, Address, MacAddress, InterfaceName, State

NetworkHelper.ProbeNeighbor(string address)
    → NeighborProbeResult(Address, MacAddress, Found)
      Throws ArgumentException if the text is not one IPv4 or IPv6 address.
```

`RouteFamily`: `All`, `Pv4`, `Pv6`. Not `IPv4` / `IPv6`.

Do not call `AddRoute`, `ChangeRoute`, `RemoveRoute`.

---

## Types to land

| Type | File | Role |
|---|---|---|
| `App` | `App.xaml.cs` | Already correct. No protocol calls. |
| `MainWindow` | `MainWindow.xaml` / `.cs` | DataContext only. No `NetworkHelper`. |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | Family, lists, probe, status, two commands. |
| `RouteIqInput` | `ViewModels/RouteIqInput.cs` | Probe parse and family map. No sockets. |

No `RouteRow`. No `NeighborRow`. Grids bind the library records. Hide `IsPersistent` by not declaring the column.

---

## Window map

Toolbar: Family combo (All default; IPv4; IPv6), Refresh, probe address box, Probe.  
Under the probe box: Status, then the last probe line.  
Top grid: routes — Destination, PrefixLength, Mask, Gateway, InterfaceName, InterfaceIndex, Metric, Protocol.  
Bottom grid: neighbors — Address, MacAddress, InterfaceName, State.

No Add, Change, Remove, Default, Flush. No collapsed write panel. No chart. No `Log`.

Combo map:

| Combo | `RouteFamily` | Neighbor filter |
|---|---|---|
| All | `All` | none |
| IPv4 | `Pv4` | `AddressFamily.InterNetwork` |
| IPv6 | `Pv6` | `AddressFamily.InterNetworkV6` |

---

## Behavior

### Input (`RouteIqInput`)

| Input | Rule |
|---|---|
| Family | Map the three labels. Unknown label → reject. |
| Probe address | Trim. Blank → do not start (command disabled, not Failed). Non-empty and `IPAddress.TryParse` fails → reject, Status Failed, do not call the library. |

### Refresh

1. If busy, command does nothing.
2. Status = `Running`. Leave the previous probe line. Do not clear the grids yet.
3. Off the UI thread: `GetRoutes(mappedFamily)` and `GetNeighbors()`, then filter neighbors.
4. Success: replace both lists. Status = `Idle`.
5. Exception: keep the previous lists. Status = `Failed`. Error text = `ex.Message` (covers `NetworkRouteDenied`).
6. `finally`: clear busy, raise CanExecute.

On open, the VM still refreshes once (Requirements: on open + Refresh). Same path as the button. Failure on open leaves empty lists and Failed — there is no previous list.

### Probe

1. Disabled while busy or when the box is blank or whitespace.
2. Parse. Reject → Status `Failed`, do not set Running, do not touch the lists.
3. Status = `Running`. Off the UI thread: `ProbeNeighbor(parsed)`.
4. `Found` → probe line is `Address` + `MacAddress`. Status = `Idle`.
5. Not found → probe line is the address and no MAC. Status = `Failed`.
6. Exception → Status = `Failed`, error text = `ex.Message`. Lists unchanged.
7. `finally`: clear busy, raise CanExecute.

Probe does not scan the neighbor grid as the primary path (R4).

---

## Implementation table

Build order is the Order column.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01-01 | Window. Delete `Log`. Family, Refresh, probe box, Probe, two grids, status line. No write controls. | Done |
| 2 | PR01-02 | `RouteIqInput` + Refresh. Pass family. Filter neighbors. Failure keeps lists. Off UI thread. | Done |
| 3 | PR01-03 | Probe. Blank disabled. Garbage rejects before the library. Found / not-found status line. | Open |
| 4 | PR01-04 | Host tests for parse and family map. No stack. | Open |
| 5 | PR01-05 | Owner runs the window against Requirements §4. | Owner |

### PR01-01

Done. `MainWindow.xaml` is the Design layout. `Log` is gone. `MainWindow.xaml.cs` is still DataContext-only. Grids are read-only and bind `NetworkRoute` / `NetworkNeighbor`. Combo items are All / IPv4 / IPv6.

Held for later slices, on purpose:

- Family is bound and not applied. Refresh still calls `GetRoutes()` with the default.
- Neighbors collection stays empty. No `GetNeighbors` yet.
- Probe `CanExecute` is false. The button is on the window and cannot start.
- No suite shell and no theme merge. Design did not ask for DnsIQ chrome.

### PR01-02

Done. `RouteIqInput.TryMapFamily` is the only family map: All / IPv4 / IPv6 → `All` / `Pv4` / `Pv6`. Unknown label fails before the library. Refresh passes that family to `GetRoutes`, calls `GetNeighbors`, and filters neighbors in the VM. Both calls run in one `Task.Run`. List replace is after `ConfigureAwait(true)`. Failure does not clear the grids. Probe line is left alone. Probe `CanExecute` is still false.

`TryParseProbe` is on the type for PR01-03. This slice does not call it.

### PR01-03

Same busy flag. `CanExecute` watches `ProbeAddress` and busy. Parse with `RouteIqInput.TryParseProbe`. Call `ProbeNeighbor` only after a successful parse.

### PR01-04

Add `tests/Vestigium.Suite.Network.Tests/RouteIqInputTests.cs`.

Must assert:

- `All` → `RouteFamily.All`
- `IPv4` → `RouteFamily.Pv4`
- `IPv6` → `RouteFamily.Pv6`
- unknown label rejects
- blank / whitespace probe is not a parse success (command stays disabled; parser returns blank)
- `127.0.0.1` and `::1` parse
- `not-an-ip` and `dns.google` reject

Keep `HostIdsTests` (`RouteIQ` is not `Network`).

Do **not** add tests that call `GetRoutes`, `GetNeighbors`, or `ProbeNeighbor`.

### PR01-05

Owner on the clone:

1. Window opens. `%ProgramData%\Vestigium\Logs\RouteIQ\` exists after first run.
2. Refresh fills route and neighbor lists without throwing out of the UI (empty lists allowed).
3. Probe with a blank address does not start.
4. Probe `127.0.0.1` returns a result line or Failed — not an unhandled exception.
5. No Add / Change / Remove / Default-route control on the window.
6. A forced print failure (if you can induce one) leaves the previous lists.

This agent does not mark first-and-ten closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.RouteIQ/MainWindow.xaml
src/Vestigium.Suite.Network.RouteIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.RouteIQ/ViewModels/RouteIqInput.cs       [NEW]
tests/Vestigium.Suite.Network.Tests/RouteIqInputTests.cs             [NEW]
```

Do not edit Shell. Do not bump or downgrade package pins. Do not add a project reference from the exe to Helpers — Shell already flows Network through. Do not edit `Vestigium.Helpers`.

When this plan finishes, move the whole `PR01/` folder to `PR-Plans/Completed/PR01/` and idle the queue README.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No row-type copy. No write fields. No Helpers edit. Combo does not display `Pv4`. |
| Theodore | Failure does not clear lists. Blank probe cannot start. Off-thread call still lands list updates on the UI thread. Tests stay off the stack. |
| Simon | Family passed to `GetRoutes` is `Pv4` / `Pv6`, not a host-invented enum the library does not have. Pin stays 1.3.6. `Found == false` is not reported as a MAC. |

---

## Next action

PR01-03. Probe. Blank stays disabled. Garbage rejects in `TryParseProbe` before `ProbeNeighbor`. Found writes address + MAC. Miss is Failed and no MAC.
