# DnsIQ — PR09 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-PLAN
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Written.
**Date:** 10 October 2026
**Binding:** [PR09 -- Requirements.md](PR09%20--%20Requirements.md) wins on this cut. PR08 wins on the pipe client. This file wins on order inside DnsIQ.

**Goal:** Monitoring grid aggregates to one row per domain. Double-click opens a details window with the per-type lines and the Sent / Received / Total counts from the watch.

**Not:** Helper changes. Automatic Lookup. Whois. Packet fields beyond what the pipe already sends.

**Order:** PR08 is closed. This starts on the current Monitoring tab.

---

## Starting point

`MonitorViewModel.Apply` replaces on Name+Type. `MonitorRow` holds Time, Pid, Name, Type, ResolverCount, PacketCount, Total. The pipe line also has Status and Answers. The grid binds to `Rows`. CaptureDetailsWindow is the pattern for a details dialog.

---

## Decision

| Call | Why |
|---|---|
| Keep raw lines, bind aggregated | The details pane needs the constituents. A second collection is cheaper than re-parsing. |
| Details is a new window | Matches CaptureDetailsWindow. Does not fight the tab. |
| Counts are already there | Map ResolverCount → Sent, PacketCount → Received. No new sensor. |
| Lookup reuses the existing command | No new NetworkHelper path. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR09-01 | Keep the Name+Type lines. Bind the grid to an aggregated collection (one row per Name, sums, last Time, TypeCount). | |
| 2 | PR09-02 | Double-click opens a details window. Summary caption with Requests / Sent / Received / Total. Grid of the lines for that Name. | |
| 3 | PR09-03 | Lookup button in the details window calls the existing path. Optional. Close does not affect the watch. | |

---

## Slices

### PR09-01

Add a private collection of the raw MonitorLines (or MonitorRows) keyed or grouped by normalized Name. On each Apply, update the raw collection and rebuild (or incrementally update) an ObservableCollection of aggregated rows that the grid binds to. Aggregated row exposes Time (max), Name, TypeCount, ResolverCount (sum), PacketCount (sum), Total (sum). Keep the existing replace logic for the raw lines so counts stay correct. ResolvedText uses the aggregated count.

### PR09-02

Add a MonitorDetailsWindow (or reuse the Capture pattern). Double-click on the Monitoring grid opens it with the Name. Window shows a caption with the four numbers and a DataGrid bound to the raw lines for that Name, sorted by Type. Columns include the existing ones plus Status and Answers when present. Window is modeless or modal; either is fine as long as the watch continues.

### PR09-03

Put a Lookup button on the details window that invokes the same LookupSelected path already wired on the view-model. Do not auto-fire. Do not touch IsBusy on the main job. Closing the window is a no-op for the pipe.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorViewModel.cs     aggregate + details command
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorView.xaml             double-click, aggregated columns
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml    [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorDetailsWindow.xaml.cs [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorDetailsViewModel.cs [NEW] (or inline)
```

Do not touch Watch.Dns. Do not add a scrape client.

---

## Next action

Write the aggregated collection and the details window. Confirm a watch with repeated domains collapses correctly and the details numbers match the sums.
