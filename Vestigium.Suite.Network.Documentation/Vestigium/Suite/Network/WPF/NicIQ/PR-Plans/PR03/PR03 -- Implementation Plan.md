# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 30 September 2026  
**Revised:** 2 October 2026 — PR03f.001–.007 are done. StatusBar 1.0.1, NumericUpDown 1.0.2, UnderConstruction 1.0.1.  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–g) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package.

Letters stay the requirements. This file is the slice list. Do not rewrite a letter from a step.

---

## Where the finished phases are

| Pin | Version |
| :--- | :--- |
| `Vestigium.Helpers.Charts` | 1.0.9 |
| `Vestigium.Helpers.PerfMon` | 0.1.6 |
| `Vestigium.Helpers.PerfMon.Cpu` | 0.1.2 |
| `Vestigium.Helpers.PerfMon.Memory` | 0.1.2 |
| `Vestigium.Helpers.PerfMon.Network` | 0.1.2 |
| `Vestigium.Helpers.SystemInfo` | 0.1.1 |
| `Vestigium.Helpers.SystemInfo.Cpu` | 0.1.1 |
| `Vestigium.Helpers.SystemInfo.Memory` | 0.1.1 |
| `Vestigium.Themes` | Suite.Network still pins 1.0.2. NuGet has 1.0.4. |
| `Vestigium.Controls.StatusBar` | 1.0.1 |
| `Vestigium.Controls.NumericUpDown` | 1.0.2 |
| `Vestigium.Controls.UnderConstruction` | 1.0.1 |

NicIQ keeps one `CachedPdhSource` for the monitor session. `HostCounters.cs`, `CpuHostFacts.cs`, `MemoryHostFacts.cs`, and the three `ThemeChrome.cs` files are deleted. The card formats SystemInfo raw numbers. The PerfMon ring still supplies the series. Three host `ThemeCatalog.cs` files remain.

---

## Overview — execution order

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | Code done. Visual smoke is the owner. |
| 2 | PR03a | `Vestigium.Helpers.Charts` — Helpers | Done. Pin 1.0.9. |
| 3 | PR03d | `Helpers.PerfMon.Cpu` / `Memory` — Helpers | Done. |
| 4 | PR03g | `Helpers.SystemInfo` + `.Cpu` + `.Memory` — Helpers | Done. Pin 0.1.1. |
| 5 | PR03b | `Vestigium.Themes` — Themes | Package done at 1.0.4. Host consume is next. |
| 6 | PR03f | `Vestigium.Controls*` — Controls | Done. Pins above. |
| 7 | PR03e | `Vestigium.Helpers.Network` — Helpers | **Parked.** |

---

## PR03c — `Vestigium.Helpers.PerfMon`

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03c.001 | Public type is `CachedPdhSource`. | Done |
| 2 | PR03c.002 | Default `SampleJob` uses that type and disposes it. | Done |
| 3 | PR03c.003 | First unprimed rate is Unavailable. | Done |
| 4 | PR03c.004 | `Retain` drops counters not in the job path list. | Done |
| 5 | PR03c.005 | `ListInstances` uses `PdhCounterInventory.Shared`. | Done |
| 6 | PR03c.006 | Source tests. | Done |
| 7 | PR03c.007 | Published 0.1.3, then 0.1.6 for the missing-category fix. | Done |
| 8 | PR03c.008 | Pin bump. Delete NicIQ `CachedPdhSource.cs`. | Done |
| 9 | PR03c.009 | Owner looks at a live adapter. Code path does not throw on Unavailable. | Code-ready |

---

## PR03a — `Vestigium.Helpers.Charts`

Published 1.0.9. Suite.Network pin is 1.0.9.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03a.001 | C-01: themed `ChartOptions` from Vestigium token strings. No Themes project reference. | Done |
| 2 | PR03a.002 | C-02: limit lines when `ChartOptions.Limits` is set. Host never `Add.HorizontalLine`. | Done |
| 3 | PR03a.003 | C-03: XMin/XMax. Default remains fit-data. | Done |
| 4 | PR03a.004 | C-04: count-axis flag for Column. | Done |
| 5 | PR03a.005 | Do not lift the two-series Line cap. | Done |
| 6 | PR03a.006 | Keep legend toggle on Charts. Charts does not persist. | Done |
| 7 | PR03a.007 | Helpers tests for themed options and limit lines. | Done |
| 8 | PR03a.008 | Pack and publish Charts. | Done |
| 9 | PR03a.009 | Pin bump. `MonitorChart` is `ChartView` only. No ScottPlot usings. | Done |
| 10 | PR03a.010 | Delete `ChartTheme` hex helper. Host still scales and computes limits. | Done |
| 11 | PR03a.011 | `using ScottPlot` absent from the NicIQ chart path. | Done |

---

