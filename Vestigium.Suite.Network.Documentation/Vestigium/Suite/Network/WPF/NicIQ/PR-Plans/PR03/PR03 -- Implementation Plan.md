# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 30 September 2026  
**Revised:** 1 October 2026 — PR03c.003 is on Helpers `main` (`01f1dc9`).  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–g) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package.

Letters stay the requirements. This file is the slice list. Do not rewrite a letter from a step.

---

## Where PR03c is

Helpers `main` is `01f1dc9`. NicIQ still has its own `CachedPdhSource.cs`. That delete is PR03c.008, after publish.

| ID | State | Evidence |
| :--- | :--- | :--- |
| PR03c.001 | Done | `public sealed class CachedPdhSource`. |
| PR03c.002 | Done | `SampleJob` default is that type and disposes it. |
| PR03c.003 | Done | First rate read is Unavailable. Level counters are not primed away. Misses stay Unavailable. |
| PR03c.004 | Next | New NIC instance must drop old counters. |

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
| Public PDH source | `CachedPdhSource` only. |
| Prime | Unprimed rate is Unavailable, never 0. |

---

## Overview — execution order

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | .001–.003 done. Next is .004. |
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
| 4 | PR03c.004 | Source lifetime follows the job. New NIC instance drops old instance counters. | Next |
| 5 | PR03c.005 | `ListInstances`: reuse `NetworkCounterCatalog.LiveInstances` if it already exists. | Planned |
| 6 | PR03c.006 | Helpers tests. | Planned |
| 7 | PR03c.007 | Pack and publish PerfMon. | Planned |
| 8 | PR03c.008 | Pin bump. Delete NicIQ `CachedPdhSource.cs`. | Planned |
| 9 | PR03c.009 | Owner smoke. | Planned |

PR03a through PR03e are unchanged and not started. PR03e stays parked.

---

## Next action

PR03c.004 on Helpers `main`. Drop counters for an instance the job no longer samples.
