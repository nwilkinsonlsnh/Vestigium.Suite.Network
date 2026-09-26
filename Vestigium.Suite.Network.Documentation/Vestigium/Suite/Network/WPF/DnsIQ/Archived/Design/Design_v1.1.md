# DnsIQ — Design v1.1

**Companion:** [Requirements_v1.1.md](Requirements_v1.1.md)  
**v1.0 map:** [Design_v1.0.md](Design_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.DnsIQ`

This is the class and window map for PR02. It is not a second lock table. Requirements 1.1 win on the window.

---

## Window

`VestigiumDefaultWindow`

```
menu
client
  TabControl
    DnsIQ
      Name, Server, Type (All default),
      Interface, Source,
      N, X (VestigiumNumericUpDown),
      Lookup, Probe, Cancel
      answer grid — Type, Name, Data, Ttl
    Dashboard
      VestigiumUnderConstruction
    Settings
      Theme, status-bar dock, default N, default X
VestigiumStatusBar    Left / Center / Right
```

---

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize` → `ThemeManager` → `AddVestigiumControls` → window. |
| `MainWindow` | Default window. DataContext only. |
| `MainViewModel` | Tab root. Owns StatusBar pushes, selected tab, shared job token. |
| `DnsIqViewModel` | Name, Server, RecordType, Bind, N, X, Answers, Lookup / Probe / Cancel. May live on MainViewModel if the file stays small. |
| `DashboardViewModel` | Title, Subject, Description for Under Construction. No series. |
| `SettingsViewModel` | Theme name, bar dock, default N, default X. |
| `DnsIqInput` | TryCreate + All + FirstConfiguredDns + DisplayType. Already exists. |
| `DnsIqQuery` | Name, Options, AllTypes. Already exists. |
| `AnswerRow` | Type, Name, Data, Ttl. |
| `PulsePlan` | N, X → spacing. `SpacingSeconds` = N==1 ? 0 : X/(N-1). Reject outside 1…60. |

Do not add `IDnsService`. Do not wrap `NetworkHelper` in Shell.

---

## Flow

### Lookup

1. `DnsIqInput.TryCreate`. Reject blank name, bad server, bad type, negative index.
2. Empty Server → `FirstConfiguredDns()`.
3. If All: one `LookupAsync` per first-and-ten type. Merge rows. Sort Type, Name, Data.
4. If one type: one `LookupAsync`.
5. Status bar: Left = rcode, Center = `result.Server` or options.Server, Right = elapsed ms.
6. Failed → clear grid.

### Probe (pulse)

1. Same TryCreate. Also validate N and X (1…60).
2. `spacing = N == 1 ? 0 : X / (N - 1.0)` seconds.
3. For i = 1..N: wait until t ≥ (i-1)*spacing; if late, start now (slip).
4. Burst: All → `Task.WhenAll` eight lookups. One type → one lookup.
5. Do not `AppendAnswers`.
6. Status bar each burst: Left = `pulse i/N`, Center = server, Right = this-burst max ms.
7. End: answered / timeout / refused, min/med/max on answered elapsed, `N/X` burst/s.
8. Cancel or fail → stop. Grid untouched.

NxDomain counts as answered. Timeout / Refused do not. Timeouts stay out of the median.

---

## Status bar map

| Slot | Binding idea |
|---|---|
| Left | `StatusLeft` |
| Center | `StatusCenter` |
| Right | `StatusRight` |
| Dock | Settings `BarPosition` |

Use the StatusBar view-model / update API from `Vestigium.Controls.StatusBar`. Do not draw a fake bar.

---

## Out of this design

Dashboard series. `LookupManyAsync`. DoH. AXFR. PropertiesGrid. Helpers pulse job. Rate spinner. Other suite hosts.
