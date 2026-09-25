# DnsIQ — PR01 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR01  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 25 September 2026  
**Binding:** Requirements v1.0 wins on the window. Design v1.0 wins on types. Helpers.Network Requirements v1.6 and package **1.2.0** win on protocol. This file wins on the slice.

**Goal:** Ship the first usable DnsIQ window: one name, optional server, eight record types, bind fields, Lookup, Probe, Cancel, status line, answer grid.

**Not:** A protocol library. Not `LookupManyAsync`. Not DoH/DoT. Not AXFR. Not a server roster. Not a chart. Not a campaign. Not a wrap of `NetworkHelper`.

---

## Starting point (repo as of this plan)

The exe already exists. It is not first-and-ten.

| Piece | Today | Required |
|---|---|---|
| `App.OnStartup` | `HostLog.Initialize(HostIds.DnsIQ)` | Keep. |
| Project refs | Shell only | Keep. Packages stay on Shell. |
| Window | Name + Lookup + Status + `Log` text box | Form + bind + Type + Probe + Cancel + grid. No `Log`. |
| `MainViewModel` | Lookup only. No cancel. No busy lock. No server/type. | Full VM per Design. |
| `AnswerRow` | Missing | Type, Name, Data, Ttl. |
| Tests | `HostIds` only | Host reject tests. Not a protocol suite. |

Treat the current `Log` property and the read-only `TextBox` as debt. Replace them. Do not keep both surfaces.

---

## Decision

One PR. Four build slices plus owner gate. The stub is the start, not a second product.

| Call | Why |
|---|---|
| One in-flight token for both buttons | D3. Second click is ignored, not queued. |
| Host rejects blank name, garbage server, garbage source, negative index **before** Status becomes Running | Acceptance 2 and 5. Library still runs `EgressBind.Validate` on send. |
| Server, when set, must be IPv4 or IPv6 | Requirements. A DNS *name* as server is out this release. |
| Type combo is eight values. Default A. Map to `DnsRecordType`. | Requirements. Do not offer `Srv` or `Any`. |
| Timeout and port stay off the form | D7. Library defaults (3 s, 53). |
| PTR is sent as typed | D6. Host does not invent `.in-addr.arpa`. |
| Failed clears the grid | D8, locked: clear. |
| Probe Status is `Answered` / `Refused` / `TimedOut` | Library `DnsProbeStatus`. Do not remap NxDomain to Failed. |
| Lookup Status is the rcode (`NoError`, `NxDomain`, …) | Requirements status list allows rcode on Lookup. |
| Cancel → Status `Cancelled`. Grid stays whatever it was after the job started (empty if we cleared on start). | Cancel is not Failed. |
| No `IDnsService` | Consume contract. Static `NetworkHelper` stays the door. |
| Host tests do not hit the wire | Suite tests are host contracts. Protocol tests live in Helpers. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Keep the `Log` box “for now” | Design is a grid. Two surfaces is fog. |
| Wrap `NetworkHelper` so tests can mock send | Forbidden façade. Validate without sending. |
| `LookupManyAsync` in the same window | Requirements Out. Second name is a second click later. |
| Timeout box | D7 allowed omit. Adding it is gold plate. |
| Call `EgressBind.Validate` from the host | Type is internal. Copy bind onto options; library validates. |
| “Fix” Probe so NxDomain is Failed | Library maps only Timeout and Refused off Answered. Host displays that map. |

---

## Library doors this slice may call

Pin: `Vestigium.Helpers.Network` **1.2.0** (already in `Directory.Build.props`).

```
NetworkHelper.LookupAsync(name, DnsLookupOptions, CancellationToken)
    → Task<DnsLookupResult>
      Answers: IReadOnlyList<DnsRecord>   // Type, Name, Ttl, Data
      Rcode: DnsRcode

NetworkHelper.ProbeDns(name, DnsLookupOptions)
    → NetworkJob<DnsProbeResult>
      RunAsync(token) / Cancel()
      Status: DnsProbeStatus { Answered, Refused, TimedOut }
      Lookup: DnsLookupResult   // grid source; may be empty
```

`DnsLookupOptions` fields this host sets: `Type`, `Server` (null when empty), `InterfaceIndex`, `SourceAddress`.  
Host does not set `Port`, `Timeout`, or `RecursionDesired`.

Wire peer (lock 28) is the library. The window does not inspect source IP of replies.

---

## Types to land

| Type | File | Role |
|---|---|---|
| `App` | `App.xaml.cs` | Already correct. Do not add protocol calls. |
| `MainWindow` | `MainWindow.xaml` / `.cs` | DataContext only. No `NetworkHelper`. |
| `MainViewModel` | `ViewModels/MainViewModel.cs` | State + three commands. |
| `AnswerRow` | `ViewModels/AnswerRow.cs` | Type, Name, Data, Ttl. |
| `DnsIqInput` | `ViewModels/DnsIqInput.cs` | Pure reject rules. No sockets. |

`DnsIqInput` is the cheapest seam that is not a second Network API. Tests call it. The VM calls it, then the library.

---

## Window map

Top row: Name, Server, Type combo (`A` default; `AAAA`, `CNAME`, `MX`, `NS`, `PTR`, `TXT`, `SOA`), Interface index, Source address, Lookup, Probe, Cancel.  
Middle: Status.  
Bottom: read-only `DataGrid` bound to `Answers` — columns Type, Name, Data, Ttl.

No tab control. No chart host. No timeout box. No port box. Placeholder on Name may be `localhost` (already the VM default).

