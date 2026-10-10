# DnsIQ — PR09 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR09-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Written. Binding for this push. Depends on Watch.Dns PR03 for Status and Answers on the pipe.
**Date:** 10 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Prior:** [Completed/PR08](../Completed/PR08/PR08%20--%20Requirements.md). Monitoring tab ships. Grid replaces on name+type.
**Upstream:** `Vestigium.Helpers.Watch.Dns` PR03 (Status and Answers preserved on the rollup).

**One sentence:** Aggregate the Monitoring grid to one row per domain and open a details window that shows the per-type lines, the Sent/Received counts, and the Status/Answers the watch now keeps.

**This version is not** a whois or HTTP scrape. Not a source-IP column (the machine is the source). Not automatic Lookup. Not a change to the Watch sensors beyond what PR03 already emits. Not a second capture path.

---

## 0. Decision

| Call | Why |
|---|---|
| Aggregate on the client | Helper key stays Name+Type. The grid is noisy. The tab already holds the lines; it can sum them. |
| One row per Name | Sum ResolverCount, PacketCount, Total. Last Time. Distinct Type count. Raw Name+Type lines stay for details. |
| Details is a window | Same pattern as CaptureDetailsWindow. Double-click opens it. Watch continues. |
| Stats are the counts we have | Requests = Total. Sent = ResolverCount (Event). Received = PacketCount (Port). Label them in the caption. |
| Destination is Answers | Source IP is this machine. The useful destination is the data in Answers (QueryResults) once PR03 preserves it. Show the string. Lookup button fills a full grid when the operator wants it. |
| Lookup stays a button | Reuse the existing path. Do not fire on open. Do not touch the pulse token. |
| No source-IP column | Local-only tool. Adding it does not inform. |

Rejected: scraping. Rejected: changing the helper key. Rejected: auto-Lookup. Rejected: a source-IP field. Rejected: making details a tab.

---

## 1. What this version is

Same Monitoring tab. Same Start / Stop / Seconds. Same pipe client. Richer row once PR03 lands.

| Surface | PR08 | PR09 |
|---|---|---|
| Grid | One row per Name+Type. | One row per Name. Sums. Last Time. TypeCount. |
| Double-click | Context menu. | Opens details window for that Name. Context menu stays. |
| Details | None. | Caption: Requests / Sent / Received / Total. Grid of Name+Type lines with Status and Answers. |
| Destination | None. | Answers string (from PR03). Lookup button for a full resolution. |
| Source IP | N/A | Omitted. |

---

## 2. Requirements

### R09-01 Grid is one row per domain

Normalize Name the same way the helper does. Rows that share a Name collapse. ResolverCount, PacketCount, and Total are sums. Time is the latest. TypeCount shows distinct types. The Name+Type lines are retained in the view-model.

### R09-02 Details window shows the breakdown and the destination

Double-click opens a window titled with the Name. Caption shows Requests / Sent / Received / Total. Read-only grid lists every Name+Type line, sorted by Type then Time. Columns include Type, Time, Pid, Resolver, Packets, Total, Status, Answers. Answers is the destination data when the Event path supplied it.

### R09-03 Lookup is operator-driven

Details window has a Lookup button that calls the existing path. It does not fire on open. It does not set Monitoring busy or the pulse token. Use it when Answers is blank or the operator wants TTL and the full set.

### R09-04 Stop and Start still work

Aggregation and the details window do not change the pipe connect, the raw-line replace logic, or the clock. Closing details does not stop the watch.

### R09-05 No source IP

The grid and the details do not show a source-IP column. The machine is the source. Destination is Answers or the Lookup result.

---

## 3. Must not change

- Watch.Dns sensors, key, or pipe format beyond PR03.
- Lookup, pulse, capture, dashboard gate, settings path.
- Elevation and UAC path.
- Unseen line and Failed status handling.
- Context-menu Lookup and Lookup + Probe.

---

## 4. Done

After Watch PR03 the pipe carries Status and Answers. The grid shows one row per domain. Double-click opens details with the counts and the per-type lines including Answers. Lookup from details works and is optional. Source IP is absent.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR09 | 10 Oct 2026 | Aggregate grid. Details window with Sent/Received and Answers as destination. Depends on Watch PR03. |
