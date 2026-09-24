# Shell — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-SHELL-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable library  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.Shell`  
**Kind:** WPF class library, `net10.0-windows`. Not an exe.  
**Library consume:** `Vestigium.Helpers.Network` 1.2.0, `Vestigium.Logging` 1.7.1, Analytics 1.0.1, Charts 1.0.1  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol. This file wins on shared chrome.

First and ten is three types every host can call on startup. No second Network API. No theme pack yet.

---

## 1. What ships

| Type | Role | First-and-ten |
|---|---|---|
| `HostIds` | APPID string constants | **Required.** |
| `HostLog` | Initialize logger + register Network catalog | **Required.** |
| `BindFields` | Observable `InterfaceIndex` + `SourceAddress` | **Required.** |
| Theme / window chrome XAML | Shared look | **Out.** Hosts keep their own `MainWindow`. |
| Bind picker UserControl | NIC dropdown | **Out.** Second down. |

---

## 2. HostIds

| Constant | Value |
|---|---|
| `PingIQ` | `PingIQ` |
| `TraceIQ` | `TraceIQ` |
| `DnsIQ` | `DnsIQ` |
| `NicIQ` | `NicIQ` |
| `RouteIQ` | `RouteIQ` |
| `ProbeHost` | `ProbeHost` |
| `ShareIQ` | `ShareIQ` |

No constant named `Network`. Hosts pass these into `HostLog.Initialize`. They do not invent a second string.

---

## 3. HostLog.Initialize(appId)

| Rule | Detail |
|---|---|
| Reject | Blank `appId` throws `ArgumentException`. |
| Directory | `%ProgramData%\Vestigium\Logs\{appId}\` created if missing. |
| Logger | `VestigiumLogger.Initialize` with `cfg.AppId = appId` and that directory. |
| Catalog | `NetworkCatalog.Register(cfg)` inside the same initialize. |
| Analytics catalog | Optional this release. Do not require it for first ten. |
| Sockets | None. |

Call once per process, before the main window shows. Hosts do not call `VestigiumLogger.Initialize` themselves.

---

## 4. BindFields

| Property | Type | Rule |
|---|---|---|
| `InterfaceIndex` | `int` | Default `0`. Meaning: not pinned. Never coerce to `1`. |
| `SourceAddress` | `string?` | Default null/empty. Host validates parse before send. |

Observable (`CommunityToolkit.Mvvm`). Shell does not call `EgressBind.Apply`. The host copies these onto the library options object.

---

## 5. What Shell must not grow

| Ban | Why |
|---|---|
| `IPingService` / `ITraceService` / wrap of `NetworkHelper` | The library is the façade. |
| Sockets, echo loops, DNS wire | Wrong project. |
| Scheduler | PingIQ / ShareIQ later, not Shell. |
| `ChartView` helper that every host must use | Charts stay on the host that has a series. Package ref on Shell is allowed; a required chart API is not. |
| Default-route or port-sweep helpers | Library non-goals. |

---

## 6. Acceptance

1. Every exe calls `HostLog.Initialize(HostIds.<ThatHost>)` on startup. |
2. `HostIdsTests` still asserts APPIDs are not `Network`. |
3. `BindFields` default index is `0`. |
4. No public method on Shell starts a Network job.

---

## 7. Out of first and ten

| Item | Why later |
|---|---|
| Shared `BindPanel` UserControl | After two hosts copy-paste the same two boxes. |
| Theme dictionary | After the first window looks finished. |
| StatusBar / CommandStrip from Vestigium WPF controls | Separate consume story. |

---

## 8. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF library |
| Architecture | MVVM types only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. HostIds, HostLog, BindFields. |
