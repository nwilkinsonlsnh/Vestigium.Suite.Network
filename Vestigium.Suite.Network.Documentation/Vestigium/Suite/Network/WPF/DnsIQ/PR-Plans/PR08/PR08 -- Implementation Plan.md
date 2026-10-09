# DnsIQ — PR08 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR08-PLAN
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live. PR08-02 done. The read is next.
**Date:** 9 October 2026
**Binding:** [PR08 -- Requirements.md](PR08%20--%20Requirements.md) wins on this cut. Watch.Dns PR02 wins on the sensor and the row. This file wins on order inside DnsIQ.

**Goal:** Monitoring tab starts `Vestigium.Helpers.Watch.Dns.exe` and replaces rows as counts change.

**Not:** The sensor. The elevation manifest. A parser for `.etl`. A change to Lookup.

**Order:** PR07-07, the owner gate, is a different push and is not closed by this file. PR08-02 does not launch until the helper accepts `source`, `seconds`, and the pipe name.

---

## Starting point

The exe exists. It rolls Event and Port into one row. It generates its own pipe name. The tab cannot connect to a name it never heard.

---

## Decision

| Call | Why |
|---|---|
| Client only | The tab is a process start and a pipe read. |
| Replace, do not append | The exe emits on change. Stacking those lines is the hundred-row bug. |
| Read first | A write waits for a reader. The tab starts the read before the exe writes. |
| Own token | Not `_cts` from the pulse. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR08-01 | Monitoring tab. Source `Event` / `Port` / `Both`. Duration 5/5/180. Start and Stop. No process yet. | Done. No process. Lookup idle. |
| 2 | PR08-02 | Launch the exe with `runas`, the source, the duration, and the pipe name. Declined UAC is a status. | Done. Missing exe is a status. |
| 3 | PR08-03 | Connect and start the read before the first write. Replace on name and type. Keep the unseen line. | Blocked |
| 4 | PR08-04 | Stop closes the client. Lookup `IsBusy` stays false. | Blocked |

---

## Slices

### PR08-01

Add the Monitoring tab. Source picker defaults to `Both`. Duration picker is 5, 10, … 180. Default 5. Start and Stop commands. Grid columns are time, pid, name, type, resolver count, port count, total. This slice does not launch a process. It proves the tab does not take the Lookup busy flag.

### PR08-02

Start builds the pipe name, then `ProcessStartInfo` with `UseShellExecute` and verb `runas`. Arguments are source, duration, and pipe name. If the user declines, catch and set the tab status. Do not set `MainViewModel.IsBusy`. If the exe does not accept the pipe name, stop and say so. Do not invent a second channel.

### PR08-03

Connect to the pipe and start the read before the exe's first write. A missed connect fails in this tab. Map each JSON line onto the grid. Same name and type replaces. The unseen line stays. A line that is not a row is status, not a crash.

### PR08-04

Stop cancels the read and closes the client. The helper is allowed to exit on disconnect. Pulse Cancel does not call this Stop.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MonitorViewModel.cs     [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/MonitorView.xaml             [NEW]
src/Vestigium.Suite.Network.DnsIQ/DnsIqWindow.xaml                   tab
```

Do not add an ETW package to DnsIQ. Do not add a port-53 socket to DnsIQ.

---

## Next action

PR08-03. Connect and start the read before the first write. Replace on name and type.
