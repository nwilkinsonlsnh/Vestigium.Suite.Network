# PR03 — NicIQ extract roadmap

**Host:** `Vestigium.Suite.Network.NicIQ`  
**Status:** Live plan — papers only this slice  
**Date:** 30 September 2026  
**Baseline:** Rev1 host on `main` after PR02 monitoring  
**Binding:** [Published NugetPackages.md](../../../../../Published%20NugetPackages.md) wins on domain. These papers win on what leaves the exe.

## Goal

NicIQ Rev1 stays shippable. PR03 does not add a tab. It names every host type that already belongs in a published package, writes the requirements for that package, and sequences the consume-back so the host shrinks.

This version is not a new host, not a plot API on Network, not `perfmon.exe`, not a tenth palette, and not `Vestigium.SystemInfo.Network`.

## Decision

Extract by **existing family** when the job already has a package. Mint a family only when the job fails that test. Owner named `Vestigium.SystemInfo` for machine facts that are not PDH and not protocol. That destination is accepted. The Network satellite is not.

| Letter | Package | Host evidence | Verdict |
|---|---|---|---|
| PR03a | `Vestigium.Helpers.Charts` | ScottPlot reach-through in `MonitorChart` | Extract |
| PR03b | `Vestigium.Themes` | `ThemeCatalog` copied in three hosts | Extract |
| PR03c | `Vestigium.Helpers.PerfMon` | `CachedPdhSource` | Extract |
| PR03d | `PerfMon.Cpu` + `PerfMon.Memory` | `HostCounters` strings | PDH catalogs only |
| PR03e | `Vestigium.Helpers.Network` | `WirelessLink` / wlanapi | Parked. Not SystemInfo |
| PR03f | `Vestigium.Controls*` | `ThemeChrome`, copied `PageViewport` | Extract |
| PR03g | **NEW** `Vestigium.SystemInfo` + `.Cpu` + `.Memory` | `CpuHostFacts` / `MemoryHostFacts` | New family. No `.Network` |

Rejected: `Helpers.Plots`, `Themes.Nord2`, CPU-into-Network, `SystemInfo.Network`, stuffing topology into PerfMon.Cpu.

## Sequence

1. PR03c PerfMon cached source
2. PR03a Charts
3. PR03d PDH catalogs
4. PR03g SystemInfo core + Cpu + Memory (Helpers repo is fine to start)
5. PR03b Themes
6. PR03f Controls
7. PR03e only if owner expands `NetworkAdapter`

## Watch

Alvin: SystemInfo mirrors PerfMon (core + satellites). No fourth network package.  
Theodore: after consume, NicIQ has no `GetLogicalProcessorInformationEx` / `GlobalMemoryStatusEx` / `using ScottPlot`.  
Simon: WLAN is Network or nothing. Topology is SystemInfo, not PerfMon.

## Next action

Owner accepts or amends. Add a SystemInfo row to Published NugetPackages.md before the first nupkg. Implement in Helpers / Themes, not this exe.
