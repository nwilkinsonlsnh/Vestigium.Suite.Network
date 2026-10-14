# SysInfoIQ — PR04 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR04  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Fill the Software tab with real local data in themed multi-column grids. Collect on the existing load path. Failed section continues. Location and bitness come from the registry key, not from a product field.

**Not:** Processes, Events. Not export. Not query bar. Not PowerShell. Not `Win32_Product`. Not remote.

---

## Starting point

PR03 closed. Collect on load. System, Storage, Network, and Security are live. Software tab is an empty Field/Value grid. Status names the section. Failed section continues. Grids scroll inside the form.

---

## Decision

One PR. Four build slices plus owner gate.

| Call | Why |
|---|---|
| Installed, Hotfixes, Features, ODBC, Environment, SQL | Matches SectionsCatalog Software / Runtime. Six sections, one tab. |
| Registry for installed software | `Win32_Product` is slow and can trigger MSI repair. Owner said no. |
| Four Uninstall keys | HKLM and HKCU, native and WOW6432Node. Location and Arch are stamped from the key. |
| Same four keys for ODBC | `SOFTWARE\ODBC` and `SOFTWARE\WOW6432Node\ODBC` under HKLM and HKCU. Kind is Driver or DSN. |
| Environment from the three scopes | Process, User, Machine. Scope is a column so the query bar can filter later. |
| SQL by service name | ServiceController filtered to MSSQL* and SQL*. No SQL browser COM. |
| One grid per section inside the tab | Same chrome as Storage, Network, Security. |
| Same load path | Owner removed buttons. Do not bring them back. |
| Pure tests | Hive map and arch map are pure. No live registry. |

### Registry map

| Key | Location | Arch |
|---|---|---|
| `HKLM\SOFTWARE\...` | LM | x64 |
| `HKLM\SOFTWARE\WOW6432Node\...` | LM | x86 |
| `HKCU\SOFTWARE\...` | CU | x64 |
| `HKCU\SOFTWARE\WOW6432Node\...` | CU | x86 |

On a 32-bit OS the native key is still labeled x86. The WOW6432Node key is absent and that open is skipped.

### Rejected alternatives

| Idea | Why out |
|---|---|
| `Win32_Product` | Slow. Can repair MSI. Registry is the door. |
| Guess bitness from a registry value | Unreliable. The key path is the fact. |
| All remaining tabs in one PR | Too big. Software only. |
| Export these sections now | Export is not default. Park it. |
| Query bar now | Owner parked it. Filter later. |

---

## Library doors this slice may call

- `Microsoft.Win32.Registry` for Uninstall and ODBC keys
- `Win32_QuickFixEngineering` via System.Management
- `MSFT_OptionalFeature` in `root\cimv2` or the DISM CIM namespace
- `Environment.GetEnvironmentVariables`
- `System.ServiceProcess.ServiceController`
- Existing status bar, `DataGrid.Compact`

---

## Types to land

| Type | File | Role |
|---|---|---|
| `SoftwareCollector` | `ViewModels/SoftwareCollector.cs` | Installed, Hotfixes, Features, ODBC, Environment, SQL. Returns row lists. |
| Row DTOs | `ViewModels/Rows/SoftwareRows.cs` | One type per section. Installed and ODBC carry Location and Arch. |
| `MainViewModel` | existing | Calls the collector on the load path. Binds the grids. |
| Section picker | `Views/SysInfoView.xaml` | Inside Software: Installed, Hotfixes, Features, ODBC, Environment, SQL. |

---

## Window map

SysInfo page, category tabs unchanged.  
Software: section tabs over a themed grid.  
Installed grid: Location, Arch, then name and version.  
ODBC grid: Location, Arch, Kind, then name.  
Status bar: current section, then Idle.

---

## Behavior

### Collect

1. Page load starts one run (existing).
2. System, Storage, Network, Security (existing).
3. Software sections. Fill Software rows.
4. Status names each section.
5. Section throw → empty grid, continue, note in final status.
6. Cap hit → note in final status.
7. Idle when the run ends.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR04.001 | Software collector + row DTOs | Open |
| 2 | PR04.002 | Software grids + section picker | Open |
| 3 | PR04.003 | Status per section, failed section continues | Open |
| 4 | PR04.004 | Pure mapping tests | Open |
| 5 | PR04.005 | Owner runs against Requirements acceptance | Owner |

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/SoftwareCollector.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/Rows/SoftwareRows.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.SysInfoIQ/Views/SysInfoView.xaml
tests/Vestigium.Suite.Network.Tests/SysInfoSoftwareTests.cs (new)
```

When this plan finishes, move the whole `PR04/` folder to `PR-Plans/Completed/PR04/`.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No `Win32_Product`. Registry and managed doors only. |
| Theodore | Failed section continues. Tests off the registry and the service list. Hive map is pure. |
| Simon | Location and Arch match the key. Earlier tabs unchanged. |

---

## Next action

PR04.001. Software collector and row DTOs.
