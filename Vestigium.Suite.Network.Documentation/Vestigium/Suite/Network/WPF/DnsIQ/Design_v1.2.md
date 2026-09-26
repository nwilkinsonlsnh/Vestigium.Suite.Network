# DnsIQ — Design v1.2

**Companion:** [Requirements_v1.2.md](Requirements_v1.2.md)  
**Prior map:** [Design_v1.1.md](Design_v1.1.md)  
**PR delta:** [PR-Plans/PR03/PR03 -- Requirements.md](PR-Plans/PR03/PR03%20--%20Requirements.md)  
**Project:** `src/Vestigium.Suite.Network.DnsIQ`

Window and class map for the live exe. Requirements 1.2 win on the window.

---

## Window

`DnsIqWindow` + `VestigiumShell` (`ShowNav=False`)

```
menu  File (Exit)
      View (bar visible, bar Top/Bottom, Theme submenu — one check)
icon  Assets/DnsIQ.ico
place primary WorkArea, centered, Manual startup
nav   HorizontalTab  DnsIQ | Dashboard (disabled until Probe) | Settings
client
  DnsIQ
    caption = live status (not a page title)
    Name, Server, Port, Type
    Interface (closed combo)
    Lookup, Probe, Cancel
    answer grid
  Dashboard
    indented HorizontalTab  Lookup | Probe
    Lookup    pie type mix | empty + Open DnsIQ
    Probe     curve + histogram + control | empty + Open DnsIQ
  Settings
    indented HorizontalTab  DnsIQ | Probe | Theme
    DnsIQ:  Port, Source (closed combo)
    Probe:  Requests, Seconds
    Theme:  palette, bar visible, dock
status bar    message | progress | detail | clock
```

NumericUpDown host style: centered text, theme card/stroke.

Child tab strips use left margin 20. Selected top tab is the only page name.

---

## Types

| Type | Role |
|---|---|
| `App` | HostLog → Themes → DI → load settings → window. `SetDashboardEnabled`. Static `Themes`, `Settings`, `Session`, `Shell`. |
| `ThemeCatalog` | Registers the Vestigium.Themes 1.0.2 palettes. |
| `ThemeChrome` | Binds window chrome to the live theme. |
| `DnsIqWindow` | Chrome. Centers on primary work area. Builds View → Theme. No protocol. |
| `MainViewModel` | Query fields, Answers, Lookup / Probe / Cancel, status bar posts, last Lookup / last pulse samples for Dashboard. |
| `SettingsViewModel` | Pages DnsIQ / Probe / Theme. Port, Source, Requests, Seconds, palette, bar. |
| `DashboardViewModel` | Lookup / Probe tab content. Hosts `FrameworkElement` from `ChartView`. `Unlock` after Probe. `ChartTheme.Paint`. |
| `ChartTheme` | Slot colors from theme brushes. Legend flags. Options for each chart. |
| `DnsIqInput` | TryCreate (name, server, type, index, source, **port**). DisplayType. FirstConfiguredDns. |
| `AdapterChoices` | Any (0) + `GetAdapters()` → name/index. |
| `SourceChoices` | Any + unicast addresses; filter by interface. |
| `DnsIqSettings` / `DnsIqSettingsStore` | DTO + load/save. Root path injectable. Exe root = ProgramData path. Legend flags on the DTO. |
| `DnsIqSession` | Load/save glue. Copies ChartTheme flags. |
| `PulsePlan` | Requests, Seconds, DueAt. Unchanged math. |
| `PulsePrelude` | Probe step 1: same Lookup as the button. |
| `AnswerRow` | Type, Name, Data, Ttl. |
| `PageViewport` | Page host helper. |

No `IDnsService`. No ScottPlot usings in the host.

---

## Persist

File: `%ProgramData%\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`

Load after Themes.Initialize. Save on change (debounce allowed). Tests pass a temp directory into the store.

DTO also keeps `ShowLegendLookup`, `ShowLegendProbeRtt`, `ShowLegendProbeDist`, `ShowLegendProbeControl`.

---

## Flow

### Lookup

1. `TryCreate` including Port.
2. Empty Server → FirstConfiguredDns.
3. All → eight types sequential; merge; sort.
4. Write grid. Push Dashboard Lookup mix.
5. Fail → clear grid.
6. Does **not** enable Dashboard.

### Probe

1. Same TryCreate + PulsePlan.TryCreate(Settings Requests/Seconds).
2. Run Lookup (step above). Fail or cancel → stop. No pulse.
3. Pulse: DueAt wait, slip, cycle types if All. Classify answered / timeout / refused. Collect RTT ms for answered.
4. Do not append pulse rows.
5. After pulse: `NumericSeries.From(rtts)`, `ChartView.Line`, `ChartView.Histogram` (bell/KDE), Control chart only if `ControlLimits` succeeds. `ChartTheme.Paint` each host.
6. `DashboardViewModel.Unlock` → Dashboard tab enabled.

### Detached chart

Charts 1.0.5 `Open in New Window` rebuilds the spec. Title = options title (e.g. Probe control). Window maximized. Background = `Vestigium.Brushes.Surface.Window` when present. Plot stretches.

---

## Status bar

| When | Message | Progress | Detail |
|---|---|---|---|
| Idle | Idle | hidden | — |
| Lookup | rcode · server · ms | hidden | — |
| Pulse | sent/total | elapsed/X × 100 | mm:ss |
| Done | summary line on the page | hidden | — |

---

## Pins

| Package | Version |
|---|---|
| Helpers.Network | 1.2.0 |
| Helpers.Analytics | 1.0.1 |
| Helpers.Charts | 1.0.5 |
| Themes | 1.0.2 |
| Controls / StatusBar / UnderConstruction | 1.0.0 |
| NumericUpDown | 1.0.1 |
| Converters | 1.0.0 |

---

## Out of this design

Live chart during pulse. History JSON. DoH. AXFR. PropertiesGrid. Other hosts.
