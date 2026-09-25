# Vestigium.Suite.Network — documentation root

First-and-ten papers for the WPF hosts. This is the map. It is not a second requirements file.

Protocol facts live in `Vestigium.Helpers.Network` Requirements v1.6.  
If a host paper and the library paper disagree on a protocol fact, **the library wins**.  
If Requirements and Design disagree on the window, **Requirements win**.  
A live `PRnn` plan is the slice. It does not reopen those two fights.

## Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| Language | C# |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Solution | `Vestigium.Suite.Network.slnx` |
| Network | `Vestigium.Helpers.Network` 1.2.0 |
| Analytics | `Vestigium.Helpers.Analytics` 1.0.1 |
| Charts | `Vestigium.Helpers.Charts` 1.0.1 |
| Logging | `Vestigium.Logging` 1.7.1 |

Charts stay on the host that owns a series. Network does not plot.  
No project reference to `Vestigium.Helpers` — package only.  
Shell does not wrap `NetworkHelper`. No port sweep. No default-route write. No HTTP client.

## Hosts

| Host | Kind | First and ten | Requirements | Design | PR-Plans |
|---|---|---|---|---|---|
| Shell | library | `HostIds`, `HostLog`, `BindFields` | [Requirements](Shell/Requirements_v1.0.md) | [Design](Shell/Design_v1.0.md) | [PR-Plans](Shell/PR-Plans/README.md) |
| PingIQ | exe | Four echoes. Reply list. Cancel. | [Requirements](PingIQ/Requirements_v1.0.md) | [Design](PingIQ/Design_v1.0.md) | [PR-Plans](PingIQ/PR-Plans/README.md) |
| TraceIQ | exe | One target. One walk. Hop list. | [Requirements](TraceIQ/Requirements_v1.0.md) | [Design](TraceIQ/Design_v1.0.md) | [PR-Plans](TraceIQ/PR-Plans/README.md) |
| DnsIQ | exe | Lookup + Probe. One name. | [Requirements](DnsIQ/Requirements_v1.0.md) | [Design](DnsIQ/Design_v1.0.md) | [PR-Plans](DnsIQ/PR-Plans/README.md) |
| NicIQ | exe | Which NIC, is it up, how fast. | [Requirements](NicIQ/Requirements_v1.0.md) | [Design](NicIQ/Design_v1.0.md) | [PR-Plans](NicIQ/PR-Plans/README.md) |
| RouteIQ | exe | Print routes and neighbors. One probe. | [Requirements](RouteIQ/Requirements_v1.0.md) | [Design](RouteIQ/Design_v1.0.md) | [PR-Plans](RouteIQ/PR-Plans/README.md) |
| ProbeHost | exe | One snapshot. Four lists. Read-only. | [Requirements](ProbeHost/Requirements_v1.0.md) | [Design](ProbeHost/Design_v1.0.md) | [PR-Plans](ProbeHost/PR-Plans/README.md) |
| ShareIQ | exe | One UNC. One run. No password. | [Requirements](ShareIQ/Requirements_v1.0.md) | [Design](ShareIQ/Design_v1.0.md) | [PR-Plans](ShareIQ/PR-Plans/README.md) |

Implement against Requirements. Design is the class and window map, not a second lock table.

DnsIQ is live: [PR01 -- Implementation Plan.md](DnsIQ/PR-Plans/PR01/PR01%20--%20Implementation%20Plan.md).

## Tree

```
WPF/
  README.md                 this index
  <Host>/
    Requirements_v1.0.md    the window
    Design_v1.0.md          class and window map
    PR-Plans/
      README.md             queue keeper — never a plan file
      PRnn/                 LIVE slice
        PRnn -- Implementation Plan.md
      Completed/PRnn/       finished plans — needs a file or Git drops the folder
```

Open `PR-Plans/README.md` to see idle vs live. If live, the paper is inside `PR-Plans/PRnn/`, not beside the queue README. That `PRnn` folder is what Solution Explorer already shows.

APPID is the host name (`PingIQ`, `DnsIQ`, …). Never `Network`.
