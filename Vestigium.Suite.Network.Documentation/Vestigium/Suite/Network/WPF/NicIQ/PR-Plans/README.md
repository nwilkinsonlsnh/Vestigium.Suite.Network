# NicIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live — [PR03 -- Implementation Plan.md](PR03/PR03%20--%20Implementation%20Plan.md)  
**Next slice:** owner accepts the extract letters, then Helpers.PerfMon cached source (PR03c) and Charts (PR03a).

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PR03/                              LIVE slice folder
    PR03 -- Implementation Plan.md   roadmap and sequence
    PR03a -- Requirements (Vestigium.Helpers.Charts).md
    PR03b -- Requirements (Vestigium.Themes).md
    PR03c -- Requirements (Vestigium.Helpers.PerfMon).md
    PR03d -- Requirements (Vestigium.Helpers.PerfMon.Cpu and Memory).md
    PR03e -- Requirements (Vestigium.Helpers.Network).md
    PR03f -- Requirements (Vestigium.Controls).md
  Completed/
    PR01/                            first and ten
    PR02/                            monitoring
```

Follow-ons stay in `PR03/`. They do not get sibling folders.

When the extract finishes, move the whole `PR03/` folder under `Completed/`. Then idle Status.

## Papers this queue implements

| Paper | Wins on |
|---|---|
| [PR03 -- Implementation Plan.md](PR03/PR03%20--%20Implementation%20Plan.md) | What leaves the exe, and in what order |
| [Published NugetPackages.md](../../../../Published%20NugetPackages.md) | Domain. Same job = same package |
| [Requirements_v1.1.md](../Requirements_v1.1.md) | Monitoring job still owned by the window |
| [Requirements_v1.0.md](../Requirements_v1.0.md) | List + detail + watch |
| Helpers.Charts / Themes / PerfMon / Network / Controls | Library contracts |

If a plan and Requirements disagree on the **job**, Requirements win.  
If a plan invents a package the catalog already named, the plan is wrong.

## This host (PR03)

Rev1 is complete as a window. PR03 moves host-local library work out.  
Source: `src/Vestigium.Suite.Network.NicIQ`
