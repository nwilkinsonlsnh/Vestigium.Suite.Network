# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live — [PR05 -- Implementation Plan.md](PR05/PR05%20--%20Implementation%20Plan.md)
**Next slice:** PR05-02

This file is the queue keeper. It is not the plan.

## Layout

```
PR-Plans/
  README.md
  PR05/
    PR05 -- Requirements.md
    PR05 -- Implementation Plan.md
  Completed/
    PR01/
    PR02/
    PR03/
    PR04/
```

Product papers live **outside** this folder:

- [Requirements_v1.2.md](../Requirements_v1.2.md) — live window lock. Unchanged by PR05 until fold.
- [Design_v1.2.md](../Design_v1.2.md) — live class / window map. Unchanged by PR05 until fold.
- Requirements_v1.1.md / Design_v1.1.md — history (PR02)
- Requirements_v1.0.md / Design_v1.0.md — history (PR01)

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.2.md](../Requirements_v1.2.md) | Window. Persist. Port. Combos. Probe prelude. Dashboard. Chrome. Unchanged by PR05 until fold. |
| [Design_v1.2.md](../Design_v1.2.md) | Types and flow. Unchanged by PR05 until fold. |
| [PR05 -- Requirements.md](PR05/PR05%20--%20Requirements.md) | Help reader and Exports workbook. |
| [PR05 -- Implementation Plan.md](PR05/PR05%20--%20Implementation%20Plan.md) | Slice order + test gate. |
| [Completed/PR04](Completed/PR04/PR04%20--%20Requirements.md) | HAR host extract, text scrape, DNS probe. Closed. |
| Helpers.Network 1.5.0 | Protocol. |
| LogParser 1.0.0 / LogParser.Har 1.0.0 / LogParser.Url 1.0.0 | Capture read and text scrape. |
| Analytics 1.0.1 / Charts 1.0.9 | Dashboard. |
| ClosedXml 1.0.1 | Workbook. Pinned. DnsIQ does not reference it until PR05-02. |
