# PR01 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR01-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Closed — accepted with deltas  
**Date:** 14 October 2026  
**Closed:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md)

This file is the definition of done for PR01. Accepted. Move this folder to `Completed/PR01/` when the product papers are folded.

---

## Must land

### PR01.001 Project scaffold — Done

- Project `src/Vestigium.Suite.Network.SysInfoIQ` (`WinExe`, `net10.0-windows`).
- Added to `Vestigium.Suite.Network.slnx`.
- References Shell. Packages: Themes, Controls, StatusBar, Converters, CommunityToolkit.Mvvm, System.Management.
- `App.xaml` / `App.xaml.cs` with `HostLog.Initialize(HostIds.SysInfoIQ)`.

### PR01.002 Chrome and pages — Done

- `SysInfoIqWindow` + `VestigiumShell`.
- HorizontalTab nav: **SysInfo** | **Settings** | **Help**.
- View menu: bar visible, Top/Bottom, Theme submenu.
- Status bar wired.
- Settings page with General + Theme.
- Help page with static copy.
- Icon: `Assets/SysInfoIQ.ico` on the exe and the window.

### PR01.003 Logging and settings persist — Done

- Logs: `C:\ProgramData\Vestigium\Logs\SysInfoIQ`.
- Settings: `C:\ProgramData\Vestigium\Settings\Diagnostics\SysInfoIQ\settings.json`.
- Persist theme, bar visible, bar dock, output folder.

### PR01.004 Base collection and display — Done with delta

- Collect runs on SysInfo page load. No Collect / Cancel buttons.
- One in-flight token.
- Base: Win32_ComputerSystem, Win32_OperatingSystem, Win32_BIOS, Win32_Processor, Win32_PhysicalMemory via System.Management.
- System tab is a themed `DataGrid.Compact` (Field | Value). Other category tabs are empty themed grids.
- No PowerShell. No wmic.exe.

### PR01.005 Export skeleton — Landed, not default

- `Exporter` writes `{Hostname}_Summary.txt`, section files, and `{Hostname}_InventoryManifest.txt` into a timestamped folder.
- Owner decision: export is **not** automatic on collect. Path line is not shown. Status stays on the bar.

### PR01.006 Host tests — Done

- APPID is `SysInfoIQ`.
- `SysInfoInput` rejects bad output paths.
- `SummaryFormatter` locked against the sample shape.
- No live CIM in unit tests.

---

## Deltas accepted

| Original | What shipped |
|---|---|
| Collect button + Cancel | Collect on load. No buttons. |
| Export after every collect | Export code exists. Not called by default. |
| Summary as text card | Summary as themed Field/Value grid. |
| Icon placeholder | `Assets/SysInfoIQ.ico` wired. |
| Category list box | Category HorizontalTabs: System, Storage, Network, Security, Software, Processes, Events. |

---

## Must not change in PR01

- Other hosts.
- Full script section list beyond base.
- Remote collection.
- Excel export.

---

## Acceptance

1. Project builds and runs. Met.
2. Logs under the SysInfoIQ path. Met.
3. SysInfo / Settings / Help tabs work. Met.
4. Collect on load produces the System grid. Met.
5. Settings survive restart. Met.
6. Tests green. Met.

---

## Parked (now PR02+)

- Storage and Network sections.
- Security, Software, Processes, Events.
- Query / filter on the grids.
- Explicit export action.
- Excel workbook.
- Elevation UI prompt.
