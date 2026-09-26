# DnsIQ — PR02a Chrome

**Parent:** [PR02 -- Implementation Plan.md](PR02%20--%20Implementation%20Plan.md)  
**Slices:** PR02-02 · PR02-03 · PR02-04 · PR02-05  
**Date:** 25 September 2026

This file is the facelift. It does not define DNS. Parent locks win.

---

## What you are assembling

`VestigiumDefaultWindow` from `Vestigium.Controls`:

- Top menu
- Empty client → we put a `TabControl` in it
- `VestigiumStatusBar` already attached. Default dock **Bottom**. Bind dock to Settings.

There is no sample exe in the Controls repo. DnsIQ *is* the sample.

---

## PR02-02 — packages and startup

1. Add version properties to `Directory.Build.props` (`VestigiumThemesVersion`, `VestigiumControlsVersion`, `VestigiumConvertersVersion`). Start at what nuget restore accepts (Controls / Converters papers say 1.0.0; Themes README says 1.0.2). If restore fails, pin what is actually packed — do not copy source.
2. PackageReference those packages plus:
   - `Vestigium.Controls.StatusBar`
   - `Vestigium.Controls.NumericUpDown`
   - `Vestigium.Controls.UnderConstruction`
3. `App.OnStartup` **before** the first window:
   - `HostLog.Initialize(HostIds.DnsIQ)` (already required)
   - `new ThemeManager()` → `Register` at least one palette → `Initialize(this, name)`
   - `ServiceCollection` → `AddVestigiumControls()` → keep the provider if the default window needs it
4. Design-time XAML must still open without a live provider (Controls contract).

Do not initialize Themes from a control library. Host only.

---

## PR02-03 — default window + tabs + status bar

Replace the current `Window` chrome with `VestigiumDefaultWindow` (inherit or compose — follow the Controls guide, do not fork the template).

Client area:

| Tab | Header | Content |
|---|---|---|
| 0 | DnsIQ | Existing form, moved. |
| 1 | Dashboard | PR02-04. |
| 2 | Settings | PR02-05. |

Status bar slots (VM pushes; do not parse strings in code-behind):

| Slot | Idle | Lookup | Pulse |
|---|---|---|---|
| Left | Idle | rcode or Running | `pulse {i}/{N}` |
| Center | blank or last server | server IP | server IP |
| Right | blank | last elapsed ms | last burst max/med ms |

If the stock menu items all go to Under Construction, either bind them to tab switch or hide them. Two ways to open Dashboard is fine. Two ways that disagree is not.

---

## PR02-04 — Dashboard Under Construction

Use `VestigiumUnderConstruction` (SRS v1.4):

- Title: `Dashboard` (limit 75)
- Subject: `Resolver pulse charts` (limit 125)
- Description: `Not in this release. Pulse numbers stay on the DnsIQ tab and the status bar.`

No ScottPlot. No fake series. Charts package stays unused.

---

## PR02-05 — Settings

Fields only:

- Theme (combo of palettes you registered)
- Status bar position (Top / Bottom) bound to the bar
- Default N, default X (NumericUpDown). DnsIQ tab starts from these. Changing them mid-pulse does not retarget the in-flight run.

No DNS server roster editor. No NIC picker. No log-folder browser this slice.

---

## Done when

- Window opens themed, bar at the bottom, three tabs exist.
- Dashboard is the Under Construction control, not a blank tab.
- DnsIQ tab still looks like today’s form until PR02b lands N/X.
- PingIQ still builds without these packages if you pinned DnsIQ-only.
