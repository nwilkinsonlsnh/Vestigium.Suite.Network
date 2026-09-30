# PR03d — Requirements (`Vestigium.Helpers.PerfMon.Cpu` / `Memory`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03D-PERFMON-SAT  
**Packages:** `Vestigium.Helpers.PerfMon.Cpu` 0.1.1, `Vestigium.Helpers.PerfMon.Memory` 0.1.1  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**APPID:** `PerfMon.Cpu` (18000–18499), `PerfMon.Memory` (confirm range in Helpers docs)  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `HostCounters.cs`

## Goal

Published satellites own the Processor and Memory **PDH** paths NicIQ already samples.

This version is not topology, not `GlobalMemoryStatusEx`, not a chart.

## Decision

**Extract the path catalogs now.**  
**Machine facts moved to PR03g** (`Vestigium.SystemInfo.Cpu` / `.Memory`).

Old belief: leave P/Invoke in the host until a second consumer.  
What arrived: owner named SystemInfo as suite infrastructure.  
New position: PerfMon stays PDH. SystemInfo owns snapshots.

## Must change

Cpu exposes preferred + fallback paths (`Processor Information` first, `Processor` fallback). Memory exposes Available MBytes, Committed Bytes, % Committed, Commit Limit, Cache Bytes. Hosts stop spelling category strings.

Missing category → fallback → Unavailable. Never a fake 0% CPU.

## Host after consume

Delete `HostCounters.cs`. Facts classes delete when PR03g publishes.

## Moved

Facts: PR03g. This letter is PDH only.
