# PR02 — NicIQ monitoring

**Host:** `Vestigium.Suite.Network.NicIQ`  
**Binding:** [Requirements_v1.1.md](../../Requirements_v1.1.md)  
**Packages:** `Vestigium.Helpers.PerfMon` 0.1.1, `Vestigium.Helpers.PerfMon.Network` 0.1.1

## Goal

Open the window already sampling the primary NIC. Let the operator pick another Up adapter. Let Settings \ Monitoring add and remove Network Interface counters.

## Not this slice

Charts. `_Total` as the default instance. Replacing `WatchAdapter`. A new host.

## Shape

1. Pin the two PerfMon packages in `Directory.Build.props`.
2. Register `PerfMonCatalog` and `NetworkPerfCatalog` next to Analytics/Charts.
3. `NicPrimaryAdapter` / `NicPdhInstance` / `MonitorCounterList` stay small and testable.
4. `SampleJob` with `Count = 1` and the window cancel token is the clock. `NetworkPerf.RunAsync` is the wrong door once the counter list is user-owned.
5. Persist `MonitorCounters` and `SelectedAdapterId`.

## Watch

Tuesday-at-2am: PDH instance name does not match `NetworkAdapter.Name`. Status line, keep trying. Do not invent a zero series.
