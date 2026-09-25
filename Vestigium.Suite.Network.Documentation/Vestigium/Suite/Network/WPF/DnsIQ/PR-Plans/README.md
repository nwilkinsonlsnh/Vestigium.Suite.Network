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

## Naming

When a live plan exists, create `PRnn/`, put the file **in that folder**, and change **Status** above to a link into the folder.

Follow-ons stay in the same `PRnn/` folder. They do not get their own sibling folder.

When the slice finishes, move the whole `PRnn/` folder under `Completed/`. Then idle Status.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.0.md](../Requirements_v1.0.md) | First-and-ten lock (PR01). |
| Requirements 1.1 (this slice) | The window after Probe is removed. |
| [Design_v1.0.md](../Design_v1.0.md) | Class map until 1.1 lands. |
| Helpers.Network Requirements v1.6 | Protocol facts |
| [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md) | The slice we are building now |

If a plan and Requirements 1.1 disagree on the window, Requirements 1.1 win.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host

PR01: one name, Lookup + Probe, All walk, adapter DNS.  
PR02: one Lookup, status names the resolver, no twin button.  
Source: `src/Vestigium.Suite.Network.DnsIQ`
