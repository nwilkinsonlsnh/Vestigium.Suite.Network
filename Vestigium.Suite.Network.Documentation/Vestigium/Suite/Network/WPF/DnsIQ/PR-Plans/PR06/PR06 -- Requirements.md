# PR06 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR06-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Draft
**Date:** 7 October 2026
**Product papers (unchanged until this PR lands):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Prior slice:** [PR05 -- Requirements.md](../PR05/PR05%20--%20Requirements.md) is still open through the owner gate. This file does not close it.

This file is the **definition of done for PR06**. It is not a new product specification. When PR06 is accepted, fold the capture-print delta into the next requirements paper, then move this folder to `Completed/PR06/`.

Baseline is the shipped Capture page: one row per host, Answers as a comma-joined cell, an address marked Skipped and not sent, row menu Lookup / Lookup + Probe.

**One sentence:** Show each capture answer on its own line, and open a details view for one host that can resolve a name or reverse an address without freezing the grid.

**This version is not** a reverse mode on the DnsIQ tab, a KQL bar, a paste box, a HAR analyzer, or a change to Lookup, pulse, or the open-file probe.

---

## Why this slice

The export the operator pulled had addresses in the Answers cell, several per host, joined by commas. Putting one of those addresses in the Name box is a forward lookup of a literal. The resolver has no A record for it. The grid clears. Capture already refuses to send an address. Those rows have nowhere to go.

The print is the problem. One cell hiding six answers cannot be read, cannot be filtered, and cannot be the sheet another desk uses. The details view is where a single host is taken apart, and where an address is allowed to become a PTR question because the page says that is the question.

---

## Must change

### R06-01 Capture grid is one line per answer

The Capture grid no longer shows one row per host with Answers packed into one cell.

| Column | Rule |
|---|---|
| Host | The name or address from the file. Repeated on every line of that host. |
| Ports | Unchanged. Repeated. |
| Hits | Unchanged. Repeated. |
| Sources | Unchanged. Repeated. |
| DNS | The host probe result: Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Repeated. |
| Error | The host error. Repeated. |
| Category | Closed list below. |
| Answer | One answer. Not a joined list. |

Category:

| Value | When |
|---|---|
| IPv4 | The answer parses as an IPv4 address. |
| IPv6 | The answer parses as an IPv6 address. |
| Address | The host itself is an address. Answer is that host. DNS is Skipped. |
| (blank) | The host has no answers. One line is still shown, so NxDomain, TimedOut, Refused, and Failed do not disappear. |

A host with four answers is four lines. A host with none is one line. Opening a new file replaces the lines. It does not append.

The open-file probe does not change. It still asks A and AAAA for a name, and it still skips an address. This slice splits the print. It does not start a reverse lookup on open.

The row menu stays Lookup, or Lookup + Probe, for that host. Double-click is not that menu.

### R06-02 Details view

Double-click a capture line opens Details for that host. Details is a view on the Capture page, not a shell nav item, not a dialog.

- Title contains the host. The DNS status is in the title line.
- Close returns to the grid. The selected shell item does not change.
- The answer list is one line per answer, with Category, Answer, and a Check column.
- A host with no answers shows one line and the DNS status. It does not show an empty page with no explanation.
- Lookup and Lookup + Probe remain available for that host.

### R06-03 Checks run beside the UI

Opening Details starts the checks. The grid and the rest of the window stay usable. The checks do not take the single Lookup / Probe / capture-probe job. Cancel on the main page does not have to cancel them. Closing Details cancels checks that have not finished.

| Host | Check |
|---|---|
| A name | A and AAAA against the resolver already on the DnsIQ tab. This confirms the host still resolves. It does not rewrite the capture grid. |
| An IPv4 address | PTR. The question name is the `in-addr.arpa` form, shown. |
| An IPv6 address | PTR. The question name is the `ip6.arpa` nibble form, shown. |

Check column values: Pending, Name, NxDomain, TimedOut, Refused, Failed. A reverse that returns a name shows that name. A reverse that does not return a name shows the rcode, not a guessed host.

Resolved is not Reachable. A check that returns an address or a name does not mean the site loaded.

The checks do not write back to the capture grid. Export does not wait for them.

### R06-04 Capture export is one line per answer

The Capture sheet uses the same line the grid shows. One answer, one row. Columns: Host, Ports, Hits, Sources, DNS, Error, Category, Answer.

Cover is unchanged. Lookup and Probe sheets are unchanged. A capture with no lines still does not write a Capture sheet. The joined Answers cell is gone.

The details Check column is not a sheet in this slice. The workbook is the capture print, not the reverse run.

---

## Must not change in PR06

- Lookup, the pulse, and the DnsIQ Name box. An address typed there is still a forward question.
- The open-file probe. An address is still Skipped and is not sent.
- Dashboard chart kinds and the Probe gate
- Help chrome and the Exports tab shape
- Settings folder
- Helpers.Network, LogParser, ClosedXml
- A KQL bar. That is PR07.

---

## Acceptance

1. A host with three answers is three capture lines. A host with none is one line, Category blank.
2. An address host is one line, Category Address, DNS Skipped. Opening the file did not send a PTR.
3. Double-click opens Details. The title contains the host. Each answer is its own line with a category.
4. Details checks run while the operator can still change page, type a name, or open another capture line. Closing Details stops checks still in flight.
5. A name check asks A and AAAA. An address check asks PTR of the arpa name, and that name is visible.
6. Export Capture matches the grid: one row per answer, no joined Answers cell.
7. Lookup of an IPv6 literal on the DnsIQ tab still does not become a reverse lookup.

---

## Parked

- PR07: a KQL bar on `Vestigium.Helpers.Kql` and `Vestigium.Controls.QueryBar`. Not this folder.
- Writing the details Check column into the workbook
- A reverse page on the shell, separate from Capture
- Pulsing a reverse
- Pasting a comma-separated cell into Details
- Help topic rewrite for this print. Fold the sentences when the view is in.

---

## Decision

| Call | Why |
|---|---|
| Split the capture print | The joined cell is what the operator could not read and could not take to another desk. |
| Reverse only on Details | Lookup must not rewrite the question. The details title says which host, and the arpa name is shown. |
| Do not reverse on file open | PR04 locked Skipped. This slice does not reopen that probe. |
| Checks do not take the main job | A details fill that blocks Lookup is the freeze the owner refused. |
| Export the grid, not the check | The sheet is the print already on screen. The reverse may still be Pending. |
| PR07 stays parked | The query bar has its own packages and its own buyer. |

### Rejected

| Idea | Why out |
|---|---|
| Auto-PTR when Name parses as an address | The Type box would lie. Already refused. |
| One details dialog | A long answer list in a dialog is the joined cell again. |
| Details as a shell item | It is one host from the capture. It does not belong in the nav walk. |
| KQL on this page in this PR | Named as PR07 so it is not smuggled in. |
