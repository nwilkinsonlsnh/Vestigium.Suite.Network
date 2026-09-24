# PR03g — Requirements (NEW) `Vestigium.Helpers.SystemInfo`

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03G-SYSINFO  
**Kind:** New Helpers family. Not an extension of PerfMon. Not an extension of Network.  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers` — projects sit next to PerfMon, Json, Network.  
**Packages:**  
- `Vestigium.Helpers.SystemInfo` — shared contract  
- `Vestigium.Helpers.SystemInfo.Cpu` — processor facts  
- `Vestigium.Helpers.SystemInfo.Memory` — memory facts  
**Status:** Draft — owner named the prefix  
**Date:** 30 September 2026  
**Evidence:** `CpuHostFacts.cs`, `MemoryHostFacts.cs`, `MonitorDetailCard.cs`

## Goal

Machine facts the OS already knows — topology, physical memory, clock, process/thread/handle census — so a host does not P/Invoke `kernel32` / `psapi` / `powrprof` to fill a card.

This version is not PDH. It is not `GetAdapters()`. It is not a chart. It is not `Vestigium.Helpers.SystemInfo.Network`. It is not a root `Vestigium.SystemInfo` prefix.

## Naming (owner lock)

Old belief: `Vestigium.SystemInfo` as a suite-root family like Themes.  
What arrived: these helper class libraries belong in Helpers.  
New position: `Vestigium.Helpers.SystemInfo*` only. Root `Vestigium.SystemInfo` is out.

Catalog family: `Vestigium.Helpers.*` already owns jobs and inventory. This is inventory of the box, not chrome.

## Why not PerfMon / Network

| Published | Owns | This family |
|---|---|---|
| `Helpers.PerfMon.Cpu` | Processor PDH samples | `GetLogicalProcessorInformationEx`, `CallNtPowerInformation` |
| `Helpers.PerfMon.Memory` | Memory PDH samples | `GlobalMemoryStatusEx`, `GetPerformanceInfo` pools |
| `Helpers.Network` | Adapters, workstation name, protocol | CPU/RAM is not a NIC |
| `Helpers.Processes` | Process jobs | Census is not a process list |
| `Helpers.WinReg` | Registry | Not a CPUID stash |

## Shape

```
src/Vestigium.Helpers.SystemInfo
src/Vestigium.Helpers.SystemInfo.Cpu
src/Vestigium.Helpers.SystemInfo.Memory
```

Same satellite pattern as PerfMon. Core holds records + Unavailable. Satellites hold the native calls.

| Package | Owns | Does not own |
|---|---|---|
| `Vestigium.Helpers.SystemInfo` | Records, Unavailable, catalog register | Device P/Invoke |
| `Vestigium.Helpers.SystemInfo.Cpu` | Sockets, cores, logical, caches, MHz, process/thread/handle census | `% Processor Time` |
| `Vestigium.Helpers.SystemInfo.Memory` | Physical total/in-use/available, commit peak, paged/nonpaged | PDH series |

Later device class = `Vestigium.Helpers.SystemInfo.*` satellite. Not a root helper. Not a chart.

## Rejected: `Vestigium.Helpers.SystemInfo.Network`

Adapters already have a package. WLAN stays PR03e on Network. Rates stay PerfMon.Network.

## Identity

| Field | Value |
|---|---|
| TFM | `net10.0-windows` |
| APPID | `SystemInfo`, `SystemInfo.Cpu`, `SystemInfo.Memory` |
| EVENTID | Propose 19000–19499 / 19500–19999 / 20000–20499. Confirm in Helpers docs before publish. |
| Logger | Register catalogs only. Never `VestigiumLogger.Initialize`. |
| Demo / CLI | None on 0.1 |

## Contract

S-01 Unavailable, never a fake 0 GHz / 0 GB.  
S-02 Snapshot, not `SampleJob`. 1–2 s cache inside the satellite is allowed.  
S-03 Records keep raw numbers (bytes, MHz, counts). Formatters are extra.  
S-04 No reference to `Helpers.PerfMon*`.  
S-05 No reference to `Helpers.Network`.

## NicIQ after consume

Delete `CpuHostFacts.cs` and `MemoryHostFacts.cs`. Card layout stays. PDH last-values stay on the ring.

## Acceptance

1. Projects live under `Vestigium.Helpers`.
2. nuget.org IDs are `Vestigium.Helpers.SystemInfo` / `.Cpu` / `.Memory`.
3. No package named `Vestigium.SystemInfo`.
4. Catalog row exists before first push.

Companion: Cpu and Memory letters in this folder.
