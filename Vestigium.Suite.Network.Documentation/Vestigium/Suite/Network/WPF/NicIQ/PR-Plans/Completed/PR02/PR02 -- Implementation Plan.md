# PR02 — NicIQ monitoring

**Host:** `Vestigium.Suite.Network.NicIQ`  
**Binding:** [Requirements_v1.1.md](../../Requirements_v1.1.md)  
**Packages:** `Vestigium.Helpers.PerfMon` 0.1.1, `Vestigium.Helpers.PerfMon.Network` 0.1.1, `Vestigium.Helpers.Charts` 1.0.7, `Vestigium.Helpers.Analytics` 1.0.1

## Goal

Open the window already sampling the primary NIC. Let the operator pick another Up adapter. Let Settings \ Monitoring add and remove Network Interface counters. Draw Throughput as one two-series plot.

## Not this slice

N-series plots. Filled area. Replacing `WatchAdapter`. A new host.

## Shape

1. Pin PerfMon 0.1.1 and Charts **1.0.7**.
2. Register PerfMon + Network + Analytics + Charts catalogs.
3. `SampleJob` is the clock. `ChartView.Line(IReadOnlyList<NumericSeries>)` is the two-line door.
4. Four tabs: Throughput (default), Packets, Integrity, Utilization. Only the visible tab paints.
5. Persist `MonitorCounters` and `SelectedAdapterId`.

## Watch

Tuesday-at-2am: PDH instance name does not match `NetworkAdapter.Name`. Status line, keep trying. Do not invent a zero series.
