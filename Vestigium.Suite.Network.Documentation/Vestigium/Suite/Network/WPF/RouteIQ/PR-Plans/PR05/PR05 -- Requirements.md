# RouteIQ — PR05 Requirements

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-PR05
**Version:** PR05
**Status:** Live. Binding for the performance slice until an implementation plan closes it.
**Date:** 6 October 2026
**Project:** `Vestigium.Suite.Network.RouteIQ`
**Kind:** WPF exe, `net10.0-windows`, MVVM
**APPID:** `RouteIQ` (never `Network`)
**Binding:** This file wins on startup and refresh performance. PR04 still wins on protocol honesty: no live vendor call unless opted in, no neighbor ICMP on Refresh, settings path, launch allow-list. Helpers.Network owns the print APIs. A plan that adds a tab, a chart, or a second data model is wrong.

PR05 is the same host, loading the prints it already has without blocking the UI thread. It is not a second product.

---

## 0. Decision

| Call | Why |
|---|---|
| One coordinator, five sources, started together | Splash already walks the tabs. That walk does not request data. `MainWindow.Warm` selects the item and pumps `Render` then `Background`. The prints start in the view-model constructor at `ApplicationIdle`: `Refresh()` and `SnapshotConnections()`. Inside `Refresh`, `Load()` calls `GetRoutes` (v4), `GetRoutes` (v6), `GetNeighbors`, `GetNetBiosNames`, `GetLmHosts` on one thread, in that order. Connections overlap that bundle only by accident of two fire-and-forget calls. The splash exists so the five prints can start together. |
| Data is not tab-gated | Routes, Neighbors, Connections, NetBIOS, and LMHOSTS share one `MainViewModel`. The views are constructed in `CreateMainWindow` before the splash walk. Selecting a tab does not fetch. Do not keep a sequential select in order to "request" a tab. |
| Drop the splash tab walk | It forces layout of every grid on the UI thread, often before the lists exist, then the lists arrive and the grids lay out again. After the prints apply, select RouteIQ and hide. Exports and Settings have no print. Help stays lazy. |
| Refresh uses the same coordinator | A second path will drift. In-flight work is one generation. A click during a refresh arms one more run. It does not stack, and it does not vanish. |
| No measured millisecond budget | We have no trace. A fake p95 is theater. The lock is overlap, UI-thread absence, and a splash cap. Time the box after it ships if a budget is still wanted. |

Rejected: a per-tab loader, a second cache, virtualizing the KQL engine, a splash redesign, charts, new probes, publishing.

---

## 1. What this version is

The host after PR04, with startup and refresh kept off the UI thread, and every print source in flight before the splash walks anything.

| Surface | Today | PR05 |
|---|---|---|
| Splash | Scripted percents. Sequential `Warm` select. Then up to 8s waiting on `PrintsReady`. | Real progress: sources finished / sources started. Hide when every source has applied or failed, or at the cap. |
| Routes, neighbors, NetBIOS, LMHOSTS | One `Task.Run(Load)`. Sequential. Packed OUI is a second hop after `Load` returns. | Five sources start together. Packed OUI starts when neighbors return. It does not hold the other four. |
| Connections | Separate `SnapshotConnections`, already off the UI thread. | Same coordinator. Still off the UI thread. |
| Refresh | `_busy` drops a second click. Replace is one `QuietCollection.Reset` per list. | Same replace. Second click coalesces to one follow-up run. |
| Watch | 1s poll. Snapshot is off-thread. Sort and `Reset` run on the UI continuation (`ConfigureAwait(true)`). | Snapshot, project, and sort off the UI thread. UI thread only `Reset`s. |
| Live vendor | Opt-in, after `_tablesReady`. Pool and pace already exist. | Stays off the splash gate. PR04 R5/R6 hold. |
| KQL | 180ms debounce, then `CollectionView.Refresh` on the dispatcher. | Stays. Not a rewrite. See T4. |

---

## 2. Evidence

| # | Fact | Where |
|---|---|---|
| E1 | Splash warm does not fetch. | `MainWindow.Warm`: `SelectedItem`, then two empty `InvokeAsync` calls. |
| E2 | Warm is a `foreach` over seven items, including Exports and Settings. | `App.Warm`. |
| E3 | Prints start at `ApplicationIdle` in the constructor, not from the splash. | `MainViewModel` constructor. |
| E4 | `Load()` is sequential on one pool thread. | `GetRoutes` v4, `GetRoutes` v6, `GetNeighbors`, `GetNetBiosNames`, `GetLmHosts`. |
| E5 | Connections are a second fire-and-forget. They overlap the bundle. The bundle does not overlap itself. | Constructor calls `_ = Refresh(); _ = SnapshotConnections();`. |
| E6 | Packed OUI waits for the whole bundle. | `ApplyPacked` is a `Task.Run` after `Load` returns. |
| E7 | List replace is already one reset. | `QuietCollection.Reset` suppresses per-item events, then raises `Reset`. |
| E8 | Watch sort runs after the await, so on the captured context. | `ConnectionWatch.Paint`. |
| E9 | Splash will hide after 80 × 100ms even if prints are not ready. | `App.Warm` loop. Keep a cap. Do not make it longer to hide a hang. |
| E10 | Views already exist before the walk. | `CreateMainWindow` assigns `Content` for every work tab. |

---

## 3. Requirements

