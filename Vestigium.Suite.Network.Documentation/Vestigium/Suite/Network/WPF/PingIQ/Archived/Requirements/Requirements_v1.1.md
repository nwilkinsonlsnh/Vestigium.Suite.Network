# PingIQ — Requirements v1.1

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-SRS-001  
**Version:** 1.1  
**Status:** Live window lock  
**Date:** 26 September 2026  
**Supersedes (window):** [Requirements_v1.0.md](Requirements_v1.0.md) for chrome, persist, combos, Probe pulse, Settings pages, Dashboard charts. 1.0 stays on disk as first-and-ten history.  
**PR delta:** [PR-Plans/PR01/PR01 -- Requirements.md](PR-Plans/PR01/PR01%20--%20Requirements.md)  
**Project:** `Vestigium.Suite.Network.PingIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `PingIQ` (never `Network`)  
**Protocol:** `Vestigium.Helpers.Network` 1.2.0  
**Chrome:** `Vestigium.Themes` 1.0.2, `Vestigium.Controls` 1.0.0, StatusBar, NumericUpDown 1.0.1, UnderConstruction, Converters  
**Dashboard numbers:** `Vestigium.Helpers.Analytics` 1.0.1, `Vestigium.Helpers.Charts` 1.0.5  
**Binding:** Helpers.Network wins on protocol (§2.4 duration/interval, bind honesty, `IcmpEchoOptions` floors). Charts paints. Analytics computes. This file wins on the window.

Rev 1 is DnsIQ’s finished window, pointed at ICMP. Four-echo first-and-ten is still true for the Echo button. Probe, persist, and charts are in because the chrome and packages already shipped on DnsIQ.

---

## 1. What ships

One host window (`PingIqWindow` + `VestigiumShell`). Three pages. Two jobs. Persist knobs. Dashboard that paints after a Probe.

| Job | Door | v1.1 |
|---|---|---|
| Echo | `NetworkHelper.IcmpEcho(target, IcmpEchoOptions)` / `Ping` alias | Writes the reply grid. Count from the query row. |
| Probe | One Echo, then host pulse of `IcmpEcho` with `Count = 1` | Prelude fills the grid. Pulse does not append rows. |
| PathMtu | `PathMtu` | Out. |
| UdpProbe | `UdpProbe` | Out. |
| Campaigns | `CreateEchoCampaign` / recipe + JSONL | Out. Scheduler is not this release. |

---

## 2. Window

### 2.1 Chrome

Copy DnsIQ Rev 1 chrome. Do not invent a second shell.

| Piece | Rule |
|---|---|
| Shell | `PingIqWindow`: File / View menu, HorizontalTab page strip, `VestigiumShell` with `ShowNav=False`, `VestigiumStatusBar`. |
| Icon | `Assets/PingIQ.ico` on the window and the exe. |
| Placement | Cold start centers on the **primary** work area (`SystemParameters.WorkArea`). Not WPF `CenterScreen`. |
| Theme | `ThemeManager` in `OnStartup` before the window parses. Palettes in `Vestigium.Themes` 1.0.2 via `ThemeCatalog`. |
| DI | `AddVestigiumControls` + `AddVestigiumConverters`. |
| Pages | **PingIQ** · **Dashboard** · **Settings**. The selected tab **is** the page title. No second heading that repeats the tab. |
| Top nav | `RadioButton.HorizontalTab`, group `PingIqMainNav`. PingIQ and Settings stay enabled. **Dashboard is disabled until a Probe finishes.** Echo alone does not unlock it. |
| Child tabs | Settings and Dashboard inner tabs use the same HorizontalTab style, indented (`Margin` 20 on the strip). |
| View menu | Status-bar visible. Status-bar Top / Bottom. **Theme** submenu: one check glyph on the active palette, one selection only. **No** nav-indent items. |
| Status bar | Left = `sent/total` during pulse (or Idle / job status). Center = time rail during pulse. Detail = elapsed. Persist visible + dock. |
| NumericUpDown | Host style: `TextAlignment=Center`. Theme brushes. |

### 2.2 PingIQ page

| Field | Rule |
|---|---|
| Target | Trim. Blank → reject, do not start a job. Placeholder `127.0.0.1`. Name or address allowed; the library resolves. Not persisted. |
| Count | NumericUpDown. Default **4**. Min 1. Max 60. Feeds `IcmpEchoOptions.Count` on Echo. Persist. Count `0` (continuous) is **out**. |
| Timeout | NumericUpDown **between Count and Interface**. Default 4000 ms. Min 10. Max 60000. Feeds `IcmpEchoOptions.Timeout`. Persist. |
| Interface | Closed combo. `GetAdapters()` + **Any (0)**. Display name + index. Not editable. Persist index. |
| Echo / Probe / Cancel | One token. |
| Results | Read-only grid. Sequence, Status, Address, RttMs, Ttl, Detail. Written by Echo and by Probe **prelude** only. |
| Summary | Sent, received, lost, loss %, min / max / avg ms when the result has them. Written with the grid. |

Not on this row: Requests, Seconds, Source, TTL, Buffer, DF.

### 2.3 Settings

Three pages, indented HorizontalTabs: **PingIQ** | **Probe** | **Theme**.

| Page | Fields |
|---|---|
| PingIQ | Timeout (default that seeds the query row). Source (closed combo). Caption: Source is the local bind address, not spoofing. |
| Probe | **Requests**, **Seconds**. |
| Theme | Palette combo. Status-bar visible. Status-bar dock. Same palette list as View → Theme. |

Labels are Requests and Seconds (no N/X).

Requests default 1000, min 1, max 10000.  
Seconds default 60, min 1, max 600.

**Source:** closed combo on Settings → PingIQ. Items = **Any** + `GetAdapters()` → `UnicastAddresses` (show address + adapter name). Not editable. Empty / Any = do not set `SourceAddress`. Specific Interface on the query page narrows the list to that adapter. Persist address; if missing at load → Any. Not spoofing.

### 2.4 Dashboard

Inner tabs **Echo** | **Probe**, indented HorizontalTabs. Empty state is a caption plus a hyperlink back to PingIQ. No page heading that repeats "Dashboard".

| Tab | After a run | Before a run |
|---|---|---|
| Echo | Status mix (pie) from last grid. Theme-painted. | "No Echo chart yet." + Open PingIQ. |
| Probe | RTT curve, histogram (bell / KDE), control chart when Analytics returns fences. Theme-painted. | "No Probe chart yet." + Open PingIQ. |

No ScottPlot types in the host. Paint after the pulse ends. Chart host menu "Open in New Window" uses Charts 1.0.5: window title = plot title, maximized, surface theme, plot fills the client.

Chart legend on/off persists per slot (Echo mix, Probe RTT, Probe dist, Probe control).

---

## 3. Persist

`C:\ProgramData\Vestigium\Settings\Diagnostics\PingIQ\settings.json`

Theme id, Count, Timeout ms, Interface index, Requests, Seconds, bar visible, bar dock, Source (or Any), four chart-legend flags.

Not Target. Not pulse samples. Not reply rows.

Load after ThemeManager initialize. Missing or corrupt file → defaults. Create the directory. No secrets.

---

## 4. Behavior

| # | Rule |
|---|---|
| P1 | `HostLog.Initialize(HostIds.PingIQ)` first, then Themes, then DI, then load settings, then the window. JSONL under `%ProgramData%\Vestigium\Logs\PingIQ\`. |
| P2 | MVVM. Code-behind does not call `NetworkHelper`. |
| P3 | One in-flight job. Echo and Probe share the token. Second click ignored until finish or cancel. |
| P4 | Options pass `Count` (Echo) or `Count = 1` (each pulse shot), `Timeout`, `InterfaceIndex`, `SourceAddress`. Leave Buffer, Ttl, DontFragment, Interval, MaxDuration, AllowBurst, StatsPath at library defaults unless this file names them. |
| P5 | Unbound echo may use BCL `Ping`. Bound echo uses the library bound path. `ProtocolForbidden` is a result, not a crash. |
| P6 | Bind miss is Failed with the library detail. The host surfaces it. |
| P7 | Echo progress may update the list as replies arrive if the job reports progress. If not, fill the list when `RunAsync` returns. Either is acceptable. |
| P8 | No `ping.exe`. No `pathping.exe`. |
| P9 | Charts paints. Analytics computes. PingIQ does not call `ChartView` until a series exists. |
| P10 | No trace, DNS, pathping, or route UI. |
| P11 | Failed **Echo** (button or prelude) clears the grid and summary. |
| P12 | Probe: (1) same Echo as the button; on failure or cancel stop; (2) pulse N echoes (`Count = 1`) over X seconds; no extra rows. |
| P13 | Pulse timing: request 1 at t=0, request N at t=X when N>1. Spacing = `X/(N-1)`. Slip if late. Do not overlap. Do not queue. |
| P14 | Success / DestinationUnreachable / TtlExpired with an RTT enter the Probe series. TimedOut / ProtocolForbidden / Failed are counts only. Timeouts are not 0 ms points. |
| P15 | Dashboard nav stays disabled until Probe completes (`DashboardViewModel.Unlock`). |
| P16 | Cancel stops the token. Status = Cancelled. Prelude rows that already landed stay. Pulse rows were never appended. |
| P17 | No converters authored in the exe. Use `Vestigium.Converters`. |

---

## 5. Acceptance (v1.1 is done when)

1. Restart restores theme, Count, Timeout, Requests, Seconds, interface, Source, bar visible, bar dock, chart legends. Target is not restored.
2. Timeout sits between Count and Interface. Echo/Probe use it. Settings → PingIQ Timeout is the default seed.
3. Interface and Source are closed combos from `GetAdapters`. No typing.
4. Requests / Seconds only on Settings → Probe. Source only on Settings → PingIQ.
5. Echo `127.0.0.1` count 4 returns a summary without throwing out of the UI (Forbidden on a locked box is a pass if Status shows it).
6. Probe fills the grid then pulses. Failed Echo does not pulse.
7. View menu has no nav indent. View → Theme shows one check on the live palette. Bar visible + dock persist.
8. Cold start is centered on the primary monitor. Window and exe show `PingIQ.ico`.
9. Top strip is HorizontalTab. Child strips are indented. No duplicate page titles.
10. Dashboard stays locked until Probe finishes. Echo tab shows a status mix after Echo. Probe tab shows curve + histogram after a pulse. Detached chart window takes the plot title and the theme.
11. Blank target does not start a job. Negative interface cannot be typed (combo only).
12. Cancel stops a running Echo or pulse.
13. Host tests off the wire are green.
14. No PathMtu button, no UdpProbe button, no campaign picker, no TTL/Buffer/DF row.

---

## 6. Out of v1.1

| Item | Why later |
|---|---|
| Count = 0 continuous | Needs duration + interval UI and library §2.4. |
| PathMtu | Separate result shape. |
| UdpProbe | Different status enum. |
| Campaign recipe persist | JSON + clock window. Wrong door for this pulse. |
| TTL / buffer / DF checkboxes | Useful, not this lock. Library defaults. |
| Live curve during pulse | Paint after the pulse ends. |
| Pulse history file / CSV | After the series is honest. |
| Other suite hosts | Untouched. |

---

## 7. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.PingIQ` |
| Protocol | `Vestigium.Helpers.Network` 1.2.0 |
| Chrome | Themes 1.0.2 + Controls / StatusBar / UnderConstruction 1.0.0 + NumericUpDown 1.0.1 + Converters 1.0.0 |
| Dashboard | Analytics 1.0.1 + Charts 1.0.5 |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Four-echo window. |
| 1.1 | 26 Sep 2026 | Rev 1 lock. DnsIQ chrome. Persist. Combos. Probe prelude + N over X. Settings PingIQ/Probe/Theme. Dashboard charts. |
