# NicIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-NICIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.NicIQ`  
**Kind:** WPF exe. APPID `NicIQ`.  
**Library jobs:** `GetWorkstation`, `GetAdapters`, `GetAdapter`, `WatchAdapter`, `SampleCounters`.

## Intent

Which NIC, is it up, did speed drop. One adapter, one duration for watch/counters.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.NicIQ)`. |
| 2 | Watch and counters do not bill and do not plot unless this host later adds Charts on a series it already has. |
| 3 | Index `0` is “not pinned,” not adapter 1. |
| 4 | No packet send from this window except through an explicit library job this paper names. |

## Non-goals

Route mutate. Echo. Port sweep.
