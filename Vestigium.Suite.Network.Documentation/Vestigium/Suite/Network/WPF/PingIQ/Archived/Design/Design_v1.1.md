# PingIQ — Design v1.1

**Companion:** [Requirements_v1.1.md](Requirements_v1.1.md)  
**Prior map:** [Design_v1.0.md](Design_v1.0.md)  
**PR delta:** [PR-Plans/PR01/PR01 -- Requirements.md](PR-Plans/PR01/PR01%20--%20Requirements.md)  
**Project:** `src/Vestigium.Suite.Network.PingIQ`

Window and class map for the live exe. Requirements 1.1 win on the window.

Copy DnsIQ Rev 1 shape. Rename the types. Do not reference the DnsIQ project.

---

## Window

`PingIqWindow` + `VestigiumShell` (`ShowNav=False`)

```
menu  File (Exit)
      View (bar visible, bar Top/Bottom, Theme submenu — one check)
icon  Assets/PingIQ.ico
place primary WorkArea, centered, Manual startup
nav   HorizontalTab  PingIQ | Dashboard (disabled until Probe) | Settings
client
  PingIQ
    caption = live status (not a page title)
    Target, Count, Timeout
    Interface (closed combo)
    Echo, Probe, Cancel
    summary line
    reply grid
  Dashboard
    indented HorizontalTab  Echo | Probe
    Echo      pie status mix | empty + Open PingIQ
    Probe     curve + histogram + control | empty + Open PingIQ
  Settings
    indented HorizontalTab  PingIQ | Probe | Theme
    PingIQ:  Timeout, Source (closed combo)
    Probe:   Requests, Seconds
    Theme:   palette, bar visible, dock
status bar    message | progress | detail | clock
```

NumericUpDown host style: centered text, theme card/stroke.

Child tab strips use left margin 20. Selected top tab is the only page name.

Replace `MainWindow` + `Log` text box. Do not keep both surfaces.

---

## Types

| Type | Role |
|---|---|
| `App` | HostLog → Themes → DI → load settings → window. `SetDashboardEnabled`. Static `Themes`, `Settings`, `Session`, `Shell`. |
| `ThemeCatalog` | Registers the Vestigium.Themes 1.0.2 palettes. Same list as DnsIQ. |
| `ThemeChrome` | Binds window chrome to the live theme. |
| `PingIqWindow` | Chrome. Centers on primary work area. Builds View → Theme. No protocol. |
| `MainViewModel` | Query fields, Replies, Echo / Probe / Cancel, status bar posts, last Echo / last pulse samples for Dashboard. |
| `SettingsViewModel` | Pages PingIQ / Probe / Theme. Timeout, Source, Requests, Seconds, palette, bar. |
| `DashboardViewModel` | Echo / Probe tab content. Hosts `FrameworkElement` from `ChartView`. `Unlock` after Probe. `ChartTheme.Paint`. |
| `ChartTheme` | Slot colors from theme brushes. Legend flags. Options for each chart. |
| `PingIqInput` | TryCreate (target, count, timeout ms, index, source). No sockets. |
| `AdapterChoices` | Any (0) + `GetAdapters()` → name/index. |
| `SourceChoices` | Any + unicast addresses; filter by interface. |
| `PingIqSettings` / `PingIqSettingsStore` | DTO + load/save. Root path injectable. Exe root = ProgramData path. Legend flags on the DTO. |
| `PingIqSession` | Load/save glue. Copies ChartTheme flags. |
| `PulsePlan` | Requests, Seconds, DueAt. Same math as DnsIQ. |
| `PulsePrelude` | Probe step 1: same Echo as the button. |
| `ReplyRow` | Sequence, Status, Address, RttMs, Ttl, Detail. Not a library type. Map from `IcmpEchoReply`. |
| `PageViewport` | Page host helper. |

No `IPingService`. No ScottPlot usings in the host. No `CreateEchoCampaign` in this design.

---

## Persist

File: `%ProgramData%\Vestigium\Settings\Diagnostics\PingIQ\settings.json`

Load after Themes.Initialize. Save on change (debounce allowed). Tests pass a temp directory into the store.

DTO keeps `ShowLegendEcho`, `ShowLegendProbeRtt`, `ShowLegendProbeDist`, `ShowLegendProbeControl`.

---

## Flow

### Echo

1. `PingIqInput.TryCreate` including Count and Timeout ms.
2. `NetworkHelper.IcmpEcho(target, options)` with Count, Timeout, InterfaceIndex, SourceAddress.
3. `await job.RunAsync(token)`.
4. Map `result.Replies` to `ReplyRow`. Summary from Sent / Received / Lost / LossPercent / MinMs / MaxMs / AverageMs.
5. Status = `result.Status` (`NetworkJobStatus`).
6. Fail or reject → clear grid and summary. Do not set Running on reject.
7. Push Dashboard Echo mix.
8. Does **not** enable Dashboard.

### Probe

1. Same TryCreate + `PulsePlan.TryCreate(Settings Requests/Seconds)`.
2. Run Echo (step above). Fail or cancel → stop. No pulse.
3. Pulse: DueAt wait, slip. Each shot is `IcmpEcho` with `Count = 1` and the same Timeout / bind. Classify answered vs timeout / forbidden / failed. Collect RTT ms for answered.
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
| Echo | job status · target · avg ms | hidden | — |
| Pulse | sent/total | elapsed/X × 100 | mm:ss |
| Done | summary line on the page | hidden | — |

---

## Library doors this design may call

Pin: `Vestigium.Helpers.Network` **1.2.0** (already in `Directory.Build.props`).

```
NetworkHelper.IcmpEcho(target, IcmpEchoOptions)
    → NetworkJob<IcmpEchoResult>
      RunAsync(token) / Cancel()
      Status: NetworkJobStatus
      Replies: IReadOnlyList<IcmpEchoReply>   // Sequence, Status, Address, RoundtripTimeMs, Ttl, Detail
      Sent, Received, Lost, LossPercent, MinMs, MaxMs, AverageMs

NetworkHelper.GetAdapters()
    → IReadOnlyList<NetworkAdapter>
```

Host does not set BufferSize, Ttl, DontFragment, Interval, MaxDuration, AllowBurst, StatsPath this release.

`CreateEchoCampaign` is the wrong door for Probe. Pulse is a host loop, same as DnsIQ.

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

Do not add a project reference from the exe to Helpers — Shell already flows Network through. Do not bump pins in this design.

---

## Out of this design

Live chart during pulse. History JSON. PathMtu pane. UdpProbe pane. Campaign file picker. TTL / buffer / DF. Count = 0. PropertiesGrid. Other hosts.