## PR03d — `Vestigium.Helpers.PerfMon.Cpu` / `Memory`

PDH path catalogs only. Facts stayed out of these packages.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03d.001 | Cpu preferred `Processor Information`, fallback `Processor`. | Done |
| 2 | PR03d.002 | Memory catalog paths. Hosts stop spelling category strings. | Done |
| 3 | PR03d.003 | Confirm Memory EVENTID before new named events. Next free is 19050. | Done |
| 4 | PR03d.004 | Tests: published paths; missing category is Unavailable. | Done |
| 5 | PR03d.005 | Pack and publish the two satellites. CPU 0.1.2, Memory 0.1.2, core 0.1.6. | Done |
| 6 | PR03d.006 | Pin bump. Delete `HostCounters.cs`. | Done |
| 7 | PR03d.007 | Do not move host facts into these packages. | Done |

---

## PR03g — `Vestigium.Helpers.SystemInfo*`

Published 0.1.1. The proposed 19000 block was already `PerfMon.Memory`, so no catalog was registered.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03g.001 | Confirm EVENTID 19000–19499 / 19500–19999 / 20000–20499. | Skipped. 19000–19499 is `PerfMon.Memory`. |
| 2 | PR03g.002 | Core project. Records + Unavailable. No P/Invoke. | Done |
| 3 | PR03g.003 | Cpu satellite. | Done |
| 4 | PR03g.004 | Memory satellite. | Done |
| 5 | PR03g.005 | S-01 through S-05. No PerfMon or Network reference. | Done |
| 6 | PR03g.006 | Headless Windows tests. | Done |
| 7 | PR03g.007 | Pack the three Helpers IDs. No root `Vestigium.SystemInfo`. | Done. 0.1.1. |
| 8 | PR03g.008 | Pin. Delete `CpuHostFacts.cs` and `MemoryHostFacts.cs`. | Done |
| 9 | PR03g.009 | NicIQ grep for those P/Invokes is zero. | Done. `wlanapi` remains for PR03e. |

---

## PR03b — `Vestigium.Themes`

Published 1.0.4. `RegisterSuiteV1` is on that package. Suite.Network still pins 1.0.2.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03b.001 | `RegisterSuiteV1` / `ThemeDefinitions.SuiteV1`. NuGet does not Register on import. | Done |
| 2 | PR03b.002 | Ids match README 1.0.2. | Done. Descriptions restored to the README text. |
| 3 | PR03b.003 | Series.1–6 on every palette. | Done. Already present. No file change. |
| 4 | PR03b.004 | No implicit TargetType in Themes.Controls for suite controls. | Done. None present. |
| 5 | PR03b.005 | Pack still eleven DLLs. | Done. Core, catalog, nine palettes. |
| 6 | PR03b.006 | Delete three host `ThemeCatalog.cs` files together. | Not started. Needs the 1.0.4 pin. |
| 7 | PR03b.007 | Hosts call RegisterSuiteV1 then Initialize. | Not started. Same consume as .006. |

---

## PR03f — `Vestigium.Controls*`

StatusBar 1.0.1, NumericUpDown 1.0.2, UnderConstruction 1.0.1. `PageViewport` lives in Shell. No `Vestigium.Controls.Theme`.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03f.001 | StatusBar binds Surface.StatusBar / Text.Primary / Stroke.Subtle. | Done |
| 2 | PR03f.002 | NumericUpDown and UnderConstruction default styles. | Done |
| 3 | PR03f.003 | One PageViewport, preferred home Shell. | Done |
| 4 | PR03f.004 | No `Vestigium.Controls.Theme`. | Done. Not on NuGet. ThemeLab stays a sample. |
| 5 | PR03f.005 | Pack touched control packages. | Done |
| 6 | PR03f.006 | Delete `ThemeChrome.cs` from three hosts. | Done |
| 7 | PR03f.007 | Palette switch updates those controls with no visual-tree walk. | Done. `DynamicResource` follows `SwitchTheme`. |

---

## PR03e — `Vestigium.Helpers.Network` (parked)

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03e.001 | Owner call on wireless association. | Parked |
| 2 | PR03e.002 | If yes: door on `NetworkHelper`. Empty, not throw. | Parked |
| 3 | PR03e.003 | If yes: SSID, PHY, quality. No keys in the log. | Parked |
| 4 | PR03e.004 | If yes: one P/Invoke inside Network. | Parked |
| 5 | PR03e.005 | If yes: delete `WirelessLink.cs`. | Parked |
| 6 | PR03e.006 | Never mint Wireless, Wlan, or SystemInfo.Network. | Parked |

---

## Next action

PR03b.006 and PR03b.007 together: pin Themes 1.0.4, call `RegisterSuiteV1`, then delete the three `ThemeCatalog.cs` files. PR03e stays parked. `WirelessLink.cs` still calls `wlanapi`.
