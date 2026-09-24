# PR03a — Requirements (`Vestigium.Helpers.Charts`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03A-CHARTS  
**Package:** `Vestigium.Helpers.Charts` (today 1.0.7 on the host pin; README on Helpers still says 1.0.6)  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**APPID:** `Charts`  
**EVENTID:** 16500–16999 (used through 16545 — allocate inside the unused tail, do not reuse)  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `src/Vestigium.Suite.Network.NicIQ/ViewModels/MonitorChart.cs`, `ChartTheme.cs`

## Goal

Charts paints every plot NicIQ already shows, including limit lines and palette colors, without the host naming a ScottPlot type.

This version is not Analytics. It does not compute UCL/CL/LCL, run rules, utilization %, or Kbps. It does not grow a plot API on Network.

## Source of the leak

`MonitorChart` calls `ChartView.Line` / `ChartView.From`, then casts the result to `WpfPlot` to:

- pin the X axis to `0..span`
- force integer Y ticks on Integrity
- add or restyle `HorizontalLine` for CL / UCL / LCL with hardcoded RGB
- `Refresh()`

`ChartTheme` reads `Vestigium.Brushes.*` off `Application.Current` and writes hex into `ChartOptions`. Legend visibility is a static on the host.

That is Charts work living in an exe. Catalog rule: ScottPlot types stay internal.

## Must change in Charts

### C-01 Theme-aware options

Add a host-callable door that fills `ChartOptions` from the live Vestigium token set:

| Option | Token |
|---|---|
| `Color` | `Vestigium.Brushes.Accent.Primary` (series 0). Series 1+ use `Vestigium.Brushes.Series.N` when present, else a documented fallback. |
| `FigureColor` | `Vestigium.Brushes.Surface.Window` |
| `DataColor` | `Vestigium.Brushes.Surface.Card` |
| `AxisColor` | `Vestigium.Brushes.Text.Primary` |
| `GridColor` | `Vestigium.Brushes.Stroke.Subtle` |

Missing token → current hex fallback. Charts may read `Application.Current` resources. It must not reference `Vestigium.Themes` types if that creates a package cycle. Tokens are strings.

Host stops shipping `ChartTheme.Hex`.

### C-02 Limit lines are a Charts job

When `ChartOptions.Limits` is set (Analytics already computed them), `Line`, `Column` / `From(Column)`, and `Control` draw CL / UCL / LCL.

- Colors come from tokens, not `new ScottPlot.Color(214, 92, 92)`. Prefer Status.Error / Status.Info / Accent or Series keys. Document the three keys.
- Patterns: UCL dashed, LCL dashed, CL denser / thicker.
- Legend text `UCL` / `CL` / `LCL`.
- Host never calls `plot.Add.HorizontalLine`.

NicIQ already computes limits with `NumericSeries.ControlLimits(MeanPlusKSigma, 3, floor: 0)`. That call stays in the host or moves only if a second consumer appears. Charts still does not invent fences.

### C-03 Time window on Line

`ChartView.Line` must honor an X span without a host cast.

Accept either:

- `ChartOptions` fields `XMin` / `XMax` (and optional `YMin` / `YMax`), or
- a small `ChartAxis` on options

NicIQ needs X = `0..N` seconds for a sliding window. Default remains “fit the data” so PingIQ / DnsIQ do not change.

### C-04 Count axis on Column

Integrity is six labeled counts via `ChartSpec { Kind = Column }`. After paint, the host forces Y from −1 to a stepped ceiling and a fixed tick interval.

Charts does that when `ChartOptions` says the Y axis is a count (name the flag). Empty / all-zero still shows a 0–10 floor so the plot is not a flat line on nothing.

### C-05 Two-series Line is enough for Rev1 pages

Throughput, Packets, CPU, Memory, Utilization are one or two series. Charts README already allows 1 or 2 `NumericSeries` on Line. Do not lift the two-series cap in this extract.

Integrity stays Column, not a six-line Line.

### C-06 Legend

`ChartView.SetLegendVisible` and `LegendToggled` already exist — NicIQ uses them. Keep them on Charts. Host may remember the last value in settings. Charts does not persist.

### C-07 No ScottPlot in any suite host

Acceptance for the consume-back: `using ScottPlot` is absent from `Vestigium.Suite.Network.*`.

## Must not change

- Analytics formulas.
- EVENTIDs 16500–16545.
- `ChartsCatalog.Register` stays host-called during `VestigiumLogger.Initialize`.
- PNG path.
- A Network plot API.

## Host after consume

| Delete from NicIQ | Keep |
|---|---|
| ScottPlot usings and `WpfPlot` casts in `MonitorChart` | Page names, counter pairing, scale (bytes → Kbps, bytes → GB) |
| `ChartTheme` brush-to-hex helper | Call `ChartView.Line` / `From` with themed options + Analytics limits |
| Hardcoded CL RGB | Strip text (last / mean / min / max / p95) — that is a status line, not a plot |

Scale belongs in the host (or Analytics later). Charts draws the numbers it is given.

## Acceptance

1. NicIQ Throughput / Packets / CPU / Memory / Utilization paint through `ChartView` only.
2. Integrity column paints through `ChartView.From` only.
3. Theme switch changes figure, data, axis, grid, and series colors on the next paint.
4. Limits appear when Analytics returns fences; they do not appear when the series is shorter than two points.
5. Missing PDH still yields “Waiting for samples.” / “—”. No zero series invented to keep ScottPlot happy.
6. Helpers tests cover themed options with a stub resource dictionary and limit-line presence without opening a window if the existing test style allows it. If headless WPF is already the Charts test pattern, follow it.

## Parked

- N-series Line.
- Filled area (rejected in NicIQ PR02).
- Live-follow while a sample is in flight beyond what Rev1 already does.
- Moving Charts into the Network package.
