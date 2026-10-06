# RouteIQ — PR05 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR05
**Version:** PR05
**Status:** Live. Step 4 done. Not moved to Completed.
**Date:** 6 October 2026
**Binding:** `PR05 -- Requirements.md`. This file wins on order. Requirements win on the load path. PR04 wins on vendor lookup, ICMP, and the settings path.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

---

## 0. Done

Step 1 closed 6 October 2026. Requirements accepted. This plan is the paper you implement.
Step 2 closed 6 October 2026. `PrintCoordinator` starts the six R1 sources together, joins with `Task.WhenAll`, and arms one follow-up. Packed OUI is not a source.
Step 3 closed 6 October 2026. `Refresh` and `BeginPrints` call the coordinator. `Load()` is gone. `LiveVendorLookup` stays inside `Refresh()`. Watch projects off the UI thread.
Step 4 closed 6 October 2026. Splash calls `BeginPrints`, reports source progress, hides on the join or at 8s, and selects RouteIQ. `Warm` is gone. The cap does not cancel the run.

The folder stays live until the owner accepts the slice.

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
| 3 | R2, R3, R9, R11, T1, T3, T5 | `MainViewModel.cs`. `ConnectionWatch.cs`. | Done. Cold start is `BeginPrints()`. `Refresh` joins the same coordinator, then opt-in vendor. A busy click arms one follow-up. Packed OUI starts from the neighbor source. |
| 4 | R6, R7, R8, T2 | `App.xaml.cs`. `MainWindow.xaml.cs`. | Done. Splash starts the print. Progress is the source that finished. Hide on join or 8s. RouteIQ stays selected. `Warm` deleted. |

`RouteIqReleaseTests` was not edited. KQL files were not edited.

---

## 2. Step 2 — coordinator

Closed 6 October 2026.

`PrintCoordinator` takes six `PrintSource` delegates. `Request` starts them together. `Task.WhenAll` is only the join. A source applies itself when it finishes. A throw is reported and does not fault the join. A second `Request` during a run returns the same task and arms one follow-up. `PrintScope.IsCurrent` is false once the next generation has started. Names are the six R1 prints. No vendor source. No dispatcher.

---

## 3. Step 3 — host wiring

Closed 6 October 2026.

Constructor no longer schedules a print. `BeginPrints` is the cold start. `Refresh` awaits the same `Request`. A click while busy calls `Request` and returns. `LiveVendorLookup` remains in the `Refresh` body, after the join. Packed OUI runs inside the neighbor source, after that snapshot, and is not a splash gate. Each list is one `Reset`. A failed source reports and leaves the last list. Watch copies slots on the caller, projects on the pool, and `Reset`s on the dispatcher.

---

## 4. Step 4 — splash

Closed 6 October 2026.

`OnStartup` shows the splash, shows the window, then calls `BeginPrints`. `ReportSplash` writes the source name and finished/started percent. `FinishSplash` waits on the join or 8 seconds, whichever is first, then selects RouteIQ and hides. The delay is not cancelled, and the print is not cancelled. `MainWindow.Warm` is deleted. Nav still ignores clicks while the splash is visible. Help stays lazy.

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
| PR05 | 6 Oct 2026 | Step 3. Host calls the coordinator. Cold start waits on step 4. |
| PR05 | 6 Oct 2026 | Step 4. Splash starts the print. Walk is gone. Folder stays live. |
