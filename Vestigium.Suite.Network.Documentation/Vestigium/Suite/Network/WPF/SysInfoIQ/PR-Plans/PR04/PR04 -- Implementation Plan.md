# SysInfoIQ — PR04 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR04  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Fill the Software tab with real local data in themed multi-column grids. Collect on the existing load path. Failed section continues. Location and bitness come from the registry key, not from a product field.

**Not:** Processes, Events. Not export. Not query bar. Not PowerShell. Not `Win32_Product`. Not remote.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR04.001 | Software collector + row DTOs | Done |
| 2 | PR04.002 | Software grids + section picker | Done |
| 3 | PR04.003 | Status per section, failed section continues | Done |
| 4 | PR04.004 | Pure mapping tests | Open |
| 5 | PR04.005 | Owner runs against Requirements acceptance | Owner |

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No `Win32_Product`. Registry and managed doors only. |
| Theodore | Failed section continues. Tests off the registry and the service list. Hive map is pure. |
| Simon | Location and Arch match the key. Earlier tabs unchanged. |

---

## Next action

PR04.004. Pure mapping tests.
