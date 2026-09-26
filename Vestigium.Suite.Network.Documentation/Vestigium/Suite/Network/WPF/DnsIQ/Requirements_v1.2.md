# DnsIQ — Requirements v1.2

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-SRS-001  
**Version:** 1.2  
**Status:** Locked for PR03  
**Date:** 26 September 2026  
**Supersedes (window):** [Requirements_v1.1.md](Requirements_v1.1.md) for persist, Port, combos, Probe prelude, Settings pages, Dashboard charts. 1.0 / 1.1 stay on disk as history.  
**PR delta:** [PR-Plans/PR03/PR03 -- Requirements.md](PR-Plans/PR03/PR03%20--%20Requirements.md)  
**Project:** `Vestigium.Suite.Network.DnsIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `DnsIQ`  
**Protocol:** `Vestigium.Helpers.Network` 1.2.0  
**Chrome:** `Vestigium.Themes`, `Vestigium.Controls`, StatusBar, NumericUpDown, UnderConstruction, Converters  
**Dashboard numbers:** `Vestigium.Helpers.Analytics` 1.0.1, `Vestigium.Helpers.Charts` 1.0.1  
**Binding:** Helpers.Network wins on protocol. Charts paints. Analytics computes. This file wins on the window.

---

## 1. What ships

One host window (`DnsIqWindow` + `VestigiumShell`). Three pages. Two jobs. Persist knobs. Dashboard that paints after a run.

| Job | Door | v1.2 |
|---|---|---|
| Lookup | `NetworkHelper.LookupAsync` | Writes the answer grid. |
| Probe | One Lookup, then host pulse of `LookupAsync` | Prelude fills the grid. Pulse does not append rows. |
| `LookupManyAsync` | Helpers | Out. One name. |

---

## 2. Window

### 2.1 Chrome

| Piece | Rule |
|---|---|
| Shell | `DnsIqWindow`: File / View menu, `VestigiumShell` pages, `VestigiumStatusBar`. |
| Theme | `ThemeManager` in `OnStartup` before the window parses. All palettes in `Vestigium.Themes` 1.0.2. |
| DI | `AddVestigiumControls` + `AddVestigiumConverters`. |
| Pages | **DnsIQ** · **Dashboard** · **Settings**. |
| View menu | Status-bar visible. Status-bar Top / Bottom. **No** nav-indent items. |
| Status bar | Left = `sent/total` during pulse (or Idle / rcode). Center = time rail during pulse. Detail = elapsed. Persist visible + dock. |

### 2.2 DnsIQ page

| Field | Rule |
|---|---|
| Name | Trim. Blank → `localhost`. Watermark `(localhost)`. Not persisted. |
| Server | Combo (this-PC DNS + public list). Editable IP. Empty = first adapter DNS. Persist. |
| Port | NumericUpDown **between Server and Type**. Default 53. Min 1. Max 65535. `DnsLookupOptions.Port`. Persist. |
| Type | Default All. Allowed: All, A, AAAA, CNAME, MX, NS, PTR, TXT, SOA. Persist. |
| Interface | Closed combo. `GetAdapters()` + **Any (0)**. Display name + index. Not editable. Persist index. |
| Lookup / Probe / Cancel | One token. |
| Results | Read-only grid. Type, Name, Data, Ttl. Written by Lookup and by Probe **prelude** only. |

Not on this row: Requests, Seconds, Source.

### 2.3 Settings

Two pages, top-left under the heading: **Theme** | **Probe**.

| Page | Fields |
|---|---|
| Theme | Palette. Status-bar visible. Status-bar dock. |
| Probe | **Requests**, **Seconds**, Port, Source. |

Labels are Requests and Seconds (no N/X).

Requests default 1000, min 1, max 10000.  
Seconds default 60, min 1, max 600.

**Source:** closed combo on Probe only. Items = **Any** + `GetAdapters()` → `UnicastAddresses` (show address + adapter name). Not editable. Empty / Any = do not set `SourceAddress`. Specific Interface narrows the list to that adapter. Persist address; if missing at load → Any. Not spoofing.

### 2.4 Dashboard

Inner tabs **Lookup** | **Probe**.

| Tab | After a run | Before a run |
|---|---|---|
| Lookup | Type mix (pie or column) from last grid. Optional TTL. | Empty card: run Lookup on the DnsIQ page. |
| Probe | RTT curve + histogram (bell / KDE). Control chart if Analytics returns fences. | Empty card: run Probe on the DnsIQ page. |

No ScottPlot types in the host. Paint after the pulse ends.

---

## 3. Persist

`C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`

Theme id, Server, Type, Interface index, Port, Requests, Seconds, bar visible, bar dock, Source (or Any).

Not Name. Not pulse samples.

Load after ThemeManager initialize. Missing or corrupt file → defaults. Create the directory. No secrets.

---

## 4. Behavior

| # | Rule |
|---|---|
| D1 | `HostLog.Initialize(HostIds.DnsIQ)` first, then Themes, then DI, then load settings, then the window. |
| D2 | MVVM. Code-behind does not call `NetworkHelper`. |
| D3 | One in-flight job. |
| D4 | Wire peer is the queried server. |
| D5 | Failed **Lookup** (button or prelude) clears the grid. |
| D6 | Probe: (1) same Lookup as the button; on failure stop; (2) pulse N lookups over X seconds; no extra rows. |
| D7 | Pulse timing: request 1 at t=0, request N at t=X when N>1. Spacing = `X/(N-1)`. Slip if late. Type All cycles the eight types. |
| D8 | NxDomain = answered for pulse totals. Timeout / Refused are counts only. |
| D9 | No `nslookup.exe`. No HTTP DNS. |
| D10 | Charts paints. Analytics computes. |

---

## 5. Acceptance (v1.2 is done when)

1. Restart restores theme, server, type, port, Requests, Seconds, interface, Source, bar visible, bar dock. Name is not restored.
2. Port sits between Server and Type. Queries use it.
3. Interface and Source are closed combos from `GetAdapters`. No typing.
4. Requests / Seconds / Source only on Settings → Probe.
5. Probe fills the grid then pulses. Failed Lookup does not pulse.
6. View menu has no nav indent. Bar visible + dock persist.
7. Dashboard Lookup shows a type mix after Lookup. Dashboard Probe shows curve + histogram after a pulse.
8. Host tests off the wire are green.

---

## 6. Out of v1.2

Live curve during pulse. Pulse history file. CSV. DoH. AXFR. PropertiesGrid. Other hosts. Spoofed Source.

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. |
| 1.1 | 25 Sep 2026 | Chrome. Pulse N over X. Dashboard UC. |
| 1.2 | 26 Sep 2026 | Persist. Port. Combos. Probe prelude. Settings pages. Dashboard charts. |
