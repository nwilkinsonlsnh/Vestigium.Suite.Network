# TraceIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.TraceIQ`

## Window

Top: Target, MaxHops (30), ProbesPerHop (1), Family combo, bind fields, Trace, Cancel.  
Middle: Status, Reached, ProbeProtocol.  
Bottom: hop grid — Ttl, Address (`*` if null), Name, probe summary.

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.TraceIQ)`. |
| `MainWindow` | DataContext only. |
| `MainViewModel` | Target, MaxHops, ProbesPerHop, Family, Bind, Status, Reached, Protocol, `ObservableCollection<HopRow>`. |
| `HopRow` | Ttl, Address, Name, Detail. Mapped from `IcmpTraceHop`. |

## Flow

1. Reject blank target and negative index.
2. `IcmpTraceOptions` from the form. `RouteFamily` from the combo.
3. `NetworkHelper.IcmpTrace(Target, options).RunAsync(token)`.
4. Replace the hop collection from `result.Hops`.
5. Status / Reached / `result.ProbeProtocol` on the header.

Progress: if `IProgress<NetworkProgress>` is easy, append hops live. If not, fill at completion. Both match Requirements.

## Out of this design

Pathping sample columns. PreferUdp checkbox. TcpPort box.
