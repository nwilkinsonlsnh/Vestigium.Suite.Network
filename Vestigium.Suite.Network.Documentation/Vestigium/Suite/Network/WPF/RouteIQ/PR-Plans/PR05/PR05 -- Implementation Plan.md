# RouteIQ — PR05 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR05
**Version:** PR05
**Status:** Live. Step 2 done.
**Date:** 6 October 2026
**Binding:** `PR05 -- Requirements.md`. This file wins on order. Requirements win on the load path. PR04 wins on vendor lookup, ICMP, and the settings path.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

---

## 0. Done

Step 1 closed 6 October 2026. Requirements accepted. This plan is the paper you implement.
Step 2 closed 6 October 2026. `PrintCoordinator` starts the six R1 sources together, joins with `Task.WhenAll`, and arms one follow-up. Packed OUI is not a source.

Not done: steps 3–4.

---

## 0.1 Mind change

Old belief: the splash walk is what loads the tabs, so PR05 should parallelize that walk.
What arrived: `MainWindow.Warm` only sets `SelectedItem` and pumps the dispatcher. The views are already constructed in `CreateMainWindow`. Prints start in the view-model constructor, and `Load()` runs them in order on one pool thread.
New position: delete the walk. Start the R1 sources together. Splash progress is sources finished, not a script.
Out: a per-tab loader. A second cache. Keeping the walk "so the grids warm."

---

## 1. Order

Each step names the files it may touch. A file not in the row is out of that step.

| Step | Closes | Files | Exit |
|---|---|---|---|
| 1 | Paper | This folder. Keeper. | Done. Requirements and this plan are the live papers. |
| 2 | R1, R4, R5, R10, T-A–T-D | `ViewModels/PrintCoordinator.cs`. `tests/.../RouteIqPrintCoordinatorTests.cs`. | Done. A run starts every source before any returns. A throw does not cancel the others. A second request arms one follow-up. A stale generation is not current. Packed OUI is not a gated source. |
| 3 | R2, R3, R9, R11, T1, T3, T5 | `MainViewModel.cs`. `ConnectionWatch.cs`. | `Refresh` and cold start call the coordinator. `Load()` is gone. `LiveVendorLookup` stays inside `Refresh()` so the existing source scan still passes. Watch projects off the UI thread. |
| 4 | R6, R7, R8, T2 | `App.xaml.cs`. `MainWindow.xaml.cs`. | Splash starts the print, reports real progress, hides on completion or at 8s, selects RouteIQ. `Warm` is gone. |

`RouteIqReleaseTests` is not edited. Step 3 must leave its four asserts true. KQL files are not edited. T4 is a ceiling, not a step.

---

## 2. Step 2 — coordinator

Closed 6 October 2026.

`PrintCoordinator` takes six `PrintSource` delegates. `Request` starts them together. `Task.WhenAll` is only the join. A source applies itself when it finishes. A throw is reported and does not fault the join. A second `Request` during a run returns the same task and arms one follow-up. `PrintScope.IsCurrent` is false once the next generation has started. Names are the six R1 prints. No vendor source. No dispatcher.

---

## 3. Step 3 — host wiring

`MainViewModel.Refresh` becomes the request. It does not call `GetRoutes`, `GetNeighbors`, `GetNetBiosNames`, or `GetLmHosts`.

Constructor stops scheduling `Refresh` and `SnapshotConnections`. Cold start is `BeginPrints()`, called by App in step 4. A second call is a `Request()`.

Each source:

1. Read on the pool thread.
2. Sort on that same thread. `ByAddress` stays.
3. Apply with one `QuietCollection.Reset` on the dispatcher, if the generation is current.
4. Report the source name.

Neighbor apply starts packed OUI on the pool. Packed does not sit in front of the other five applies. Live vendor stays where it is: after the lists are up, only when `LiveVendorLookup` is true. That call stays textually inside `Refresh()`, after the coordinator join, so `RouteIqReleaseTests.Refresh_print_does_not_scan` still sees `LiveVendorLookup` and does not see `LookupOuiAsync`, `Ping`, or `ProbeNeighbors` in the method body. Do not move the opt-in behind a helper that hides the token from the scan.

Failure: leave the last list. `Report` names the source. Do not clear a grid to show the error.

`_busy` remains the command gate. A click while busy calls `Request()` and returns. It does not start a second `Refresh` body.

`ConnectionWatch`:

- Snapshot is the connections source. It still bumps `_batch` and replaces `_slots`. Watch does not start at splash.
- `Paint` copies the slots on the caller, projects and sorts on the pool, then `Reset`s on the dispatcher. Do not read `_slots` from the pool thread.
- Poll stays 1s. Window stays 5–180. Glyphs stay.

`PrintsReady` becomes "the six gated sources have applied or failed for this generation." App reads it. Do not invent a seventh flag for packed OUI.

---

## 4. Step 4 — splash

`App.OnStartup` shows the splash, shows the window, calls `BeginPrints()`. Delete the `foreach` over `Warm`. Delete the scripted percents.

`ReportSplash` is an `Action<string, double>` set in `CreateMainWindow`, same pattern as `ReportStatus`. Text is the source that just finished, or `Prints` when the join completes. Percent is finished / started. No 15/30/45 table.

Hide when the join completes, or after 8 seconds, whichever is first. The cap does not cancel the run. A late apply may land if it is still the current generation. Then select the RouteIQ item and `HideSplash`.

Delete `MainWindow.Warm`. `ShowSplash` and `HideSplash` stay. The nav handlers that no-op while the splash is visible stay. Do not select Neighbors, Connections, NetBIOS, LMHOSTS, Exports, or Settings during splash.

Help stays lazy. Exports and Settings are not sources.

---

## 5. Watch

| Watch | Failure |
|---|---|
| Coordinator | A channel, a hosted service, or a degree-of-parallelism knob. `Task.WhenAll` is the join. |
| Apply | Per-row `Add` on the UI thread. `Reset` already exists. |
| Vendor | `LookupOuiAsync` moving back into the gated sources. PR04. |
| ICMP | A ping inside the print. PR04. |
| Scan test | Refactoring `Refresh()` until `LiveVendorLookup` is no longer in the method body. |
| Splash | Putting the tab walk back so the grids "feel warm." |
| Cap | Raising 8s, or cancelling the run when it fires. |
| KQL | A second collection, or editing `KqlBars.cs`. |
| Controls | Opening `Vestigium.Controls` to flip a grid style. Local grids do not set `EnableRowVirtualization` false. Leave them. |
| Siblings | Editing PingIQ, DnsIQ, or NicIQ in this PR. |
| Help | A document pipeline. |

---

## 6. Out of this plan

New tab. New print. Charts. KQL rewrite. Help rewrite. Publishing. A shared splash control. A millisecond budget. An IP Helper lock on our side.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR05 | 6 Oct 2026 | Plan opened. Step 1 done. Walk deleted. Six sources, one coordinator, splash cap stays 8s. |
| PR05 | 6 Oct 2026 | Step 2. `PrintCoordinator` and `RouteIqPrintCoordinatorTests`. No host wiring. |
