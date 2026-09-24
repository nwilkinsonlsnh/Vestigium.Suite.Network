# PingIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.PingIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `PingIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol (§2.4 duration/interval, bind honesty). This file wins on the window.

First and ten is “is that host answering.” Four echoes. A list of replies. Cancel. Bind optional. Campaigns and PathMtu wait for second down.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| Echo | `NetworkHelper.IcmpEcho(target, IcmpEchoOptions)` / `Ping` alias | **Required.** The window. |
| PathMtu | `PathMtu` | **Out.** Second down. |
| UdpProbe | `UdpProbe` | **Out.** Second down. |
| Campaigns | recipe + JSONL | **Out.** Second down. Scheduler is not this release. |

---

## 2. Window

| Field | Rule |
|---|---|
| Target | Required. Trim. Blank → reject, do not start a job. Placeholder `127.0.0.1`. Name or address allowed; the library resolves. |
| Count | Integer. Default **4**. Minimum 1 this release. Count `0` (continuous) is **out** of first ten. |
| Timeout | Optional. Empty = library default. If set, must sit in the library 10 ms–60 s window. |
| Interface index | Optional. Empty or `0` = not pinned. Negative → reject. Do not rewrite `0` to `1`. |
| Source address | Optional. Empty = not pinned. If set, must parse as an IP. |
| Echo | Starts the job. Disabled while running. |
| Cancel | Cancels the token. Status = Cancelled. Partial replies stay on the list. |
| Status | Idle / Running / Success / TimedOut / Failed / Cancelled / ProtocolForbidden. |
| Summary | Sent, received, lost, min / max / avg ms when the result has them. |
| Replies | One row per echo: sequence, status, address, RTT ms, detail. |
| Error | Exception or reject text on the status line. |

One form + one list. No chart this release. Analytics may compute nothing until Charts is an explicit second-down item.

---

## 3. Behavior

| # | Rule |
|---|---|
| P1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.PingIQ)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\PingIQ\`. |
| P2 | MVVM. Code-behind does not call `NetworkHelper`. |
| P3 | Options pass `Count`, `Timeout` when set, `InterfaceIndex`, `SourceAddress`. |
| P4 | One in-flight job. Second Echo click is ignored until finish or cancel. |
| P5 | Unbound echo may use BCL `Ping`. Bound echo uses the library bound path. ProtocolForbidden is a result, not a crash. |
| P6 | Bind miss is Failed with the library detail. The host surfaces it. |
| P7 | Progress may update the list as replies arrive if the job reports progress. If not, fill the list when `RunAsync` returns. Either is acceptable this release. |
| P8 | No `ping.exe`. |
| P9 | Charts package may sit on Shell. PingIQ first-and-ten does not call `ChartView`. |
| P10 | No trace, DNS, pathping, or route UI. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Blank target does not start a job.
3. Echo `127.0.0.1` count 4 returns a summary without throwing out of the UI (Forbidden on a locked box is a pass if Status shows it).
4. Negative interface index is rejected before send.
5. Cancel stops a running echo. |
6. No PathMtu button, no UdpProbe button, no campaign picker, no chart.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| Count = 0 continuous | Needs duration + interval UI and §2.4. |
| PathMtu | Separate result shape. |
| UdpProbe | Different status enum. |
| Campaign recipe persist | JSON + clock window. |
| RTT chart | After a series is owned on the form. |
| TTL / buffer / DF checkboxes | Useful, not Tuesday. Default library values. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.PingIQ`, `BindFields` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Four-echo window. |
