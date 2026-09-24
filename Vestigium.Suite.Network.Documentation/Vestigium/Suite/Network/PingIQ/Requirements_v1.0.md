# PingIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.PingIQ`  
**Kind:** WPF exe. APPID `PingIQ`.  
**Library jobs:** `IcmpEcho` / `Ping`, `PathMtu`, `UdpProbe`. Echo campaigns.

## Intent

One window for “is that host answering.” Four echoes by default. Continuous only under the library duration/interval rules.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.PingIQ)`. |
| 2 | Echo options pass `Bind.InterfaceIndex` and `Bind.SourceAddress`. Omit or `0` leaves the stack to choose. |
| 3 | Count default 4. Count `0` is continuous and must carry a duration. |
| 4 | RTT series may go to Analytics then Charts on this host. Network does not plot. |
| 5 | PathMtu and UdpProbe are extra doors on this host, not their own exe. |
| 6 | Campaigns (recipe + JSONL) live here. TraceIQ does not grow a scheduler. |

## Non-goals

Path walk. DNS lookup. Route write. Port sweep. HTTP client.
