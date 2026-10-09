# DnsIQ — PR08 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR08-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Written. Not startable until `Vestigium.Helpers.DnsWatch` PR01 is closed.
**Date:** 8 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Upstream:** `Vestigium.Helpers` / `Vestigium.Documentation/Vestigium/Helpers/DnsWatch/PR01 -- Requirements.md`

**One sentence:** A Monitoring tab that starts the elevated DnsWatch exe and shows the rows it sends back.

**This version is not** a second watcher inside DnsIQ. Not an elevation of the DnsIQ process. Not the pulse. Not a HAR. Not proof that the machine made no other DNS attempt.

---

## 0. Decision

| Call | Why |
|---|---|
| Helper first | DnsWatch PR01 owns the sensor. This paper is the caller. A thread that builds the tab before that exe exists has nothing to start. |
| Tab starts an exe | `runas` so UAC shows once. DnsIQ stays unelevated. Lookup and the pulse do not flip to require admin. |
| No CLI | DnsWatch is a windowless exe. This tab is the clock and the grid. |
| Pipe in, no capture file | Rows arrive on the pipe DnsWatch opens. This tab does not parse an `.etl` in PR08. |
| Unseen stays on screen | The first row from the exe is shown. The tab does not delete it. |

Rejected: a library reference that watches port 53 inside this process. Rejected: elevating DnsIQ. Rejected: a hidden console. Rejected: sharing the pulse token.

---

## 1. What this version is

Same window. New tab. The tab is a client.

| Surface | PR08 |
|---|---|
| Tab | Monitoring. Duration picker. Start. Stop. Grid. |
| Duration | Default 5. Step 5. Max 180. Same bounds as DnsWatch. The tab does not invent a fourth bound. |
| Start | Launches `Vestigium.Helpers.DnsWatch.exe` with `runas`, the duration, the mode, and the pipe name. |
| Grid | Time, process, pid, name, type, status, answers, mode. |
| Mode | Resolver by default. Packet is a choice, labeled. |
| Failure | If the exe does not start, or the pipe does not open, status says why. The rest of the window stays usable. |
| Busy | This watch does not set the Lookup `IsBusy` flag. Cancel on the pulse does not cancel this watch. |

---

## 2. Requirements

### R08-01 The tab does not watch

No ETW session in this process. No socket on port 53 in this process. The only DNS work this tab does is read rows.

### R08-02 Start is `runas`

The button launches the helper elevated. The user sees UAC. After accept, no console appears. If they decline, status says the watch was not started.

### R08-03 The clock is the helper clock

The picker offers 5 through 180 on a step of 5. The value sent is the value DnsWatch accepts. A reject from the exe is shown. It is not retried with a different number.

### R08-04 The grid is the pipe

Rows append as lines arrive. The first line is the unseen notice. Stop closes the client side. The helper exits on disconnect or on its own clock.

### R08-05 A failed start leaves Lookup alone

Pipe timeout, missing exe, or declined UAC sets status on this tab. `IsBusy` on the lookup job stays false. The pulse cap is not involved.

---

## 3. Must not change

- Lookup, pulse, capture, details, settings path.
- Dashboard unlock rule from PR07.
- KQL. Still parked.
- DnsWatch sensors. Changes to 3008 or port 53 go in the helper repo.

---

## 4. Done

A new thread reads this file, confirms DnsWatch PR01 is closed, then follows the implementation plan. If PR01 is open, the thread stops and says so.
