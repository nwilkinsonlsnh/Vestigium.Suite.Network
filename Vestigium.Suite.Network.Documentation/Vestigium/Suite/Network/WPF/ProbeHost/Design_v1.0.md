# ProbeHost — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.ProbeHost`

## Window

Header: HostName, DomainName, CapturedUtc, Snapshot button.  
Four lists (grid or stacked group boxes):

- Adapters — Name, Status, Type, Mac, Speed
- Routes — Destination/PrefixLength, Gateway, Metric
- Neighbors — Address, Mac, State
- Connections — Protocol, Local, Remote, State, ProcessId — take first **500**, show “showing 500 of N” when N > 500

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.ProbeHost)`. |
| `MainViewModel` | Header fields, four collections, ConnectionCaption, Snapshot command. |

## Flow

1. Construct and Snapshot call `GetSnapshot()` only.
2. Assign `Workstation`, `Routes`, `Neighbors`, `Connections.Take(500)`.
3. Do not call `GetStatistics` or `GetNetBios` this release.

## Out of this design

Filter boxes. Export. Timer refresh. NetBIOS pane.
