# PR02 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR02-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md), [SectionsCatalog.md](../../SectionsCatalog.md)

This file is the definition of done for PR02. Storage and Network category grids get real data. System stays as PR01 left it.

Baseline: PR01 closed. Collect on load. System tab is a Field/Value grid. Storage and Network tabs are empty themed grids. No PowerShell. No automatic export.

---

## Must land

### PR02.001 Storage collect

- Collect on the same load path as System. Failed section continues.
- Sections:
  - DiskDrives — `Win32_DiskDrive`
  - LogicalDisks — `Win32_LogicalDisk`
  - Partitions — `Win32_DiskPartition` (or MSFT_Partition if the namespace is available)
  - Volumes — `Win32_Volume` (or MSFT_Volume)
- No PowerShell. No wmic.exe.

### PR02.002 Storage grid

- Storage tab binds a multi-column `DataGrid.Compact`.
- One grid, or one grid per section with a section picker. Default: one grid per section, selectable inside the tab.
- Columns match the useful fields from the sample files (model, size, free, filesystem, letter, interface, serial, offset, type). Not every WMI property.
- Sizes shown as GB to two decimals where the sample used bytes.
- Empty result is an empty grid, not an error.

### PR02.003 Network collect

- Same load path. Failed section continues.
- Sections:
  - IPConfiguration — `NetworkInterface.GetAllNetworkInterfaces()` + `GetIPProperties`
  - DNSConfiguration — per-adapter `DnsAddresses`
  - NetworkInterfaceCards — `NetworkInterface` (MAC, speed, status, description)
  - Routes — `IPGlobalProperties` or CIM `MSFT_NetRoute` / `Win32_IP4RouteTable`
  - ARPTable — CIM or P/Invoke `GetIpNetTable`. Cap or page if the table is large.
  - ListeningPorts — `IPGlobalProperties.GetActiveTcpListeners()` + UDP + process name where available
  - TCPConnections — `IPGlobalProperties.GetActiveTcpConnections()` + owning process where available
- No PowerShell. No netstat.exe. No wmic.exe.

### PR02.004 Network grid

- Network tab binds themed multi-column grids, one per section, selectable inside the tab.
- Columns match the useful fields from the samples (address, mask, gateway, DNS, MAC, speed, state, local, remote, PID, process).
- Loopback and tunnel adapters are included but marked. Not filtered out.
- Empty result is an empty grid.

### PR02.005 Status and failure

- Status bar names the section being collected.
- A failed section does not abort the run. The grid for that section shows a single error row or stays empty with the status noting the failure.
- Collect still runs on load. No buttons return.

### PR02.006 Tests

- Pure tests for row mapping (byte → GB, adapter filter labels, route row shape).
- No live CIM. No live sockets.

---

## Must not change in PR02

- System tab shape and fields.
- Settings and Help pages.
- Other hosts.
- Security, Software, Processes, Events sections.
- Automatic export.
- Remote collection.
- Excel.
- Query box (the grid is the surface; filter lands later).

---

## Acceptance

1. Open the app. System grid fills as before.
2. Storage tab shows DiskDrives, LogicalDisks, Partitions, Volumes with real local data.
3. Network tab shows the seven sections with real local data.
4. A failed section does not blank the others.
5. Status bar names the section. Idle when done.
6. Tests green. No live CIM.

---

## Parked

- Query / filter on the grids.
- Explicit export of Storage and Network.
- Security, Software, Processes, Events (later PRs).
- Elevation prompt.
- Remote targets.
