# PR03g — Requirements (NEW) `Vestigium.SystemInfo`

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03G-SYSINFO  
**Kind:** New family. Not an extension of PerfMon. Not an extension of Network.  
**Packages this letter names:**  
- `Vestigium.SystemInfo` — shared contract  
- `Vestigium.SystemInfo.Cpu` — processor facts  
- `Vestigium.SystemInfo.Memory` — memory facts  
**Repo (start):** `nwilkinsonlsnh/Vestigium.Helpers` until the owner splits a repo. Same pattern as PerfMon satellites.  
**Status:** Draft — owner destination accepted; shape below is Dave  
**Date:** 30 September 2026  
**Evidence in NicIQ:** `CpuHostFacts.cs`, `MemoryHostFacts.cs`, CPU/Memory columns in `MonitorDetailCard.cs`

## Goal

One contract for **machine facts the OS already knows** — topology, physical memory, clock, process/thread/handle census — so a host does not P/Invoke `kernel32` / `psapi` / `powrprof` to fill a card.

This version is not PDH. It is not `GetAdapters()`. It is not a chart. It is not `Vestigium.SystemInfo.Network`.

## Why a new family (domain test)

| Already published | Owns | Why this is not that |
|---|---|---|
| `Vestigium.Helpers.PerfMon.Cpu` | Processor **PDH samples** (`% Processor Time`, User, Privileged) | Facts here are `GetLogicalProcessorInformationEx` and `CallNtPowerInformation`. Different door, different lifetime (snapshot vs 1 s job). |
| `Vestigium.Helpers.PerfMon.Memory` | Memory **PDH samples** (Available MBytes, Committed Bytes, …) | Facts here are `GlobalMemoryStatusEx` / `GetPerformanceInfo` page counts. |
| `Vestigium.Helpers.Network` | Adapter inventory, workstation name, protocol jobs | Box CPU/RAM is not a NIC. |
| `Vestigium.Helpers.Processes` | Process jobs (existing Helpers project) | Census counts are not a process list and not a start/kill API. |
| `Vestigium.Helpers.WinReg` | Registry | Do not stash CPUID in the registry helper. |

Catalog rule: same domain = same package. CPU topology failed the PerfMon domain test in PR03d. The owner now wants infrastructure. That is a new family, not a junk drawer on PerfMon.Cpu.

## Shape

Mirror PerfMon, not a kitchen sink.

```
host
  → SystemInfo.Cpu.Read() / Live()
  → SystemInfo.Memory.Read()
       → shared records + Unavailable
            → Win32 / native
```

| Package | Owns | Does not own |
|---|---|---|
| `Vestigium.SystemInfo` | Records, `Unavailable`, snapshot options, catalog register hook | P/Invoke for a specific device class |
| `Vestigium.SystemInfo.Cpu` | Sockets, cores, logical, cache sizes, current/max MHz, process/thread/handle census | `% Processor Time`. That stays PerfMon.Cpu. |
| `Vestigium.SystemInfo.Memory` | Physical total / in-use / available, commit peak, paged / nonpaged pool | PDH `Available MBytes` series. That stays PerfMon.Memory. |

A new device class later (Disk geometry, SMBIOS) is a new satellite under `Vestigium.SystemInfo.*`, not a root helper and not a chart package.

## Rejected: `Vestigium.SystemInfo.Network`

Two packages would own adapters. Catalog forbids that.

| Ask | Home |
|---|---|
| Adapter list, MAC, unicast, DHCP, metric, `GetWorkstation` | `Vestigium.Helpers.Network` (already) |
| SSID / PHY / quality | PR03e — still Network, still parked until owner expands `NetworkAdapter` |
| Adapter **rates** | `Vestigium.Helpers.PerfMon.Network` |

Do not mint `Vestigium.SystemInfo.Network`. Do not copy `WirelessLink.cs` here.

## Identity

| Field | Value |
|---|---|
| Prefix | `Vestigium.SystemInfo` / `Vestigium.SystemInfo.Cpu` / `Vestigium.SystemInfo.Memory` |
| TFM | `net10.0-windows` (Win32 facts). Do not fake support on non-Windows. |
| APPID | `SystemInfo` on core. Satellites `SystemInfo.Cpu`, `SystemInfo.Memory`. |
| EVENTID | Propose **19000–19499** core, **19500–19999** Cpu, **20000–20499** Memory. Confirm against Helpers requirements before first publish. Do not reuse PerfMon or Network ranges. |
| Logger | Register catalogs only. Never `VestigiumLogger.Initialize`. |
| Demo / CLI | None on 0.1. Same rule as PerfMon 0.1. |

Name is `Vestigium.SystemInfo`, not `Vestigium.Helpers.SystemInfo`. Owner named the family. It is still a helper job; it does not live under Themes or Controls.

## Contract

### S-01 Unavailable, never a fake zero

Missing API, `DllNotFound`, `EntryPointNotFound`, access denied → structured Unavailable / dash fields. Do not publish `0 GHz` or `0 GB` to mean “we could not look.”

### S-02 Snapshot, not a sample clock

`Read()` / `Live()` are point-in-time. Caching for 1–2 s inside the satellite is allowed so a card refresh does not hammer `CallNtPowerInformation`. The 1 s **rate** clock stays `SampleJob`.

### S-03 Host formats for the window

Library returns numbers + units where the OS gives numbers (bytes, MHz, counts). Display strings may ship as convenience formatters, but the record must keep the raw value.

### S-04 No PDH types

SystemInfo projects do not reference `Vestigium.Helpers.PerfMon*`.

### S-05 No Network types

SystemInfo does not reference `Vestigium.Helpers.Network`. Domain name on a NIC card stays `NetworkAdapter.DnsSuffix` / `GetWorkstation`.

## NicIQ after consume

| Delete | Keep |
|---|---|
| `CpuHostFacts.cs` | Card layout in `MonitorDetailCard` |
| `MemoryHostFacts.cs` | PDH last-values from `MonitorRing` (PerfMon) |
| Direct `DllImport` of `psapi` / `kernel32` / `powrprof` for these facts | Adapter columns from `NetworkAdapter` |

CPU card: PerfMon last `% Processor Time` + `SystemInfo.Cpu` topology/live.  
Memory card: PerfMon last Available/Committed + `SystemInfo.Memory` physical/pools.

## Must not happen

- `Vestigium.SystemInfo.Network`
- `Vestigium.Helpers.HostFacts`
- Folding topology into PerfMon.Cpu
- Vendor CPU SDK without a new owner line
- Logging the whole snapshot as payload

## Acceptance (family)

1. A headless test on Windows reads Cpu + Memory without a WPF host.
2. NicIQ has zero copies of `GetLogicalProcessorInformationEx` / `GlobalMemoryStatusEx`.
3. nuget.org has no `Vestigium.SystemInfo.Network`.
4. Published NugetPackages.md gains a SystemInfo family row before first push.

## Parked

- Disk geometry / SMART
- GPU name / VRAM (OS-exposed only; no vendor SDK)
- SMBIOS / serial
- Linux/macOS ports
- Moving `GetWorkstation` out of Network
