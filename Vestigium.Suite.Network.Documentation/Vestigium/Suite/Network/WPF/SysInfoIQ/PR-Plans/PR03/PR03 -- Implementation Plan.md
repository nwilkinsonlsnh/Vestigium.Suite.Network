# SysInfoIQ — PR03 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Fill the Security tab with real local data in themed multi-column grids. Collect on the existing load path. Failed section continues.

**Not:** Software, Processes, Events. Not export. Not query. Not PowerShell. Not remote.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03.001 | Security collector + row DTOs | Done |
| 2 | PR03.002 | Security grids + section picker | Done |
| 3 | PR03.003 | Status per section, failed section continues | Done |
| 4 | PR03.004 | Pure mapping tests | Open |
| 5 | PR03.005 | Owner runs against Requirements acceptance | Owner |

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No netsh. Managed doors only. |
| Theodore | Failed section continues. Caps on rules and drivers. Tests off the wire and the cert store. |
| Simon | Columns match the sample files. System, Storage, Network unchanged. |

---

## Next action

PR03.004. Pure mapping tests.
