# PR03g-Memory — Requirements (NEW) `Vestigium.SystemInfo.Memory`

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03G-SYSINFO-MEM  
**Package:** `Vestigium.SystemInfo.Memory`  
**Depends on:** `Vestigium.SystemInfo`  
**Status:** Draft  
**Date:** 30 September 2026  
**Evidence:** `MemoryHostFacts.cs` — this file is the 0.1 spec.

## Goal

Replace `MemoryHostFacts` with a published satellite. Same numbers NicIQ already shows on the Memory card that are **not** PDH series.

This version is not `Available MBytes` / `Committed Bytes` time series. Those stay `Vestigium.Helpers.PerfMon.Memory`.

## Surface (0.1)

Physical total / available / in-use from `GlobalMemoryStatusEx`. Commit peak, kernel paged, kernel nonpaged from `GetPerformanceInfo` × page size. Raw values are bytes.

Failed API → Unavailable on the fields that API owns.

## Door

`MemoryFacts.Read()`. Host may call once per card refresh.

## EVENTID

20000–20499. Failed native only. Do not log byte counts as payload.

## Split with PerfMon.Memory

| Number | Package |
|---|---|
| Physical total / in use / available right now | SystemInfo.Memory |
| Commit peak, paged / nonpaged pools | SystemInfo.Memory |
| Available MBytes **series**, Committed Bytes **series**, % Committed | PerfMon.Memory |

NicIQ Memory card uses both. That is correct. One card, two packages.

## Host consume

Delete `MemoryHostFacts.cs`.
