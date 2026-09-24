# Vestigium.Suite.Network — WPF paper index

**Checked:** 4 October 2026  
**This file:** `Vestigium/Suite/Network/WPF/README.md`  
**Not:** a second requirements file. Not the pin file.

First-and-ten papers for the WPF hosts. Solution Explorer shows this node under `WPF`, beside the host folders. The suite briefing is the sibling above it: [../README.md](../README.md).

Protocol facts live in `Vestigium.Helpers.Network` Requirements v1.6.  
If a host paper and the library paper disagree on a protocol fact, **the library wins**.  
If Requirements and Design disagree on the window, **Requirements win**.  
A live `PRnn` plan is the slice. It does not reopen those two fights.  
Which package version restores: `Directory.Build.props`. This index does not override it.

## Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| Language | C# |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Solution | `Vestigium.Suite.Network.slnx` |
| Network | `Vestigium.Helpers.Network` 1.3.6 |
| Analytics | `Vestigium.Helpers.Analytics` 1.0.1 |
| Charts | `Vestigium.Helpers.Charts` 1.0.9 |
| Logging | `Vestigium.Logging` 1.7.1 |

The 25 Sep index said Network 1.2.0 and Charts 1.0.1. That lost. Both current pins are published. Do not roll a host back to match an old line in this file.

Charts stay on the host that owns a series. Network does not plot.  
No project reference to `Vestigium.Helpers` — package only, through Shell.  
Shell does not wrap `NetworkHelper`. No port sweep. No default-route write. No HTTP client.

APPID is the host name (`PingIQ`, `DnsIQ`, `RouteIQ`, …). Never `Network`. Never `Shell`.

## Hosts

Current papers only. Older revisions sit in `Archived/` or `Completed/` and are not the lock.

| Host | Kind | First and ten | Requirements | Design | Queue |
|---|---|---|---|---|---|
| Shell | library | `HostIds`, `HostLog`, `BindFields` | [v1.0](Shell/Requirements_v1.0.md) | [v1.0](Shell/Design_v1.0.md) | Idle — [queue](Shell/PR-Plans/README.md) |
| PingIQ | exe | Echo grid. Probe N/X. Persist. Charts. | [v1.2](PingIQ/Requirements_v1.2.md) | [v1.2](PingIQ/Design_v1.2.md) | Live — [PR01](PingIQ/PR-Plans/PR01/PR01%20--%20Implementation%20Plan.md) |
| TraceIQ | exe | One target. One walk. Hop list. | [v1.0](TraceIQ/Requirements_v1.0.md) | [v1.0](TraceIQ/Design_v1.0.md) | Idle. Keeper is in the wrong folder: [PR-Plans/PR01/README.md](TraceIQ/PR-Plans/PR01/README.md). It belongs at `PR-Plans/README.md`. |
| DnsIQ | exe | One name. Lookup + Probe. | [v1.2](DnsIQ/Requirements_v1.2.md) | [v1.2](DnsIQ/Design_v1.2.md) | Keeper still says Live PR03. The plan file is in [Completed/PR03](DnsIQ/PR-Plans/Completed/PR03/PR03%20--%20Implementation%20Plan.md). PR01 and PR02 are completed. Not live on PR02. |
| NicIQ | exe | Which NIC, is it up, how fast. | [v1.2](NicIQ/Requirements_v1.2.md) | [v1.1](NicIQ/Design_v1.1.md) | Live — [PR03](NicIQ/PR-Plans/PR03/PR03%20--%20Implementation%20Plan.md) |
| RouteIQ | exe | Print routes and neighbors. One probe. No default-route write. | [v1.0](RouteIQ/Requirements_v1.0.md) | [v1.0](RouteIQ/Design_v1.0.md) | Live — [PR03](RouteIQ/PR-Plans/PR03/PR03%20--%20Implementation%20Plan.md) |
| ProbeHost | exe | One snapshot. Four lists. Read-only. | [v1.0](ProbeHost/Requirements_v1.0.md) | [v1.0](ProbeHost/Design_v1.0.md) | Idle — [queue](ProbeHost/PR-Plans/README.md) |
| ShareIQ | exe | One UNC. One run. No password. | [v1.0](ShareIQ/Requirements_v1.0.md) | [v1.0](ShareIQ/Design_v1.0.md) | Idle — [queue](ShareIQ/PR-Plans/README.md) |

Implement against the Requirements link in that row. Design is the class and window map, not a second lock table.

NicIQ v1.1 still sits beside v1.2. v1.2 wins. v1.0 is under `NicIQ/Completed/`.  
DnsIQ v1.0 and v1.1 are under `DnsIQ/Archived/`. v1.2 wins.  
PingIQ v1.0 is under `PingIQ/Archived/` (and a duplicate `Archive/`). v1.2 wins. The PingIQ queue keeper still cites v1.1. That citation is stale.

## Tree

What Solution Explorer shows under `WPF`:

```
WPF/
  DnsIQ/ NicIQ/ PingIQ/ ProbeHost/ RouteIQ/ ShareIQ/ Shell/ TraceIQ/
  README.md                 this index
```

The suite briefing and the package catalog are not in this folder. They sit above it:

```
Network/
  README.md                 suite briefing
  WPF/                      this folder
Published NugetPackages.md  sibling of Suite, under Vestigium/
```

Inside a host:

```
<Host>/
  Requirements_vN.md        the window
  Design_vN.md              class and window map
  PR-Plans/
    README.md               queue keeper — never a plan file
    PRnn/                   LIVE slice
      PRnn -- Implementation Plan.md
    Completed/PRnn/         finished plans — needs a file or Git drops the folder
```

Open the queue keeper to see idle vs live. If live, the paper is inside `PR-Plans/PRnn/`, not beside the queue README.

TraceIQ is the exception until that keeper is moved: the idle README is `TraceIQ/PR-Plans/PR01/README.md`, and there is no `TraceIQ/PR-Plans/README.md`.
