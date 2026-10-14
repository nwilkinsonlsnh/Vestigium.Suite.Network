# PR03 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR03-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Closed — accepted  
**Date:** 14 October 2026  
**Closed:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md), [SectionsCatalog.md](../../SectionsCatalog.md)

Security tab has real data. System, Storage, and Network unchanged. Software, Processes, Events remain empty by design.

---

## Must land — Done

### PR03.001 Security collect — Done

Profiles and Rules via MSFT_Net*. Certificates via X509Store. Drivers via Win32_PnPSignedDriver and Win32_SystemDriver.

### PR03.002 Security grid — Done

Section picker: Profiles | Rules | Certificates | Drivers.

### PR03.003 Status and failure — Done

Status names the section. Failed section continues. Caps noted.

### PR03.004 Tests — Done

Pure mapping tests. No live CIM.

### Follow-ups landed during acceptance

- Rules query fixed (`SELECT *`, enum labels). All rules, not only enabled.
- Certificates from Local Machine and Current User. Location is the first column.
- Drivers are all PnP plus system drivers, not signed-only. Source is the first column.
- Shell scroll disabled so grids scroll inside the form. Loading overlay while collecting.

---

## Acceptance — Met

1. System, Storage, Network still fill.
2. Security tab shows the four sections with real local data.
3. Caps noted. Failed section does not blank the others.
4. Status names the section.
5. Tests green.

---

## Parked (now PR04+)

- Software tab (PR04).
- Processes, Events.
- Query bar / KQL.
- Explicit export.
- Elevation prompt.
- Remote targets.
