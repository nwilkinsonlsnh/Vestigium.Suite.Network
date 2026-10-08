# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live — [PR07 -- Implementation Plan.md](PR07/PR07%20--%20Implementation%20Plan.md)
**Next slice:** PR07-07 owner gate
**Done:** PR07-01 through PR07-06. PR06 paper is Closed. Host tests are off the wire.
**Not this push:** KQL bar. Packages are pinned. DnsIQ does not reference them.

This file is the queue keeper. It is not the plan.

## Layout

```
PR-Plans/
  README.md
  PR07/
    PR07 -- Requirements.md
    PR07 -- Implementation Plan.md
  Completed/
    PR01/
    PR02/
    PR03/
    PR04/
    PR05/
    PR06/
```

Product papers live **outside** this folder:

- [Requirements_v1.2.md](../Requirements_v1.2.md) — live window lock. Unchanged until fold.
- [Design_v1.2.md](../Design_v1.2.md) — live class / window map. Unchanged until fold.
- Archived Requirements_v1.1.md / Design_v1.1.md — history (PR02)
- Archived Requirements_v1.0.md / Design_v1.0.md — history (PR01)

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.2.md](../Requirements_v1.2.md) | Window. Persist path. Port. Combos. Probe prelude. Dashboard gate. Chrome. |
| [Design_v1.2.md](../Design_v1.2.md) | Types and flow. |
| [PR07 -- Requirements.md](PR07/PR07%20--%20Requirements.md) | Details check owner. Fan-out caps. HAR gate. Settings ACL. Mapped PTR. |
| [PR07 -- Implementation Plan.md](PR07/PR07%20--%20Implementation%20Plan.md) | Slice order. PR07-06 done. Owner gate is not closed by this file. |
| [Completed/PR06](Completed/PR06/PR06%20--%20Requirements.md) | One capture line per answer. Details window. Reverse only there. Closed. |
| [Completed/PR05](Completed/PR05/PR05%20--%20Requirements.md) | Help reader and Exports workbook. Closed. |
| [Completed/PR04](Completed/PR04/PR04%20--%20Requirements.md) | HAR host extract, text scrape, DNS probe. Closed. |
| Helpers.Network 1.5.0 | Protocol. |
| LogParser 1.0.0 / LogParser.Har 1.0.0 / LogParser.Url 1.0.0 | Capture read and text scrape. |
| Analytics 1.0.1 / Charts 1.0.9 | Dashboard. |
| ClosedXml 1.0.1 | Workbook. |

KQL (`Vestigium.Helpers.Kql` 1.0.4, `Vestigium.Controls.QueryBar` 1.0.3) stays parked. It is not PR07.
