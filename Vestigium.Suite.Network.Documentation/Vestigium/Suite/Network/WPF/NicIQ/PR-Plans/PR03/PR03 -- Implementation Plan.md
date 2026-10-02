# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**APPID:** `NicIQ`  
**Status:** Closed  
**Date:** 30 September 2026  
**Revised:** 2 October 2026 — PR03a–g are done. PR03h is recorded, not started.  
**Binding:** Published NugetPackages.md wins on domain. Each letter (PR03a–h) wins on that package. This file wins on order and step IDs.

**Goal:** Move host-local library work out of NicIQ. Rev1 window stays shippable.

**Not:** A new host. Not `perfmon.exe`. Not a tenth palette. Not a root `Vestigium.SystemInfo` prefix. Not `Vestigium.Helpers.SystemInfo.Network`. Not a second shell package. Not `Vestigium.Helpers.Wireless`.

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
| `Vestigium.Themes` | 1.0.4 |
| `Vestigium.Controls.StatusBar` | 1.0.1 |
| `Vestigium.Controls.NumericUpDown` | 1.0.2 |
| `Vestigium.Controls.UnderConstruction` | 1.0.1 |
| `Vestigium.Helpers.Network` | 1.3.5 |

NicIQ keeps one `CachedPdhSource` for the monitor session. `HostCounters.cs`, `CpuHostFacts.cs`, `MemoryHostFacts.cs`, the three `ThemeChrome.cs` files, the three `ThemeCatalog.cs` files, and `WirelessLink.cs` are deleted. Hosts call `Themes.RegisterSuiteV1()` then `Initialize`. The card reads `NetworkHelper.TryWirelessAssociation`. The PerfMon ring still supplies the series.

---

## Overview — execution order

| Order | Phase | Package / repo | Why this slot |
| ---: | :--- | :--- | :--- |
| 1 | PR03c | `Vestigium.Helpers.PerfMon` — Helpers | Done. |
| 2 | PR03a | `Vestigium.Helpers.Charts` — Helpers | Done. Pin 1.0.9. |
| 3 | PR03d | `Helpers.PerfMon.Cpu` / `Memory` — Helpers | Done. |
| 4 | PR03g | `Helpers.SystemInfo` + `.Cpu` + `.Memory` — Helpers | Done. Pin 0.1.1. |
| 5 | PR03b | `Vestigium.Themes` — Themes | Done. Pin 1.0.4. |
| 6 | PR03f | `Vestigium.Controls*` — Controls | Done. |
| 7 | PR03e | `Vestigium.Helpers.Network` — Helpers | Done. Pin 1.3.5. |
| 8 | PR03h | NicIQ adapter details — Suite.Network | Recorded. Not started. |

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
| 9 | PR03g.009 | NicIQ grep for those P/Invokes is zero. | Done. `wlanapi` stays inside Network. |

---

## PR03b — `Vestigium.Themes`

Published 1.0.4. Suite.Network pins 1.0.4. Hosts call `RegisterSuiteV1`.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03b.001 | `RegisterSuiteV1` / `ThemeDefinitions.SuiteV1`. NuGet does not Register on import. | Done |
| 2 | PR03b.002 | Ids match README 1.0.2. | Done. Descriptions restored to the README text. |
| 3 | PR03b.003 | Series.1–6 on every palette. | Done. Already present. No file change. |
| 4 | PR03b.004 | No implicit TargetType in Themes.Controls for suite controls. | Done. None present. |
| 5 | PR03b.005 | Pack still eleven DLLs. | Done. Core, catalog, nine palettes. |
| 6 | PR03b.006 | Delete three host `ThemeCatalog.cs` files together. | Done |
| 7 | PR03b.007 | Hosts call RegisterSuiteV1 then Initialize. | Done |

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

## PR03e — `Vestigium.Helpers.Network`

Published 1.3.5. Suite.Network pins 1.3.5. No Wireless package.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03e.001 | Owner call on wireless association. | Done. Yes. |
| 2 | PR03e.002 | Door on `NetworkHelper`. Empty, not throw. | Done |
| 3 | PR03e.003 | SSID, PHY, quality. No keys in the log. | Done |
| 4 | PR03e.004 | One P/Invoke inside Network. | Done |
| 5 | PR03e.005 | Delete `WirelessLink.cs`. | Done |
| 6 | PR03e.006 | Never mint Wireless, Wlan, or SystemInfo.Network. | Standing |

---

## PR03h — NicIQ adapter details

Host slice. No new package. The strip stays a glance. The form is the inventory.

Card title on a NIC page is the active adapter name (`Wi-Fi`, `Ethernet 2`). CPU and memory keep their own titles. No selection stays `Network`.

Bottom right of the details pane is a link, label `Adapter details`. Click opens the form. Double-click on the adapter row does the same thing. The link is the visual aid. Double-click is the shortcut. Hover underlines. Focus uses the theme accent.

Three card rows, same shape as CPU:

| Row | Wi-Fi | Ethernet |
| ---: | :--- | :--- |
| 1 | Send, receive, PHY and band | Send, receive, link speed |
| 2 | SSID, signal | MAC, status |
| 3 | IPv4, domain | IPv4, gateway |

The form is one window titled with the adapter name. Four blocks, two columns, empty cells as a dash: Identity, Link, Addresses, Driver. Wi-Fi adds BSSID, Rx, Tx, and security (`WPA2-Enterprise · CCMP · 802.1X`) only when the adapter is `Wireless80211`. Ethernet does not show blank Wi-Fi labels. Band folds into the PHY cell. Profile name stays off. Keys stay off. The form reads `NetworkAdapter` and `WirelessAssociation`. It does not call `wlanapi`. Channel, frequency, and dBm wait for a later WLAN query.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03h.001 | NIC page title is the active adapter name. | Not started |
| 2 | PR03h.002 | Bottom-right `Adapter details` link. Double-click is the same open. | Not started |
| 3 | PR03h.003 | Three card rows. Wi-Fi and Ethernet differ. | Not started |
| 4 | PR03h.004 | Detail form: Identity, Link, Addresses, Driver. | Not started |
| 5 | PR03h.005 | Wi-Fi block only: BSSID, Rx, Tx, security. No profile, no keys. | Not started |
| 6 | PR03h.006 | Ethernet omits the Wi-Fi block. | Not started |
| 7 | PR03h.007 | Form does not call `wlanapi`. Channel and dBm stay out. | Standing |

---

## Close

PR03a through PR03g are done. PR03e.006 still forbids a Wireless package. PR03h is the next host slice and is not started. PR03c.009 remains an owner visual check, not a code slice.
