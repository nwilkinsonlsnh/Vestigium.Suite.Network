# PR03 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR03-REQ  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**Status:** Draft  
**Date:** 26 September 2026  
**Product papers (unchanged until this PR lands):** [Requirements_v1.1.md](../../Requirements_v1.1.md), [Design_v1.1.md](../../Design_v1.1.md)

This file is the **definition of done for PR03**. It is not a new product specification. When PR03 is accepted, fold these deltas into Requirements_v1.2 / Design_v1.2 beside the other product papers, then move this folder to `Completed/PR03/`.

Baseline is the PR02 exe: Vestigium window, Lookup grid, pulse N lookups over X seconds, Settings Theme / Probe, Dashboard still a placeholder.

---

## Source (what it is)

**Not spoofing.** Spoofing is putting a source IP on the wire that this machine does not own. DnsIQ will not do that.

**Source** is an optional **local bind address**. This PC may have several IPv4/IPv6 addresses (Ethernet, Wi-Fi, VPN). Empty / **Any** = Windows picks. A picked address = bind the UDP socket to that address before the query leaves, the same idea as `ping -S 192.168.1.20`.

| Field | Question |
|---|---|
| Server | Which resolver do we ask? |
| Interface | Which NIC / index? |
| Source | Which of *this PC's* addresses does the packet leave from? |

Addresses come only from `NetworkHelper.GetAdapters()` → `UnicastAddresses`. The user cannot type an IP. If it is not on the list, it is not on this machine.

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
| Type | |
| Interface index | |
| Port | |
| Requests | |
| Seconds | |
| Status-bar **visible** | |
| Status-bar **dock** (Top / Bottom) | |
| Source address (or Any) | |

Load after `ThemeManager.Initialize`. Missing file = current defaults (Requests=1000, Seconds=60, Port=53, Type=All, adapter DNS, LightBlue, bar visible Bottom, Source = Any). Create the directory if needed. No secrets.

If a persisted Source is no longer on the adapter list at load, fall back to Any.

### R03-02 Port on the query row and in Settings

- Control: `VestigiumNumericUpDown`.
- Place on DnsIQ: **between Server and Type**.
- Default 53. Range 1–65535. Unsigned.
- Feeds `DnsLookupOptions.Port`.
- Settings → Probe also has Port as the default that seeds the DnsIQ page.

### R03-03 Interface is a closed combo

- Replace the Interface text box on the DnsIQ page.
- Items: `NetworkHelper.GetAdapters()` plus **Any (0)**.
- Display: adapter name + index.
- Selected value: interface index.
- **Not editable.** User picks. They do not type.

### R03-04 Source is a closed combo on Settings → Probe

- Control: ComboBox, **not editable**. Not a text box.
- Items: **Any** plus every `UnicastAddress.Address` from `NetworkHelper.GetAdapters()`.
- Display: address, and adapter name when useful (`192.168.1.20  Ethernet`).
- Selected value: that address, or empty for Any.
- Feeds `DnsLookupOptions.SourceAddress` only when not Any.
- Lives **only** on Settings → Probe. Not on the DnsIQ row.
- When Interface is a specific NIC, the Source list **narrows** to that adapter's unicast addresses plus Any. When Interface is Any (0), the list is every adapter.
- Loopback may appear if the library reports it; do not invent extras.

### R03-05 Settings layout and Probe page

Settings keeps two pages, top-left under the heading: **Theme** | **Probe**.

`TabControl.Standard` must not center the form.

**Theme:** palette, status-bar visible, status-bar dock.

**Probe:** Requests, Seconds, Port, Source.

Labels are **Requests** and **Seconds**. Drop `(N)` and `(X)`.

Requests and Seconds are **not** on the DnsIQ Lookup row. Probe reads the Settings values (and the persisted file).

### R03-06 DnsIQ query row

**Name | Server | Port | Type** then **Interface**. Lookup / Probe / Cancel.

No Requests, Seconds, or Source on that row.

### R03-07 Probe starts with one Lookup

Probe is two steps, one token:

1. Run the same Lookup as the Lookup button (Type All = eight types). Fill the answer grid. Failed Lookup clears the grid and **does not** start the pulse.
2. If that Lookup finishes without cancel, run the pulse (N lookups over X seconds). Pulse does **not** append rows. Last Lookup rows stay.

Status: Lookup line first, then pulse `sent/total` + elapsed as today.

### R03-08 View menu

Remove **Increase nav indent** and **Decrease nav indent**.

Keep status-bar visible and Top / Bottom. Those two persist (R03-01).

### R03-09 Dashboard has Lookup and Probe tabs

Remove the single Under Construction page as the only Dashboard content.

| Tab | After a run | Before a run |
|---|---|---|
| Lookup | Last Lookup: type mix (pie or column). Optional TTL strip. | Short empty card: run Lookup on the DnsIQ page. |
| Probe | Last pulse: RTT curve + distribution. | Short empty card: run Probe on the DnsIQ page. |

### R03-10 Probe dashboard uses Analytics + Charts

Pinned packages only: `Vestigium.Helpers.Analytics` 1.0.1, `Vestigium.Helpers.Charts` 1.0.1.

| View | Source |
|---|---|
| Curve | `ChartView.Line` or `Scatter` — sample index vs RTT ms, answered only |
| Shape | `ChartView.Histogram` with bell / KDE |
| Control chart | `NumericSeries.ControlLimits(MovingRange)` then `ChartView.Control` if Analytics returns fences; hide if it refuses |

DnsIQ does not reference ScottPlot. Charts paints. Analytics computes.

Timeouts and refused stay **counts**. They are not RTT points.

Paint **after** the pulse ends. Live-follow is not required to close PR03.

### R03-11 Lookup dashboard is smaller than Probe

Type-count pie or column from the last grid. Optional TTL column/box. No fake live tail.

---

## Must not change in PR03

- Helpers.Network protocol surface
- Pulse timing math (N over X, slip, cycle types)
- Spoofing a source IP this machine does not own
- CSV export, DoH, AXFR, PropertiesGrid
- Pulse history across process restarts
- Other suite hosts

---

## Acceptance

1. Change theme, server, type, port, Requests, Seconds, interface, Source, bar visible, bar dock. Exit. Start. Same values. Name is not restored. Dead Source address becomes Any.
2. Port 53 sits between Server and Type. Lookup/Probe use that port.
3. Interface combo lists NICs. Typing is impossible.
4. Source combo lists unicast addresses from `GetAdapters` plus Any. Typing is impossible. Settings → Probe only.
5. Requests / Seconds are on Settings → Probe only, top-left. DnsIQ row does not show them.
6. Probe fills the grid with one Lookup, then pulses. Pulse does not add rows. Failed Lookup does not pulse.
7. View menu has no nav-indent items. Bar visible + dock survive restart.
8. Dashboard Lookup and Probe tabs exist. After Lookup, a type mix. After Probe, a curve and a histogram.

---

## Parked (not required to close PR03)

- Live curve while Probe runs
- Last-pulse JSON next to settings
- Per-type colour on the curve
- Loss sparkline on the status bar
