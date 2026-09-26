# DnsIQ — Requirements v1.2 (PR03 draft)

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-REQ-012  
**Status:** Draft — PR03  
**Date:** 26 September 2026  
**Supersedes for new work:** 1.1 chrome + pulse locks stay unless this file says otherwise.

PR02 is closed. This file is the product ask for PR03. It is not an implementation plan.

---

## 1. Why PR03

PR02 gave a Vestigium window, Lookup, and a resolver pulse. The next cut is **memory**, **bind truth**, and a **Dashboard that earns the tab**.

---

## 2. Persist (must)

Save and restore, machine-wide:

`C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\`

Suggested file: `settings.json`.

| Field | Notes |
|---|---|
| Theme id | Last `SwitchTheme` |
| Server | Combo address |
| Type | Including All |
| Interface index | From the adapter list |
| Port | UDP 1–65535, default 53 |
| Requests (N) | Pulse |
| Seconds (X) | Pulse |
| Status-bar dock | Top / Bottom |

Do **not** persist Name (the question). Do **not** persist pulse samples in v1.2 unless we later add a history file.

Load on startup after ThemeManager initialize. Save on change (debounce is allowed). Missing file = PR02 defaults (N=1000, X=60, Port=53, Type=All, adapter DNS, LightBlue).

Create the directory if it does not exist. ACL is OS default. No secrets in the file.

---

## 3. DnsIQ page fields (must)

Order on the query row:

**Name | Server | Port | Type**

Then **Interface | Source** as today, except Interface changes shape.

| Field | Control | Rules |
|---|---|---|
| Port | `VestigiumNumericUpDown` | Default 53. Min 1. Max 65535. Unsigned. Between Server and Type. Same value feeds `DnsLookupOptions.Port`. |
| Interface | ComboBox, **not editable** | Items = `NetworkHelper.GetAdapters()` plus a **Any (0)** row. Display name + index. Selected value = interface index. User may pick; they may not type. |
| Source | TextBox | Unchanged. Optional local address. |

Port also lives on Settings → Probe as the default that seeds the DnsIQ page (same as N/X).

---

## 4. Dashboard (must)

Replace the single Under Construction pane with **two inner tabs**:

| Tab | Job |
|---|---|
| Lookup | Last successful Lookup. Record mix + optional TTL / rcode strip. Not a live tail. |
| Probe | Last completed pulse. Time series of answered RTT + a distribution view. |

Empty state: Under Construction **copy is gone**. Use a short “Run Lookup / Probe on the DnsIQ page” card until there is a session result.

### 4.1 Probe charts (first paint)

Use packages already pinned: `Vestigium.Helpers.Analytics` 1.0.1 and `Vestigium.Helpers.Charts` 1.0.1.

| View | How |
|---|---|
| Curve | `ChartView.Line` or `Scatter` of sample index vs RTT ms (answered only). |
| Shape | `ChartView.Histogram` with `ShowBellCurve` / `ShowKde` so the owner sees how tight the resolver is. |
| Optional second | `ChartView.Control` from `NumericSeries.ControlLimits(MovingRange)` if N is large enough for fences. If Analytics rejects the series, hide the control chart. Do not invent UCL. |

Timeouts / refused stay **counts** on the strip (and on the DnsIQ status line). They are not points on the RTT curve.

Charts paints. Analytics computes. DnsIQ does not call ScottPlot.

### 4.2 Lookup charts (first paint)

Keep this smaller than Probe.

| View | How |
|---|---|
| Mix | `ChartView.Pie` or `Column` of answer **Type** counts from the last grid. |
| Optional | TTL column / box if there are answers. |

---

## 5. Out of PR03

- CSV export
- Pulse history across process restarts
- DoH / TCP-only / AXFR
- Changing Helpers.Network protocol
- PropertiesGrid
- Fake live charts while Probe is running (v1.2 paints **after** the pulse ends; live-follow is PR04 if wanted)

---

## 6. Ideas parked (not locks)

- Live curve that appends during the pulse
- Save last pulse JSON next to settings for reopen
- Per-type colour on the curve
- Loss % sparkline on the status bar
- Name watermark persist skip (already skipped)

---

## 7. Acceptance (owner)

1. Kill DnsIQ, change theme / server / type / port / N / X / interface, restart — same values.
2. Port 53 on the glass, between Server and Type; a Lookup to 53 still works.
3. Interface combo lists NICs; typing is impossible.
4. Dashboard Lookup and Probe tabs exist. After a pulse, a curve and a histogram appear. After a Lookup, a type mix appears.
5. Settings Theme / Probe fields sit **top-left** under the Settings heading.
