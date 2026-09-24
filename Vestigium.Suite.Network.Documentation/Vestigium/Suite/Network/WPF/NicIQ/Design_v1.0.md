# NicIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.NicIQ`

## Window

Header: workstation host / domain.  
Toolbar: Refresh, Include down, Duration (default 10 s), Watch, Cancel.  
Left: adapter grid — Name, Status, Type, MacAddress, SpeedBitsPerSecond, Id.  
Right: detail text or fields for the selected `NetworkAdapter` (Description, unicasts, gateways, DNS, DHCP, NetBIOS).

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.NicIQ)`. |
| `MainViewModel` | Header, IncludeDown, DurationSeconds, SelectedAdapter, Adapters collection, Detail, Status, Watch/Refresh/Cancel. |

## Flow

1. On construct and Refresh: `GetWorkstation()` for the header, `GetAdapters()` (or query with IncludeDown) for the list.
2. Selection → detail from the same record. `GetAdapter` only if the list row is stale.
3. Watch requires a selection. Duration clamped 1–60. `WatchAdapter` + token.
4. Inventory failure keeps the last list and sets Status.

Use `Status`, never `OperationalStatus`.

## Out of this design

`SampleCounters`. Chart sparkline. Disable/rename NIC.
