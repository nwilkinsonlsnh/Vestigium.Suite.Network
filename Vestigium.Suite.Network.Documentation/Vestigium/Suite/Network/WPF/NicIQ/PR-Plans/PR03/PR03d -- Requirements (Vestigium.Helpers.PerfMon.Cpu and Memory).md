# PR03d — Requirements (`Vestigium.Helpers.PerfMon.Cpu` / `Memory`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03D-PERFMON-SAT  
**Packages:** `Vestigium.Helpers.PerfMon.Cpu` 0.1.1, `Vestigium.Helpers.PerfMon.Memory` 0.1.1  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**APPID:** `PerfMon.Cpu` (18000–18499), `PerfMon.Memory` (range not printed on NuGet — confirm in Helpers docs before allocating)  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `HostCounters.cs`, `CpuHostFacts.cs`, `MemoryHostFacts.cs`

## Goal

Published satellites own the Processor and Memory counter paths NicIQ already samples.

This version is not a kitchen sink, not a chart, and not a third stats package.

## Two different piles in the exe

| Pile | Files | Domain test |
|---|---|---|
| PDH paths | `HostCounters` — `% Processor Time`, `% User Time`, `% Privileged Time` on `Processor Information(_Total)` with `Processor` fallback; Memory `Available MBytes`, `Committed Bytes`, `% Committed Bytes In Use`, `Commit Limit`, `Cache Bytes` | **Same domain as the satellites.** Extract. |
| Machine facts via P/Invoke | `CpuHostFacts` — `GetLogicalProcessorInformationEx`, `CallNtPowerInformation`, process/thread/handle counts. `MemoryHostFacts` — `GlobalMemoryStatusEx`, commit peak / paged / nonpaged from `GetPerformanceInfo` | **Not PDH.** Catalog: Cpu is processor PDH samples. Memory is commit / available / machine samples. Facts are adjacent, not the same job. |

## Decision (Dave)

**Extract the path catalogs now.**  
**Leave topology / GlobalMemoryStatusEx in the host until a second consumer exists or the owner expands the satellite purpose in writing.**

Reason: a `Vestigium.Helpers.HostFacts` package would be a sibling invented to clean one exe. Expanding PerfMon.Cpu past PDH without an owner line turns the satellite into a diagnostics junk drawer. NicIQ can keep the P/Invoke until PingIQ or a future HostIQ needs the same numbers.

Rejected: folding these counters into `Vestigium.Helpers.Network`. Rates that are not adapter-scoped do not belong there.

## Must change in the satellites

### D-01 Public path lists

Cpu exposes the preferred + fallback paths NicIQ uses (`Processor Information` first, `Processor` fallback). Memory exposes the Memory category paths above.

Hosts stop spelling category strings.

### D-02 Sample façades already exist

`CpuSampleOptions` / `MemorySampleOptions` already wrap `SampleJob`. If they do not cover User + Privileged + the Memory set NicIQ plots, extend the options. Do not make NicIQ assemble `CounterPath` by hand.

### D-03 Unavailable

Missing category (`Processor Information` on an old box) → try fallback → still missing → Unavailable. Never a fake 0% CPU.

## Must not change (this letter)

- `CpuHostFacts` / `MemoryHostFacts` stay in NicIQ.
- No vendor GPU SDK pattern applied here.
- Disk / Gpu / PageFile satellites are out of NicIQ PR03.

## Host after consume

Delete `HostCounters.cs`. CPU and Memory monitor pages ask the satellites for paths. Facts classes remain until an owner decision on D-park.

## Acceptance

1. NicIQ references PerfMon.Cpu / Memory catalogs instead of string constants.
2. `_Total` is an instance the satellite already documents. NicIQ v1.1 still does not default a NIC to `_Total`.
3. No new root helper.

## Parked (needs owner)

Move `CpuHostFacts` into PerfMon.Cpu and `MemoryHostFacts` into PerfMon.Memory as non-PDH machine facts. Only if the owner writes that into the satellite intent. Until then they are host code.
