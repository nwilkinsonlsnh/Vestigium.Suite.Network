# DnsIQ — PR08 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR08-PLAN
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Written. Blocked on `Vestigium.Helpers.DnsWatch` PR01.
**Date:** 8 October 2026
**Binding:** [PR08 -- Requirements.md](PR08%20--%20Requirements.md) wins on this cut. DnsWatch PR01 wins on the sensor. This file wins on order inside DnsIQ.

**Goal:** Monitoring tab starts DnsWatch and renders the pipe.

**Not:** The sensor. The elevation manifest. A parser for `.etl`. A change to Lookup.

**Order:** Do not open PR08-01 until DnsWatch PR01-06 is closed. PR07-07, the owner gate, is a different push and is not closed by this file.

---

## Starting point

DnsWatch does not exist yet. This tab has no exe to start. Papers only.

---

## Decision

| Call | Why |
|---|---|
| Client only | The tab is a process start and a pipe read. |
| Own token | Not `_cts` from the pulse. |
| Status on this tab | A miss does not write the Lookup status bar. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR08-01 | Monitoring tab. Duration 5/5/180. Start and Stop. No process yet. | Blocked |
| 2 | PR08-02 | Launch DnsWatch with `runas`. Declined UAC is a status, not a throw. | Blocked |
| 3 | PR08-03 | Read the pipe. Append rows. Keep the unseen line. | Blocked |
| 4 | PR08-04 | Stop closes the client. Lookup `IsBusy` stays false. | Blocked |

---

## Slices

### PR08-01

Add the Monitoring tab. Picker is 5, 10, … 180. Default 5. Start and Stop commands. Grid bound to a row collection. This slice does not launch a process. It proves the tab does not take the Lookup busy flag.

### PR08-02

Start builds the pipe name, then `ProcessStartInfo` with `UseShellExecute` and verb `runas`. Arguments are duration, mode, pipe name. If the user declines, catch and set the tab status. Do not set `MainViewModel.IsBusy`.

### PR08-03

Connect to the pipe. Read UTF-8 lines. Map each line onto the grid row. The first line stays. A line that is not a row is status, not a crash.

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

None in this repo. Next build turn is DnsWatch PR01-01 in `Vestigium.Helpers`.
