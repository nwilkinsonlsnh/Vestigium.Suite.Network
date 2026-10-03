# NicIQ — Requirements v1.2

**Document ID:** VEST-SUITE-NETWORK-NICIQ-SRS-001  
**Version:** 1.2  
**Status:** Current window  
**Date:** 2 October 2026  
**Project:** `src/Vestigium.Suite.Network.NicIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `NicIQ` (never `Network`)  
**Binding:** [Requirements_v1.0.md](Requirements_v1.0.md) is first and ten. [Requirements_v1.1.md](Requirements_v1.1.md) is the monitoring slice. This file keeps both and adds the details window, View menu, chart rules, and license. Helpers.Network wins on inventory. Helpers.PerfMon wins on PDH.

---

## 1. Kept from v1.0

List adapters, one selected adapter, workstation header, one watch, keep the last list on inventory failure, no packet send, no route mutate. Rules N1–N10 still hold unless a row below says otherwise.

v1.0 said counters and charts were out of first ten. v1.1 shipped sampling. Charts now ship on Monitoring.

---

## 2. Kept from v1.1

| Job | Door | Rule |
|---|---|---|
| List adapters | `NetworkHelper.GetAdapters()` | Unchanged. |
| Primary NIC | host pick | Lowest IPv4 metric among **Up** adapters that have a unicast address. Saved Id wins if that adapter is still Up. |
| Live instances | `NetworkCounterCatalog.LiveInstances("Network Interface")` | Map adapter Name/Description onto a PDH instance. Slash → underscore. |
| Sample | `SampleJob` + `NetworkCounterCatalog.Paths` | One-second jobs, cancel token from the window. |
| Counter list | `NetworkInterface.Counters` | Settings \ Monitoring add/remove. Persist in `settings.json`. |

| Piece | Rule |
|---|---|
| Main tab | Header **Monitoring**. Key stays `NicIQ`. |
| Open | Refresh inventory, select primary (or saved) NIC, start sampling. |
| Active NIC | Combo of **Up** adapters. Changing it restarts the sample job and updates an open adapter detail window. |
| Samples | Latest value per selected counter. Unavailable is "—", never a fake zero. |
| Settings \ Monitoring | Available list vs selected list. Add / Remove. Empty selected list falls back to Bytes Received/Sent/Total per sec. |
| Close | Cancel the monitor token. |

Status watch (`WatchAdapter`) stays a separate button. It does not own the sample clock.

v1.1 acceptance still applies: opens on Monitoring and samples without a click, combo is Up adapters only, a missing PDH instance is a status line, Refresh / Watch / Cancel still work.

---

## 3. View menu

| Piece | Rule |
|---|---|
| Status bar visible | Checkable. Bound to `ShowStatusBar`. |
| Divider | After visibility. |
| Dock | Status bar at Bottom. Status bar at Top. |
| Divider | After the dock pair. |
| Adapter details | Opens the adapter detail window for the active monitor NIC. Follows later adapter changes. |
| System details | Opens the system detail window. Not an adapter page. |
| Divider | Before Theme. |
| Theme | Existing theme submenu. One check on the active palette. |

Dividers are visible on the popup. `Stroke.Subtle` matches the menu card in the dark palettes and is not used for this line. The standard style is `Separator.Menu` in `Vestigium.Themes` 1.0.6. NicIQ also sets a local template (`#9AA8B8`) because menu chrome ships an implicit separator that can hide the theme style inside the popup.

CPU and Memory links do not open Adapter details.

---

## 4. System details

One window. Title is the machine name. Copy writes the same fields. Tabs use `RadioButton.HorizontalTab`, the same style as NicIQ / Monitoring / Settings.

No hotfix list.

| Tab | Fields |
|---|---|
| Windows | Date/time, host name, operating system line (name, bits, version, build), language, edition, display version, build, DirectX. |
| Computer | Manufacturer, model, system type, processor (name, logical CPU count, approximate GHz), BIOS, domain, locale, time zone. |
| Memory | Total physical, in use, available, commit peak. |
| Page File | Location, system managed, minimum, maximum, total, used, available. |
| Display | Name, manufacturer, chip, approximate total memory, VRAM, shared memory. |

Page file location comes from `PagingFiles`. A leading `?:` means the system drive, not a drive letter. Show one colon (`C:\pagefile.sys`, not `C::\pagefile.sys`). A `0 0` size pair is system managed.

DirectX acceleration flags (DirectDraw, Direct3D, AGP, Ultimate) are out. They need a DXGI runtime read.

---

## 5. Monitoring charts

| Chart | Rule |
|---|---|
| Throughput | Receive and send use two different colors. |
| Packets | Receive and send use two different colors. |
| Utilization | One line. |
| CPU | User and Privileged use two different colors. |
| Memory | Physical in use and available. The two add to installed RAM. No commit charge and no control-limit line on this chart. Scale is total installed RAM. |

---

## 6. License

The suite is MIT. Copyright `Copyright (c) 2026 Vestigium / Wilkinson Business`. `Directory.Build.props` attaches `LICENSE` and `README.md` to NicIQ, DnsIQ, PingIQ, ProbeHost, RouteIQ, ShareIQ, Shell, and TraceIQ. Both copy next to the build and pack at the package root.

---

## 7. Acceptance added in 1.2

1. View shows three dividers: under status-bar visibility, under the dock items, and above Theme.
2. View → Adapter details opens the adapter page for the NIC selected on Monitoring, and follows a later change.
3. View → System details opens the machine page. CPU and Memory do not link to adapter details.
4. System details has Windows, Computer, Memory, Page File, and Display.
5. Page file location is a real drive letter. System-managed `?:` does not show as `?:` or `C::`.
6. Memory chart in use plus available reads as the installed RAM. Hotfixes are absent.

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. List, detail, watch. Kept as [Requirements_v1.0.md](Requirements_v1.0.md). |
| 1.1 | 29 Sep 2026 | PerfMon sample on primary NIC. Settings counter list. Kept as [Requirements_v1.1.md](Requirements_v1.1.md). |
| 1.2 | 2 Oct 2026 | Monitoring chart colors and memory scale. System details (systeminfo, Winver, dxdiag fields, no hotfixes). Page File tab and `?:` drive. View menu dividers, Adapter details, System details. MIT license and readme attached from `Directory.Build.props`. |
