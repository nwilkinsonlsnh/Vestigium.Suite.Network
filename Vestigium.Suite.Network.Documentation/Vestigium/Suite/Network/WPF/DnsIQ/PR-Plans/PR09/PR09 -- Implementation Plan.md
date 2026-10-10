# DnsIQ — PR09 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-PLAN
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Written. Depends on Watch.Dns PR03 for Status and Answers.
**Date:** 10 October 2026
**Binding:** [PR09 -- Requirements.md](PR09%20--%20Requirements.md) wins on this cut. PR08 wins on the pipe client. Watch PR03 wins on the payload. This file wins on order inside DnsIQ.

**Goal:** Monitoring grid aggregates to one row per domain. Double-click opens a details window with the per-type lines, the Sent/Received counts, and the Answers the watch now keeps.

**Not:** Helper changes beyond PR03. Automatic Lookup. Source IP. Whois.

**Order:** Watch PR03 first (or in parallel if the fields are already blank-safe). Then these slices.

---

## Starting point

`MonitorViewModel.Apply` replaces on Name+Type. `MonitorLine` already deserializes Status and Answers. The grid binds to `Rows`. CaptureDetailsWindow is the dialog pattern. Watch PR03 makes Status and Answers non-empty on Event-sourced keys.

---

## Decision

| Call | Why |
|---|---|
| Keep raw lines, bind aggregated | Details needs the constituents. |
| Details is a new window | Matches CaptureDetailsWindow. |
| Counts are already there | ResolverCount → Sent, PacketCount → Received. |
| Answers is the destination | Show the string. Lookup button for the full set. |
| Lookup reuses the existing command | No new NetworkHelper path. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR09-01 | Keep Name+Type lines. Bind grid to aggregated collection (one row per Name, sums, last Time, TypeCount). | |
| 2 | PR09-02 | Double-click opens details window. Caption with Requests / Sent / Received / Total. Grid of lines with Status and Answers. | |
| 3 | PR09-03 | Lookup button in details calls the existing path. Optional. Close does not affect the watch. No source-IP column. | |

---

## Slices

### PR09-01

Private collection of raw lines grouped by normalized Name. On Apply, update raw and rebuild (or increment) an ObservableCollection of aggregated rows. Aggregated row: Time (max), Name, TypeCount, ResolverCount (sum), PacketCount (sum), Total (sum). ResolvedText uses the aggregated count. Raw lines retain Status and Answers.

### PR09-02

MonitorDetailsWindow (CaptureDetailsWindow pattern). Double-click opens it with the Name. Caption shows the four numbers. DataGrid bound to the raw lines for that Name, sorted by Type. Columns: Type, Time, Pid, Resolver, Packets, Total, Status, Answers. Answers is the destination when present.

### PR09-03

Lookup button invokes the existing LookupSelected path. Do not auto-fire. Do not touch main-job IsBusy. No source-IP column anywhere. Closing the window is a no-op for the pipe.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorViewModel.cs
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorView.xaml
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml          [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml.cs       [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorDetailsViewModel.cs    [NEW]
```

Do not edit Watch.Dns. Do not add a scrape client. Do not add a source-IP field.

---

## Next action

Confirm Watch PR03 is landed or blank-safe. Then implement the aggregated collection and the details window. Verify a watch with repeated domains collapses and the details Answers match the pipe.
