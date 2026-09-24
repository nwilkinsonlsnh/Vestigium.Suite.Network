# ProbeHost — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-PROBEHOST-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.ProbeHost`  
**Kind:** WPF exe. APPID `ProbeHost`.  
**Library jobs:** `GetSnapshot`, `GetConnections`, `GetStatistics`, `GetNetBios`.

## Intent

Read-only dump of the box for a ticket. Not a mutate tool.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.ProbeHost)`. |
| 2 | Snapshot is read-only. No route write. No echo from this window. |
| 3 | NetBIOS is Windows-only and not emulated on Linux. |
| 4 | This host is last in the product cut. It is a dump, not the demo. |

## Non-goals

Campaigns. Pathping. Default-route write. Port sweep.
