# RouteIQ — PR-Plans

**Host:** `Vestigium.Suite.Network.RouteIQ`
**APPID:** `RouteIQ`
**Status:** Live — [PR07 -- Implementation Plan](PR07/PR07%20--%20Implementation%20Plan.md)

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
| [PR04 -- Requirements.md](Completed/PR04/PR04%20--%20Requirements.md) | Revision 2 window. Protocol honesty. |
| [PR05 -- Requirements.md](Completed/PR05/PR05%20--%20Requirements.md) | Startup and refresh performance. |
| [PR06 -- Requirements.md](Completed/PR06/PR06%20--%20Requirements.md) | Host story and exception catalog. |
| [PR07 -- Requirements.md](PR07/PR07%20--%20Requirements.md) | Neighbor parser and port catalog leave the host. |
| [PR07 -- Implementation Plan.md](PR07/PR07%20--%20Implementation%20Plan.md) | The slice we are building now. |

If a plan and PR07 disagree on the offload, PR07 requirements win. The plan wins on order.
If a plan and PR06 disagree on the log, PR06 requirements win.
If a plan and PR05 disagree on the load path, PR05 requirements win.
If a plan and PR04 disagree on vendor lookup, ICMP, or the settings path, PR04 wins.
If a plan invents a protocol the library does not own, the plan is wrong.

PR01, PR03, PR04, PR05, and PR06 are under `Completed/`. There is no PR02. Do not reopen them.

## This host

Print routes, neighbors, connections, NetBIOS, and LMHOSTS. KQL, export, settings, watch, Help.
PR05 keeps those prints off the UI thread and starts them together at splash.
PR06 writes what the host did, and hands trapped failures to `VestigiumLog.Thrown`.
PR07 deletes the host neighbor parser and the host port catalog once Helpers.Network `1.4.6` can answer both.
No mutate form. No default-route button. No row dump in the log.
Source: `src/Vestigium.Suite.Network.RouteIQ`
