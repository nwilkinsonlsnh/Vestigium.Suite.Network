# PR03 — NicIQ extract roadmap

**Host:** `Vestigium.Suite.Network.NicIQ`  
**Status:** Live plan — papers only this slice  
**Date:** 30 September 2026  
**Binding:** Published NugetPackages.md wins on domain.

## Goal

Move host-local library work out of NicIQ. Rev1 window stays shippable.

This version is not a new host, not `perfmon.exe`, not a tenth palette, not a root `Vestigium.SystemInfo` prefix, and not `Vestigium.Helpers.SystemInfo.Network`.

## Decision

Owner lock: SystemInfo helper class libraries live in **`Vestigium.Helpers`**, IDs `Vestigium.Helpers.SystemInfo*`.

| Letter | Package | Verdict |
|---|---|---|
| PR03a | `Vestigium.Helpers.Charts` | Extract ScottPlot leak |
| PR03b | `Vestigium.Themes` | Extract copied ThemeCatalog |
| PR03c | `Vestigium.Helpers.PerfMon` | Cached PDH source |
| PR03d | `PerfMon.Cpu` / `Memory` | PDH path catalogs only |
| PR03e | `Vestigium.Helpers.Network` | WLAN parked on Network |
| PR03f | `Vestigium.Controls*` | ThemeChrome / viewport |
| PR03g | **NEW** `Vestigium.Helpers.SystemInfo` + `.Cpu` + `.Memory` | Snapshots. Helpers repo. |

Rejected: root `Vestigium.SystemInfo`, `Helpers.SystemInfo.Network`, topology stuffed into PerfMon.Cpu.

## Sequence

1. PR03c cached PDH  
2. PR03a Charts  
3. PR03d PDH catalogs  
4. PR03g `src/Vestigium.Helpers.SystemInfo*`  
5. PR03b Themes  
6. PR03f Controls  
7. PR03e only if `NetworkAdapter` grows wireless

## Next action

Stand the three projects up in Vestigium.Helpers. Add the family row to the catalog before the first nupkg.
