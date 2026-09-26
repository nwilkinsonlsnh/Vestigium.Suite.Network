# DnsIQ — Requirements v1.1

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-SRS-001  
**Version:** 1.1  
**Status:** Locked for PR02  
**Date:** 25 September 2026  
**Supersedes (window):** [Requirements_v1.0.md](Requirements_v1.0.md) for chrome, tabs, and Probe. v1.0 stays the first-and-ten lock.  
**Project:** `Vestigium.Suite.Network.DnsIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `DnsIQ` (never `Network`)  
**Protocol:** `Vestigium.Helpers.Network` 1.2.0  
**Chrome:** `Vestigium.Themes`, `Vestigium.Controls`, `Vestigium.Controls.StatusBar`, `Vestigium.Controls.NumericUpDown`, `Vestigium.Controls.UnderConstruction`, `Vestigium.Converters`  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol. Controls / Themes / Converters win on chrome. This file wins on the window.

---

## 1. What ships

One `VestigiumDefaultWindow`. Three tabs. Two jobs. No second process. No chart.

| Job | Door | v1.1 |
|---|---|---|
| Lookup | `NetworkHelper.LookupAsync` | **Required.** Writes the answer grid. |
| Probe | Host loop of `LookupAsync` (same options) | **Required.** Resolver pulse: N bursts over X seconds. Does **not** write the grid. |
| `ProbeDns` | Helpers | Available. Host may use it. Pulse still must not write the grid. |
| `LookupManyAsync` | Helpers | **Out.** One name. |

Dashboard is a tab that ships **Under Construction**. That is acceptance, not a defect.

---

## 2. Window

### 2.1 Chrome

| Piece | Rule |
|---|---|
| Shell window | `VestigiumDefaultWindow`: menu, client, `VestigiumStatusBar`. |
| Theme | Host calls `ThemeManager` in `OnStartup` before the window parses. At least one palette registered. |
| DI | Host calls `AddVestigiumControls()` in `OnStartup`. |
| Status bar | Dock Bottom unless Settings says Top. Left / Center / Right. |
| Tabs | **DnsIQ** · **Dashboard** · **Settings**. No fourth tab this release. |

### 2.2 DnsIQ tab

| Field | Rule |
|---|---|
| Name | Required. Trim. Blank → reject, do not call the library. Placeholder may be `localhost`. |
| Server | Optional. Empty = first non-loopback adapter DNS (IPv4 first). If set, must parse as IPv4 or IPv6. Garbage → reject before send. |
| Type | Default **All**. Allowed: All, A, AAAA, CNAME, MX, NS, PTR, TXT, SOA. Anything else is out. |
| Interface index | Optional integer. Empty or `0` = not pinned. Negative → reject. Do not rewrite `0` to `1`. |
| Source address | Optional. Empty = not pinned. If set, must parse as an IP. |
| N | Burst count for Probe. NumericUpDown. Default 10. Min 1. Max 60. |
| X | Pulse duration seconds. NumericUpDown. Default 10. Min 1. Max 60. |
| Lookup | One walk. Writes the grid. Disabled while a job is running. |
| Probe | Pulse. Does not write the grid. Disabled while a job is running. |
| Cancel | Cancels the in-flight token. Status becomes Cancelled. Grid unchanged on pulse cancel. |
| Results | Read-only grid. Type, Name, Data, Ttl. Written by Lookup only. Sorted Type then Name. Wire names (AAAA, TXT), not enum ToString. |

No timeout box. No port box. No Rate spinner. Rate is derived (`N / X` bursts per second) and shown when a pulse ends.

### 2.3 Dashboard tab

`VestigiumUnderConstruction`.

| Property | Value |
|---|---|
| Title | Dashboard |
| Subject | Resolver pulse charts |
| Description | Not in this release. Pulse numbers stay on the DnsIQ tab and the status bar. |

No chart host. No fake series.

### 2.4 Settings tab

| Field | Rule |
|---|---|
| Theme | Combo of palettes the host registered. |
| Status bar position | Top or Bottom. Bound to the bar. |
| Default N | Seeds the DnsIQ tab N on first load. |
| Default X | Seeds the DnsIQ tab X on first load. |

Changing N/X on Settings during a pulse does not retarget the in-flight run. No server roster. No NIC picker.

### 2.5 Status bar

| Slot | Idle | Lookup | Pulse |
|---|---|---|---|
| Left | Idle | Running or rcode | `pulse {i}/{N}` |
| Center | last server or blank | resolver IP | resolver IP |
| Right | blank | elapsed ms | this-burst ms; summary at end |

Center is an IP (`172.16.0.5`), not `pi.hole`.

---

## 3. Behavior

| # | Rule |
|---|---|
| D1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.DnsIQ)` first, then Themes, then `AddVestigiumControls`, then the window. JSONL under `%ProgramData%\Vestigium\Logs\DnsIQ\`. |
| D2 | MVVM. ViewModels own state. Code-behind does not call `NetworkHelper`. |
| D3 | One in-flight job. Lookup and Probe share the token. Second click ignored until done or cancel. |
| D4 | Bind fields pass through. Shell does not apply bind. |
| D5 | Wire peer is the queried server (library lock 28). |
| D6 | PTR: send the name as typed. Host does not invent `.in-addr.arpa` unless the library already does. |
| D7 | Per-query timeout is the library default. No timeout box. |
| D8 | Failed **Lookup** clears the grid. Failed or cancelled **Probe** does not. |
| D9 | No `nslookup.exe`, no `dig`, no `Resolve-DnsName`. |
| D10 | Network / Analytics / Charts / Logging stay package references. Charts is not drawn. |
| D11 | Type All on Lookup = eight types, sequential or as already implemented, then merge rows. |
| D12 | Type All on Probe = eight `LookupAsync` **in parallel** per burst. One type timeout does not cancel the other seven. |
| D13 | Pulse timing: burst 1 at t=0. Burst N at t=X when N>1. Spacing = `X / (N - 1)` seconds. N=1 → one burst, no wait. |
| D14 | If a burst overruns the next slot, **slip**: start the next burst when the current one ends. Do not overlap. Do not queue. |
| D15 | Pulse does not append answers to the grid. |
| D16 | Empty Server uses `GetAdapters()` DNS list (skip loopback, IPv4 first). |
| D17 | No converters authored in the exe. Use `Vestigium.Converters`. |

NxDomain on a query counts as **answered** for pulse totals. Timeout and Refused do not. Timeouts do not enter min/med/max as 0.

---

## 4. Acceptance (v1.1 is done when)

1. Window opens themed. APPID log folder exists after first run.
2. Three tabs exist. Dashboard is the Under Construction control.
3. Blank name does not send.
4. `localhost` Lookup returns a status line and does not throw out of the UI.
5. Lookup All writes typed rows (not a single A pile from `GetHostAddresses` when adapter DNS exists).
6. Probe 10 / 10 s updates the bar (`pulse i/10`) and leaves the Lookup grid alone.
7. Cancel stops a running Lookup or pulse.
8. N or X outside 1…60 is rejected before the loop.
9. Negative interface index is rejected before send.
10. No HTTP client, no OUI, no AXFR, no chart on Dashboard.

---

## 5. Out of v1.1

| Item | Why later |
|---|---|
| Dashboard charts / P95 series | Tab is Under Construction on purpose. |
| `LookupManyAsync` | One name. |
| DoH / DoT | Library does not speak HTTPS DNS. |
| AXFR / IXFR | Denied. |
| Helpers pulse `NetworkJob` | Host loop is enough. |
| PropertiesGrid | Settings is four fields. |
| Rate spinner | Derived from N and X. |
| CSV / favorites / history | After the pulse is honest. |
| Other suite hosts | Untouched. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.DnsIQ`, `BindFields` |
| Protocol | `Vestigium.Helpers.Network` 1.2.0 |
| Chrome | Themes + Controls + StatusBar + NumericUpDown + UnderConstruction + Converters |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Lookup + Probe. One name. |
| 1.1 | 25 Sep 2026 | Default window. Three tabs. Probe is N over X. Dashboard UC. Adapter DNS. Type default All. |
