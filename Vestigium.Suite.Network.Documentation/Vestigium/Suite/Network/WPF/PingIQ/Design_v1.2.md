# PingIQ — Design v1.2

**Companion:** [Requirements_v1.2.md](Requirements_v1.2.md)  
**Prior maps:** [Archived/Design/Design_v1.1.md](Archived/Design/Design_v1.1.md), [Archived/Design/Design_v1.0.md](Archived/Design/Design_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.PingIQ`  
**Status:** Rev 1 as-built  
**Date:** 27 September 2026

Requirements 1.2 win on the window. This file is the type map of the live exe.

---

## Window

`PingIqWindow` + `VestigiumShell` (`ShowNav=False`)

```
menu     File (Exit)
         View (bar visible, bar Top/Bottom, Theme submenu)
icon     Assets/PingIQ.ico
place    WorkArea center; restored cap 1100×980 fitted to work area
nav      PingIQ | Dashboard | Settings
PingIQ   Target combo (editable MRU)
         Ping, Probe, Cancel
         grid  Seq | Status | Address | RTT (ms) | TTL | Hops | Detail
         Results expander (status, summary, Analytics, gates)
Dashboard  Ping | Probe charts
Settings   PingIQ | Probe | MRU | Theme
status     sent/total | progress | elapsed
```

---

## Types

| Type | Role |
|---|---|
| `App` | Themes → DI → session → window. ComboBoxItem host template. |
| `PingIqWindow` | Chrome, work-area fit, min/max without free grow. |
| `MainViewModel` | Target/MRU, Ping/Probe/Cancel, Replies, summary, gates, dispatcher marshal. |
| `SettingsViewModel` | Four settings pages. Persist on change. MRU CRUD against `Host.Targets`. |
| `DashboardViewModel` | Ping / Probe chart hosts. Unlock after a finished job. |
| `PingIqInput` | Count 1–99, Delay 10–60000, bind. Reply timeout 4000 ms. |
| `PingIqQuery` | Target + `IcmpEchoOptions`. |
| `TargetResolve` | `IsAddress`, `PickIp` from `DnsLookupResult`. |
| `TargetHistory` | Most-recent-first, case-insensitive dedupe, cap 1–25. |
| `PulsePlan` | Requests, DurationMs, Spacing = DurationMs/Requests, DueAt. |
| `PulsePrelude` | First Probe shot must be allowed or the pulse stops. |
| `ProbeGates` | Start / End / Last send / Last packet / Return wait text. |
| `ProbeCalc` | Plan caption only. |
| `PopulationStats` | Success RTT list → `NumericSeries.Full` text + chart points. |
| `HopEstimate` | TTL → hop bucket. |
| `ReplyRow` | Grid row. |
| `AdapterChoices` / `SourceChoices` | Any + adapters / unicast. |
| `PingIqSettings` / `Store` / `Session` | JSON under ProgramData Diagnostics\PingIQ. |

No `IPingService`. No `Process.Start("ping.exe")`. No host-side raw ICMP.

---

## Persist

`%ProgramData%\Vestigium\Settings\Diagnostics\PingIQ\settings.json`

`DurationMs` is the Probe window. `Seconds` is written as DurationMs/1000 so older files still load.

---

## Flow

### Resolve

1. `TargetResolve.IsAddress` → use Target.  
2. Else `NetworkHelper.LookupAsync` A, then AAAA.  
3. Cache IP when ResolveOnce.  
4. Every `IcmpEcho` target is that IP.

### Ping

1. `PingIqInput.TryCreate`.  
2. For i in 1..Count: wait Delay×(i−1), `IcmpEcho` Count=1, append row on UI thread.  
3. Population stats + Dashboard Ping page.

### Probe

1. `PulsePlan.TryCreate(RequestCount, DurationMs)`.  
2. Clock starts. At each DueAt start `IcmpEcho` Count=1 without awaiting the reply.  
3. `Task.WhenAll`.  
4. Gates from clock. Population stats + Dashboard Probe page.

UI collection edits go through `OnUi` / `BeginInvoke`. Do not mutate `Replies` on the pool thread.

---

## Settings Probe math

```
spacing_ms = DurationMs / Requests
DueAt(i)   = spacing_ms × (i − 1)
```

Default: 300 requests, 5000 ms → ≈16.7 ms apart, End gate 5.000 s.

---

## Dashboard

`NumericSeries.From` success RTT after the job ends.  
`ChartView` line / histogram / control.  
Probe page three rows, min height 240, scroll if needed.

---

## Out

TraceIQ owns hops-as-path. PingIQ Hops column is a TTL estimate only.  
Rev 1 is closed. Next change set is a new rev.
