# PR03 — Requirements

**Document ID:** VEST-SUITE-NETWORK-SYSINFOIQ-PR03-REQ  
**Host:** `Vestigium.Suite.Network.SysInfoIQ`  
**Status:** Open  
**Date:** 14 October 2026  
**Product papers:** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md), [SectionsCatalog.md](../../SectionsCatalog.md)

This file is the definition of done for PR03. The Security tab gets real data. System, Storage, and Network stay as PR01/PR02 left them.

Baseline: PR02 closed. Collect on load. Security tab is an empty themed grid. No PowerShell. No automatic export.

---

## Must land

### PR03.001 Security collect

- Collect on the same load path. Failed section continues.
- Sections:
  - FirewallProfiles — `MSFT_NetFirewallProfile` (root\StandardCimv2). Domain, Private, Public. Enabled state, default inbound/outbound.
  - FirewallRules — `MSFT_NetFirewallRule`. Cap at 1000 enabled rules. Note truncation.
  - Certificates — `X509Store` for LocalMachine My, Root, CA, TrustedPublisher. Thumbprint, subject, issuer, not-after, has private key.
  - SignedDrivers — `Win32_PnPSignedDriver`. Device name, driver version, is signed, signer, driver date. Cap at 500.
- No PowerShell. No netsh. No wmic.exe.

### PR03.002 Security grid

- Security tab binds multi-column `DataGrid.Compact`, one grid per section.
- Section picker: **Profiles | Rules | Certificates | Drivers**
- Columns match the useful fields from the samples. Not every CIM property.
- Empty result is an empty grid, not an error.
- Truncated sections note the cap in the status bar.

### PR03.003 Status and failure

- Status bar names the section: `Security / Profiles`, `Security / Rules`, and so on.
- A failed section does not abort the run. Its grid stays empty. The final status lists the failed section.
- Collect still runs on load. No buttons return.

### PR03.004 Tests

- Pure tests for row shape, cap constants, and the failure list.
- No live CIM. No live certificate store.

---

## Must not change in PR03

- System, Storage, Network tabs.
- Settings and Help pages.
- Other hosts.
- Software, Processes, Events sections.
- Automatic export.
- Remote collection.
- Excel.
- Query box.

---

## Acceptance

1. Open the app. System, Storage, and Network still fill.
2. Security tab shows Profiles, Rules, Certificates, and Drivers with real local data.
3. Firewall rules and signed drivers respect their caps. Status notes the cap.
4. A failed section does not blank the others.
5. Status bar names the section. Idle when done.
6. Tests green. No live CIM.

---

## Parked

- Software tab (installed software, hotfixes, features, ODBC, env, SQL).
- Processes tab (processes, services, tasks, shares).
- Events tab (7-day errors, SystemInformation, drivers).
- Query / filter.
- Explicit export of Security.
- Elevation prompt.
- Remote targets.
