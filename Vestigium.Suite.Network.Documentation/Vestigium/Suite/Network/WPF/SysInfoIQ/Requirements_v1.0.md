# SysInfoIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-SRS-001  
**Version:** 1.0  
**Status:** Draft — foundation lock for PR01  
**Date:** 14 October 2026  
**Project:** `Vestigium.Suite.Network.SysInfoIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `SysInfoIQ`  
**Chrome:** `Vestigium.Themes`, `Vestigium.Controls`, StatusBar, HorizontalTab, VestigiumShell  
**Logging:** `Vestigium.Logging` → `C:\ProgramData\Vestigium\Logs\SysInfoIQ`  
**Helpers:** `Vestigium.Helpers` (FileIo, ClosedXML or text export as needed)  
**Binding:** This file wins on the window and inventory scope. Helpers win on shared utilities. Themes win on chrome.

---

## 1. What ships

One host window (`SysInfoIqWindow` + `VestigiumShell`). Three pages. One primary job: collect, organize, and export a local Windows system inventory that replaces the existing PowerShell collector.

| Job | Door | v1.0 |
|---|---|---|
| Collect | CIM / System.Management, System.Net.NetworkInformation, System.Diagnostics, Registry, X509Store on the local machine | Base system summary first. Full catalog in SectionsCatalog.md. Later PRs add sections. |
| Organize | Read-only grids / expandable sections / summary cards | Display hostname, domain, OS, hardware basics. |
| Export | Folder of UTF-8 text files (or single workbook) matching the script layout | Summary + collected sections. |

Not in this version: Remote multi-machine scan. Change-control audit. Compliance scoring. Live monitoring. PowerShell execution.

---

## 2. Window

### 2.1 Chrome

| Piece | Rule |
|---|---|
| Shell | `SysInfoIqWindow`: File / View menu, HorizontalTab page strip, `VestigiumShell` with `ShowNav=False`, `VestigiumStatusBar`. |
| Icon | `Assets/SysInfoIQ.ico` on the window and the exe (to be created). |
| Placement | Cold start centers on the **primary** work area. |
| Theme | `ThemeManager` in `OnStartup` before the window parses. Palettes via `ThemeCatalog`. |
| DI | `AddVestigiumControls` + `AddVestigiumConverters`. |
| Pages | **SysInfo** · **Settings** · **Help**. Selected tab is the page title. No duplicate heading. |
| Top nav | `RadioButton.HorizontalTab`, group `SysInfoIqMainNav`. All three enabled. |
| Child tabs | Settings inner tabs (if any) use the same HorizontalTab style, indented. |
| View menu | Status-bar visible. Status-bar Top / Bottom. **Theme** submenu: one check glyph on the active palette. |
| Status bar | Left = collection progress or Idle. Center = time. Detail = elapsed or section count. Persist visible + dock. |

### 2.2 SysInfo page

| Field | Rule |
|---|---|
| Collect | Button starts the inventory. One in-flight job. Cancel available. |
| Progress | Status bar shows current section. |
| Summary | Always visible card whose text matches the locked block in SectionsCatalog.md (Hostname through Inventory Location, including Domain Role and OS Product Type labels). |
| Sections | Expandable or tabbed list of collected sections (initially Summary + Base System). Later: Hardware, Storage, Network, Users/Security, Software, Services, Events, Drivers, etc. |
| Results | Read-only. No editing. |

### 2.3 Settings

Pages (indented HorizontalTabs as needed): **General** | **Theme**.

| Page | Fields |
|---|---|
| General | Output folder (default: Desktop or script-equivalent). Sections to collect (checkboxes, default all for later PRs; PR01 locks base). Elevate note. |
| Theme | Palette combo. Status-bar visible. Status-bar dock. |

### 2.4 Help

Static content matching DnsIQ HelpView style. Covers: purpose, elevation, log location, export location, what is collected, known limitations (local only, no remote).

---

## 3. Persist

`C:\ProgramData\Vestigium\Settings\Diagnostics\SysInfoIQ\settings.json`

Theme id, output folder, status-bar visible, status-bar dock, selected sections (when added).

Load after ThemeManager initialize. Missing or corrupt → defaults. Create the directory. No secrets.

---

## 4. Behavior

| # | Rule |
|---|---|
| S1 | `HostLog.Initialize(HostIds.SysInfoIQ)` first, then Themes, then DI, then load settings, then the window. |
| S2 | MVVM. Code-behind does not call CIM or file I/O. |
| S3 | One in-flight collection. |
| S4 | Collection uses managed .NET (System.Management for Win32_*/MSFT_*, System.Net.NetworkInformation for adapters/TCP, System.Diagnostics for processes/services/events, Registry/X509Store where appropriate). Fallbacks documented in SectionsCatalog.md. |
| S5 | Elevation recommended but not required for base summary. Document what is incomplete without admin. |
| S6 | Export writes UTF-8 text files named `{Hostname}_{Section}.txt` into a timestamped folder under the configured output path. Include a manifest. |
| S7 | Failed section writes an error file instead of failing the whole run. |
| S8 | No PowerShell process. No `powershell.exe` / `pwsh.exe` / `wmic.exe` invocation. Pure managed code. |

---

## 5. Acceptance (v1.0 is done when)

1. Window opens with SysInfo / Settings / Help tabs matching DnsIQ chrome.
2. Logging appears under `C:\ProgramData\Vestigium\Logs\SysInfoIQ` after first run.
3. Collect button gathers base system summary and displays it.
4. Settings persist theme, bar, and output folder.
5. Help page renders.
6. Export produces a folder with at least the summary file.
7. Host tests for input validation and APPID are green. No live CIM required for unit tests.

---

## 6. Out of v1.0

Full script parity (all sections from SectionsCatalog.md). Remote targets. Scheduled collection. Excel workbook export (text first). msinfo32.exe launch. Charts.

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 14 Oct 2026 | Foundation. Base summary + chrome + logging + export skeleton. SectionsCatalog.md added for full inventory organization and .NET mapping. |
