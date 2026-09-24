# RouteIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.RouteIQ`  
**Kind:** WPF exe. APPID `RouteIQ`.  
**Library jobs:** `GetRoutes`, Option C add/change/remove, `GetNeighbors`, `ProbeNeighbor`.

## Intent

Print the table both families. Mutate only under library Option C. Probe one neighbor address.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.RouteIQ)`. |
| 2 | `0.0.0.0/0` and `::/0` write stay denied. The host must not offer a default-route write button. |
| 3 | Missing admin / `CAP_NET_ADMIN` surfaces `NetworkRouteDenied`. No `route.exe` / `ip` / `netsh` spawn. |
| 4 | Neighbor probe is one address. `GetNeighbors` is the table. |
| 5 | IfIndex omit or `0` does not become `1`. |

## Non-goals

Default-route write. Port sweep. HTTP.
