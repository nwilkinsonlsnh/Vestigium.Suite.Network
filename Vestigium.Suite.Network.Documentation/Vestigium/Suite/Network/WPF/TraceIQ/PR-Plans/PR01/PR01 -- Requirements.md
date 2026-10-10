# TraceIQ — PR01 Requirements

**Document ID:** VEST-SUITE-NETWORK-TRACEIQ-PR01-REQ
**Host:** `Vestigium.Suite.Network.TraceIQ`
**APPID:** `TraceIQ`
**Status:** Written. Binding for this push.
**Date:** 10 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.0.md](../../Requirements_v1.0.md), [Design_v1.0.md](../../Design_v1.0.md)
**Library:** `Vestigium.Helpers.Network` (`NetworkHelper.IcmpTrace`, `IcmpTraceOptions`, `IcmpTraceResult`, `IcmpTraceHop`)
**Prior:** None. The project is a stub: a target box, a Trace button, a status line, and a text log of hops.

**One sentence:** Stand the TraceIQ shell, design the trace form, write the host log, and run one traceroute to a hop grid.

**This version is not** pathping. Not a Prefer UDP checkbox. Not a TcpPort box. Not an RTT chart. Not a second target. Not `tracert.exe`.

---

## 0. Decision

| Call | Why |
|---|---|
| One form, one grid | Requirements v1.0 is first and ten. Show the path. |
| Shell matches the suite | DnsIQ already stands a Vestigium shell. TraceIQ uses the same chrome, not a bare window. |
| HostLog before the window | T1. APPID folder is the proof the host started. |
| Hop grid, not a text log | The stub dumps hops into a text box. The form reads a grid: Ttl, Address, Name, probes. |
| Library owns the fallback | ICMP may fall to UDP or TCP. The host shows the settled protocol. It does not pick it. |
| Cancel keeps what arrived | Partial hops stay on the grid. |
| Pathping waits | Second phase. No sample-loss columns. |

Rejected: pathping in this slice. Rejected: PreferUdp. Rejected: a TcpPort box. Rejected: a chart. Rejected: shelling to tracert.

---

## 1. What this version is

A WPF exe that opens, logs, takes one target, walks the path, and prints the hops.

| Surface | Stub today | PR01 |
|---|---|---|
| Window | Bare `MainWindow`. | Suite shell. Trace is the page. |
| Logging | `HostLog.Initialize` only. | Initialize before the window. APPID folder on first run. |
| Form | Target, Trace. | Target, Max hops, Probes per hop, Family, Interface, Source, Trace, Cancel. |
| Result | Text log of `ttl address`. | Hop grid: Ttl, Address or `*`, Name, probe summary. |
| Header | Status string. | Status, Reached, settled protocol. |
| Job | No cancel. No gate. | One in-flight job. Cancel stops it. |

---

## 2. Requirements

### R01-01 Host starts and logs

`App.OnStartup` calls `HostLog.Initialize(HostIds.TraceIQ)` before the window shows. Logs land under `%ProgramData%\Vestigium\Logs\TraceIQ\`. The APPID is `TraceIQ`, never `Network`.

### R01-02 The shell is the window

TraceIQ uses the suite shell. The first page is Trace. Settings can wait. There is no File menu on the page. The window title is TraceIQ.

### R01-03 The form is the walk

| Field | Rule |
|---|---|
| Target | Required. Trim. Blank does not start. Placeholder `127.0.0.1`. |
| Max hops | Default 30. Range 1–64. |
| Probes per hop | Default 1. Range 1–10. |
| Family | All / IPv4 / IPv6. Default All. |
| Interface | Optional. Empty or `0` is not pinned. Negative is rejected before send. Do not rewrite `0` to `1`. |
| Source | Optional. Must parse if set. |
| Trace | Starts the walk. Disabled while running. |
| Cancel | Cancels the token. Partial hops stay. |

### R01-04 One walk, one grid

`NetworkHelper.IcmpTrace(target, options).RunAsync(token)`. Options carry MaxHops, ProbesPerHop, Family, InterfaceIndex, SourceAddress. TcpPort stays at the library default. One in-flight job. Code-behind does not call `NetworkHelper`.

The grid is one row per hop: Ttl, Address or `*`, Name if PTR filled, probe summary. PTR miss stays empty. TCP mid-path hops may stay `*`.

Status is Idle / Running / Success / TimedOut / Failed / Cancelled. The header also shows Reached and the settled protocol (ICMP / UDP / TCP).

Hops may append as they arrive. If the job only completes as a block, fill the list at the end. Either matches Requirements.

### R01-05 Cancel keeps the path so far

Cancel stops the walk. Hops already shown stay. Status becomes Cancelled. A second Trace replaces the grid.

### R01-06 No pathping, no chart, no shell-out

No Pathping button. No sample-loss columns. No RTT chart. No `tracert`, no `traceroute`, no `pathping.exe`.

---

## 3. Must not change

- Helpers.Network protocol. The host does not invent a fallback.
- DnsIQ, NicIQ, PingIQ, ProbeHost, RouteIQ, ShareIQ.
- The APPID. It stays `TraceIQ`.

---

## 4. Done

- Window opens under the suite shell. APPID log folder exists after first run.
- Blank target does not start.
- Trace `127.0.0.1` with Max hops 8 returns a hop grid or a terminal status without throwing out of the UI.
- Negative interface index is rejected before send.
- Cancel stops a running walk and keeps hops already shown.
- No Pathping button. No sample-loss columns.
