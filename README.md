# Vestigium.Suite.Network

WPF diagnostic hosts for `Vestigium.Helpers.Network`. Not the protocol library. The library stays in [Vestigium.Helpers](https://github.com/nwilkinsonlsnh/Vestigium.Helpers).

## Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| Language | C# |
| TFM | `net10.0-windows` (.NET 10 LTS) |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Solution | `Vestigium.Suite.Network.slnx` |

## Consume

Pinned in `Directory.Build.props`:

| Package | Version |
|---|---|
| `Vestigium.Helpers.Network` | 1.2.0 |
| `Vestigium.Helpers.Analytics` | 1.0.1 |
| `Vestigium.Helpers.Charts` | 1.0.1 |
| `Vestigium.Logging` | 1.7.1 |

Charts stay on the host. Network does not plot.

Each exe initializes logging with **its** APPID (`PingIQ`, not `Network`) and calls `NetworkCatalog.Register`.

## Projects

| Project | Kind | Owns |
|---|---|---|
| `Shell` | class library | Logging init, bind fields, chrome. Not a second protocol API. |
| `PingIQ` | exe | Echo, campaigns, PathMtu, UdpProbe |
| `TraceIQ` | exe | Trace, Pathping |
| `DnsIQ` | exe | Lookup, ProbeDns |
| `NicIQ` | exe | Adapters, WatchAdapter, SampleCounters |
| `RouteIQ` | exe | Routes, neighbors. Default route write stays denied. |
| `ProbeHost` | exe | Snapshot dump. Read-only. |
| `ShareIQ` | exe | Share campaigns via Network + FileIo |

Open `Vestigium.Suite.Network.slnx` in Visual Studio 2026.

## Rules

- No project reference to `Vestigium.Helpers`. Package only.
- Shell does not wrap `NetworkHelper` behind a second façade.
- No port sweep. No default-route write. No HTTP client.
