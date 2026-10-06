# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live — [PR04 -- Implementation Plan.md](PR04/PR04%20--%20Implementation%20Plan.md)
**Next slice:** PR04-01

This file is the queue keeper. It is not the plan.

## Layout

```
PR-Plans/
  README.md
  PR04/
    PR04 -- Requirements.md
    PR04 -- Implementation Plan.md
  Completed/
    PR01/
    PR02/
    PR03/
```

Product papers live **outside** this folder:

- [Requirements_v1.2.md](../Requirements_v1.2.md) — live window lock
- [Design_v1.2.md](../Design_v1.2.md) — live class / window map
- Requirements_v1.1.md / Design_v1.1.md — history (PR02)
- Requirements_v1.0.md / Design_v1.0.md — history (PR01)

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.2.md](../Requirements_v1.2.md) | Window. Persist. Port. Combos. Probe prelude. Dashboard. Chrome. Unchanged by PR04 until fold. |
| [Design_v1.2.md](../Design_v1.2.md) | Types and flow. Unchanged by PR04 until fold. |
| [PR04 -- Requirements.md](PR04/PR04%20--%20Requirements.md) | HAR host extract, text scrape of `.txt` dumps, DNS probe. |
| [PR04 -- Implementation Plan.md](PR04/PR04%20--%20Implementation%20Plan.md) | Slice order + test gate. |
| Helpers.Network 1.2.0 | Protocol |
| LogParser 1.0.0 / LogParser.Har 1.0.0 | Capture read. Not packed yet. |
| Analytics 1.0.1 / Charts 1.0.5 | Dashboard |
