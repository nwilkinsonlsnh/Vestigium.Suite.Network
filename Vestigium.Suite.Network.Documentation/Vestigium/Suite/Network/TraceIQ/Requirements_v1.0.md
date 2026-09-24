# TraceIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-TRACEIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.TraceIQ`  
**Kind:** WPF exe. APPID `TraceIQ`.  
**Library jobs:** `IcmpTrace` / `Trace`, `Pathping`.

## Intent

One window for path walk and pathping sample. Same bind on walk and sample.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.TraceIQ)`. |
| 2 | Trace and pathping pass bind fields through. UDP sample must apply bind. |
| 3 | Pathping phase 2 reuses the walk protocol. |
| 4 | PTR miss stays empty. TCP mid-path hops may stay `*`. |
| 5 | No echo campaign UI. No DNS UI. |

## Non-goals

Ping campaigns. Adapter watch. Default-route write. Port sweep.
