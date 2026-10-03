# NicIQ — Design v1.1

**Companion:** [Requirements_v1.2.md](Requirements_v1.2.md)  
**Baseline:** [Design_v1.0.md](Design_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.NicIQ`  
**Date:** 2 October 2026

v1.0 types remain. This file adds the types from the details and menu work. Requirements v1.1 stays the monitoring slice.

## Window

`NicIqWindow` hosts File and View. View groups are status-bar visibility, dock top or bottom, Adapter details and System details, then Theme. Dividers use a local `MenuDividerTemplate` (`#9AA8B8`) so the line still paints when menu chrome's implicit separator hides `Separator.Menu`.

Monitoring charts live on `MonitorChart`. Memory is in-use and available, scaled to installed RAM. Throughput, packets, and CPU user/privileged each use two colors.

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.NicIQ)`. Unchanged from v1.0. |
| `MainViewModel` | Adapter list, watch, monitor loop, active NIC. Unchanged role. Adapter details reads it. |
| `NicIqWindow` | View menu. `AdapterDetails_Click` finds `MainViewModel` on a shell page and calls `AdapterDetailWindow.ShowFor(..., followMonitor: true)`. `SystemDetails_Click` calls `SystemDetailWindow.ShowFor`. |
| `SystemDetailForm` | Registry and `CpuFacts` read. Windows, computer, memory, display. `ReadPageUsage` via `GlobalMemoryStatusEx`. |
| `SystemDetailForm.PageFile` | `PagingFiles` parse. `?:` becomes the system drive with one colon. `0 0` is system managed. Total, used, available. |
| `SystemDetailWindow` | Horizontal tabs: Windows, Computer, Memory, Page File, Display. Copy uses `CopyText`. |
| `MonitorChart.Memory` | Physical in use = total − available. Available is the other series. |

## Flow

1. System details reads once on open. It does not follow the adapter combo.
2. Adapter details follows the Monitoring selection when opened from the strip or from View.
3. Page file location: if the registry path starts with `?`, replace that marker with `Path.GetPathRoot(Environment.SystemDirectory)` and keep a single colon.
4. Display reads the display class `{4d36e968-e325-11ce-bfc1-08002be10318}`. VRAM is `HardwareInformation.qwMemorySize`. Shared is installed RAM minus that size when the size is smaller.

## Packages

| Package | Version | Use |
|---|---|---|
| `Vestigium.Themes` | 1.0.6 | `Separator.Menu`, `MenuItem.Standard`, `RadioButton.HorizontalTab`. |
| `Vestigium.Helpers.SystemInfo` | 0.1.1 | `CpuFacts`, `MemoryFacts`. |
| `Vestigium.Controls` | 1.0.0 | Shell and menu chrome. Implicit separator is why NicIQ sets its own divider template. |

## Out of this design

Hotfixes. DirectX acceleration flags. A second menu divider owned by the host after menu chrome stops shipping its own separator.

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | List, detail, watch. Kept as [Design_v1.0.md](Design_v1.0.md). |
| 1.1 | 2 Oct 2026 | System details, page file, memory chart, View menu. Companion is requirements v1.2. |
