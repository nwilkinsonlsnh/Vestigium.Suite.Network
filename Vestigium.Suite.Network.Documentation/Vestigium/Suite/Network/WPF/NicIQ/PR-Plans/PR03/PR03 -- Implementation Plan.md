# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 30 September 2026  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–g) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package.

Letters stay the requirements. This file is the slice list. Do not rewrite a letter from a step.

---

## Owner locks this plan already has

| Lock | Meaning |
|---|---|
| Helpers family | SystemInfo IDs are `Vestigium.Helpers.SystemInfo*` in `nwilkinsonlsnh/Vestigium.Helpers`. |
| PerfMon stays PDH | Topology / `GlobalMemoryStatusEx` are PR03g, not PR03d. |
| No SystemInfo.Network | Adapters and WLAN stay on Helpers.Network (PR03e). |
| ScottPlot stays internal | Hosts never `using ScottPlot`. |
| Themes does not style suite controls | Tokens in Themes. Default styles on Controls. |
| PR03e is parked | Do not expand `NetworkAdapter` in the same breath as Charts / Themes. |

---

## Overview — execution order

Letters are named a–g by package. Work is not a then b. Cache the PDH source before the charts consume rates. Publish SystemInfo before deleting host P/Invoke. Themes list before Controls bind tokens.

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | Rate counters are zero until the source lives. Everything that charts CPU/NIC rates waits on this. |
| 2 | PR03a | `Vestigium.Helpers.Charts` — Helpers | Kill ScottPlot reach-through. Needs live samples so acceptance is real. |
| 3 | PR03d | `Helpers.PerfMon.Cpu` / `Memory` — Helpers | Path catalogs only. After core source exists. |
| 4 | PR03g | `Helpers.SystemInfo` + `.Cpu` + `.Memory` — Helpers | New family. After PDH is honest so cards keep both snapshot and rate. |
| 5 | PR03b | `Vestigium.Themes` — Themes | One suite palette list. Before Controls consume tokens on three hosts. |
| 6 | PR03f | `Vestigium.Controls*` — Controls | StatusBar / NumericUpDown / UnderConstruction bind tokens. After T-01 list exists. |
| 7 | PR03e | `Vestigium.Helpers.Network` — Helpers | **Parked.** Only if owner expands `NetworkAdapter` for WLAN. |

Do not start PR03b and PR03f in parallel with different token strings. Do not publish SystemInfo before the catalog row is in Published NugetPackages.md (already reserved on this branch).

Each phase ends with: library ships → pin bump on Suite.Network → NicIQ consume → letter acceptance.

---

## PR03c — `Vestigium.Helpers.PerfMon`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**Letter:** [PR03c -- Requirements (Vestigium.Helpers.PerfMon).md](PR03c%20--%20Requirements%20%28Vestigium.Helpers.PerfMon%29.md)  
**Evidence:** `CachedPdhSource.cs`  
**Pin today:** 0.1.1 → bump after P-01

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03c.001 | Name the public source type (`CachedPdhSource` or `LocalPdhSource`). One type. Host will not subclass. | Planned |
| 2 | PR03c.002 | Default `SampleJob` with no `Source` uses that type. Open one `PerformanceCounter` per path. Prime on first read. | Planned |
| 3 | PR03c.003 | Missing instance / InvalidOperation / access denied → `SampleRecord.Unavailable`. Never a fake 0 for “not primed.” Expose primed-or-Unavailable. | Planned |
| 4 | PR03c.004 | Source lifetime follows the job. Dispose opened counters on job dispose. New NIC instance drops old instance counters. | Planned |
| 5 | PR03c.005 | `ListInstances`: reuse `NetworkCounterCatalog.LiveInstances` if it already exists. Do not add a second lister. | Planned |
| 6 | PR03c.006 | Helpers tests: busy NIC `Bytes Received/sec` can be non-zero after prime; missing instance is Unavailable; dispose does not leak counters. | Planned |
| 7 | PR03c.007 | Pack and publish PerfMon (core only this slice unless satellites must retarget). | Planned |
| 8 | PR03c.008 | Suite.Network pin bump. Delete `src/Vestigium.Suite.Network.NicIQ/ViewModels/CachedPdhSource.cs`. `MainViewModel` stops passing a host source. | Planned |
| 9 | PR03c.009 | Owner smoke: one-second sample on a live adapter is not stuck at 0 after prime. VPN flap → Unavailable, no dispatcher throw. | Planned |

