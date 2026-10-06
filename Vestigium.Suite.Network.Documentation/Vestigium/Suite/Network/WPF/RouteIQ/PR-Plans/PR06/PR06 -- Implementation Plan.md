# RouteIQ — PR06 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR06
**Version:** PR06
**Status:** Live. Step 2 done. Not moved to Completed.
**Date:** 6 October 2026
**Binding:** `PR06 -- Requirements.md`. This file wins on order. Requirements win on the log. PR05 wins on the load path. PR04 wins on vendor lookup, ICMP, and the settings path.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

---

## 0. Done

Step 1 closed 6 October 2026. Requirements accepted. This plan is the paper you implement.
Step 2 closed 6 October 2026. `RouteIqCatalog` registers the 16 story rows and the `RouteIQ` / `Host` taxonomy. `RouteIqLog` owns the fixed messages. `Fail` writes `Thrown`, then the story. Cancel is rejected. The sink is the test seam.

The folder stays live until the owner accepts the slice.

---

## 0.1 Mind change

Old belief: `PrintCoordinator.OnPrintFault` is the trap, and the host only has to log there.
What arrived: `LoadIpv4`, `LoadIpv6`, `LoadNeighbors`, `LoadNetBios`, and `LoadLmHosts` already catch and `Report`. Those exceptions never reach the coordinator. `ResolveLiveVendors` catches per address and drops the exception. `ExportFolder()` can throw before the export `try`.
New position: log at the catch that already exists. Coordinator fault is the escape hatch only. Vendor failure is one line after the join, not one line per OUI.
Out: rethrowing from each source so the coordinator can log. A second story line for the same failure.

---

## 1. Order

Each step names the files it may touch. A file not in the row is out of that step.

| Step | Closes | Files | Exit |
|---|---|---|---|
| 1 | Paper | This folder. Keeper. | Done. Requirements and this plan are the live papers. |
| 2 | R1 catalog, R2, R3 door, T-A, T-C | `ViewModels/RouteIqCatalog.cs`. `ViewModels/RouteIqLog.cs`. `tests/.../RouteIqLogTests.cs`. | Done. Sixteen rows, no 10075. `Fail` is Thrown then the story. Cancel does not call the sink. A MAC or a path is rejected. |
| 3 | R3–R8, R11, T-B, T-D, T-E | `PrintCoordinator.cs`. `MainViewModel.cs`. `RouteIqExport.cs`. `RouteIqSession.cs`. `ConnectionWatch.cs`. `tests/.../RouteIqPrintCoordinatorTests.cs`. | A run emits one start. A source catch logs one failure and leaves the list. Vendor miss is one line. Settings reason is logged by the session. Watch cancel is not a failure. |
| 4 | R1 initialize, R9, R10 | `App.xaml.cs`. | Catalog registered in the existing callback. Host started / stopped. Dispatcher and domain unhandled call `Thrown`. |

`KqlBars.cs` is not edited. A compile miss already reports. It is not an exception. `NeighborGridRow.cs` is not edited. Per-row parse misses stay quiet.

---

## 2. Step 2 — catalog and door

Closed 6 October 2026.

`RouteIqCatalog.Register` registers the taxonomy, then `cfg.RegisterEvent` for the section 3 ids. Category `RouteIQ`. Subcategory `Host`. Full name `Vestigium.Suite.Network.RouteIQ.Events.{Name}`. No 10075. RouteIQ took a direct `Vestigium.Logging` reference because the shell package is not transitive. `InternalsVisibleTo` is the test seam only.

`RouteIqLog` owns the MESSAGE constants. A source name must be a coordinator name or `vendor`. Settings take a file name, not a path. Watch stop is `operator` or `cancel`. `Fail` rejects null and `OperationCanceledException`, writes `Thrown`, then the failure story. `Fail(Exception)` is the no-story door for step 4. Production sink is `VestigiumLog`. Tests set `Sink` and do not initialize the logger.

