# Shell — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.Shell`  
**Kind:** WPF class library

## Shape

```
Shell
  HostIds.cs          constants
  HostLog.cs          Initialize(appId)
  BindFields.cs       ObservableObject
```

No `Views/`. No `Services/`. Hosts reference this project. They do not reference each other.

## Types

| Type | Notes |
|---|---|
| `HostIds` | Public const strings. One per exe. |
| `HostLog` | Static. Creates the log directory, `VestigiumLogger.Initialize`, `NetworkCatalog.Register`. |
| `BindFields` | `partial ObservableObject`. `[ObservableProperty] int InterfaceIndex`, `string? SourceAddress`. |

## Call order

```
App.OnStartup
  → HostLog.Initialize(HostIds.PingIQ)   // or TraceIQ / …
  → MainWindow
       DataContext = MainViewModel
         Bind = new BindFields()
         options.InterfaceIndex = Bind.InterfaceIndex
         options.SourceAddress  = Bind.SourceAddress
         NetworkHelper.<Job>(…)
```

## Non-design

No `INetworkHost`. No `EchoClient`. No `EgressBind.Apply` here. Charts package may be referenced; Shell exposes no `ChartView` helper this release.
