# SysInfoIQ — PR03 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Fill the Security tab with real local data in themed multi-column grids. Collect on the existing load path. Failed section continues.

**Not:** Software, Processes, Events. Not export. Not query. Not PowerShell. Not remote.

---

## Starting point

PR02 closed. Collect on load. System, Storage, and Network tabs are live. Security tab is an empty Field/Value grid. Status names the section. Failed section continues.

---

## Decision

One PR. Four build slices plus owner gate.

| Call | Why |
|---|---|
| Profiles, Rules, Certificates, Drivers | Matches SectionsCatalog Security / Firewall. Four sections, one tab. |
| Cap rules at 1000, drivers at 500 | Samples are large. Cap and note, same as ARP/TCP. |
| Certificates from LocalMachine stores | X509Store is managed. My, Root, CA, TrustedPublisher cover the sample. |
| One grid per section inside the tab | Same chrome as Storage and Network. |
| Same load path | Owner removed buttons. Do not bring them back. |
| Pure tests | No live CIM or cert store in the test project. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| All four remaining tabs in one PR | Too big. Security only. |
| netsh advfirewall | Owner said no external process. |
| Every certificate store | LocalMachine My/Root/CA/TrustedPublisher is enough for rev 1. |
| Export these sections now | Export is not default. Park it. |

---

## Library doors this slice may call

- `MSFT_NetFirewallProfile` and `MSFT_NetFirewallRule` in `root\StandardCimv2` via System.Management
- `Win32_PnPSignedDriver` via System.Management
- `System.Security.Cryptography.X509Certificates.X509Store`
- Existing status bar, `DataGrid.Compact`

---

## Types to land

| Type | File | Role |
|---|---|---|
| `SecurityCollector` | `ViewModels/SecurityCollector.cs` | Profiles, Rules, Certificates, Drivers. Returns row lists. |
| Row DTOs | `ViewModels/Rows/SecurityRows.cs` | One type per section. |
| `MainViewModel` | existing | Calls the collector on the load path. Binds the grids. |
| Section picker | `Views/SysInfoView.xaml` | Inside Security: Profiles, Rules, Certificates, Drivers. |

---

## Window map

SysInfo page, category tabs unchanged.  
Security: section tabs (Profiles | Rules | Certificates | Drivers) over a themed grid.  
Status bar: current section, then Idle.

---

## Behavior

### Collect

1. Page load starts one run (existing).
2. System, Storage, Network (existing).
3. Security sections. Fill Security rows.
4. Status names each section.
5. Section throw → empty grid, continue, note in final status.
6. Cap hit → note in final status.
7. Idle when the run ends.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03.001 | Security collector + row DTOs | Open |
| 2 | PR03.002 | Security grids + section picker | Open |
| 3 | PR03.003 | Status per section, failed section continues | Open |
| 4 | PR03.004 | Pure mapping tests | Open |
| 5 | PR03.005 | Owner runs against Requirements acceptance | Owner |

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/SecurityCollector.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/Rows/SecurityRows.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.SysInfoIQ/Views/SysInfoView.xaml
tests/Vestigium.Suite.Network.Tests/SysInfoSecurityTests.cs (new)
```

When this plan finishes, move the whole `PR03/` folder to `PR-Plans/Completed/PR03/`.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No netsh. Managed doors only. |
| Theodore | Failed section continues. Caps on rules and drivers. Tests off the wire and the cert store. |
| Simon | Columns match the sample files. System, Storage, Network unchanged. |

---

## Next action

PR03.001. Security collector and row DTOs.