| # | Rule |
|---|---|
| R1 | At splash show, start these together: IPv4 routes, IPv6 routes, neighbors, connections, NetBIOS names, LMHOSTS. Do not await one before starting the next. |
| R2 | No print API runs on the UI thread. That is `GetRoutes`, `GetNeighbors`, `GetConnections`, `GetNetBiosNames`, `GetLmHosts`, packed OUI lookup, address sort, and connection projection. |
| R3 | The UI thread applies a finished list with one `Reset` per collection. It does not add row by row. |
| R4 | A source that throws keeps the last list and reports. The other sources still apply. Empty is still allowed. |
| R5 | Packed OUI starts when the neighbor snapshot returns. It does not delay routes, connections, NetBIOS, or LMHOSTS. |
| R6 | Splash progress is sources finished over sources started. Scripted 15/30/45 percents are out. |
| R7 | Splash does not select Neighbors, Connections, NetBIOS, LMHOSTS, Exports, or Settings in order to load them. After apply, select RouteIQ, then hide. |
| R8 | Splash hides when every source has applied or failed, or at 8 seconds, whichever is first. A hung source cannot trap the window. |
| R9 | Refresh and cold start share the coordinator. Refresh does not ping. Refresh does not call `LookupOuiAsync` unless the operator opted in. Opt-in vendor work stays after the lists are visible. It does not hold the splash. |
| R10 | One generation in flight. A refresh requested while busy arms exactly one follow-up. It does not run two prints at once. |
| R11 | Watch tick: read, project, and sort off the UI thread. UI thread only replaces the collection. Watch does not start at splash. |
| R12 | Code-behind still does not call `NetworkHelper`. No `route.exe`, `netsh`, `arp`, or `ip`. No elevation. |

---

## 4. Tweaks in this slice

Cheap, because the code is already there. Not a new design.

| # | Tweak | Do | Do not |
|---|---|---|---|
| T1 | Sequential `Load` | Split into the R1 sources. Sort each list on the pool thread that read it. | Invent a scheduler, a channel, or a priority queue. `Task.WhenAll` is enough. |
| T2 | Splash walk | Delete the select loop. Progress text names the source that just finished, or "Prints" when all have. | Animate the bar. Do not warm Help. |
| T3 | Watch `Paint` | Build the row array off-thread. `Reset` on the dispatcher. | Change the 1s poll, the 5–180s window, or the mark glyphs. |
| T4 | KQL | Leave the 180ms debounce. If a filter pass is moved off-thread, marshal one `Refresh` back. A bad compile still reports and leaves the rows. | Replace `CollectionView`. Do not filter by copying into a second collection. |
| T5 | Dropped refresh | Coalesce (R10). | A queue of refreshes. The operator gets the latest, not a history. |
| T6 | Row virtualization | Do not turn it off. Grids stay `EnableRowVirtualization` at the default (true) unless a style already forced it off — if it did, turn it back on. | A custom virtual panel. |

Not a tweak: `HelpView` size. PR03 rejected a document pipeline. Leave it.

---

## 5. Failure

| Case | Required behavior |
|---|---|
| One API throws | That list stays. Status names the source. Splash still hides. |
| One API hangs | Cap fires at 8s. Window is usable. A late result may apply if it is still the current generation. A stale generation must not overwrite a newer refresh. |
| Operator hits Refresh during a print | Current run finishes. One follow-up runs. No pile-up. |
| IP Helper serializes the calls internally | Accept it. Our code still starts them together. Do not add a client-side lock to "be nice" to the stack. |
| Opt-in vendor lookup is on | Lists show first. Vendor stamps arrive after. Splash does not wait on the vendor pool. |
| Settings file missing or unwritable | PR04 behavior. Not this slice. |

Tuesday-at-2am failure this slice exists to stop: the UI thread inside a print, or the operator staring at a splash that is selecting empty tabs while `Load()` is still walking NetBIOS.

---

## 6. Tests

In `Vestigium.Suite.Network.Tests`. No network. No stack.

| # | Lock |
|---|---|
| T-A | A fake five-source run starts every source before any of them complete. |
| T-B | A source that throws does not cancel the others. The failed list is unchanged. |
| T-C | A second refresh during a run produces one follow-up, not two concurrent runs. |
| T-D | A generation that finishes after a newer refresh does not write. |
| T-E | PR04 locks still hold: refresh does not ping, live vendor stays default-off, bad KQL does not clear rows. |

No UI test. No splash pixel test.

---

## 7. Acceptance

1. Cold start issues the five print sources before any of them return. Verified by T-A, not by a stopwatch.
2. Selecting a tab is not what starts its print.
3. Splash hides on completion or at 8s. The bar is not a script.
4. Refresh of a busy host does not freeze the window and does not drop the request.
5. A failed source leaves the other grids filled.
6. Watch still reports open / added / dropped / reopened. The tick does not sort on the UI thread.
7. PR04 acceptance 1–7 still hold.
8. `dotnet test` for the new locks passes without a network.

---

## 8. Out

| Item | Why |
|---|---|
| New tab or new print | No buyer. This slice is the load path. |
| Charts, dashboard, sparklines | Rejected before. Still no buyer. |
| Rewrite KQL or the query bar | Debounce exists. T4 is a ceiling, not a project. |
| Help pipeline | Rejected in PR03. |
| Live vendor on by default | PR04. Permanent for this host. |
| ICMP on refresh | PR04. |
| Changing the 8s cap upward | Hides a hang. |
| Publishing the exe | Not this slice. |
| A shared splash control for the other hosts | RouteIQ only. PingIQ and DnsIQ are not this paper. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR05 | 6 Oct 2026 | Performance slice open. Parallel prints at splash. UI thread stays off the print APIs. Splash tab walk dropped. Refresh coalesces. |
