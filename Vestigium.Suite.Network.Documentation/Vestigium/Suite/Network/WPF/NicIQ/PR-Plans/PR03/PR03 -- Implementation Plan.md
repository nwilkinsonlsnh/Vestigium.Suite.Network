# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Live  
**Date:** 30 September 2026  
**Revised:** 1 October 2026 — PerfMon version is 0.1.2 on Helpers `main` (`8dd1c80`). nuget.org push is still the owner.  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–g) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package.

Letters stay the requirements. This file is the slice list. Do not rewrite a letter from a step.

---

## Where PR03c is

Helpers `main` is `8dd1c80`. Package version is 0.1.2. Not on nuget.org until the owner pushes. NicIQ still has its own `CachedPdhSource.cs`. That delete is PR03c.008, after the push.

| ID | State | Evidence |
| :--- | :--- | :--- |
| PR03c.001 | Done | `public sealed class CachedPdhSource`. |
| PR03c.002 | Done | `SampleJob` default is that type and disposes it. |
| PR03c.003 | Done | First rate read is Unavailable. |
| PR03c.004 | Done | `Retain` drops counters the job does not sample. |
| PR03c.005 | Done | `ListInstances` calls `PdhCounterInventory.Shared.LiveInstances`. |
| PR03c.006 | Done | `PerfMonPR03cSourceTests`. |
| PR03c.007 | Packed | Version 0.1.2. nuget.org push is the owner. Do not pin NicIQ until that push exists. |
| PR03c.008 | Next | After nuget.org has 0.1.2: pin bump, delete NicIQ `CachedPdhSource.cs`. |

ME01 ReadyBoost failure was a stale empty assert. The typed catalog already lists those counters. Hyper-V stays empty.

---

## Overview — execution order

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | Packed at 0.1.2. Waiting on nuget.org. |
| 2 | PR03a | `Vestigium.Helpers.Charts` — Helpers | After the pin. |
| 3 | PR03d | `Helpers.PerfMon.Cpu` / `Memory` — Helpers | Path catalogs only. |
| 4 | PR03g | `Helpers.SystemInfo` + `.Cpu` + `.Memory` — Helpers | New family. |
| 5 | PR03b | `Vestigium.Themes` — Themes | One suite palette list. |
| 6 | PR03f | `Vestigium.Controls*` — Controls | Bind tokens. After T-01. |
| 7 | PR03e | `Vestigium.Helpers.Network` — Helpers | **Parked.** |

---

## PR03c — `Vestigium.Helpers.PerfMon`

**Repo:** `nwilkinsonlsnh/Vestigium.Helpers` `main`  
**Pin today:** 0.1.1 on Suite.Network. Packed version is 0.1.2.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03c.001 | Public type is `CachedPdhSource`. | Done |
| 2 | PR03c.002 | Default `SampleJob` uses that type and disposes it. | Done |
| 3 | PR03c.003 | First unprimed rate is Unavailable. | Done |
| 4 | PR03c.004 | `Retain` drops counters not in the job path list. | Done |
| 5 | PR03c.005 | `ListInstances` uses `PdhCounterInventory.Shared`. | Done |
| 6 | PR03c.006 | Source tests. | Done |
| 7 | PR03c.007 | Version 0.1.2. Owner pushes the nupkg. | Packed |
| 8 | PR03c.008 | Pin bump. Delete NicIQ `CachedPdhSource.cs`. | Next |
| 9 | PR03c.009 | Owner smoke. | Planned |

PR03a through PR03e tables are unchanged. PR03e stays parked.

---

## Next action

Owner packs and pushes `Vestigium.Helpers.PerfMon` 0.1.2 to nuget.org. Then PR03c.008.
