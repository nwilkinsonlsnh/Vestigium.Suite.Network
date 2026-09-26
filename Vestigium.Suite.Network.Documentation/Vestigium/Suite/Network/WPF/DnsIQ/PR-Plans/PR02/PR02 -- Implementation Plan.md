# DnsIQ — PR02 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR02  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 25 September 2026  
**Binding:** Requirements 1.1 (written in this slice) win on the window. Helpers.Network 1.2.0 wins on protocol. Vestigium.Controls / Themes / Converters win on chrome. Do not invent a second DNS client. Do not invent a second status bar.

**Goal:** One Vestigium window. Three tabs. Lookup reads records. Probe is a resolver pulse you can steer (N bursts over X seconds). Dashboard is honest Under Construction. Status bar tells server, progress, and time.

**Not:** Charts. CSV. DoH. AXFR. PropertiesGrid. A Helpers campaign class. Pulse rows in the answer grid. Rate as a third spinner.

Follow-ons in this folder (same number, do not make PR03 yet):

| File | What it is |
|---|---|
| [PR02 -- Implementation Plan.md](PR02%20--%20Implementation%20Plan.md) | This file. The attack. |
| [PR02a -- Chrome.md](PR02a%20--%20Chrome.md) | Packages, default window, tabs, status bar. |
| [PR02b -- Pulse.md](PR02b%20--%20Pulse.md) | N over X, burst rules, status math. |

Build in table order. Do not start PR02b code before the window has tabs.

---

## Why this slice exists

PR01 shipped a working record reader. The owner then asked two things that v1.0 did not have:

1. A **resolver pulse** that is not a twin Lookup.
2. A **Vestigium facelift** — default form, status bar, NumericUpDown, themes, converters — and a Dashboard tab that is allowed to be empty.

Probe keeps the button. It gets a job: N bursts over X seconds.

---

## Locks (do not reopen in a slice)

| Lock | Value |
|---|---|
| Window | `VestigiumDefaultWindow` (menu + client + `VestigiumStatusBar`). Client = three tabs. |
| Tabs | **DnsIQ** (work) · **Dashboard** (Under Construction) · **Settings** (theme, dock, default N/X). |
| Lookup | One walk. Writes the grid. Status = rcode · server · ms. Type All = eight types. |
| Probe | Pulse. Does **not** write the grid. Last Lookup rows stay. |
| Burst | Type All = eight `LookupAsync` **in parallel**. One type = one query. |
| N | Burst count. NumericUpDown. Default **10**. Min 1. Max 60. |
| X | Duration seconds. NumericUpDown. Default **10**. Min 1. Max 60. |
| Timing | Burst 1 at t=0. Burst N at t=X when N>1. Spacing = `X / (N - 1)` seconds. N=1 → one burst, no wait. |
| Overrun | If a burst is still running when the next slot is due, **slip**: start the next burst when the current one ends. Do not overlap. Do not queue a pile. |
| Rate | Derived: `N / X` bursts per second. Show it at the end. Do not add a Rate box. |
| Delay box | None. N and X are the knobs. |
| Server | Empty = first configured adapter DNS (already in `DnsIqInput`). Status shows that IP. |
| Type default | **All**. |
| Failed Lookup | Clears the grid. |
| Failed / cancel pulse | Does not clear the grid. |
| One in-flight | Lookup and Probe share one token. Second click ignored until cancel or done. |
| Dashboard | `VestigiumUnderConstruction` only. No chart host. |
| Settings | Theme display name, status-bar Top/Bottom, default N, default X. No protocol. |
| Pins | `Directory.Build.props`. Shell (or DnsIQ if Shell must stay thin this week) consumes packages. Do not copy control source into the exe. |
| Protocol | Still `NetworkHelper.LookupAsync` / `ProbeDns`. Pulse is a **host loop**. Not a new library job this slice. |
| Other hosts | Untouched. |

### Packages this slice consumes

Confirm restore against nuget.org. These are the names. Versions move in `Directory.Build.props` if a newer pack is already published.

| Package | Why |
|---|---|
| `Vestigium.Themes` | `ThemeManager` in `OnStartup` before the window parses. Register at least one palette (LightBlue is the Controls README example). |
| `Vestigium.Controls` | `AddVestigiumControls()`, `VestigiumDefaultWindow`. |
| `Vestigium.Controls.StatusBar` | `VestigiumStatusBar`. Left / Center / Right. |
| `Vestigium.Controls.NumericUpDown` | N and X. |
| `Vestigium.Controls.UnderConstruction` | Dashboard tab. |
| `Vestigium.Converters` | Visibility / format. No converters in the exe. |

