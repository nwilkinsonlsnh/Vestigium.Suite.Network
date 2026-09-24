# RouteIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.RouteIQ`

## Window

Toolbar: Family (All / IPv4 / IPv6), Refresh, Probe address, Probe.  
Top list: routes — Destination, PrefixLength, Mask, Gateway, InterfaceName, InterfaceIndex, Metric, Protocol.  
Bottom list: neighbors — Address, MacAddress, InterfaceName, State.  
Status line under the probe box.

No Add / Change / Remove / Default-route controls in XAML. Do not leave collapsed write fields “that we will bind later.”

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.RouteIQ)`. |
| `MainViewModel` | Family, Routes, Neighbors, ProbeAddress, Status, Refresh/Probe commands. |

## Flow

1. Refresh: `GetRoutes()` + `GetNeighbors()`. Filter by Family in the VM if the get door returns All.
2. Probe: parse ProbeAddress. `ProbeNeighbor(address)`. Status = address + MAC or Failed.
3. Print failure keeps last lists.

## Out of this design

Option C write form. Neighbor flush. Saved route sets.
