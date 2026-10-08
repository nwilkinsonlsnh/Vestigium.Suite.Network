# PR06 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR06
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Draft
**Date:** 7 October 2026
**Binding:** [PR06 -- Requirements.md](PR06%20--%20Requirements.md) wins on this slice. Requirements_v1.2 wins on Lookup, pulse, and the open-file probe until this PR folds. This file wins on order.

**Goal:** One capture line per answer, a details view for one host, and a PTR check for an address that does not freeze the window.

**Not:** A reverse mode on the DnsIQ tab. Not a change to the open-file probe. Not the KQL bar. Not a details sheet in the workbook.

---

## Starting point

| Piece | Today | Required |
|---|---|---|
| Capture grid | One `HarHostRow` per host. `Answers` is a comma-joined string. | One line per answer. Category. A host with no answers still has a line. |
| Address | `CaptureProbe` returns Skipped and does not send. | Same on open. Details asks PTR. |
| Details | None. Row menu is Lookup / Lookup + Probe. | Double-click opens a view on the Capture page. Title contains the host. |
| Export | Capture sheet writes the host row, Answers in one cell. | Same columns as the new grid. One row per answer. |
| Job | One in-flight job for Lookup, Probe, and the capture probe. | Details checks do not take that job. |

---

## Decision

Split the print. Keep the probe. Put the reverse on the view that says it is a reverse.

| Call | Why |
|---|---|
| A line type beside `HarHostRow` | The host row stays the probe result. The grid binds the exploded lines. Do not make the probe emit commas and split them again. |
| Details replaces the grid on the Capture page | Close brings the grid back. No shell item. |
| Arpa name built in the host, shown on the line | Helpers.Network is not edited in this slice. The PTR question name is visible. |
| Checks on a side token | Closing the view cancels them. The main Cancel is the main job. |
| Export reads the lines | One writer. The sheet cannot drift from the grid. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Split the Answers string in the export only | The grid would still be the cell the operator cannot read. |
| Reverse inside `CaptureProbe.Map` | Reopens the Skipped rule. File open would send traffic it does not send today. |
| A shell page per host | The nav walk fills with hosts. |
| Block the UI until the checks return | The owner said the reach-out must not affect the window. |

---

## Types to land

| Type | Role |
|---|---|
| `CaptureLine` | Host, Ports, Hits, Sources, DNS, Error, Category, Answer. What the grid and the sheet bind. |
| `CaptureLines` | Builds the lines from the host rows after the probe maps them. No network. |
| `CaptureDetailsView` | Title, answer lines, Check column, close. |
| `ReverseName` | IPv4 to `in-addr.arpa`. IPv6 to `ip6.arpa`. Rejects a non-address. |
| `DetailsCheck` | Parallel A/AAAA for a name, PTR for an address. Writes the Check column. Does not write the grid. |

`DnsIqWorkbook.CaptureTable` writes `CaptureLine`, not `HarHostRow`.

---

## Window map

```
Capture
  grid: Host, Ports, Hits, Sources, DNS, Error, Category, Answer
  double-click -> Details (same page)
    title: host · DNS
    lines: Category, Answer, Check, Question name
    Close
```

File → Open capture stays. The row menu stays.

---

## Behavior

1. Open file. Probe runs as it does today. Lines replace the grid.
2. Double-click builds Details for that host and starts checks. Pending is the first Check value.
3. A name fans out A and AAAA. An address builds the arpa name, shows it, asks PTR.
4. Results land on the details lines. The capture grid DNS column does not move.
5. Close cancels the details token.
6. Export Capture writes the lines. It does not wait for Check.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR06-01 | `CaptureLine` and the grid. No details. No reverse. | |
| 2 | PR06-02 | Details view. Double-click. Title. One line per answer. Check stays blank. | |
| 3 | PR06-03 | Parallel checks. Arpa name shown. Close cancels. Grid does not change. | |
| 4 | PR06-04 | Capture sheet writes the lines. | |
| 5 | PR06-05 | Host tests. Off the wire. | |
| 6 | PR06-06 | Owner gate on the clone. | |

### PR06-01

Explode after `CaptureProbe.Map`. Category from `IPAddress.TryParse`. Address hosts get one Address line. Empty answers get one blank Category line.

### PR06-02

View swaps on the Capture page. No `NetworkHelper` call in this slice.

### PR06-03

Side token. UI thread only for the property write. A check that throws sets Failed on that line and leaves the others running.

### PR06-04

One writer. Drop the joined Answers column from the sheet.

### PR06-05

Assert line counts, Category, the arpa form for the sample `2600:9000:27d1:4000:7:951d:7a80:93a1`, and that an address open-path is still Skipped. No socket.

### PR06-06

Owner on the clone:

1. Open a capture with a multi-answer host. Count the lines.
2. Double-click. Title has the host. The window still scrolls.
3. An address line shows the arpa name, then a PTR result or a rcode.
4. Export Capture. One row per answer. No comma-joined Answers cell.
5. Type the same IPv6 literal on the DnsIQ tab. It still does not reverse.

This plan does not mark the owner gate closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/CaptureLine.cs          [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/CaptureLines.cs         [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/ReverseName.cs          [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DetailsCheck.cs         [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/HarView.xaml
  grid columns, double-click
src/Vestigium.Suite.Network.DnsIQ/Views/CaptureDetailsView.xaml      [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/CaptureDetailsView.xaml.cs    [NEW]
src/Vestigium.Suite.Network.DnsIQ/DnsIqWorkbook.cs
  Capture sheet columns

tests/… line split, category, arpa name, Skipped unchanged
```

Do not edit Helpers.Network. Do not edit the DnsIQ Name box. Do not add the KQL packages.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No shell item. No second writer. Lines come from the host row, not from splitting the comma string. |
| Theodore | Empty-answer host still has a line. Close cancels. A failed check does not fail the page. Export does not wait. |
| Simon | Open-file probe still skips addresses. Lookup still does not reverse. The arpa name on the details line is the question that was sent. |

---

## Next action

Papers only. PR05-05 is still the open owner gate. PR06-01 waits until that gate is called. PR07 is the KQL bar and is not in this folder.
