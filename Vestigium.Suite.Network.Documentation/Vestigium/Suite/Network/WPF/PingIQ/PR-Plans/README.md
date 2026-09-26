# PingIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.PingIQ`  
**APPID:** `PingIQ`  
**Status:** Live — [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md)  
**Next slice:** PR01-03

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR01/                              LIVE slice folder
    PR01 -- Requirements.md
    PR01 -- Implementation Plan.md
  Completed/
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/PingIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/PingIQ/PR-Plans/PR01/PR01 -- Implementation Plan.md` |
| Finished plan | `WPF/PingIQ/PR-Plans/Completed/PRnn/` |

**Do not** place `PRnn -- Implementation Plan.md` next to this README. Solution Explorer already has a `PRnn` folder. The plan file lives inside it.

## Naming

When a live plan exists, create `PRnn/`, put the file **in that folder**, and change **Status** above to a link into the folder.

| File | Meaning | Disk |
|---|---|---|
| `PR01 -- Implementation Plan.md` | First slice | `PR-Plans/PR01/` |
| `PR01a -- Implementation Plan.md` | Follow-on on the same number | still `PR-Plans/PR01/` |

Follow-ons stay in the same `PRnn/` folder. They do not get their own sibling folder.

When the slice finishes, move the whole `PRnn/` folder under `Completed/`. Then idle Status. Do not leave a live `PRnn/` and a new number at the same time.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Window. Persist. Combos. Probe prelude. Dashboard. Chrome. |
| [Design_v1.1.md](../Design_v1.1.md) | Types and flow. |
| [PR01 -- Requirements.md](PR01/PR01%20--%20Requirements.md) | Slice delta. |
| [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md) | Slice order + test gate. |
| Helpers.Network 1.2.0 | Protocol |
| Analytics 1.0.1 / Charts 1.0.5 | Dashboard |

If a plan and Requirements disagree on the window, Requirements win.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host (Rev 1)

Echo grid. Probe N over X. Persist. Charts. No campaign. No PathMtu.  
Source: `src/Vestigium.Suite.Network.PingIQ`
