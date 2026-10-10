# DnsIQ — PR09 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-PLAN
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Written. Depends on Watch.Dns PR03 for Status and Answers on every line.
**Date:** 10 October 2026
**Binding:** [PR09 -- Requirements.md](PR09%20--%20Requirements.md) wins on this cut. PR08 wins on the pipe client. Watch PR03 wins on the payload. This file wins on order inside DnsIQ.

**Goal:** Monitoring grid aggregates to one row per domain. Details window shows every response line from the watch window, not only the last.

**Not:** Helper changes beyond PR03. Automatic Lookup. Source IP. History accumulation inside the helper.

**Order:** Watch PR03 first (or in parallel if blank-safe). Then these slices.

---

## Starting point

`MonitorViewModel.Apply` replaces on Name+Type. `MonitorLine` already deserializes Status and Answers. After PR03 those fields are populated on Event-sourced lines. The grid binds to `Rows`. CaptureDetailsWindow is the dialog pattern.

---

## Decision

| Call | Why |
|---|---|
| Keep every line | Details must show the sequence of Status and Answers. Replace-only loses the wins and losses. |
| Bind aggregated | Grid stays one row per Name. Counts sum. |
| Details lists the lines | Sorted by Type then Time. Answers on each row is the destination for that response. |
| Lookup reuses the existing command | No new NetworkHelper path. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR09-01 | Keep every Name+Type line. Bind grid to aggregated collection (one row per Name, sums, last Time, TypeCount). | |
| 2 | PR09-02 | Double-click opens details window. Caption with the four numbers. Grid of every retained line for that Name. | |
| 3 | PR09-03 | Lookup button calls the existing path. Optional. Close does not affect the watch. No source-IP column. | |

---

## Slices

### PR09-01

Private collection that appends (or updates in place while retaining prior lines) every raw MonitorLine. On Apply, update the raw collection and rebuild an ObservableCollection of aggregated rows. Aggregated row: Time (max), Name, TypeCount, ResolverCount (sum), PacketCount (sum), Total (sum). ResolvedText uses the aggregated count. Raw lines retain their own Status and Answers.

### PR09-02

MonitorDetailsWindow. Double-click opens it with the Name. Caption shows Requests / Sent / Received / Total. DataGrid bound to every raw line for that Name, sorted by Type then Time. Columns: Type, Time, Pid, Resolver, Packets, Total, Status, Answers.

### PR09-03

Lookup button invokes the existing LookupSelected path. Do not auto-fire. Do not touch main-job IsBusy. No source-IP column. Closing the window is a no-op for the pipe.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorViewModel.cs
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorView.xaml
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml          [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml.cs       [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorDetailsViewModel.cs    [NEW]
```

Do not edit Watch.Dns. Do not add a scrape client. Do not add a source-IP field. Do not accumulate history in the helper.

---

## Next action

Confirm Watch PR03 emits Status and Answers on each line. Then implement the retained lines and the details window. Verify a watch with repeated domains collapses in the grid and lists every response in details.
