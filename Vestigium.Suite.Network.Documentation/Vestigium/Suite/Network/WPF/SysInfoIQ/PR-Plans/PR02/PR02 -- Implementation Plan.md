# SysInfoIQ — PR02 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PLAN-PR02  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**APPID:** `SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. This file wins on the slice.

**Goal:** Fill the Storage and Network category tabs with real local data in themed multi-column grids. Collect on the existing load path. Failed section continues.

**Not:** Security, Software, Processes, Events. Not export. Not query. Not PowerShell. Not remote.

---

## Starting point

PR01 closed. Collect on load. System tab is a Field/Value `DataGrid.Compact`. Storage and Network tabs are empty grids of the same style. `Collector.CollectBase` returns the five system sections. `Exporter` exists and is not called.

---

## Decision

One PR. Six build slices plus owner gate.

| Call | Why |
|---|---|
| Storage then Network | Storage is pure CIM. Network mixes NetworkInformation and CIM. Do the simpler first. |
| One grid per section inside the tab | Sample files are different shapes. One shared Field/Value grid hides the structure. A section picker keeps the chrome quiet. |
| Useful columns only | Full WMI dumps are noise. Match the sample fields. |
| Cap ARP and TCP | Busy machines produce large tables. Cap with a note in status if truncated. |
| Same load path | Owner removed buttons. Do not bring them back. |
| Pure tests | No live CIM in the test project. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| One giant Field/Value grid for Storage | Loses the table shape. Later query needs columns. |
| Collect Storage/Network on tab switch only | Load already collects System. Keep one run. |
| Invoke ipconfig / netstat | Owner said no. |
| Export these sections now | Export is not default. Park it. |

---

## Library doors this slice may call

- `System.Management.ManagementObjectSearcher` for DiskDrive, LogicalDisk, DiskPartition, Volume, IP4RouteTable
- `System.Net.NetworkInformation.NetworkInterface` and `IPGlobalProperties`
- P/Invoke `GetIpNetTable` only if CIM ARP is incomplete
- Existing `HostLog`, status bar, `DataGrid.Compact`

---

## Types to land

| Type | File | Role |
|---|---|---|
| `StorageCollector` | `ViewModels/StorageCollector.cs` | DiskDrives, LogicalDisks, Partitions, Volumes. Returns row lists. |
| `NetworkCollector` | `ViewModels/NetworkCollector.cs` | IP, DNS, NICs, Routes, ARP, Listeners, TCP. Returns row lists. |
| Row DTOs | `ViewModels/Rows/` | One type per section. Properties match grid columns. |
| `MainViewModel` | existing | Calls the new collectors on the load path. Binds the grids. |
| Section picker | `Views/SysInfoView.xaml` | Inside Storage and Network, a HorizontalTab or combo selects the section grid. |

---

## Window map

SysInfo page, category tabs unchanged.  
Storage: section tabs (Disks | Logical | Partitions | Volumes) over a themed grid.  
Network: section tabs (IP | DNS | NICs | Routes | ARP | Listeners | TCP) over a themed grid.  
Status bar: current section, then Idle.

---

## Behavior

### Collect

1. Page load starts one run (existing).
2. System sections first (existing).
3. Storage sections. Fill Storage rows.
4. Network sections. Fill Network rows.
5. Status names each section.
6. Section throw → error row or empty grid, continue.
7. Idle when the run ends.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR02.001 | Storage collector + row DTOs | Open |
| 2 | PR02.002 | Storage grids + section picker | Open |
| 3 | PR02.003 | Network collector + row DTOs | Open |
| 4 | PR02.004 | Network grids + section picker | Open |
| 5 | PR02.005 | Status per section, failed section continues | Open |
| 6 | PR02.006 | Pure mapping tests | Open |
| 7 | PR02.007 | Owner runs against Requirements acceptance | Owner |

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/StorageCollector.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/NetworkCollector.cs (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/Rows/** (new)
src/Vestigium.Suite.Network.SysInfoIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.SysInfoIQ/Views/SysInfoView.xaml
tests/Vestigium.Suite.Network.Tests/SysInfoStorageTests.cs (new)
tests/Vestigium.Suite.Network.Tests/SysInfoNetworkTests.cs (new)
```

When this plan finishes, move the whole `PR02/` folder to `PR-Plans/Completed/PR02/`.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No PowerShell. No netstat. No ipconfig. Managed doors only. |
| Theodore | Failed section continues. Caps on ARP and TCP. Tests off the wire. |
| Simon | Columns match the sample files. Sizes are GB to two decimals. System tab unchanged. |

---

## Next action

PR02.001. Storage collector and row DTOs.
