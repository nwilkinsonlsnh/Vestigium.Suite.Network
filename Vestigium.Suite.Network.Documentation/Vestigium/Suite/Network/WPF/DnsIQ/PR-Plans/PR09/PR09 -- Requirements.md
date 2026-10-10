# DnsIQ — PR09 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Written. Binding for this push.
**Date:** 10 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Prior:** [Completed/PR08](../Completed/PR08/PR08%20--%20Requirements.md). Monitoring tab ships. Grid replaces on name+type. Helper rollup is the same key.
**Upstream:** `Vestigium.Helpers.Watch.Dns` (pipe rows already carry Time, Pid, Name, Type, Status, Answers, ResolverCount, PacketCount, Total).

**One sentence:** Aggregate the Monitoring grid to one row per domain and open a details window on double-click that shows the per-type breakdown and the watch-window counts.

**This version is not** a change to the Watch.Dns sensors or rollup. Not a live whois or HTTP scrape during the watch. Not a full packet decoder. Not automatic Lookup for every domain. Not a new chart. Not a second capture path.

---

## 0. Decision

| Call | Why |
|---|---|
| Aggregate on the client | The helper emits on name+type. The grid is noisy when one domain appears many times. The tab already replaces; it can also sum. |
| One row per Name | Normalize the same way the helper does. Sum ResolverCount, PacketCount, Total. Last Time. Distinct Type count. |
| Details is a window | Same pattern as CaptureDetailsWindow. Double-click a domain row opens it. Do not swap the Monitoring page. |
| Stats are the counts we already have | Requests = Total. Sent = ResolverCount (ETW QuerySent/Completed). Received = PacketCount (port-53 questions). Total = Sent + Received. Label them that way in the details. |
| Answers and Status surface | The pipe already carries them on the Event path. Show them in the details grid when present. Blank when the row came from Port only. |
| Lookup is a button, not automatic | Reuse the existing LookupSelected path. Operator chooses. No fan-out on details open. |
| No helper change this PR | Sensors stay. If we want source IP or more packet fields later, that is a Helpers PR. |

Rejected: changing the rollup key in the helper. Rejected: scraping whois or reputation on every row. Rejected: keeping the current multi-row grid and only adding a filter. Rejected: making details a third tab.

---

## 1. What this version is

Same Monitoring tab. Same Start / Stop / Seconds. Same pipe client.

| Surface | PR08 | PR09 |
|---|---|---|
| Grid | One row per Name+Type. Replaces on match. | One row per Name. Sums counts. Shows last Time and type count. |
| Double-click | Context menu Lookup. | Opens details window for that Name. Context menu stays. |
| Details | None. | Window. Summary line: Requests, Sent, Received, Total. Grid of the constituent Name+Type rows (Type, Time, Pid, Resolver, Packets, Total, Status, Answers). |
| Enrichment | Lookup from context menu. | Same Lookup button inside the details window. Optional. |
| Unseen / Failed | Status text. | Unchanged. |

---

## 2. Requirements

### R09-01 Grid is one row per domain

Normalize Name the same way the helper does. Rows that share a Name collapse. ResolverCount, PacketCount, and Total are sums. Time is the latest. A TypeCount column shows how many distinct types contributed. The existing Name+Type rows are kept in the view-model so details can show them.

### R09-02 Details window shows the breakdown

Double-click a grid row opens a window titled with the Name. A caption shows Requests / Sent / Received / Total for that domain across the watch window. A read-only grid lists every Name+Type line that rolled into it, sorted by Type then Time. Columns: Type, Time, Pid, Resolver, Packets, Total, Status, Answers. Empty Answers or Status stay blank.

### R09-03 Lookup stays operator-driven

The details window has a Lookup button that calls the same path the context menu uses. It does not fire on open. It does not set the Monitoring IsBusy or the pulse token.

### R09-04 Stop and Start still work

Aggregation and the details window do not change the pipe connect, the replace logic for the raw lines, or the clock. Closing the details window does not stop the watch.

---

## 3. Must not change

- Watch.Dns exe, sensors, rollup key, pipe format.
- Lookup, pulse, capture, dashboard gate, settings path.
- Elevation and UAC path.
- The unseen line and Failed status handling.
- Context-menu Lookup and Lookup + Probe.

---

## 4. Done

A thread reads this file, then follows the implementation plan. The grid shows one row per domain after a watch. Double-click opens the details with the counts and the per-type lines. Lookup from details works and is optional.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR09 | 10 Oct 2026 | Aggregate Monitoring grid. Details window with per-type breakdown and Sent/Received counts. |
