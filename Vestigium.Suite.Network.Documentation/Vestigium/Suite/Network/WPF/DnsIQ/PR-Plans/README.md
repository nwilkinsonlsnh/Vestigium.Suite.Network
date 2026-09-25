# DnsIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live — [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md)

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR01/                              LIVE slice folder
    PR01 -- Implementation Plan.md   the paper you implement
  Completed/
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/DnsIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/DnsIQ/PR-Plans/PRnn/PRnn -- Implementation Plan.md` |
| Finished plan | `WPF/DnsIQ/PR-Plans/Completed/PRnn/` |

**Do not** place `PR01 -- Implementation Plan.md` next to this README. Solution Explorer already has a `PR01` folder. The plan file lives inside it.

## Naming

When a live plan exists, create `PRnn/`, put the file **in that folder**, and change **Status** above to a link into the folder.

| File | Meaning | Disk |
|---|---|---|
| `PR01 -- Implementation Plan.md` | First slice | `PR-Plans/PR01/` |
| `PR01a -- Implementation Plan.md` | Follow-on on the same number | still `PR-Plans/PR01/` |
| `PR01b -- Implementation Plan.md` | Next follow-on | still `PR-Plans/PR01/` |
| `PR01c -- Implementation Plan.md` | Next follow-on | still `PR-Plans/PR01/` |

Follow-ons stay in the same `PRnn/` folder. They do not get their own sibling folder.

When the slice finishes, move the whole `PRnn/` folder under `Completed/`. Then idle Status. Do not leave a live `PRnn/` and a new number at the same time.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.0.md](../Requirements_v1.0.md) | The window |
| [Design_v1.0.md](../Design_v1.0.md) | Class and window map |
| Helpers.Network Requirements v1.6 | Protocol facts |
| [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md) | The slice we are building now |

If a plan and Requirements disagree on the window, Requirements win.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host (first and ten)

Lookup + Probe. One name. No campaign. No plot.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
