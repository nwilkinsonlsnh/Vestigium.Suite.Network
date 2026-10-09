# DnsIQ — PR08 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR08-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Written. The helper PR01 and PR02 are closed. PR08-02 is blocked until the exe accepts the pipe name the tab passes.
**Date:** 9 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Upstream:** `Vestigium.Helpers` / `Vestigium.Documentation/Vestigium/Helpers/Watch/Dns/PR02 -- Requirements.md`

**One sentence:** A Monitoring tab that starts `Vestigium.Helpers.Watch.Dns.exe` and replaces rows as the counts change.

**This version is not** a second watcher inside DnsIQ. Not an elevation of the DnsIQ process. Not the pulse. Not a HAR. Not an `.etl` parser. Not proof that the machine made no other DNS attempt.

---

## 0. Decision

| Call | Why |
|---|---|
| The tab is the client | The exe owns Event 3008 and the port bind. This process does not. |
| Source is a choice | `Event`, `Port`, or `Both`. The picker defaults to `Both`. That matches the exe: a missing arg is `Both`, and the type itself rejects a missing source. |
| The tab passes the pipe name | The exe currently generates the name and the tab cannot see it. PR08-02 does not start until the exe accepts the name this tab passes. |
| Connect, then read, then the exe writes | A write on the pipe waits for a reader. The tab connects and starts reading before the first row. The exe waits for that client up to the clock. |
| Replace on name and type | One hundred queries are one grid row. A later line for the same name and type replaces the earlier one. The unseen line is not a query row. |
| Port is a bind | It counts questions addressed to this host. It does not count queries this host sent. The unseen line says so. The tab shows that line. |

Rejected: a library reference that watches port 53 inside this process. Rejected: elevating DnsIQ. Rejected: a hidden console. Rejected: sharing the pulse token. Rejected: stacking one row per event.

---

## 1. What this version is

Same window. New tab. The tab is a client.

| Surface | PR08 |
|---|---|
| Tab | Monitoring. Source picker. Duration picker. Start. Stop. Grid. |
| Source | `Event`, `Port`, `Both`. Default `Both`. |
| Duration | Default 5. Step 5. Max 180. Same bounds as the exe. |
| Start | `runas` on `Vestigium.Helpers.Watch.Dns.exe`. Arguments are source, duration, and the pipe name. |
| Grid | Time, pid, name, type, resolver count, port count, total. Pid blank when the exe omitted it. |
| Unseen | First line stays. It is not a query row. |
| Failure | Declined UAC, exit 1, exit 3, or exit 4 sets status on this tab. Lookup stays usable. |
| Busy | This watch does not set Lookup `IsBusy`. Pulse Cancel does not stop this watch. |

---

## 2. Requirements

### R08-01 The tab does not watch

No ETW session in this process. No socket on port 53 in this process. The only DNS work this tab does is read rows.

### R08-02 Start is `runas`, and the pipe name is an argument

The button launches the helper elevated. The user sees UAC. After accept, no console appears. If they decline, status says the watch was not started. The arguments are the source, the duration, and the pipe name this tab created. The exe must listen on that name. A generated name the tab cannot see is not this contract.

### R08-03 The client reads before the exe writes

Connect, start the read, then let the exe write. A missed connect fails in the tab. It does not sit on the native wait.

### R08-04 The grid replaces

A line with the same normalized name and type replaces the row. Counts are `resolverCount`, `packetCount`, and `total`. The unseen line stays above the grid. A line that is not a row is status, not a crash.

### R08-05 A failed start leaves Lookup alone

Pipe timeout, missing exe, or declined UAC sets status on this tab. `IsBusy` on the lookup job stays false. The pulse cap is not involved.

---

## 3. Must not change

- Lookup, pulse, capture, details, settings path.
- Dashboard unlock rule from PR07.
- KQL. Still parked.
- The sensors. Changes to 3008 or the bind go in the helper repo.

---

## 4. Done

A new thread reads this file, confirms the exe accepts the pipe name, then follows the implementation plan. If it does not, the thread stops at PR08-02 and says so. PR08-01, the tab with no process, can start now.
