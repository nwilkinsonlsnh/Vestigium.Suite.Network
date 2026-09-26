# PR03 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR03  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 26 September 2026  
**Binding:** [PR03 -- Requirements.md](PR03%20--%20Requirements.md) wins on what this slice must change. Product [Requirements_v1.1.md](../../Requirements_v1.1.md) / [Design_v1.1.md](../../Design_v1.1.md) stay until this plan writes v1.2. Helpers.Network 1.2.0 wins on protocol. Analytics 1.0.1 / Charts 1.0.1 win on Dashboard paint.

**Goal:** Remember knobs. Port on the query row. Interface and Source are closed lists from `GetAdapters`. Probe Lookup-then-pulse. Dashboard Lookup / Probe charts. Product papers catch up.

**Not:** Live charts mid-pulse. CSV. DoH. AXFR. Spoofed Source. Other hosts.

This folder:

| File | What it is |
|---|---|
| [PR03 -- Requirements.md](PR03%20--%20Requirements.md) | Delta. Definition of done. |
| [PR03 -- Implementation Plan.md](PR03%20--%20Implementation%20Plan.md) | This file. The attack. |

Build in table order. Papers before code. Tests stay off the wire.

---

## Product papers (must update)

PR03 does **not** leave the application spec at 1.1.

| Order | File | Job |
|---|---|---|
| PR03-01 | `WPF/DnsIQ/Requirements_v1.2.md` | Fold every R03-* lock into the product SRS. 1.1 stays on disk as history. 1.2 becomes the live window paper. |
| PR03-01 | `WPF/DnsIQ/Design_v1.2.md` | Window map, Settings pages, persist path, Dashboard tabs, Probe prelude, Source/Interface lists, Charts/Analytics calls. |
| PR03-01 | `PR-Plans/README.md` | Point product papers at 1.2 after those files exist. |

Do not rewrite 1.0 / 1.1. Do not put v1.2 inside `PR-Plans/`.

1.2 must say, at least:

- Persist path and field list (R03-01)
- Port 1–65535 default 53 between Server and Type
- Interface combo from `GetAdapters` + Any (0)
- Source combo from unicast addresses + Any, Settings → Probe only
- Requests / Seconds labels, Settings → Probe only, DnsIQ row does not show them
- Probe = Lookup (grid) then pulse (no extra rows); failed Lookup does not pulse
- View menu: no nav indent; bar visible + dock persist
- Dashboard Lookup / Probe; empty cards; curve + histogram after pulse; type mix after Lookup
- Pulse math stays PR02 (N over X, slip, cycle types)

---

## Tests now (inventory)

Project: `tests/Vestigium.Suite.Network.Tests`

| File | What it proves | Still valid? |
|---|---|---|
| `DnsIqInputTests` | Blank name → localhost. Empty server accepts. IPv4 server. Hostname server rejects. Empty source. Garbage source rejects. Index 0 / −1. Types A, AAAA, All, SRV reject. | Yes as **input mapping**. Needs Port. Source list is a new type — keep IP parse tests for the mapper. |
| `PulsePlanTests` | Requests 1–10000. Seconds 1–600. N=1 spacing 0. 1000/60 DueAt last = 60 s. Decimal truncate. | Yes. Do not reopen math. |
| `HostIdsTests` | APPID `DnsIQ`. | Yes. Untouched. |

Gaps this plan must close (all off the wire):

| New / extend | Assert |
|---|---|
| `DnsIqInputTests` Port | Default 53. 1 and 65535 accept. 0 and 65536 reject. Passed through to `DnsLookupOptions.Port`. |
| `AdapterChoices` / `SourceChoices` | Any row exists. Items are built from adapter records (name+index; unicast+name). Interface filter narrows Source. Missing persisted Source → Any. |
| `DnsIqSettingsStore` | Round-trip theme, server, type, interface, port, requests, seconds, bar visible, dock, source. Missing file → defaults. Corrupt JSON → defaults, no throw. Dead source → Any. Tests use a **temp folder**, never `C:\ProgramData` in CI. |
| `PulsePrelude` (or VM helper) | Failed Lookup flag means pulse does not start. Success flag means pulse may start. No `LookupAsync` in the test — boolean / result object only. |

