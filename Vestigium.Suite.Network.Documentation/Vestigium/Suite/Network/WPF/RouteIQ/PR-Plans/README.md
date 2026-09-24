# RouteIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.RouteIQ`
**APPID:** `RouteIQ`
**Status:** Live — [PR04 -- Implementation Plan](PR04/PR04%20--%20Implementation%20Plan.md)

This file is the queue keeper. It is not the plan. Do not delete it when the plan is idle.

## Layout

```
PR-Plans/
  README.md                          this file — never a plan
  PRnn/                              LIVE slice folder (create when a plan opens)
    PRnn -- Implementation Plan.md   the paper you implement
  Completed/
    PRnn/                            finished slice (needs a file or Git drops it)
```

| Location | Holds |
|---|---|
| Queue keeper | `WPF/RouteIQ/PR-Plans/README.md` (this file) |
| Live plan | `WPF/RouteIQ/PR-Plans/PRnn/PRnn -- Implementation Plan.md` |
| Finished plan | `WPF/RouteIQ/PR-Plans/Completed/PRnn/` |

**Do not** place `PRnn -- Implementation Plan.md` next to this README. Solution Explorer already has a `PRnn` folder. The plan file lives inside it.

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
| [Requirements_v1.0.md](../Requirements_v1.0.md) | Historical first-and-ten. Not the live window. |
| [Design_v1.0.md](../Design_v1.0.md) | Historical class map. Not the live window. |
| Helpers.Network | Protocol facts. Props pin wins on the package version. |
| [PR04 -- Requirements.md](PR04/PR04%20--%20Requirements.md) | Revision 2 window. |
| [PR04 -- Implementation Plan.md](PR04/PR04%20--%20Implementation%20Plan.md) | The slice we are building now. |

If a plan and PR04 disagree on the window, PR04 wins.
If a plan invents a protocol the library does not own, the plan is wrong.

PR01 and PR03 are under `Completed/`. There is no PR02. Do not reopen them. Do not treat PR03 as live.

## This host

Print routes, neighbors, connections, NetBIOS, and LMHOSTS. KQL, export, settings, watch, Help.
PR04 closes the production holes: no live vendor call and no neighbor ICMP on Refresh, settings at ProgramData\Vestigium\Settings\RouteIQ, launch allow-list, props pins, tests.
No mutate form. No default-route button.
Source: `src/Vestigium.Suite.Network.RouteIQ`
