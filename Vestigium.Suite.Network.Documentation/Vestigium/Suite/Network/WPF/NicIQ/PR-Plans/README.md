# NicIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live — [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md)  
**Next slice:** owner verify monitoring on a box with more than one NIC.

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR02/                              LIVE slice folder
    PR02 -- Implementation Plan.md   the paper you implement
  Completed/
    PR01/                            first and ten
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/NicIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/NicIQ/PR-Plans/PR02/PR02 -- Implementation Plan.md` |
| Finished plan | `WPF/NicIQ/PR-Plans/Completed/PRnn/` |

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
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Monitoring job |
| [Requirements_v1.0.md](../Requirements_v1.0.md) | List + detail + watch |
| [Design_v1.0.md](../Design_v1.0.md) | Job types. Not chrome. |
| DnsIQ / PingIQ Rev 1 | Chrome, pins, startup order |
| [PR02 -- Implementation Plan.md](PR02/PR02%20--%20Implementation%20Plan.md) | Slice order |
| Helpers.PerfMon 0.1.1 / Helpers.PerfMon.Network 0.1.1 | Sample clock and catalog |
| Helpers.Network | Inventory + status watch |

If a plan and Requirements disagree on the **job**, Requirements win.  
If a plan and the owner lock disagree on **chrome**, the owner lock wins.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host (PR02)

Open already sampling the primary NIC. Pick list. Settings \ Monitoring counters.  
Source: `src/Vestigium.Suite.Network.NicIQ`
