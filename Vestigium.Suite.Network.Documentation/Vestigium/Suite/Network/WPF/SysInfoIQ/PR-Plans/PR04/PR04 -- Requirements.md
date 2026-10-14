# PR04 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR04-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md), [SectionsCatalog.md](../../SectionsCatalog.md)

This file is the definition of done for PR04. The Software tab gets real data. System, Storage, Network, and Security stay as earlier PRs left them.

Baseline: PR03 closed. Collect on load. Software tab is an empty themed grid. No PowerShell. No automatic export. Query bar is parked.

---

## Must land

### PR04.001 Software collect

- Collect on the same load path. Failed section continues.
- Sections:
  - InstalledSoftware — registry Uninstall keys. Four opens:
    - `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` → LM, x64
    - `HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` → LM, x86
    - `HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` → CU, x64
    - `HKCU\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall` → CU, x86
    - Location and Arch come from which key was opened, not from a product field.
    - Name, version, publisher, install date.
    - Prefer registry over `Win32_Product`.
  - Hotfixes — `Win32_QuickFixEngineering`. HotFixID, description, installed on, installed by.
  - Features — `MSFT_OptionalFeature`. Name, state. Enabled and disabled both. Cap at 500.
  - ODBC — drivers and DSNs from the same four hives:
    - `HKLM\SOFTWARE\ODBC` → LM, x64
    - `HKLM\SOFTWARE\WOW6432Node\ODBC` → LM, x86
    - `HKCU\SOFTWARE\ODBC` → CU, x64
    - `HKCU\SOFTWARE\WOW6432Node\ODBC` → CU, x86
    - Location and Arch from the key. Kind is Driver or DSN.
  - Environment — `Environment.GetEnvironmentVariables` for Process, User, and Machine. Name, value, scope.
  - SQL — `ServiceController` filtered to MSSQL* and SQL* names. Name, display, status, start type.
- No PowerShell. No wmic.exe. No `Win32_Product`.

### PR04.002 Software grid

- Software tab binds multi-column `DataGrid.Compact`, one grid per section.
- Section picker: **Installed | Hotfixes | Features | ODBC | Environment | SQL**
- Installed columns start with **Location** (LM / CU) then **Arch** (x64 / x86).
- ODBC columns start with **Location** then **Arch** then **Kind** (Driver / DSN).
- Other columns match the useful fields from the samples. Not every registry value.
- Empty result is an empty grid, not an error.
- Truncated sections note the cap in the status bar.

### PR04.003 Status and failure

- Status bar names the section: `Software / Installed`, `Software / Hotfixes`, and so on.
- A failed section does not abort the run. Its grid stays empty. The final status lists the failed section.
- Collect still runs on load. No buttons return.

### PR04.004 Tests

- Pure tests for row shape, the hive-to-location map, the hive-to-arch map, cap constants, and the failure list.
- No live registry. No live services.

---

## Must not change in PR04

- System, Storage, Network, Security tabs.
- Settings and Help pages.
- Other hosts.
- Processes and Events sections.
- Automatic export.
- Remote collection.
- Excel.
- Query bar.

---

## Acceptance

1. Open the app. System, Storage, Network, and Security still fill.
2. Software tab shows Installed, Hotfixes, Features, ODBC, Environment, and SQL with real local data.
3. Installed rows show LM or CU and x64 or x86 from the key that produced them.
4. ODBC rows show the same Location and Arch, plus Driver or DSN.
5. Environment shows Process, User, and Machine.
6. A failed section does not blank the others.
7. Status bar names the section. Idle when done.
8. Tests green. No live registry.

---

## Parked

- Processes tab (processes, services, tasks, shares).
- Events tab (7-day errors, SystemInformation, drivers).
- Query bar / KQL.
- Explicit export of Software.
- Elevation prompt.
- Remote targets.
