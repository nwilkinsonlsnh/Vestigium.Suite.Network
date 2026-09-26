# DnsIQ — Requirements v1.2

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-SRS-001  
**Version:** 1.2  
**Status:** Live window lock  
**Date:** 26 September 2026  
**Supersedes (window):** [Requirements_v1.1.md](Requirements_v1.1.md) for persist, Port, combos, Probe prelude, Settings pages, Dashboard charts. 1.0 / 1.1 stay on disk as history.  
**PR delta:** [PR-Plans/PR03/PR03 -- Requirements.md](PR-Plans/PR03/PR03%20--%20Requirements.md)  
**Project:** `Vestigium.Suite.Network.DnsIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `DnsIQ`  
**Protocol:** `Vestigium.Helpers.Network` 1.2.0  
**Chrome:** `Vestigium.Themes` 1.0.2, `Vestigium.Controls` 1.0.0, StatusBar, NumericUpDown 1.0.1, UnderConstruction, Converters  
**Dashboard numbers:** `Vestigium.Helpers.Analytics` 1.0.1, `Vestigium.Helpers.Charts` 1.0.5  
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
| Shell | `DnsIqWindow`: File / View menu, HorizontalTab page strip, `VestigiumShell` with `ShowNav=False`, `VestigiumStatusBar`. |
| Icon | `Assets/DnsIQ.ico` on the window and the exe. |
| Placement | Cold start centers on the **primary** work area (`SystemParameters.WorkArea`). Not WPF `CenterScreen`. |
| Theme | `ThemeManager` in `OnStartup` before the window parses. Palettes in `Vestigium.Themes` 1.0.2 via `ThemeCatalog`. |
| DI | `AddVestigiumControls` + `AddVestigiumConverters`. |
| Pages | **DnsIQ** · **Dashboard** · **Settings**. The selected tab **is** the page title. No second heading that repeats the tab. |
| Top nav | `RadioButton.HorizontalTab`, group `DnsIqMainNav`. DnsIQ and Settings stay enabled. **Dashboard is disabled until a Probe finishes.** Lookup alone does not unlock it. |
| Child tabs | Settings and Dashboard inner tabs use the same HorizontalTab style, indented (`Margin` 20 on the strip). |
| View menu | Status-bar visible. Status-bar Top / Bottom. **Theme** submenu: one check glyph on the active palette, one selection only. **No** nav-indent items. |
| Status bar | Left = `sent/total` during pulse (or Idle / rcode). Center = time rail during pulse. Detail = elapsed. Persist visible + dock. |
| NumericUpDown | Host style: `TextAlignment=Center`. Theme brushes. |

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

Three pages, indented HorizontalTabs: **DnsIQ** | **Probe** | **Theme**.

| Page | Fields |
|---|---|
| DnsIQ | Port (default that seeds the query row). Source (closed combo). Caption: Source is the local bind address, not spoofing. |
| Probe | **Requests**, **Seconds**. |
| Theme | Palette combo. Status-bar visible. Status-bar dock. Same palette list as View → Theme. |

Labels are Requests and Seconds (no N/X).

Requests default 1000, min 1, max 10000.  
Seconds default 60, min 1, max 600.

**Source:** closed combo on Settings → DnsIQ. Items = **Any** + `GetAdapters()` → `UnicastAddresses` (show address + adapter name). Not editable. Empty / Any = do not set `SourceAddress`. Specific Interface on the query page narrows the list to that adapter. Persist address; if missing at load → Any. Not spoofing.

### 2.4 Dashboard

Inner tabs **Lookup** | **Probe**, indented HorizontalTabs. Empty state is a caption plus a hyperlink back to DnsIQ. No page heading that repeats "Dashboard".

| Tab | After a run | Before a run |
|---|---|---|
| Lookup | Type mix (pie) from last grid. Theme-painted. | "No Lookup chart yet." + Open DnsIQ. |
| Probe | RTT curve, histogram (bell / KDE), control chart when Analytics returns fences. Theme-painted. | "No Probe chart yet." + Open DnsIQ. |

No ScottPlot types in the host. Paint after the pulse ends. Chart host menu "Open in New Window" uses Charts 1.0.5: window title = plot title, maximized, surface theme, plot fills the client.

Chart legend on/off persists per slot (Lookup, Probe RTT, Probe dist, Probe control).

---

## 3. Persist

`C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`

Theme id, Server, Type, Interface index, Port, Requests, Seconds, bar visible, bar dock, Source (or Any), four chart-legend flags.

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
| D11 | Dashboard nav stays disabled until Probe completes (`DashboardViewModel.Unlock`). |

---

## 5. Acceptance (v1.2 is done when)

1. Restart restores theme, server, type, port, Requests, Seconds, interface, Source, bar visible, bar dock, chart legends. Name is not restored.
2. Port sits between Server and Type. Queries use it. Settings → DnsIQ Port is the default seed.
3. Interface and Source are closed combos from `GetAdapters`. No typing.
4. Requests / Seconds only on Settings → Probe. Source only on Settings → DnsIQ.
5. Probe fills the grid then pulses. Failed Lookup does not pulse.
6. View menu has no nav indent. View → Theme shows one check on the live palette. Bar visible + dock persist.
7. Cold start is centered on the primary monitor. Window and exe show `DnsIQ.ico`.
8. Top strip is HorizontalTab. Child strips are indented. No duplicate page titles.
9. Dashboard stays locked until Probe finishes. Lookup tab shows a type mix after Lookup. Probe tab shows curve + histogram after a pulse. Detached chart window takes the plot title and the theme.
10. Host tests off the wire are green.

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
| 1.2 | 26 Sep 2026 | Chrome lock: HorizontalTab nav, Dashboard gated on Probe, Settings DnsIQ/Probe/Theme, View → Theme check, primary-monitor center, icon, centered NUD, Charts 1.0.5 detached window, no duplicate titles. |
