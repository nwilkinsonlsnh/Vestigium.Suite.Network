# PR02 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR02-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Closed — accepted  
**Date:** 14 October 2026  
**Closed:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md), [SectionsCatalog.md](../../SectionsCatalog.md)

Storage and Network category grids have real data. System tab unchanged. Security, Software, Processes, Events remain empty by design.

---

## Must land — Done

### PR02.001 Storage collect — Done

DiskDrives, LogicalDisks, Partitions, Volumes via System.Management. Failed section continues.

### PR02.002 Storage grid — Done

Section picker: Disks | Logical | Partitions | Volumes. Themed DataGrid.Compact. Sizes in GB.

### PR02.003 Network collect — Done

IP, DNS, NICs via NetworkInterface. Routes via Win32_IP4RouteTable. ARP via MSFT_NetNeighbor. Listeners and TCP via IPGlobalProperties. Caps on ARP and TCP.

### PR02.004 Network grid — Done

Section picker: IP | DNS | NICs | Routes | ARP | Listeners | TCP.

### PR02.005 Status and failure — Done

Status names the section. Failed section continues. Final status notes failures and caps.

### PR02.006 Tests — Done

Pure mapping tests. No live CIM.

---

## Acceptance — Met

1. System grid fills as before.
2. Storage tab shows the four sections.
3. Network tab shows the seven sections.
4. A failed section does not blank the others.
5. Status bar names the section.
6. Tests green. No live CIM.

---

## Parked (now PR03+)

- Security tab (PR03).
- Software, Processes, Events.
- Query / filter on the grids.
- Explicit export of Storage and Network.
- Elevation prompt.
- Remote targets.
