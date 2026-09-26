# PR03 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR03-REQ  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**Status:** Draft  
**Date:** 26 September 2026  
**Product papers (unchanged until this PR lands):** [Requirements_v1.1.md](../../Requirements_v1.1.md), [Design_v1.1.md](../../Design_v1.1.md)

This file is the **definition of done for PR03**. It is not a new product specification. When PR03 is accepted, fold these deltas into Requirements_v1.2 / Design_v1.2 beside the other product papers, then move this folder to `Completed/PR03/`.

Baseline is the PR02 exe: Vestigium window, Lookup grid, pulse N lookups over X seconds, Settings Theme / Probe, Dashboard still a placeholder.

---

## Must change

### R03-01 Persist session knobs

Write and read:

`C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\`

File: `settings.json`.

| Persist | Do not persist |
|---|---|
| Theme id | Name (the question) |
| Server | Pulse samples / charts |
| Type | Source address |
| Interface index | |
| Port | |
| Requests (N) | |
| Seconds (X) | |
| Status-bar dock | |

Load after `ThemeManager.Initialize`. Missing file = current defaults (N=1000, X=60, Port=53, Type=All, adapter DNS, LightBlue, bar Bottom). Create the directory if needed. No secrets.

### R03-02 Port on the query row and in Settings

- Control: `VestigiumNumericUpDown`.
- Place: **between Server and Type**.
- Default 53. Range 1–65535. Unsigned.
- Feeds `DnsLookupOptions.Port`.
- Settings → Probe also has Port as the default that seeds the DnsIQ page.

### R03-03 Interface is a closed combo

- Replace the Interface text box.
- Items: `NetworkHelper.GetAdapters()` plus **Any (0)**.
- Display: adapter name + index.
- Selected value: interface index.
- **Not editable.** User picks. They do not type.

### R03-04 Settings fields sit top-left

Theme / Probe chrome and the fields under them align top-left under the Settings heading. `TabControl.Standard` must not center the form.

### R03-05 Dashboard has Lookup and Probe tabs

Remove the single Under Construction page as the only Dashboard content.

| Tab | After a run | Before a run |
|---|---|---|
| Lookup | Last Lookup: type mix (pie or column). Optional TTL strip. | Short empty card: run Lookup on the DnsIQ page. |
| Probe | Last pulse: RTT curve + distribution. | Short empty card: run Probe on the DnsIQ page. |

### R03-06 Probe dashboard uses Analytics + Charts

Pinned packages only: `Vestigium.Helpers.Analytics` 1.0.1, `Vestigium.Helpers.Charts` 1.0.1.

| View | Source |
|---|---|
| Curve | `ChartView.Line` or `Scatter` — sample index vs RTT ms, answered only |
| Shape | `ChartView.Histogram` with bell / KDE |
| Control chart | `NumericSeries.ControlLimits(MovingRange)` then `ChartView.Control` if Analytics returns fences; hide if it refuses |

DnsIQ does not reference ScottPlot. Charts paints. Analytics computes.

Timeouts and refused stay **counts**. They are not RTT points.

v1 of this PR paints **after** the pulse ends. Live-follow is not required to close PR03.

### R03-07 Lookup dashboard is smaller than Probe

Type-count pie or column from the last grid. Optional TTL column/box. No fake live tail.

---

## Must not change in PR03

- Helpers.Network protocol surface
- Pulse timing math (N over X, slip, cycle types)
- Lookup still owns the answer grid; Probe still does not write it
- CSV export, DoH, AXFR, PropertiesGrid
- Pulse history across process restarts
- Other suite hosts

---

## Acceptance

1. Change theme, server, type, port, N, X, interface, bar dock. Exit. Start. Same values.
2. Port 53 sits between Server and Type. Lookup/Probe use that port.
3. Interface combo lists NICs. Typing is impossible.
4. Settings Theme / Probe fields are top-left.
5. Dashboard Lookup and Probe tabs exist. After Lookup, a type mix. After Probe, a curve and a histogram.

---

## Parked (not required to close PR03)

- Live curve while Probe runs
- Last-pulse JSON next to settings
- Per-type colour on the curve
- Loss sparkline on the status bar
