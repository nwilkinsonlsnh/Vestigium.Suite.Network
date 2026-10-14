# SysInfoIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Draft  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Ship the first usable SysInfoIQ window: chrome matching DnsIQ, logging, settings persist, base system collection and display, text export of the summary and base sections.

**Not:** Full script parity. Not a remote scanner. Not PowerShell. Not Excel. Not charts.

---

## Starting point

No project exists. Empty.

---

## Decision

One PR. Six build slices plus owner gate.

| Call | Why |
|---|---|
| Mirror DnsIQ chrome exactly for the three pages | Owner instruction. HorizontalTab, Settings, Help, status bar, theme. |
| Base only (ComputerSystem, OS, BIOS, Processor, Memory) | Cheapest that proves collect → display → export. Script has 30+ sections; later PRs add them. |
| CIM via System.Management (or Microsoft.Management.Infrastructure) | No PowerShell. No wmic. Standard managed .NET. |
| Text files first | Matches the script output. Excel later if wanted. |
| One in-flight | Same as DnsIQ. |
| Failed section continues | Script behavior. |
| Host tests off the machine | No live WMI in unit tests. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Collect every section in PR01 | Too big. Base proves the pipe. |
| Invoke PowerShell or wmic under the covers | Owner said no. |
| Excel only | Text is the script format and cheaper. |
| Remote targets | Out of v1.0. |

---

## Library doors this slice may call

- `HostLog.Initialize(HostIds.SysInfoIQ)`
- `ThemeManager` / `ThemeCatalog`
- `VestigiumShell` / `VestigiumStatusBar`
- Settings JSON via Helpers or simple System.Text.Json
- CIM queries (local)

---

## Types to land

| Type | File | Role |
|---|---|---|
| `App` | `App.xaml.cs` | HostLog → Themes → DI → settings → window. |
| `SysInfoIqWindow` | `SysInfoIqWindow.xaml` / `.cs` | Chrome only. |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | Collect / Cancel / Summary / Sections. |
| `SettingsViewModel` | `ViewModels/SettingsViewModel.cs` | General + Theme. |
| `SysInfoSettings` / `SysInfoSettingsStore` | `ViewModels/` | DTO + persist. |
| `SysInfoInput` | `ViewModels/SysInfoInput.cs` | Path validation. |
| `Collector` | `ViewModels/Collector.cs` | Base CIM calls. Returns sections. |
| `InventorySection` | `ViewModels/InventorySection.cs` | Name + content. |
| Views | `Views/SysInfoView.xaml`, `SettingsView.xaml`, `HelpView.xaml` | Pages. |

---

## Window map

Top strip: SysInfo | Settings | Help.  
SysInfo: Collect, Cancel, Summary card, sections area.  
Settings: General (output folder), Theme (palette, bar).  
Help: static.  
Status bar: progress / idle.

---

## Behavior

### Collect

1. If busy, ignore.
2. Validate output path via `SysInfoInput`.
3. Create `{Hostname}_Inventory_{yyyyMMdd_HHmmss}` folder.
4. Query the five base classes. Build Summary string.
5. Display. Write files. Write manifest.
6. Section error → error file, continue.
7. Cancel → stop remaining queries.
8. Status bar updates per section.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01.001 | Project scaffold + slnx + App + HostLog | Open |
| 2 | PR01.002 | Window chrome, three pages, status bar, centering | Open |
| 3 | PR01.003 | Settings store + load/save + logging verified | Open |
| 4 | PR01.004 | Collector + Summary + display | Open |
| 5 | PR01.005 | Export folder + manifest + error files | Open |
| 6 | PR01.006 | Host tests (APPID, path reject) | Open |
| 7 | PR01.007 | Owner runs against Requirements §5 | Owner |

---

## Files this plan expects to touch

```
Vestigium.Suite.Network.slnx
src/Vestigium.Suite.Network.SysInfoIQ/** (new)
tests/Vestigium.Suite.Network.Tests/SysInfoIqInputTests.cs (new)
Vestigium.Suite.Network.Documentation/Vestigium/Suite/Network/WPF/SysInfoIQ/** (these docs)
```

When this plan finishes, move the whole `PR01/` folder to `PR-Plans/Completed/PR01/`.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No façade. Chrome matches DnsIQ. Base CIM only. |
| Theodore | Cancel does not throw. Failed section continues. Tests stay off the wire / CIM. |
| Simon | Summary fields match the script exactly. Elevation note is honest. |

---

## Next action

PR01.001. Scaffold the project and wire HostLog.
