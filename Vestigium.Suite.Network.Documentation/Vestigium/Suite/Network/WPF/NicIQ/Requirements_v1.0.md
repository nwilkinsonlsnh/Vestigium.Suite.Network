# NicIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.NicIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `NicIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol. This file wins on the window.

First and ten is “which NIC, is it up, how fast.” One list. One selected adapter. One watch. No packet send. No plot required.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| List adapters | `NetworkHelper.GetAdapters()` | **Required.** Default on open. |
| One adapter | `NetworkHelper.GetAdapter(...)` | **Required** when the list has a selection. |
| Box identity | `NetworkHelper.GetWorkstation()` | **Required.** Host name + domain on the header. |
| Watch | `NetworkHelper.WatchAdapter(...)` | **Required.** One adapter, one duration. |
| Counters | `NetworkHelper.SampleCounters(...)` | **Out of first ten.** Same host later. Do not hide a second engine in this window. |

---

## 2. Window

| Piece | Rule |
|---|---|
| Header | Workstation host name and domain from `GetWorkstation`. Captured time optional. |
| Refresh | Reloads `GetAdapters()`. Enabled when idle. |
| Include down | Checkbox. Default **on**. Passes through to the query if the library accepts `NetworkAdapterQuery.IncludeDown`. |
| List | One row per adapter: Name, Status, Type, MAC, Speed (bits/s or blank), Id. |
| Detail | Selected row: Description, unicast addresses, gateways, DNS servers, DHCP server/lease when present, NetBIOS-over-TCP enum. |
| Duration | Watch duration. Default **10 s**. Floor 1 s. Ceiling 60 s this release. |
| Watch | Runs watch on the **selected** adapter. Disabled with no selection or while a watch is running. |
| Cancel | Cancels the watch token. |
| Status | Idle / Running / Failed / Cancelled plus last watch outcome (oper-status + speed if the library returns them). |
| Error | Exception text on the status line. No required message box. |

One window. No tabs. No chart control this release. Speed is a number on the row, not a plot.

---

## 3. Behavior

| # | Rule |
|---|---|
| N1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.NicIQ)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\NicIQ\`. |
| N2 | MVVM. Code-behind does not call `NetworkHelper`. |
| N3 | Field names match the library: `Status`, not `OperationalStatus`. Speed is `SpeedBitsPerSecond`. |
| N4 | Index `0` is not a NIC. Selection is by `Id` / `Name` from the list. The host does not invent IfIndex `1`. |
| N5 | Watch is one adapter and one duration. It does not bill. It does not plot. |
| N6 | One in-flight watch. Refresh during watch is allowed to be disabled. |
| N7 | Failed inventory is a status line, not a crash. Clear or keep the last list — pick **keep last list**, show the error. |
| N8 | No `Get-NetAdapter`, no `ipconfig`, no `ethtool` spawn. |
| N9 | Charts package may sit on Shell. NicIQ first-and-ten does not call `ChartView`. |
| N10 | No echo, trace, DNS, or route mutate from this exe. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Refresh fills a list without throwing out of the UI (empty list is allowed on a locked-down box). |
3. Header shows a host name from `GetWorkstation`. |
4. Selecting a row fills detail (addresses may be empty). |
5. Watch with no selection does not start. |
6. Watch for 1–10 s on a selected up adapter returns a status line (up/down/speed or Failed). |
7. Cancel stops a running watch. |
8. No counter button, no chart, no send-packet button.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| `SampleCounters` | Second job. Needs a duration story and usually a series. |
| Live sparkline / Charts | After counters exist. |
| Rename / disable / DHCP renew | Mutate. Not this library door. |
| Bind-to-this-NIC helper for PingIQ | Wrong host. Copy the Id by hand. |
| Wireless SSID / BSSID extras | Not on `NetworkAdapter`. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.NicIQ` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. List + detail + watch. |
