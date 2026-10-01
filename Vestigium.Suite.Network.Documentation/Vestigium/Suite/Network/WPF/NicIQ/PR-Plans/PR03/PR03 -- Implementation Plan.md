# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 30 September 2026  
**Revised:** 1 October 2026 — PR03c.005 is on Helpers `main` (`2ae5826`).  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–g) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package.

Letters stay the requirements. This file is the slice list. Do not rewrite a letter from a step.

---

## Where PR03c is

Helpers `main` is `2ae5826`. NicIQ still has its own `CachedPdhSource.cs`. That delete is PR03c.008, after publish.

| ID | State | Evidence |
| :--- | :--- | :--- |
| PR03c.001 | Done | `public sealed class CachedPdhSource`. |
| PR03c.002 | Done | `SampleJob` default is that type and disposes it. |
| PR03c.003 | Done | First rate read is Unavailable. Level counters are not primed away. |
| PR03c.004 | Done | `Retain` drops counters the job does not sample. A miss drops the dead handle. |
| PR03c.005 | Done | `ListInstances` calls `PdhCounterInventory.Shared.LiveInstances`. Same door as `NetworkCounterCatalog.LiveInstances`. No second walk. |
| PR03c.006 | Next | Helpers tests. |

---

## Overview — execution order

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | .001–.005 done. Next is .006. |
| 2 | PR03a | `Vestigium.Helpers.Charts` — Helpers | After rates are honest. |
| 3 | PR03d | `Helpers.PerfMon.Cpu` / `Memory` — Helpers | Path catalogs only. |
| 4 | PR03g | `Helpers.SystemInfo` + `.Cpu` + `.Memory` — Helpers | New family. |
| 5 | PR03b | `Vestigium.Themes` — Themes | One suite palette list. |
| 6 | PR03f | `Vestigium.Controls*` — Controls | Bind tokens. After T-01. |
| 7 | PR03e | `Vestigium.Helpers.Network` — Helpers | **Parked.** |

---

## PR03c — `Vestigium.Helpers.PerfMon`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers` `main`  
**Pin today:** 0.1.1 → bump at .007

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03c.001 | Public type is `CachedPdhSource`. One type. Host does not subclass. | Done |
| 2 | PR03c.002 | Default `SampleJob` uses that type and disposes it. | Done |
| 3 | PR03c.003 | Miss → Unavailable. First unprimed rate is Unavailable, never a fake 0. | Done |
| 4 | PR03c.004 | `Retain` drops counters not in the job path list. Miss drops the dead handle. | Done |
| 5 | PR03c.005 | `ListInstances` uses `PdhCounterInventory.Shared`. Core does not reference the Network satellite. | Done |
| 6 | PR03c.006 | Helpers tests: busy NIC can be non-zero after prime; missing instance is Unavailable; dispose does not leak. | Next |
| 7 | PR03c.007 | Pack and publish PerfMon. | Planned |
| 8 | PR03c.008 | Pin bump. Delete NicIQ `CachedPdhSource.cs`. | Planned |
| 9 | PR03c.009 | Owner smoke. | Planned |

---

## PR03a — `Vestigium.Helpers.Charts`

Not started.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03a.001 | C-01: themed `ChartOptions` from Vestigium token strings. No Themes project reference. | Planned |
| 2 | PR03a.002 | C-02: limit lines when `ChartOptions.Limits` is set. Host never `Add.HorizontalLine`. | Planned |
| 3 | PR03a.003 | C-03: XMin/XMax. Default remains fit-data. | Planned |
| 4 | PR03a.004 | C-04: count-axis flag for Column. | Planned |
| 5 | PR03a.005 | Do not lift the two-series Line cap. | Planned |
| 6 | PR03a.006 | Keep legend toggle on Charts. Charts does not persist. | Planned |
| 7 | PR03a.007 | Helpers tests for themed options and limit lines. | Planned |
| 8 | PR03a.008 | Pack and publish Charts. | Planned |
| 9 | PR03a.009 | Pin bump. `MonitorChart` is `ChartView` only. No ScottPlot usings. | Planned |
| 10 | PR03a.010 | Delete `ChartTheme` hex helper. Host still scales and computes limits. | Planned |
| 11 | PR03a.011 | `using ScottPlot` absent from Suite.Network. | Planned |

---

## PR03d — `Vestigium.Helpers.PerfMon.Cpu` / `Memory`

PDH path catalogs only. Facts are PR03g. Not started.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03d.001 | Cpu preferred `Processor Information`, fallback `Processor`. | Planned |
| 2 | PR03d.002 | Memory catalog paths. Hosts stop spelling category strings. | Planned |
| 3 | PR03d.003 | Confirm Memory EVENTID before new named events. | Planned |
| 4 | PR03d.004 | Tests: published paths; missing category is Unavailable. | Planned |
| 5 | PR03d.005 | Pack and publish the two satellites. | Planned |
| 6 | PR03d.006 | Pin bump. Delete `HostCounters.cs`. | Planned |
| 7 | PR03d.007 | Do not move host facts into these packages. | Planned |

---

## PR03g — NEW `Vestigium.Helpers.SystemInfo*`

Not started. IDs are `Vestigium.Helpers.SystemInfo*`.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03g.001 | Confirm EVENTID 19000–19499 / 19500–19999 / 20000–20499. | Planned |
| 2 | PR03g.002 | Core project. Records + Unavailable. No P/Invoke. | Planned |
| 3 | PR03g.003 | Cpu satellite. | Planned |
| 4 | PR03g.004 | Memory satellite. | Planned |
| 5 | PR03g.005 | S-01 through S-05. No PerfMon or Network reference. | Planned |
| 6 | PR03g.006 | Headless Windows tests. | Planned |
| 7 | PR03g.007 | Pack the three Helpers IDs. No root `Vestigium.SystemInfo`. | Planned |
| 8 | PR03g.008 | Pin. Delete `CpuHostFacts.cs` and `MemoryHostFacts.cs`. | Planned |
| 9 | PR03g.009 | NicIQ grep for those P/Invokes is zero. | Planned |

---

## PR03b — `Vestigium.Themes`

Not started.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03b.001 | `RegisterSuiteV1` / `ThemeDefinitions.SuiteV1`. NuGet does not Register on import. | Planned |
| 2 | PR03b.002 | Ids match README 1.0.2. | Planned |
| 3 | PR03b.003 | Series.1–6 on every palette. | Planned |
| 4 | PR03b.004 | No implicit TargetType in Themes.Controls for suite controls. | Planned |
| 5 | PR03b.005 | Pack still eleven DLLs. | Planned |
| 6 | PR03b.006 | Delete three host `ThemeCatalog.cs` files together. | Planned |
| 7 | PR03b.007 | Hosts call RegisterSuiteV1 then Initialize. | Planned |

---

## PR03f — `Vestigium.Controls*`

Not started. Depends on PR03b.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03f.001 | StatusBar binds Surface.StatusBar / Text.Primary / Stroke.Subtle. | Planned |
| 2 | PR03f.002 | NumericUpDown and UnderConstruction default styles. | Planned |
| 3 | PR03f.003 | One PageViewport, preferred home Shell. | Planned |
| 4 | PR03f.004 | No `Vestigium.Controls.Theme`. | Planned |
| 5 | PR03f.005 | Pack touched control packages. | Planned |
| 6 | PR03f.006 | Delete `ThemeChrome.cs` from three hosts. | Planned |
| 7 | PR03f.007 | Palette switch updates those controls with no visual-tree walk. | Planned |

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

PR03c.006 on Helpers `main`. Tests for prime, Unavailable, and dispose. No live-wire test unless the fixture already has one.