Combo display strings are the Requirements names. Map:

| Combo | `DnsRecordType` |
|---|---|
| A | `A` |
| AAAA | `Aaaa` |
| CNAME | `Cname` |
| MX | `Mx` |
| NS | `Ns` |
| PTR | `Ptr` |
| TXT | `Txt` |
| SOA | `Soa` |

---

## Behavior

### Input (`DnsIqInput.TryCreate`)

Reject and return a message. Do not call the library.

| Input | Rule |
|---|---|
| Name | Trim. Blank → reject. |
| Server | Trim. Empty → `options.Server = null`. Else `IPAddress.TryParse` or reject. |
| Source | Trim. Empty → `null`. Else parse as IP or reject. |
| Interface index | `< 0` → reject. `0` stays `0`. Do not rewrite to `1`. |
| Type | Must be one of the eight. Unknown combo value → reject. |

### Job

1. If busy, command does nothing (`CanExecute` false).
2. Validate. On reject: Status = the message (or `Failed` plus status line). Do not set Running.
3. Clear `Answers`. Status = `Running`. New `CancellationTokenSource`.
4. Lookup: `await NetworkHelper.LookupAsync(name, options, token)`. Status = `result.Rcode.ToString()`. Map `result.Answers` to rows. Empty answers keep the rcode on Status and an empty grid.
5. Probe: `var job = NetworkHelper.ProbeDns(name, options); await job.RunAsync(token)`. Hold the job so Cancel can call `job.Cancel()` as well as cancel the token. Status = `result.Status.ToString()`. Map `result.Lookup.Answers`.
6. `OperationCanceledException` / `TaskCanceledException` → Status = `Cancelled`.
7. Any other exception → Status = `Failed`. Status line / error text = `ex.Message`. Grid already cleared at start; leave it empty (D8).
8. `finally`: dispose token, clear busy, raise CanExecute.

Cancel command: if a `NetworkJob` is live, `job.Cancel()`. Always cancel the token. Status becomes `Cancelled` when the await exits through cancel.

### Bind

`Bind.InterfaceIndex` and `Bind.SourceAddress` copy onto `DnsLookupOptions`. Shell does not apply bind.

---

## Implementation table

Build order is the Order column.

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR01-01 | Window + types. Replace `Log`. Add Server, Type, bind boxes, Probe, Cancel, `Answers` grid, `AnswerRow`. | Open |
| 2 | PR01-02 | `DnsIqInput` + Lookup path. Reject rules. Map answers. Failed clears. | Open |
| 3 | PR01-03 | Probe + one in-flight + Cancel. Both buttons disabled while Running. | Open |
| 4 | PR01-04 | Host tests for rejects and APPID. No wire. | Open |
| 5 | PR01-05 | Owner runs the window against Requirements §4. | Owner |

### PR01-01

Rewrite `MainWindow.xaml` to the Design layout. Delete the `Log` text box and the `Log` property. `MainWindow.xaml.cs` stays DataContext-only. `AnswerRow` is a plain record or small class — no logic.

### PR01-02

`DnsIqInput` returns either options + trimmed name or a reject reason. Lookup command is the only library call in this slice. `ConfigureAwait(true)` is acceptable on the VM if the command starts on the UI thread; do not marshal by hand.

### PR01-03

Same options builder as Lookup. Probe uses `NetworkJob`. One `_busy` / `_cts` / optional `_probeJob` field. `RelayCommand` `CanExecute` tied to busy. Do not queue a second job.

### PR01-04

Add `tests/Vestigium.Suite.Network.Tests/DnsIqInputTests.cs` (name may vary; behavior names, not release ids).

Must assert:

- blank / whitespace name rejects
- `localhost` with empty server accepts
- server `8.8.8.8` accepts; server `dns.google` rejects
- source `not-an-ip` rejects; empty source accepts
- interface `-1` rejects; `0` accepts
- type `A` and `AAAA` accept; a string outside the eight rejects

Keep `HostIdsTests` (`DnsIQ` is not `Network`).

Do **not** add tests that call `LookupAsync` or `ProbeDns`. Those belong in Helpers.

### PR01-05

Owner on the clone:

1. Window opens. `%ProgramData%\Vestigium\Logs\DnsIQ\` exists after first run.
2. Blank name does not send.
3. `localhost` Lookup returns a status line without throwing out of the UI.
4. Probe on a silent or refused name returns Answered, Refused, or TimedOut.
5. Negative index rejected before or at send (host reject is enough).
6. Cancel stops a running lookup.
7. No HTTP client, no OUI, no zone-transfer, no server-list walker.

This agent does not mark first-and-ten closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/MainWindow.xaml
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MainViewModel.cs
src/Vestigium.Suite.Network.DnsIQ/ViewModels/AnswerRow.cs          [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DnsIqInput.cs         [NEW]
tests/Vestigium.Suite.Network.Tests/DnsIqInputTests.cs             [NEW]
```

Do not edit Shell. Do not bump package pins. Do not add a project reference from the exe to Helpers — Shell already flows Network through.

When this plan finishes, move it to `PR-Plans/Completed/PR01/` and idle the queue README.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No façade. No timeout box. No second window. Grid columns match `DnsRecord`. |
| Theodore | Cancel and Failed do not throw out of the UI. CanExecute actually blocks the second click. Tests stay off the wire. |
| Simon | Probe Status is the library enum, not a host invention. Server-as-hostname stays rejected until Requirements change. |

---

## Next action

PR01-01. Replace the log box with the form and grid so Lookup/Probe have somewhere to land.