Do **not** add `Vestigium.Controls.PropertiesGrid` in this slice.

`Vestigium.Controls*` does not reference Themes. The **host** initializes both.

---

## Window map

```
VestigiumDefaultWindow
  menu (File/View may switch tabs; do not leave stock items on Under Construction only)
  client
    TabControl
      DnsIQ     Name, Server, Type, Interface, Source,
                N, X, Lookup, Probe, Cancel,
                answer grid (Lookup only)
      Dashboard VestigiumUnderConstruction
                  Title: Dashboard
                  Subject: Resolver pulse charts
                  Description: Not in PR02.
      Settings  Theme, status-bar dock, default N, default X
  VestigiumStatusBar  dock Bottom unless Settings says Top
    Left   Idle | Running | rcode | pulse 3/10
    Center server IP
    Right  last ms | med ms | derived rate at end
```

No timeout box. No port box. No chart. No Probe twin grid.

---

## Implementation table

| Order | ID | Do | Paper | State |
| ---: | :--- | :--- | :--- | :--- |
| 1 | PR02-01 | Requirements 1.1 + Design 1.1. Locks above. | this | Open |
| 2 | PR02-02 | Pins + `ThemeManager` + `AddVestigiumControls` in DnsIQ `App.OnStartup`. HostLog still first. | PR02a | Open |
| 3 | PR02-03 | Main window becomes default form + three tabs + status bar wired to VM. | PR02a | Open |
| 4 | PR02-04 | Dashboard tab = Under Construction. | PR02a | Open |
| 5 | PR02-05 | Settings tab: theme, dock, default N/X. | PR02a | Open |
| 6 | PR02-06 | DnsIQ tab: current fields + NumericUpDown N/X. Lookup unchanged except status formatter (server · ms). | PR02b | Open |
| 7 | PR02-07 | Probe = pulse. Host loop. Parallel All. Slip. No grid writes. Progress on status bar. | PR02b | Open |
| 8 | PR02-08 | Host tests off the wire: N/X reject 0 and 61; spacing math; AllTypes. | PR02b | Open |
| 9 | PR02-09 | Owner gate: tabs exist, Dashboard is UC, Lookup still fills grid, Probe 10/10s shows 3/10 then a summary, Pi-hole sees bursts. | Owner | Open |

### PR02-01

Add `Requirements_v1.1.md` and `Design_v1.1.md` next to v1.0. Keep v1.0 as the first-and-ten lock. Point the queue README at 1.1.

Must say the locks table. Must say Dashboard is Under Construction on purpose.

### PR02-02

`Directory.Build.props` gets version properties. PackageReference on Shell **or** DnsIQ — pick one surface and do not duplicate. Prefer Shell if the other hosts will take the same chrome later; DnsIQ-only is allowed if you do not want PingIQ pulling Themes this week.

`App.OnStartup` order:

1. `HostLog.Initialize(HostIds.DnsIQ)`
2. `ThemeManager` register + `Initialize`
3. `ServiceCollection` + `AddVestigiumControls`
4. then the window

### PR02-03 / 04 / 05

See [PR02a -- Chrome.md](PR02a%20--%20Chrome.md).

### PR02-06 / 07 / 08

See [PR02b -- Pulse.md](PR02b%20--%20Pulse.md).

### PR02-09

Owner on the clone. This agent does not mark PR02 closed.

---

## Files this plan expects to touch

```
Directory.Build.props
src/Vestigium.Suite.Network.Shell/Vestigium.Suite.Network.Shell.csproj   (if pins live here)
src/Vestigium.Suite.Network.DnsIQ/App.xaml.cs
src/Vestigium.Suite.Network.DnsIQ/MainWindow.xaml
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.DnsIQ/Views/              (Dns tab, Dashboard, Settings — new)
tests/Vestigium.Suite.Network.Tests/DnsIqInputTests.cs
Vestigium/.../WPF/DnsIQ/Requirements_v1.1.md          [NEW]
Vestigium/.../WPF/DnsIQ/Design_v1.1.md                [NEW]
```

Do not edit PingIQ / TraceIQ / others. Do not bump Helpers.Network. Do not add PropertiesGrid.

When this plan finishes, move the whole `PR02/` folder to `PR-Plans/Completed/PR02/` and idle the queue. Move `PR01/` under `Completed/` at the same time if it is still sitting live.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | Dashboard has no fake chart. Probe does not refill the grid. No Rate spinner. |
| Theodore | One token. Slip, not overlap. Tests stay off the wire. |
| Simon | Status Center is the resolver IP. Themes init before StartupUri. |

---

## Next action

PR02-01. Write Requirements 1.1 and Design 1.1 from the locks table. Then PR02-02 pins.
