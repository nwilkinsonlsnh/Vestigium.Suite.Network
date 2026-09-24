# ShareIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-SHAREIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.ShareIQ`  
**Kind:** WPF exe. APPID `ShareIQ`.  
**Library jobs:** `PlanShareProbe` and share campaigns through Network + FileIo.

## Intent

Share reachability campaigns. No password field. File work is FileIo, not a spawn of `robocopy`.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.ShareIQ)`. |
| 2 | No password / credential field on this host. |
| 3 | Recipe and results stay under the campaign root. |
| 4 | I/O verbs come from FileIo via the Network share door. |

## Non-goals

Echo UI. Trace UI. Default-route write. Port sweep.