Do **not** add UI tests that open the window. Do **not** hit 8.8.8.8. Do **not** write ProgramData from xUnit.

Existing tests must stay green after each slice. A slice that breaks `PulsePlanTests` or current `DnsIqInputTests` is not done.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR03-01 | Product Requirements_v1.2 + Design_v1.2. Queue README points at 1.2. | Open |
| 2 | PR03-02 | Test project: Port cases + settings store against temp dir + adapter/source list helpers. All current tests still pass. | Open |
| 3 | PR03-03 | Persist load/save. Path under ProgramData in the exe; injectable root for tests. Seed Theme / Server / Type / Interface / Port / Requests / Seconds / bar / Source. | Open |
| 4 | PR03-04 | DnsIQ row: Name, Server, Port, Type, Interface combo. Strip Requests, Seconds, Source from that row. View menu: drop nav indent. | Open |
| 5 | PR03-05 | Settings → Probe: Requests, Seconds, Port, Source combo. Theme: palette, bar visible, dock. Top-left. | Open |
| 6 | PR03-06 | Probe = Lookup then pulse. Failed Lookup stops. Pulse does not append rows. | Open |
| 7 | PR03-07 | Dashboard Lookup + Probe tabs. Empty cards. After Lookup: type mix. After pulse: ChartView line + histogram. Control chart if Analytics returns fences. | Open |
| 8 | PR03-08 | `dotnet test` tests/Vestigium.Suite.Network.Tests — zero failures. | Open |
| 9 | PR03-09 | Owner gate on the clone. | Owner |

### PR03-01

Write the two product files. Stop. No code in that slice except README links.

### PR03-02

Add the test types first where the API is obvious (Port on `TryCreate`, settings DTO, choice builders). Red then green is allowed. Wire tests stay forbidden.

### PR03-03

`settings.json` schema matches R03-01. Create directory. Debounce save is allowed. Load after Themes init.

### PR03-04 / 05

`GetAdapters()` once per session (refresh on Settings open is enough). Source list filters by selected Interface.

### PR03-06

One token for both steps. Cancel during Lookup never starts the pulse. Cancel during pulse keeps the Lookup grid.

### PR03-07

DnsIQ PackageReference already has Analytics/Charts versions in `Directory.Build.props`. Add the refs on the DnsIQ csproj if missing. `ChartsCatalog.Register` next to existing logging init if the Charts README requires it. Host APPID stays `DnsIQ`.

### PR03-08

```
dotnet test tests/Vestigium.Suite.Network.Tests --nologo
```

Must be 0 failed. Fix before owner gate.

### PR03-09

Owner: persist survives restart, Port between Server and Type, combos not editable, Probe fills grid then pulses, Dashboard paints, View menu clean, tests green.

---

## Files this plan expects to touch

```
WPF/DnsIQ/Requirements_v1.2.md          NEW product paper
WPF/DnsIQ/Design_v1.2.md                NEW product paper
PR-Plans/README.md
src/.../DnsIQ/App.xaml.cs
src/.../DnsIQ/DnsIqWindow.xaml
src/.../DnsIQ/ViewModels/DnsIqInput.cs
src/.../DnsIQ/ViewModels/MainViewModel.cs
src/.../DnsIQ/ViewModels/SettingsViewModel.cs
src/.../DnsIQ/Views/DnsIqView.xaml
src/.../DnsIQ/Views/SettingsView.xaml
src/.../DnsIQ/Views/DashboardView.xaml
src/.../DnsIQ/Vestigium.Suite.Network.DnsIQ.csproj
tests/.../DnsIqInputTests.cs
tests/.../PulsePlanTests.cs             (must stay green)
tests/.../DnsIqSettingsStoreTests.cs    NEW
tests/.../AdapterChoiceTests.cs         NEW
```

Do not edit PingIQ / TraceIQ. Do not bump Helpers.Network. Do not add PropertiesGrid.

When PR03 finishes: fold is already in 1.2; move `PR03/` to `Completed/PR03/`.

---

## Next action

PR03-01. Product Requirements_v1.2 and Design_v1.2.
