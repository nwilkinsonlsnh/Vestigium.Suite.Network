# TraceIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-TRACEIQ-PR01-PLAN
**Host:** `Vestigium.Suite.Network.TraceIQ`
**APPID:** `TraceIQ`
**Status:** Written.
**Date:** 10 October 2026
**Binding:** [PR01 -- Requirements.md](PR01%20--%20Requirements.md) wins on this cut. [Requirements_v1.0.md](../../Requirements_v1.0.md) wins on the window. [Design_v1.0.md](../../Design_v1.0.md) wins on the class map. Helpers.Network wins on protocol.

**Goal:** Stand the shell, design the trace form, write the host log, and run one traceroute to a hop grid.

**Not:** Pathping. PreferUdp. TcpPort box. Chart. A second target. `tracert.exe`.

**Order:** Shell and log first. Then the form. Then the walk.

---

## Starting point

`src/Vestigium.Suite.Network.TraceIQ` is a stub. `App.OnStartup` calls `HostLog.Initialize(HostIds.TraceIQ)`. `MainWindow` is a bare window with Target, Trace, Status, and a text log. `MainViewModel.RunTraceAsync` calls `NetworkHelper.IcmpTrace` with MaxHops 8 and writes `ttl address` lines. No cancel. No grid. No shell. No family, probes, or bind controls.

The library door is already there: `NetworkHelper.IcmpTrace(target, IcmpTraceOptions).RunAsync(token)` returns `IcmpTraceResult` with Status, Reached, ProbeProtocol, and Hops.

---

## Decision

| Call | Why |
|---|---|
| Reuse the suite shell | DnsIQ already has the chrome. TraceIQ should look like the suite, not a one-off form. |
| Replace the text log with a grid | The operator reads hops, not a dump. |
| Keep the stub's walk | The call is right. The form and the grid around it are what is missing. |
| Fill at completion is acceptable | Live append is better if `IProgress` is easy. Both match Requirements. |

---

## Implementation table

| Slice | Id | Work | Status |
|---|---|---|---|
| 1 | PR01-01 | Shell and host log. Window opens under the suite chrome. APPID folder on first run. | |
| 2 | PR01-02 | Trace form. Target, Max hops, Probes per hop, Family, Interface, Source, Trace, Cancel. Validation before send. | |
| 3 | PR01-03 | One walk. Hop grid. Status, Reached, settled protocol. Cancel keeps hops already shown. | |

---

## Slices

### PR01-01

Stand the suite shell the way DnsIQ does. `HostLog.Initialize(HostIds.TraceIQ)` stays before the window. The Trace page is the selected item. Drop the bare title text block. The window title is TraceIQ.

### PR01-02

Form fields on the Trace page. Target defaults to `127.0.0.1`. Max hops defaults to 30, range 1–64. Probes per hop defaults to 1, range 1–10. Family is All / IPv4 / IPv6, default All. Interface and Source come from the existing bind fields. Blank target does not start. Negative interface index is rejected before send. Empty or `0` interface is not pinned and is not rewritten to `1`. Trace is disabled while a job runs. Cancel is enabled while a job runs.

### PR01-03

`RunTraceAsync` builds `IcmpTraceOptions` from the form and calls `NetworkHelper.IcmpTrace(target, options).RunAsync(token)`. One in-flight job. Map `IcmpTraceHop` to a `HopRow`: Ttl, Address or `*`, Name, probe summary. Bind a DataGrid. Header shows Status, Reached, and `ProbeProtocol`. Cancel cancels the token and leaves the rows that already arrived. A new Trace clears the grid first. Code-behind does not call `NetworkHelper`.

If the library exposes progress, append hops as they arrive. If not, replace the collection when the result returns. Do not block the UI.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.TraceIQ/App.xaml.cs
src/Vestigium.Suite.Network.TraceIQ/MainWindow.xaml
src/Vestigium.Suite.Network.TraceIQ/MainWindow.xaml.cs
src/Vestigium.Suite.Network.TraceIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.TraceIQ/ViewModels/HopRow.cs          [NEW]
src/Vestigium.Suite.Network.TraceIQ/Views/TraceView.xaml         [NEW]
src/Vestigium.Suite.Network.TraceIQ/Views/TraceView.xaml.cs      [NEW]
```

Do not add a Pathping page. Do not add a chart. Do not shell to tracert. Do not change Helpers.Network.

---

## Next action

Implement PR01-01. Confirm the window opens under the shell and the APPID log folder exists. Then the form. Then the walk.
