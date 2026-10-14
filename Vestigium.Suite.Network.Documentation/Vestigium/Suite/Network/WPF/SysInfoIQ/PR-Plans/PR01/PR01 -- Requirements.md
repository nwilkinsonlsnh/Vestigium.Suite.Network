# PR01 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR01-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Draft  
**Date:** 14 October 2026  
**Product papers (to be updated when this PR lands):** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md)

This file is the **definition of done for PR01**. It is the first slice. When accepted, fold any deltas into the product papers and move this folder to `Completed/PR01/`.

Baseline: empty. No SysInfoIQ project exists yet.

---

## Must land

### PR01.001 Project scaffold

- New project `src/Vestigium.Suite.Network.SysInfoIQ` (`WinExe`, `net10.0-windows`).
- Add to `Vestigium.Suite.Network.slnx` under `/Suite/Network/WPF/`.
- Reference `Vestigium.Suite.Network.Shell`.
- Package references: Themes, Controls, StatusBar, Converters, Helpers (minimum).
- `App.xaml` / `App.xaml.cs` with `HostLog.Initialize(HostIds.SysInfoIQ)`.
- Assets folder placeholder for icon.

### PR01.002 Chrome and pages

- `SysInfoIqWindow` + `VestigiumShell`.
- HorizontalTab nav: **SysInfo** | **Settings** | **Help**.
- View menu: bar visible, Top/Bottom, Theme submenu with single check.
- Status bar wired.
- Cold start centered on primary work area.
- Settings page with General + Theme (indented tabs).
- Help page with static copy (purpose, logs, elevation, export).

### PR01.003 Logging and settings persist

- Logs land in `C:\ProgramData\Vestigium\Logs\SysInfoIQ`.
- Settings store at `C:\ProgramData\Vestigium\Settings\Diagnostics\SysInfoIQ\settings.json`.
- Persist theme, bar visible, bar dock, output folder.
- Load after Themes.Initialize. Defaults on missing file.

### PR01.004 Base collection and display

- Collect button on SysInfo page.
- One in-flight token. Cancel.
- Collect base: Win32_ComputerSystem, Win32_OperatingSystem, Win32_BIOS, Win32_Processor, Win32_PhysicalMemory via System.Management.
- Compute Summary text exactly matching the locked block in SectionsCatalog.md (including Domain Role / OS Product Type labels, uptime format, memory GB to two decimals, and Inventory Location).
- Display summary card + raw section text or grids.
- No PowerShell. No wmic.exe.

### PR01.005 Export skeleton

- After successful collect, write `{Hostname}_Summary.txt` and the raw section files into a timestamped folder under the configured output path.
- Write `{Hostname}_InventoryManifest.txt`.
- Failed section writes an error file instead of aborting.

### PR01.006 Host tests

- APPID is `SysInfoIQ`.
- `SysInfoInput` rejects bad output paths.
- No live CIM in unit tests.

---

## Must not change in PR01

- Other hosts.
- Full script section list (Hardware beyond base, Storage, Network, Users, Software, Services, Events, Drivers, SQL, msinfo32).
- Remote collection.
- Excel export.

---

## Acceptance

1. Project builds and runs.
2. Logs appear under the SysInfoIQ path.
3. SysInfo / Settings / Help tabs work and match DnsIQ chrome style.
4. Collect produces a readable summary and writes the export folder.
5. Settings survive restart.
6. Tests green.

---

## Parked

- Additional sections from the script.
- Section selection checkboxes.
- Excel workbook.
- Elevation UI prompt.
