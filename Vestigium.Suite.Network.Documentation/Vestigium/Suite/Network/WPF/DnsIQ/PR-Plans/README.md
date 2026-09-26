# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live — [PR03 -- Requirements.md](PR03/PR03%20--%20Requirements.md)

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR03/                              LIVE slice folder
    PR03 -- Requirements.md          what must change for PR03 to close
  Completed/
    PR01/
    PR02/
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/DnsIQ/PR-Plans/README.md` (this file) |
| Live PR folder | `WPF/DnsIQ/PR-Plans/PRnn/` |
| PR requirements | `PRnn/PRnn -- Requirements.md` — delta only |
| PR plan / design | same folder, when written |
| Product requirements / design | `WPF/DnsIQ/Requirements_v*.md`, `Design_v*.md` — not inside PR-Plans |
| Finished PR | `WPF/DnsIQ/PR-Plans/Completed/PRnn/` |

**Do not** place plan files next to this README.  
**Do not** put a new product Requirements_vN in the PR folder.  
**Do not** replace product Requirements_vN until the PR is accepted.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Current product lock (PR02 accepted). |
| [Design_v1.1.md](../Design_v1.1.md) | Current window / class map. |
| Helpers.Network Requirements v1.6 | Protocol facts |
| Vestigium.Controls / Themes / Converters | Chrome |
| Vestigium.Helpers.Analytics / Charts | Dashboard numbers and paint |
| [PR03 -- Requirements.md](PR03/PR03%20--%20Requirements.md) | Changes required to close PR03 |

## This host

PR01: one name, Lookup + Probe.  
PR02: Vestigium window. Lookup owns the grid. Probe is N lookups over X seconds.  
PR03: remember knobs, Port, Interface combo, Dashboard Lookup / Probe charts.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
