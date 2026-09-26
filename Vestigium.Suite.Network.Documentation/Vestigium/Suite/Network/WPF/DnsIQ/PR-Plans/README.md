# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live — [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md)

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR01/                              first-and-ten (move under Completed when idle)
    PR01 -- Implementation Plan.md
  PR02/                              LIVE slice folder
    PR02 -- Implementation Plan.md   attack plan
    PR02a -- Chrome.md               packages, default window, tabs, UC
    PR02b -- Pulse.md                N bursts over X seconds
  Completed/
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/DnsIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/DnsIQ/PR-Plans/PRnn/PRnn -- Implementation Plan.md` |
| Follow-ons | same `PRnn/` folder |
| Finished plan | `WPF/DnsIQ/PR-Plans/Completed/PRnn/` |

**Do not** place plan files next to this README.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.0.md](../Requirements_v1.0.md) | First-and-ten lock (PR01). |
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Tabs, pulse N/X, chrome. Window paper for PR02. |
| [Design_v1.0.md](../Design_v1.0.md) | First-and-ten map. |
| [Design_v1.1.md](../Design_v1.1.md) | Class and window map for PR02. |
| Helpers.Network Requirements v1.6 | Protocol facts |
| Vestigium.Controls / Themes / Converters | Chrome |
| [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md) | The slice we are building now |

## This host

PR01: one name, Lookup + Probe both filled the grid.  
PR02: Vestigium window. Lookup owns the grid. Probe is N bursts over X seconds. Dashboard is Under Construction.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