---

## 3. Step 3 — host wiring

`PrintCoordinator` gains one optional `Action<int>? started`, called once at the top of `RunOnce` with the generation. Existing join, fault, and progress behavior stay. The new argument is last so current tests still compile. One new test: a run reports one start, a follow-up reports a second start, and two concurrent runs do not happen.

`MainViewModel` passes that callback to `RouteIqLog.PrintRequested`. Do not log the request in `BeginPrints` or `Refresh`. Those return the same task when busy.

Each `Load*` catch calls `RouteIqLog.Fail` and still `Report`s. It does not rethrow. The `finally` still marks the source printed. After a successful `Replace`, one `Print source applied` with `source` and `count`. Stale generation returns before the apply line.

`OnPrintFault` calls `Fail` only for a throw that escaped the source. Sources that already logged must not escape. A throw inside the fault handler is `Thrown` once, then swallowed. Progress throw stays quiet.

`ResolveLiveVendors` keeps the per-address catch so one miss does not kill the pool. It keeps the first exception. After `WhenAll`, if that exception is set, one `Fail` with `source=vendor`. No per-OUI line.

Probe catch calls `Fail` (10030). Status text may still contain the address. The log properties are `result=hit` or `result=miss` only. No address property.

The four clipboard commands call `Fail` (10080) and still `Report`.

`RouteIqExport.WriteExport` moves `ExportFolder()` inside the `try`. Catch calls `Fail` (10040) and still `Report`. Success writes 10035 with `sheets`, not the path.

`RouteIqSettingsStore` stays a result object. `RouteIqSession` logs 10045 on a clean load and `Fail` / 10050 when `Reason` is set. The store does not reference the logger.

`ConnectionWatch` logs 10055 on start and 10060 on operator stop or cancel. The existing non-cancel catch calls `Fail` (10065) and still reports. Cancel does not call `Fail`.

---

## 4. Step 4 — process hooks

`App.OnStartup` registers `RouteIqCatalog` inside the existing `HostLog.Initialize` callback, next to KQL and ClosedXml. Then `RouteIqLog.HostStarted`. Do not call `VestigiumLogger.Initialize` again.

`DispatcherUnhandledException`: `RouteIqLog.Fail` with no story id, then `e.Handled = true` for a command or print fault. Do not mark handled an exception that already passed through a step 3 catch.

`AppDomain.CurrentDomain.UnhandledException`: `Thrown` only. Do not try to keep the process up.

`OnExit` or `MainWindow.Closed` writes `Host stopped` once. Not both.

---

## 5. Watch

| Watch | Failure |
|---|---|
| Double log | A source catch that rethrows, so coordinator fault writes a second 10020. |
| Catalog | Registering a story id below 10000, or writing 10075. |
| Rows | Destination, MAC, 5-tuple, NetBIOS name, LMHOSTS line, or query text in MESSAGE or PROPERTIES. |
| Vendor | One `Thrown` per OUI. The pool catch stays. The log is after the join. |
| Cancel | `OperationCanceledException` passed to `Fail`. |
| KQL | Editing `KqlBars.cs` to log a compile miss. |
| Library | Wrapping `NetworkHelper` to re-log a line the package already writes. |
| Seam | A fake `VestigiumLogger`. The sink on `RouteIqLog` is the seam. |
| Splash / ICMP | Any change to the 8s cap, the coordinator join, or a ping on refresh. |
| Siblings | Editing PingIQ, DnsIQ, or NicIQ in this PR. |

---

## 6. Out of this plan

Log viewer. Seal. Archive. Janitor. Custom exception rows. Per-row parse logging. Publishing. A shared log helper for the other hosts.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR06 | 6 Oct 2026 | Plan opened. Step 1 done. Log at the existing catch. One start callback. Vendor miss is one line. |
| PR06 | 6 Oct 2026 | Step 2. Catalog and door. No host wiring. |
