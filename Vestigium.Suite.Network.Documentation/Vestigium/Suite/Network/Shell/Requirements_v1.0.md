# Shell — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-SHELL-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.Shell`  
**Kind:** WPF class library. Not an exe.

## Intent

Shared chrome for every Network suite host. Logging start, host APPID strings, bind fields, later theme. Not a protocol API.

## Locks

| # | Lock |
|---|---|
| 1 | `HostLog.Initialize(appId)` creates `%ProgramData%\Vestigium\Logs\{appId}` and calls `VestigiumLogger.Initialize` plus `NetworkCatalog.Register`. |
| 2 | Host APPID is `PingIQ`, `TraceIQ`, `DnsIQ`, `NicIQ`, `RouteIQ`, `ProbeHost`, or `ShareIQ`. Never `Network`. |
| 3 | `BindFields` holds `InterfaceIndex` and `SourceAddress` only. It does not apply bind and does not guess IfIndex `1`. |
| 4 | Hosts call `NetworkHelper` themselves. Shell does not wrap echo, trace, DNS, route, or snapshot. |
| 5 | No sockets. No scheduler. No plot. Charts stay on the host that has a series. |

## Non-goals

`IPingService`. Second Network façade. Default-route write. Port sweep.
