# ProbeHost — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-PROBEHOST-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.ProbeHost`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `ProbeHost` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol. This file wins on the window.

First and ten is a read-only dump for a ticket. Snapshot the box. Show the pieces. Do not send packets. Do not write routes.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| Snapshot | `NetworkHelper.GetSnapshot()` | **Required.** One button. Fills every pane. |
| Connections | `GetConnections` | **Out as a separate button.** Use snapshot.Connections. |
| Statistics | `GetStatistics` | **Out.** Second down if a tech asks for TCP counters. |
| NetBIOS | `GetNetBios` | **Out.** Windows-only extra. Not the first dump. |

---

## 2. Window

| Piece | Rule |
|---|---|
| Header | Workstation host name, domain, captured UTC from the snapshot. |
| Snapshot | Reloads `GetSnapshot()`. Disabled while a snapshot is running (should be fast; still guard). |
| Adapters | Name, Status, Type, MAC, speed, unicast count. |
| Routes | Destination/prefix, gateway, interface, metric. |
| Neighbors | Address, MAC, interface, state. |
| Connections | Protocol, local, remote, state, PID when present. Cap the on-screen list at **500** rows. If the snapshot is larger, show “showing 500 of N.” |
| Status | Idle / Running / Failed. |
| Error | Exception text on the status line. |

One window. Four lists (or four group boxes). No tabs required. No filter box required this release. No export.

---

## 3. Behavior

| # | Rule |
|---|---|
| H1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.ProbeHost)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\ProbeHost\`. |
| H2 | MVVM. Code-behind does not call `NetworkHelper`. |
| H3 | Constructor may run one snapshot. Failed snapshot leaves empty lists + error. |
| H4 | Read-only. No route add/remove. No echo. No DNS. |
| H5 | Field names match the library (`Status` on adapters, `Gateway` on routes). |
| H6 | No `ipconfig`, no `netstat`, no `arp` spawn. |
| H7 | Charts stay unused. |
| H8 | Connection list cap is a UI cap, not a library cap. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Snapshot fills header with a host name without throwing out of the UI.
3. Four lists bind (empty is allowed). |
4. More than 500 connections does not hang the dispatcher — cap applies. |
5. No mutate button. No ping button. No NetBIOS button.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| `GetStatistics` | Extra counters pane. |
| `GetNetBios` | Windows-only; not the dump. |
| Filter / search | After the four lists work. |
| Export JSON / ticket paste | Nice. |
| Auto-refresh timer | Easy to abuse. Manual Snapshot only. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.ProbeHost` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. One snapshot, four lists. |
