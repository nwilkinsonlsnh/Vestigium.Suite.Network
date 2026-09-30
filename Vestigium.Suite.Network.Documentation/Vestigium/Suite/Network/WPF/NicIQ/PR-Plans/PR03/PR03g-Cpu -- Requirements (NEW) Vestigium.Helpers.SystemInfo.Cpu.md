# PR03g-Cpu — Requirements (NEW) `Vestigium.Helpers.SystemInfo.Cpu`

**Package:** `Vestigium.Helpers.SystemInfo.Cpu`  
**Depends on:** `Vestigium.Helpers.SystemInfo`  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers` — `src/Vestigium.Helpers.SystemInfo.Cpu`  
**Evidence:** `CpuHostFacts.cs`

## Goal

Replace `CpuHostFacts`. Not `% Processor Time` (that is `Helpers.PerfMon.Cpu`).

## 0.1 surface

Topology: sockets, cores, logical, L1–L4 from `GetLogicalProcessorInformationEx`.  
Live: process/thread/handle from `GetPerformanceInfo`; current/max MHz from `CallNtPowerInformation` level 11.

Doors: `CpuFacts.Host`, `CpuFacts.Live()`. Failed native → Unavailable.

EVENTID 19500–19999. No MHz in log payloads.

Delete `CpuHostFacts.cs` from NicIQ when this publishes.
