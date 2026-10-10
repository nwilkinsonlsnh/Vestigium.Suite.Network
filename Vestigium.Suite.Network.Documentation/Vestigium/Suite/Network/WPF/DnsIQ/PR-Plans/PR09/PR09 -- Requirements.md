# DnsIQ — PR09 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Written. Binding for this push. Depends on Watch.Dns PR03 for Status and Answers on every pipe line.
**Date:** 10 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Prior:** [Completed/PR08](../Completed/PR08/PR08%20--%20Requirements.md). Monitoring tab ships. Grid replaces on name+type.
**Upstream:** `Vestigium.Helpers.Watch.Dns` PR03 (Status and Answers on every emitted line).

**One sentence:** Aggregate the Monitoring grid to one row per domain and open a details window that shows every response line from the watch window, not just the last.

**This version is not** a whois or HTTP scrape. Not a source-IP column. Not automatic Lookup. Not a history list inside the helper. Not a second capture path.

---

## 0. Decision

| Call | Why |
|---|---|
| Keep every line | The helper emits on change. Status and Answers ride on each line after PR03. The client must retain them so details can show the sequence of wins and losses, not only the final answer. |
| Aggregate the grid | One row per Name. Sum the counts. Last Time. TypeCount. The raw lines stay. |
| Details shows the sequence | Caption with Requests / Sent / Received / Total. Grid of every Name+Type line for that Name, with its Status and Answers. Sort by Type then Time. |
| Counts still sum | ResolverCount = Sent, PacketCount = Received, Total = both. The history of answers is separate from the counters. |
| Lookup stays a button | Reuse the existing path when Answers is blank or the operator wants the full set with TTL. |
| No source IP | The machine is the source. Destination is the Answers on each line or the Lookup result. |

Rejected: last-only in the details. Rejected: accumulating history in the helper. Rejected: scraping. Rejected: auto-Lookup. Rejected: a source-IP field.

---

## 1. What this version is

Same Monitoring tab. Same Start / Stop / Seconds. Same pipe client. The client now keeps the sequence.

| Surface | PR08 | PR09 |
|---|---|---|
| Grid | One row per Name+Type (replace). | One row per Name (sums). |
| Lines kept | Replaced away. | Every Name+Type line retained. |
| Double-click | Context menu. | Opens details window. |
| Details | None. | Caption + grid of every line for that Name (Type, Time, Status, Answers, counts). |
| Destination | None. | Answers on each line. Lookup button for more. |

---

## 2. Requirements

### R09-01 Grid is one row per domain

Normalize Name the same way the helper does. Rows that share a Name collapse for the grid. ResolverCount, PacketCount, and Total are sums. Time is the latest. TypeCount shows distinct types. Every raw Name+Type line is retained.

### R09-02 Details shows every response, not the last

Double-click opens a window titled with the Name. Caption shows Requests / Sent / Received / Total across the window. Read-only grid lists every retained line for that Name, sorted by Type then Time. Columns include Type, Time, Pid, Resolver, Packets, Total, Status, Answers. The operator sees the sequence of answers and statuses, not only the final one.

### R09-03 Lookup is operator-driven

Details window has a Lookup button that calls the existing path. It does not fire on open. Use it when the stream has no Answers or the operator wants TTL and the complete set.

### R09-04 Stop and Start still work

Keeping the lines and opening details does not change the pipe connect or the clock. Closing details does not stop the watch.

### R09-05 No source IP

No source-IP column. The machine is the source. Destination is the Answers on the lines or the Lookup result.

---

## 3. Must not change

- Watch.Dns sensors, key, or pipe format beyond PR03.
- Lookup, pulse, capture, dashboard gate, settings path.
- Elevation and UAC path.
- Unseen line and Failed status handling.
- Context-menu Lookup and Lookup + Probe.

---

## 4. Done

After Watch PR03 the pipe lines carry Status and Answers. The grid shows one row per domain. Details lists every line for that domain so the operator can see the full set of responses. Counts match the sums. Lookup from details is optional. Source IP is absent.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR09 | 10 Oct 2026 | Aggregate grid. Details keeps and shows every response line. Depends on Watch PR03 emit. |
