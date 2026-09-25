# TraceIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-TRACEIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.TraceIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `TraceIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol (trace family, bind, TCP fallback). This file wins on the window.

First and ten is “show the path.” One target. One walk. Hop list. Cancel. Pathping waits for second down.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| Trace | `NetworkHelper.IcmpTrace(target, IcmpTraceOptions)` / `Trace` alias | **Required.** |
| Pathping | `NetworkHelper.Pathping(...)` | **Out.** Walk + sample is a second window-worth of UX. |

---

## 2. Window

| Field | Rule |
|---|---|
| Target | Required. Trim. Blank → reject. Placeholder `127.0.0.1`. |
| Max hops | Default **30**. Range 1–64. |
| Probes per hop | Default **1**. Range 1–10. |
| Family | All / IPv4 / IPv6. Default All. |
| Interface index | Optional. Empty or `0` = not pinned. Negative → reject. Do not rewrite `0` to `1`. |
| Source address | Optional. Must parse if set. |
| Trace | Starts the walk. Disabled while running. |
| Cancel | Cancels the token. Partial hops stay. |
| Status | Idle / Running / Success / TimedOut / Failed / Cancelled. Reached yes/no. Settled protocol (ICMP / UDP / TCP). |
| Hops | One row per hop: TTL, address or `*`, name if PTR filled, probe statuses. |
| Error | Exception or reject on the status line. |

One form + one hop list. No RTT chart. No pathping sample columns.

---

## 3. Behavior

| # | Rule |
|---|---|
| T1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.TraceIQ)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\TraceIQ\`. |
| T2 | MVVM. Code-behind does not call `NetworkHelper`. |
| T3 | Options pass MaxHops, ProbesPerHop, Family, InterfaceIndex, SourceAddress. TcpPort stays at the library default unless we add a box later. |
| T4 | One in-flight job. |
| T5 | Library may fall ICMP → UDP → TCP. The host shows the settled protocol. It does not pick the fallback itself. |
| T6 | PTR miss stays empty. TCP mid-path hops may stay `*`. |
| T7 | Progress may add hops as they arrive. If the job only completes as a block, fill the list at the end. Either is acceptable. |
| T8 | No `tracert`, no `traceroute`, no `pathping.exe`. |
| T9 | Charts unused this release. |
| T10 | No echo campaign, no DNS, no route write. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Blank target does not start. |
3. Trace `127.0.0.1` with MaxHops 8 returns a hop list or a terminal status without throwing out of the UI. |
4. Negative interface index is rejected before send. |
5. Cancel stops a running walk and keeps hops already shown. |
6. No Pathping button. No sample-loss columns.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| Pathping | Second phase UX (samples, hop loss, link loss). |
| Prefer UDP checkbox | Library option exists; not Tuesday. |
| TcpPort box | Default 80/443 later. |
| Per-probe RTT chart | After the hop grid works. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.TraceIQ`, `BindFields` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Trace walk only. |
