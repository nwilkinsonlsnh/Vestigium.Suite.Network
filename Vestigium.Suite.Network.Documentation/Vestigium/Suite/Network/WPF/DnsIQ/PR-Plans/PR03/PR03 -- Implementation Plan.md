# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 26 September 2026  
**Binding:** Product [Requirements_v1.2.md](../../Requirements_v1.2.md) / [Design_v1.2.md](../../Design_v1.2.md) win on the window. [PR03 -- Requirements.md](PR03%20--%20Requirements.md) is the slice delta. Helpers.Network 1.2.0 wins on protocol. Analytics 1.0.1 / Charts 1.0.1 win on Dashboard paint.

**Goal:** Remember knobs. Port on the query row. Interface and Source are closed lists from `GetAdapters`. Probe Lookup-then-pulse. Dashboard Lookup / Probe charts. Product papers catch up.

**Not:** Live charts mid-pulse. CSV. DoH. AXFR. Spoofed Source. Other hosts.

This folder:

| File | What it is |
|---|---|
| [PR03 -- Requirements.md](PR03%20--%20Requirements.md) | Delta. Definition of done. |
| [PR03 -- Implementation Plan.md](PR03%20--%20Implementation%20Plan.md) | This file. The attack. |

Build in table order. Papers before code. Tests stay off the wire.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03-01 | Product Requirements_v1.2 + Design_v1.2. Queue README points at 1.2. | Done |
| 2 | PR03-02 | Test project: Port cases + settings store against temp dir + adapter/source list helpers. All current tests still pass. | Open |
| 3 | PR03-03 | Persist load/save. Path under ProgramData in the exe; injectable root for tests. Seed Theme / Server / Type / Interface / Port / Requests / Seconds / bar / Source. | Open |
| 4 | PR03-04 | DnsIQ row: Name, Server, Port, Type, Interface combo. Strip Requests, Seconds, Source from that row. View menu: drop nav indent. | Open |
| 5 | PR03-05 | Settings → Probe: Requests, Seconds, Port, Source combo. Theme: palette, bar visible, dock. Top-left. | Open |
| 6 | PR03-06 | Probe = Lookup then pulse. Failed Lookup stops. Pulse does not append rows. | Open |
| 7 | PR03-07 | Dashboard Lookup + Probe tabs. Empty cards. After Lookup: type mix. After pulse: ChartView line + histogram. Control chart if Analytics returns fences. | Open |
| 8 | PR03-08 | `dotnet test` tests/Vestigium.Suite.Network.Tests — zero failures. | Open |
| 9 | PR03-09 | Owner gate on the clone. | Owner |

### PR03-02

Add the test types first where the API is obvious (Port on `TryCreate`, settings DTO, choice builders). Red then green is allowed. Wire tests stay forbidden.

Existing: `DnsIqInputTests`, `PulsePlanTests`, `HostIdsTests`. Must stay green.

### PR03-03

`settings.json` schema matches R03-01. Create directory. Debounce save is allowed. Load after Themes init.

### PR03-04 / 05

`GetAdapters()` once per session (refresh on Settings open is enough). Source list filters by selected Interface.

### PR03-06

One token for both steps. Cancel during Lookup never starts the pulse. Cancel during pulse keeps the Lookup grid.

### PR03-07

Add Analytics/Charts PackageReference on DnsIQ if missing. `ChartsCatalog.Register` next to logging init if required. Host APPID stays `DnsIQ`.

### PR03-08

```
dotnet test tests/Vestigium.Suite.Network.Tests --nologo
```

Must be 0 failed. Fix before owner gate.

### PR03-09

Owner: persist survives restart, Port between Server and Type, combos not editable, Probe fills grid then pulses, Dashboard paints, View menu clean, tests green.

---

## Next action

PR03-02. Port tests, settings store against a temp folder, adapter/source choice helpers. Keep PulsePlan green.
