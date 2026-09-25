# ProbeHost — PR-Plans

**Host:** `Vestigium.Suite.Network.ProbeHost`  
**APPID:** `ProbeHost`  
**Status:** No current implementation plan.

This folder is kept so the GitHub tree matches Solution Explorer. Do not delete this file when the plan is idle.

## Layout

| Location | Holds |
|---|---|
| Current plans | `WPF/ProbeHost/PR-Plans` (this folder) |
| Prior plans | `WPF/ProbeHost/PR-Plans/Completed` |

Live plan stays in this folder. Finished plans move under `Completed/PRnn/` before the next number opens. Git does not keep empty directories — a finished plan needs a file under `Completed/PRnn/` or that folder disappears on GitHub.

## Naming

When a live plan exists, put it here and change **Status** above.

| File | Meaning |
|---|---|
| `PR01 -- Implementation Plan.md` | First slice |
| `PR01a -- Implementation Plan.md` | Follow-on on the same number |
| `PR01b -- Implementation Plan.md` | Next follow-on |
| `PR01c -- Implementation Plan.md` | Next follow-on |

Do not leave a finished plan and a new plan in this folder at the same time.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [Requirements_v1.0.md](../Requirements_v1.0.md) | The window |
| [Design_v1.0.md](../Design_v1.0.md) | Class and window map |
| Helpers.Network Requirements v1.6 | Protocol facts |
| A live `PRnn` plan in this folder | The slice we are building now |

If a plan and Requirements disagree on the window, Requirements win.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host (first and ten)

One snapshot. Four lists. Read-only. No packet send. No route write.  
Source: `src/Vestigium.Suite.Network.ProbeHost`
