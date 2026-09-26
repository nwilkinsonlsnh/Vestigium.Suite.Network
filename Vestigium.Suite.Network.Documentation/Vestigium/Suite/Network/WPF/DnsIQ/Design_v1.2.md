# DnsIQ — Design v1.2

**Companion:** [Requirements_v1.2.md](Requirements_v1.2.md)  
**Prior map:** [Design_v1.1.md](Design_v1.1.md)  
**PR delta:** [PR-Plans/PR03/PR03 -- Requirements.md](PR-Plans/PR03/PR03%20--%20Requirements.md)  
**Project:** `src/Vestigium.Suite.Network.DnsIQ`

Window and class map for PR03. Requirements 1.2 win on the window.

---

## Window

`DnsIqWindow` + `VestigiumShell`

```
menu  File (Exit)  View (bar visible, bar Top/Bottom)
client
  DnsIQ
    Name, Server, Port, Type
    Interface (closed combo)
    Lookup, Probe, Cancel
    answer grid
  Dashboard
    Lookup tab    type mix | empty card
    Probe tab     curve + histogram | empty card
  Settings
    Theme | Probe   (top-left)
    Theme: palette, bar visible, dock
    Probe: Requests, Seconds, Port, Source (closed combo)
status bar    message | progress | detail | clock
```

---

## Types

| Type | Role |
|---|---|
| `App` | HostLog → Themes → DI → load settings → window. |
| `DnsIqWindow` | Chrome. No protocol. |
| `MainViewModel` | Query fields, Answers, Lookup / Probe / Cancel, status bar posts, last Lookup / last pulse samples for Dashboard. |
| `SettingsViewModel` | Theme, bar, Requests, Seconds, Port, Source. |
| `DashboardViewModel` | Lookup / Probe tab content. Hosts `FrameworkElement` from `ChartView`. |
| `DnsIqInput` | TryCreate (name, server, type, index, source, **port**). DisplayType. FirstConfiguredDns. |
| `AdapterChoices` | Any (0) + `GetAdapters()` → name/index. |
| `SourceChoices` | Any + unicast addresses; filter by interface. |
| `DnsIqSettings` / `DnsIqSettingsStore` | DTO + load/save. Root path injectable. Exe root = ProgramData path. |
| `PulsePlan` | Requests, Seconds, DueAt. Unchanged math. |
| `AnswerRow` | Type, Name, Data, Ttl. |

No `IDnsService`. No ScottPlot usings in the host.

---

## Persist

File: `%ProgramData%\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`

Load after Themes.Initialize. Save on change (debounce allowed). Tests pass a temp directory into the store.

---

## Flow

### Lookup

1. `TryCreate` including Port.
2. Empty Server → FirstConfiguredDns.
3. All → eight types sequential; merge; sort.
4. Write grid. Push Dashboard Lookup mix.
5. Fail → clear grid.

### Probe

1. Same TryCreate + PulsePlan.TryCreate(Settings Requests/Seconds).
2. Run Lookup (step above). Fail or cancel → stop. No pulse.
3. Pulse: DueAt wait, slip, cycle types if All. Classify answered / timeout / refused. Collect RTT ms for answered.
4. Do not append pulse rows.
5. After pulse: `NumericSeries.From(rtts)`, `ChartView.Line` or Scatter, `ChartView.Histogram` (bell/KDE), Control chart only if `ControlLimits` succeeds.

---

## Status bar

| When | Message | Progress | Detail |
|---|---|---|---|
| Idle | Idle | hidden | — |
| Lookup | rcode · server · ms | hidden | — |
| Pulse | sent/total | elapsed/X × 100 | mm:ss |
| Done | summary line on the page | hidden | — |

---

## Out of this design

Live chart during pulse. History JSON. DoH. AXFR. PropertiesGrid. Other hosts.