---

## PR03a — `Vestigium.Helpers.Charts`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**Letter:** [PR03a -- Requirements (Vestigium.Helpers.Charts).md](PR03a%20--%20Requirements%20%28Vestigium.Helpers.Charts%29.md)  
**Evidence:** `MonitorChart.cs`, `ChartTheme.cs`  
**EVENTID:** 16500–16999, used through 16545 — new ids only in the unused tail  
**Pin today:** 1.0.7

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03a.001 | C-01: door that fills `ChartOptions` from live Vestigium token strings (Accent.Primary, Surface.Window / Card, Text.Primary, Stroke.Subtle, Series.N). Missing token → current hex fallback. No `Vestigium.Themes` project reference. | Planned |
| 2 | PR03a.002 | C-02: when `ChartOptions.Limits` is set, `Line` / `Column` / `From` / `Control` draw CL / UCL / LCL. Token colors, dashed UCL/LCL, thicker CL, legend text UCL/CL/LCL. Host never `Add.HorizontalLine`. | Planned |
| 3 | PR03a.003 | C-03: `ChartOptions` XMin/XMax (optional YMin/YMax). Default remains fit-data so PingIQ / DnsIQ do not move. | Planned |
| 4 | PR03a.004 | C-04: count-axis flag for Column. Integrity Y from −1 to a stepped ceiling; all-zero still 0–10 floor. | Planned |
| 5 | PR03a.005 | Do not lift the two-series Line cap. Integrity stays Column. | Planned |
| 6 | PR03a.006 | Keep `SetLegendVisible` / `LegendToggled` on Charts. Charts does not persist. | Planned |
| 7 | PR03a.007 | Helpers tests: stub resource dictionary for themed options; limit lines present without inventing fences. | Planned |
| 8 | PR03a.008 | Pack and publish Charts. | Planned |
| 9 | PR03a.009 | Suite.Network pin bump. Rewrite `MonitorChart` to `ChartView` only. Delete ScottPlot usings and `WpfPlot` casts. | Planned |
| 10 | PR03a.010 | Delete `ChartTheme` hex helper. Host still computes scale (bytes → Kbps / GB) and Analytics `ControlLimits`. | Planned |
| 11 | PR03a.011 | Grep `using ScottPlot` across `Vestigium.Suite.Network.*` — zero hits. Theme switch repaints figure/data/axis/grid/series on next paint. | Planned |

---

## PR03d — `Vestigium.Helpers.PerfMon.Cpu` / `Memory`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**Letter:** [PR03d -- Requirements (Vestigium.Helpers.PerfMon.Cpu and Memory).md](PR03d%20--%20Requirements%20%28Vestigium.Helpers.PerfMon.Cpu%20and%20Memory%29.md)  
**Evidence:** `HostCounters.cs`  
**This letter is PDH only.** Facts are PR03g.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03d.001 | Cpu catalog: preferred `Processor Information` paths, fallback `Processor`. Missing category → fallback → Unavailable. Never a fake 0% CPU. | Planned |
| 2 | PR03d.002 | Memory catalog: Available MBytes, Committed Bytes, % Committed, Commit Limit, Cache Bytes. Hosts stop spelling category strings. | Planned |
| 3 | PR03d.003 | Confirm Memory EVENTID range in Helpers docs before new named events. Do not reuse 18000–18499. | Planned |
| 4 | PR03d.004 | Helpers tests: path strings are the published ones; missing category is Unavailable. | Planned |
| 5 | PR03d.005 | Pack and publish the two satellites. | Planned |
| 6 | PR03d.006 | Suite.Network pin bump. Delete `HostCounters.cs`. Wire Monitor pages to catalog paths. | Planned |
| 7 | PR03d.007 | Do **not** move `CpuHostFacts` / `MemoryHostFacts` into these packages. | Planned |

