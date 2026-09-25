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
    PR02 -- Implementation Plan.md   the paper you implement
  Completed/
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/DnsIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/DnsIQ/PR-Plans/PRnn/PRnn -- Implementation Plan.md` |
| Finished plan | `WPF/DnsIQ/PR-Plans/Completed/PRnn/` |

**Do not** place `PRnn -- Implementation Plan.md` next to this README. The plan file lives inside `PRnn/`.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.0.md](../Requirements_v1.0.md) | First-and-ten lock (PR01). |
| Requirements 1.1 (this slice) | Two buttons, split surfaces. |
| [Design_v1.0.md](../Design_v1.0.md) | Class map until 1.1 lands. |
| Helpers.Network Requirements v1.6 | Protocol facts |
| [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md) | The slice we are building now |

## This host

PR01: one name, Lookup + Probe both filled the grid.  
PR02: Lookup owns the grid. Probe is reachability only. Status names the resolver.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
