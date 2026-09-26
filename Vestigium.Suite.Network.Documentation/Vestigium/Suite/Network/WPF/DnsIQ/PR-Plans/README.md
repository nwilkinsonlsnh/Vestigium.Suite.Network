# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live — [PR03 -- Implementation Plan.md](PR03/PR03%20--%20Implementation%20Plan.md)

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md
  PR03/                              LIVE
    PR03 -- Requirements.md          delta — what must change
    PR03 -- Implementation Plan.md   attack + test gate
  Completed/
    PR01/
    PR02/
```

| Location | Holds |
|---|---|
| Queue keeper | this file |
| Live PR folder | `PR-Plans/PRnn/` |
| PR requirements | `PRnn/PRnn -- Requirements.md` — delta only |
| PR plan | `PRnn/PRnn -- Implementation Plan.md` |
| Product requirements / design | `WPF/DnsIQ/Requirements_v*.md`, `Design_v*.md` |
| Finished PR | `PR-Plans/Completed/PRnn/` |

Product papers stay at **1.1** until PR03-01 writes **1.2**. Then 1.2 is live. 1.1 remains history.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Current product lock until PR03-01. |
| [Design_v1.1.md](../Design_v1.1.md) | Current window map until PR03-01. |
| [PR03 -- Requirements.md](PR03/PR03%20--%20Requirements.md) | Changes required to close PR03. |
| [PR03 -- Implementation Plan.md](PR03/PR03%20--%20Implementation%20Plan.md) | Slice order + test gate. |
| Helpers.Network 1.2.0 | Protocol |
| Analytics 1.0.1 / Charts 1.0.1 | Dashboard numbers and paint |

## Tests

`tests/Vestigium.Suite.Network.Tests` — `DnsIqInputTests`, `PulsePlanTests`, `HostIdsTests`.  
PR03 extends input (Port), adds settings-store and adapter/source list tests, keeps PulsePlan green, stays off the wire. PR03-08 is `dotnet test` with zero failures.

## This host

PR01: Lookup + Probe.  
PR02: Vestigium window. Pulse N over X.  
PR03: persist, Port, combos, Probe prelude, Dashboard charts, product 1.2.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