---

## PR03g — NEW `Vestigium.Helpers.SystemInfo*`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**Letters:** [PR03g](PR03g%20--%20Requirements%20%28NEW%29%20Vestigium.Helpers.SystemInfo.md) · [Cpu](PR03g-Cpu%20--%20Requirements%20%28NEW%29%20Vestigium.Helpers.SystemInfo.Cpu.md) · [Memory](PR03g-Memory%20--%20Requirements%20%28NEW%29%20Vestigium.Helpers.SystemInfo.Memory.md)  
**Evidence:** `CpuHostFacts.cs`, `MemoryHostFacts.cs`

Stand core first. Satellites depend on core. One Helpers PR is allowed if the three projects ship together.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03g.001 | Confirm EVENTID 19000–19499 / 19500–19999 / 20000–20499 against Helpers requirements. If taken, pick the next free block and update the catalog. | Planned |
| 2 | PR03g.002 | Add `src/Vestigium.Helpers.SystemInfo` — records, Unavailable, catalog register. TFM `net10.0-windows`. No P/Invoke. No Demo / CLI. Never `VestigiumLogger.Initialize`. | Planned |
| 3 | PR03g.003 | Add `src/Vestigium.Helpers.SystemInfo.Cpu` — `CpuFacts.Host` / `Live()`. Topology from `GetLogicalProcessorInformationEx`. Census from `GetPerformanceInfo`. MHz from `CallNtPowerInformation` level 11. | Planned |
| 4 | PR03g.004 | Add `src/Vestigium.Helpers.SystemInfo.Memory` — `MemoryFacts.Read()`. Physical from `GlobalMemoryStatusEx`. Pools / commit peak from `GetPerformanceInfo` × page size. Raw values are bytes. | Planned |
| 5 | PR03g.005 | S-01 through S-05: Unavailable not 0 GHz / 0 GB. Snapshot not SampleJob. No project reference to PerfMon* or Network. | Planned |
| 6 | PR03g.006 | Headless Windows tests: Cpu + Memory read without a WPF host. Failed native → Unavailable fields. | Planned |
| 7 | PR03g.007 | Pack IDs `Vestigium.Helpers.SystemInfo` / `.Cpu` / `.Memory` only. Refuse a nupkg named `Vestigium.SystemInfo`. | Planned |
| 8 | PR03g.008 | Suite.Network pin. Delete `CpuHostFacts.cs` and `MemoryHostFacts.cs`. Card layout stays. PDH last-values stay on the ring. | Planned |
| 9 | PR03g.009 | Grep NicIQ for `GetLogicalProcessorInformationEx` / `GlobalMemoryStatusEx` — zero. | Planned |

---

## PR03b — `Vestigium.Themes`

**Repo:** `nwilkinsonlsnh/Vestigium.Themes`  
**Letter:** [PR03b -- Requirements (Vestigium.Themes).md](PR03b%20--%20Requirements%20%28Vestigium.Themes%29.md)  
**Evidence:** `ThemeCatalog.cs` in NicIQ, DnsIQ, PingIQ  
**Pin today:** 1.0.2

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03b.001 | T-01: ship `ThemeDefinitions.SuiteV1` and/or `ThemeManager.RegisterSuiteV1()`. NuGet still does not Register on import. Per-id `FromPack` stays for two-palette hosts. | Planned |
| 2 | PR03b.002 | Ids, display names, assembly names, `isDark`, descriptions match README 1.0.2. Do not drift. | Planned |
| 3 | PR03b.003 | T-02: confirm Series.1–6 exist on every palette. Hole is a Themes bug, not a host hex table. Additive key only if a hole is real. | Planned |
| 4 | PR03b.004 | T-04: no implicit TargetType in `Vestigium.Themes.Controls` for NumericUpDown / UnderConstruction / StatusBar. | Planned |
| 5 | PR03b.005 | Pack still eleven product DLLs. No `Vestigium.Themes.Nord2`. | Planned |
| 6 | PR03b.006 | Suite.Network pin. Delete `ThemeCatalog.cs` from NicIQ, DnsIQ, and PingIQ in the same consume commit. | Planned |
| 7 | PR03b.007 | Each host: `RegisterSuiteV1()` then `Themes.Initialize` before the first window parses. | Planned |

