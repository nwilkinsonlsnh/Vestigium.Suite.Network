# PR03g-Cpu — Requirements (NEW) `Vestigium.SystemInfo.Cpu`

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03G-SYSINFO-CPU  
**Package:** `Vestigium.SystemInfo.Cpu`  
**Depends on:** `Vestigium.SystemInfo`  
**Status:** Draft  
**Date:** 30 September 2026  
**Evidence:** `CpuHostFacts.cs` — this file is the 0.1 spec.

## Goal

Replace `CpuHostFacts` with a published satellite. Same numbers NicIQ already shows on the CPU card.

This version is not `% Processor Time`. That stays `Vestigium.Helpers.PerfMon.Cpu`.

## Surface (0.1)

**Topology** (stable for the process): Sockets, Cores, Logical, L1/L2/L3/L4 cache size from `GetLogicalProcessorInformationEx`.

**Live** (refresh ≤ 1 s): Process/Thread/Handle counts from `GetPerformanceInfo`; Current and Max MHz from `CallNtPowerInformation` level 11.

Failed native call → Unavailable. Do not advertise guessed topology as measured.

## Doors

`CpuFacts.Host` — topology, cached for process life.  
`CpuFacts.Live()` — census + clocks, 1 s cache allowed.

No `Initialize`. Safe to bind on the dispatcher.

## EVENTID

19500–19999. Named events for failed native calls only. Do not log MHz as a payload body.

## Tests

- Topology: sockets ≥ 1, logical ≥ cores when both measured.
- Live: process count > 0 after a successful `GetPerformanceInfo`. Zero processes is Unavailable.
- Missing `powrprof` → clocks Unavailable, topology still attempted.

## Host consume

Delete `CpuHostFacts.cs`. `MonitorDetailCard.Cpu` reads the satellite. Utilization % still comes from the PerfMon ring.
