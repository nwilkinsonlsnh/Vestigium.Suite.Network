# RouteIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.RouteIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `RouteIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol (locks 11, 29, 34). This file wins on the window.

First and ten is print the table and ask one neighbor. Mutate is second down. Default route is never a button.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| Print routes | `NetworkHelper.GetRoutes()` | **Required.** On open + Refresh. |
| Print neighbors | `NetworkHelper.GetNeighbors()` | **Required.** Second list on the same window. |
| Probe one | `NetworkHelper.ProbeNeighbor(address)` | **Required.** One address box. |
| Add / change / remove | Option C write | **Out.** Second down. Needs admin UX and deny rules on the form. |
| Default `0.0.0.0/0` or `::/0` write | Denied by library | **Never.** No button. No checkbox. |

---

## 2. Window

| Piece | Rule |
|---|---|
| Family | All / IPv4 / IPv6. Default All. Passes `RouteFamily` if the get-routes door accepts it; otherwise filter the list in the host. |
| Refresh | Reloads routes and neighbors. |
| Route list | Destination/prefix, mask if present, gateway, interface name/index, metric, protocol. |
| Neighbor list | Address, MAC, interface, state. |
| Probe address | Required for Probe. Must parse as IPv4 or IPv6. |
| Probe | Runs `ProbeNeighbor`. Disabled while running or when the box is blank. |
| Status | Idle / Running / Failed + last probe line (address + MAC or empty). |
| Error | Exception or `NetworkRouteDenied` text. No crash. |

One window. Two lists. No write form this release.

---

## 3. Behavior

| # | Rule |
|---|---|
| R1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.RouteIQ)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\RouteIQ\`. |
| R2 | MVVM. Code-behind does not call `NetworkHelper`. |
| R3 | Interface index `0` on any later write form is “stack chooses.” Never rewrite to `1`. First ten has no write form. |
| R4 | Probe is one address. It does not scan the neighbor table as the primary path (library lock 48 / 45). |
| R5 | Failed print leaves the last lists and shows the error. |
| R6 | No `route.exe`, no `ip`, no `netsh`, no `arp` spawn. |
| R7 | IPv6 print is required when the stack has rows. Empty IPv6 list is allowed. |
| R8 | Charts unused. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Refresh fills route and neighbor lists without throwing out of the UI (empty lists allowed). |
3. Probe with a blank address does not start. |
4. Probe `127.0.0.1` returns a result line or Failed — not an unhandled exception. |
5. No Add / Change / Remove / Default-route control on the window.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| Option C add/change/remove | Admin + deny UX. Own paper. |
| Default-route write | Library lock 34. Permanent non-goal. |
| Neighbor flush-one | Privileged. Not this down. |
| Persist route set | Config management. Not this host’s first job. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.RouteIQ` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Print + one neighbor probe. |
