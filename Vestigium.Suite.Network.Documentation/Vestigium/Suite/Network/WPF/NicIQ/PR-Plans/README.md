# NicIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live — [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md)  
**Next slice:** PR01-02 (inventory)

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
| Queue keeper | `WPF/NicIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/NicIQ/PR-Plans/PR01/PR01 -- Implementation Plan.md` |
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
| [Requirements_v1.0.md](../Requirements_v1.0.md) | The job: list + detail + watch |
| [Design_v1.0.md](../Design_v1.0.md) | Job types. Not chrome. |
| DnsIQ / PingIQ Rev 1 | Chrome, pins, startup order |
| [PR01 -- Implementation Plan.md](PR01/PR01%20--%20Implementation%20Plan.md) | Slice order |
| Helpers.Network 1.2.0 | Protocol |
| Analytics 1.0.1 / Charts 1.0.5 / Themes 1.0.2 / Controls 1.0.0 | Skeleton |

If a plan and Requirements disagree on the **job**, Requirements win.  
If a plan and the owner lock disagree on **chrome**, the owner lock wins.  
If a plan invents a protocol the library does not own, the plan is wrong.

## This host (PR01)

Suite skeleton + which NIC, is it up, how fast. No `SampleCounters`. No packet send.  
Source: `src/Vestigium.Suite.Network.NicIQ`
