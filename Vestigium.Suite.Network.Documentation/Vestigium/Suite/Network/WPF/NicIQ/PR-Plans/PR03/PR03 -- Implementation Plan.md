# PR03 — NicIQ extract roadmap

**Host:** `Vestigium.Suite.Network.NicIQ`  
**Status:** Live plan — papers only this slice  
**Date:** 30 September 2026  
**Baseline:** Rev1 host on `main` after PR02 monitoring  
**Binding:** [Published NugetPackages.md](../../../../../Published%20NugetPackages.md) wins on domain. These papers win on what leaves the exe.

## Goal

NicIQ Rev1 stays shippable. PR03 does not add a tab. It names every host type that already belongs in a published package, writes the requirements for that package, and sequences the consume-back so the host shrinks.

This version is not a new host, not a plot API on Network, not `perfmon.exe`, and not a tenth palette.

## Decision

Extract by **existing family**. Same domain = same package. A new filter is not a sibling.

| Letter | Package | Host evidence | Verdict |
|---|---|---|---|
| [PR03a](PR03a%20--%20Requirements%20(Vestigium.Helpers.Charts).md) | `Vestigium.Helpers.Charts` | `MonitorChart.cs` uses `ScottPlot.WPF`, tick generators, and `HorizontalLine`. `ChartTheme.cs` maps palette brushes into `ChartOptions`. | **Extract. First.** Charts already owns drawing. ScottPlot stays internal. |
| [PR03b](PR03b%20--%20Requirements%20(Vestigium.Themes).md) | `Vestigium.Themes` | `ThemeCatalog.cs` copied in NicIQ, DnsIQ, PingIQ. Host `App.xaml` restyles suite controls. | **Extract.** Themes owns palettes + catalog look. Hosts register; they do not reprint the nine `FromPack` lines. |
| [PR03c](PR03c%20--%20Requirements%20(Vestigium.Helpers.PerfMon).md) | `Vestigium.Helpers.PerfMon` | `CachedPdhSource` exists because the stock source opens and disposes every read, so rate counters stay at zero. | **Extract.** Sample contract lives in core. A host must not own the PDH lifetime. |
| [PR03d](PR03d%20--%20Requirements%20(Vestigium.Helpers.PerfMon.Cpu%20and%20Memory).md) | `PerfMon.Cpu` + `PerfMon.Memory` | `HostCounters` hard-codes Processor / Memory paths. CPU and Memory pages already sample them. | **Extract the catalogs.** Topology / `GlobalMemoryStatusEx` facts are a separate fight — see that paper. |
| [PR03e](PR03e%20--%20Requirements%20(Vestigium.Helpers.Network).md) | `Vestigium.Helpers.Network` | `WirelessLink.cs` talks to `wlanapi.dll` for SSID / PHY / quality. | **Parked unless owner expands NetworkAdapter.** Inventory owns adapters. WLAN is not a new package. |
| [PR03f](PR03f%20--%20Requirements%20(Vestigium.Controls).md) | `Vestigium.Controls*` | `ThemeChrome` walks the visual tree to paint `VestigiumStatusBar`. `PageViewport` is copied per host. | **Extract chrome bind + viewport.** Themes does not grow new controls. |

Rejected alternatives:

- A `Vestigium.Helpers.Plots` package — catalog already forbids it.
- A `Vestigium.Themes.Nord2` or per-host theme pack — one nupkg, nine palettes.
- Folding CPU counters into Network — PerfMon.Cpu already exists.
- Leaving `CachedPdhSource` in the exe because “it works” — Tuesday failure is a rate that stays zero on a fresh box when someone swaps back to the stock source.

## What stays in NicIQ

| Piece | Why |
|---|---|
| Adapter list, detail window, Watch | Requirements v1.0 job. Network already supplies the doors. |
| Monitoring tabs and which counters sit on which page | Window. Not a chart kind. |
| `MonitorRing`, settings store, session, primary-NIC pick | Host state. |
| `MainViewModel` sample loop wiring | Host owns APPID, tokens, and the window lifetime. |
| Wireless card **until** PR03e lands | Do not invent `Vestigium.Helpers.Wireless`. |

## Sequence

Libraries publish first. The host consumes second. Do not edit NicIQ and the library in the same commit as if that were delivery.

1. **PR03c** PerfMon cached source — rates are wrong without it. Blocks honest charts.
2. **PR03a** Charts — stop the ScottPlot leak. Theme-aware options. Limit lines through `ChartOptions.Limits`, not `plot.Add.HorizontalLine`.
3. **PR03d** Cpu / Memory catalogs — delete `HostCounters` after the satellites expose the same paths.
4. **PR03b** Themes — one register helper. Delete the three host `ThemeCatalog` files together.
5. **PR03f** Controls — StatusBar paints from tokens. Delete `ThemeChrome`. Share viewport if a second host still copies it.
6. **PR03e** only after the owner says SSID belongs on `NetworkAdapter`.

NicIQ consume PR (still this number, later commit): pin the new package versions in `Directory.Build.props`, delete the host copies, keep behavior.

## Tuesday-at-2am

PDH instance name ≠ `NetworkAdapter.Name`. Status line. No fake zero series. That rule does not move.

Second failure: stock PerfMon source without cache → Bytes/sec reads as 0 after a package bump. PR03c exists so that is not a host workaround.

Third failure: theme switch leaves the ScottPlot figure on Light Blue because the host poked `WpfPlot` after `ChartView.Line`. PR03a exists so Charts reads tokens on each paint.

## Watch

| Role | Watch |
|---|---|
| Alvin | No new package names. Shape stays inside the published families. |
| Theodore | After extract, NicIQ has zero `using ScottPlot`. Rate counters still move. Theme switch repaints charts and the status bar. |
| Simon | WLAN and CPU topology are not “obviously PerfMon.” Do not smuggle them into Charts. |

## Next action

Owner accepts or amends the six letters. Implementation starts in **Vestigium.Helpers** (PR03c) and **Vestigium.Themes** (PR03b), not in this exe.
