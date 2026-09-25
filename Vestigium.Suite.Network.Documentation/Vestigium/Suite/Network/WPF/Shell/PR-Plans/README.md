# Shell — PR-Plans

**Host:** `Vestigium.Suite.Network.Shell`  
**Kind:** class library (not an exe)  
**APPID:** none — hosts pass `HostIds.*`. Shell does not log as `Network` or as `Shell`.  
**Status:** No current implementation plan.

This folder is kept so the GitHub tree matches Solution Explorer. Do not delete this file when the plan is idle.

## Layout

| Location | Holds |
|---|---|
| Current plans | `WPF/Shell/PR-Plans` (this folder) |
| Prior plans | `WPF/Shell/PR-Plans/Completed` |

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
| [Requirements_v1.0.md](../Requirements_v1.0.md) | Shared chrome |
| [Design_v1.0.md](../Design_v1.0.md) | Class map |
| Helpers.Network Requirements v1.6 | Protocol facts |
| A live `PRnn` plan in this folder | The slice we are building now |

If a plan and Requirements disagree on the chrome, Requirements win.  
If a plan wraps `NetworkHelper` behind a second façade, the plan is wrong.

## This host (first and ten)

`HostIds`, `HostLog`, `BindFields`. No second Network API. No theme pack.  
Source: `src/Vestigium.Suite.Network.Shell`