---

## PR03f — `Vestigium.Controls*`

**Repo:** `nwilkinsonlsnh/Vestigium.Controls`  
**Letter:** [PR03f -- Requirements (Vestigium.Controls).md](PR03f%20--%20Requirements%20%28Vestigium.Controls%29.md)  
**Evidence:** `ThemeChrome.cs`, `App.xaml` TargetType, `PageViewport.cs` × 3  
**Depends on:** PR03b tokens already published

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03f.001 | F-01: `VestigiumStatusBar` default template DynamicResource Surface.StatusBar / Text.Primary / Stroke.Subtle. Fallback Card. | Planned |
| 2 | PR03f.002 | F-02: NumericUpDown and UnderConstruction ship default styles using the same tokens NicIQ put in `App.xaml`. | Planned |
| 3 | PR03f.003 | F-03: one `PageViewport` — preferred home `Vestigium.Suite.Network.Shell`. Not a fourth control package. Not three copies. | Planned |
| 4 | PR03f.004 | No `Vestigium.Controls.Theme`. No NicIQ-only control package for monitor cards. | Planned |
| 5 | PR03f.005 | Pack and publish the touched control packages. | Planned |
| 6 | PR03f.006 | Suite.Network pin. Delete `ThemeChrome.cs` from NicIQ, DnsIQ, PingIQ together. Slim host `App.xaml` theme TargetTypes. | Planned |
| 7 | PR03f.007 | Owner smoke: palette switch updates StatusBar, NumericUpDown, UnderConstruction with no visual-tree walk. | Planned |

---

## PR03e — `Vestigium.Helpers.Network` (parked)

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**Letter:** [PR03e -- Requirements (Vestigium.Helpers.Network).md](PR03e%20--%20Requirements%20%28Vestigium.Helpers.Network%29.md)  
**Evidence:** `WirelessLink.cs`  
**Gate:** owner says `NetworkAdapter` includes wireless association. Until then NicIQ keeps the file.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03e.001 | Owner call: expand `NetworkAdapter` for SSID / PHY / quality, or keep host-private, or delete the card later. | Parked |
| 2 | PR03e.002 | If yes: N-01 door on `NetworkHelper`. Missing WLAN / non-Wi-Fi / no association → empty, not throw. | Parked |
| 3 | PR03e.003 | If yes: N-02 fields SSID, PHY, quality 0–100. BSSID optional. Do not log keys. EVENTID in 14561–14999. | Parked |
| 4 | PR03e.004 | If yes: one P/Invoke surface inside Network. Pack Network. | Parked |
| 5 | PR03e.005 | If yes: Suite.Network pin. Delete `WirelessLink.cs`. Card reads the adapter. | Parked |
| 6 | PR03e.006 | Never mint `Helpers.Wireless`, `Helpers.Wlan`, or `Helpers.SystemInfo.Network`. | Parked |

---

## Repos this plan touches

| Repo | Phases |
|---|---|
| `nwilkinsonlsnh/Vestigium.Helpers` | c, a, d, g, e |
| `nwilkinsonlsnh/Vestigium.Themes` | b |
| `nwilkinsonlsnh/Vestigium.Controls` | f |
| `nwilkinsonlsnh/Vestigium.Suite.Network` | consume-back every phase; PageViewport prefers Shell |

Do not ProjectReference a sibling host. Do not bump a pin before that package is on nuget.org.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | Same door names as the letter. No façade. No ScottPlot in a host. SystemInfo projects sit under Helpers. |
| Theodore | Unavailable is not 0. Dispose does not leak counters. Tests stay off the live wire unless the letter says Windows-headless. |
| Simon | Order table above. PR03e stays parked. PR03d does not grow P/Invoke. |

---

## Next action

PR03c.001 in Vestigium.Helpers. Name the default source. Do not start Charts until that source is the `SampleJob` default.
