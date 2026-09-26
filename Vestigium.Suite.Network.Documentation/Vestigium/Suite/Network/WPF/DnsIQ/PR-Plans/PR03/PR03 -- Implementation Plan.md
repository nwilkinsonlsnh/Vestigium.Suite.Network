# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 26 September 2026  
**Binding:** Product [Requirements_v1.2.md](../../Requirements_v1.2.md) / [Design_v1.2.md](../../Design_v1.2.md) win on the window. [PR03 -- Requirements.md](PR03%20--%20Requirements.md) is the slice delta.

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03-01 | Product Requirements_v1.2 + Design_v1.2. Queue README points at 1.2. | Done |
| 2 | PR03-02 | Test project: Port cases + settings store against temp dir + adapter/source list helpers. All current tests still pass. | Done |
| 3 | PR03-03 | Persist load/save. Path under ProgramData in the exe; injectable root for tests. Seed Theme / Server / Type / Interface / Port / Requests / Seconds / bar / Source. | Open |
| 4 | PR03-04 | DnsIQ row: Name, Server, Port, Type, Interface combo. Strip Requests, Seconds, Source from that row. View menu: drop nav indent. | Open |
| 5 | PR03-05 | Settings → Probe: Requests, Seconds, Port, Source combo. Theme: palette, bar visible, dock. Top-left. | Open |
| 6 | PR03-06 | Probe = Lookup then pulse. Failed Lookup stops. Pulse does not append rows. | Open |
| 7 | PR03-07 | Dashboard Lookup + Probe tabs. Empty cards. After Lookup: type mix. After pulse: ChartView line + histogram. Control chart if Analytics returns fences. | Open |
| 8 | PR03-08 | `dotnet test` tests/Vestigium.Suite.Network.Tests — zero failures. | Open |
| 9 | PR03-09 | Owner gate on the clone. | Owner |

## Next action

PR03-03. Wire `DnsIqSettingsStore` into startup (exe uses DefaultRoot). Seed knobs from the file.
