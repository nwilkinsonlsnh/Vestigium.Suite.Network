# PR04 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR04
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live
**Date:** 6 October 2026
**Binding:** [PR04 -- Requirements.md](PR04%20--%20Requirements.md) wins on this slice. Requirements_v1.2 wins on the existing window until this PR folds. Helpers.Network 1.2.0 wins on the lookup call. This file wins on order.

**Goal:** Pack the two LogParser projects, read a HAR down to hosts, scrape URLs and domain names out of a `.txt` dump, and probe those names from DnsIQ with the knobs the window already has.

**Not:** A HAR analyzer. Not a TCP check on port 8443. Not a change to Lookup, pulse, or Dashboard. Not an xlsx reader. Not a paste box.

---

## Starting point

| Piece | Today | Required |
|---|---|---|
| DnsIQ | Lookup, Probe pulse, persist, Dashboard | File → Open capture (`.har` / `.txt`), HAR tab, probe loop |
| `Vestigium.Helpers` on GitHub | No LogParser projects (checked 6 Oct 2026) | `LogParser` (types + text scrape) + `LogParser.Har`, pack 1.0.0 |
| Suite consume rule | Package only. No project reference to Helpers. | Same. Pin after pack. |
| Corpus | Two HARs attached to the ask, not in git | Trimmed fixtures in the Har test project |

Owner has created the two projects. This plan does not rename them and does not add a third.

---

## Decision

Libraries first, window second. DnsIQ cannot legally project-ref the parser, so the package has to exist before the exe calls it.

| Call | Why |
|---|---|
| Shared project is types plus `TextHostReader` | The dump door is not HAR. It belongs in the shared library. |
| `HarReader` is a static door | Same shape as `NetworkHelper`. No façade in the exe. |
| Valid `.har` uses `HarReader`; other text uses `TextHostReader` | Structured read keeps ports. A sheet saved as `.txt` never pretends to be a HAR. |
| Probe is a loop of `LookupAsync` (A, then AAAA) | Already the protocol door. `ProbeDns` is the pulse job. Do not overload it. Same loop for both doors. |
| One in-flight token for Lookup, pulse, and HAR probe | D3 already. A second click is ignored. |
| Sequential | Ten hosts. Parallel is a stunt. |
| HAR tab disabled until parse succeeds | Same gate idea as Dashboard. An error must not show an empty "success" page. |
| Fixtures trimmed from the two captures | Assert the host set. Do not commit 2.5 MB of Auth0 SVG. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Parse in the exe "until the package exists" | That is how the parser never leaves the exe. |
| `ILogParser` + registry | Two static doors. An interface does not buy a swap. |
| Paste box, or native `.xlsx` | Owner's intake is already a `.txt`. |
| Reuse the Lookup grid for HAR rows | Different columns. Mixing them loses the last answer set. |
| Probe with Type All | Eight queries per host. The question is resolve, not MX. |
| TCP connect to observed port | Different failure mode. Parked. |

---

## Library doors this slice may call

New packages, both **1.0.0**:

```
HarReader.Read(Stream) → LogReadResult          Format = Har
HarReader.ReadFile(string path) → LogReadResult

TextHostReader.Read(Stream) → LogReadResult     Format = Text
TextHostReader.ReadFile(string path) → LogReadResult

LogReadResult.Hosts : IReadOnlyList<LogHost>
LogHost.Host, Ports, HitCount, Sources, IsAddress
```

Existing, unchanged:

```
NetworkHelper.LookupAsync(name, DnsLookupOptions, CancellationToken) → DnsLookupResult
```

`DnsLookupOptions` from the same `DnsIqInput.TryCreate` the Lookup button uses, with `Type` forced to `A` then `AAAA`. Server / Port / Interface / Source stay the live knobs.

---

## Types to land

| Type | Project | Role |
|---|---|---|
| `LogFormat`, `LogHostSource`, `LogHost`, `LogReadResult`, `TextHostReader` | LogParser | Shared model and the dump scrape. |
| `HarReader` | LogParser.Har | The only HAR door. |
| `HarPageViewModel` | DnsIQ | Grid + Probe DNS + Cancel. No JSON. |
| `HarHostRow` | DnsIQ | One grid row. DNS columns filled by the probe. |

`App` gains `SetHarEnabled`. `DnsIqWindow` gains the File item and the tab. Code-behind still does not call `NetworkHelper` or `HarReader`.

---

## Window map

```
menu  File (Open capture…, Exit)   *.har;*.txt
nav   HorizontalTab  DnsIQ | Dashboard | HAR (disabled until parse) | Settings
client
  HAR
    caption = file name, or the parse error
    Probe DNS, Cancel
    grid: Host, Ports, Hits, Sources, DNS, Answers
```

No chart. No Requests / Seconds. No second Server box.

---

## Behavior

### Parse

1. Dialog. User cancel → no change.
2. `.har` and the content is a HAR object → `HarReader.ReadFile`. Oversize and missing `entries` throw; catch in the VM.
3. Else → `TextHostReader.ReadFile`. This is the `.txt` dump, the pasted email, the sheet saved as text, and a `.har` that is not JSON.
4. Success → replace rows, clear DNS columns, enable HAR tab, select it.
5. Failure (oversize, binary) → clear rows, disable HAR tab, status = message.

### Probe

1. If busy, ignore.
2. `DnsIqInput.TryCreate` from the live query fields (name may be blank; HAR probe does not use Name). Reject on a bad server / port / source the same way Lookup does.
3. Walk hosts. `IsAddress` → `Skipped`, no call.
4. A then AAAA. Map the pair onto the DNS column per R04-04.
5. Cancel → stop the walk. Rows already filled stay. Status `Cancelled`.
6. `finally` clears busy.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR04-01 | LogParser types + `TextHostReader`. Pack 1.0.0. No HAR, no sockets. | Open |
| 2 | PR04-02 | `HarReader` + corpus tests. Pack 1.0.0. | Open |
| 3 | PR04-03 | Pin both packages in suite `Directory.Build.props`. File → Open capture (`.har` / `.txt`). HAR tab + grid. No probe yet. | Open |
| 4 | PR04-04 | Probe DNS loop. A + AAAA. Cancel. Skipped addresses. | Open |
| 5 | PR04-05 | `dotnet test` Har project and suite host tests. Zero failures. | Open |
| 6 | PR04-06 | Owner gate on the two captures. | Owner |

### PR04-01

Land `Vestigium.Helpers.LogParser` in the Helpers solution (or pack the project the owner already created, same package id). Public surface is the types in R04-01 plus `TextHostReader` (R04-06). Tests in this project, off the wire:

- URL + mailto + bare domain in one `.txt` yield three hosts.
- `notes.txt` in that file is not a host.
- IPv4 literal is `IsAddress`.
- `e.g.` is not a host.

### PR04-02

`HarReader` as R04-02. Fixtures:

- `cannot-reach.har` — the three hosts, port 8443, the timed-out auth host present.
- `sso.har` — the ten hosts, no server IP as a host.
- `data-uri.har` — one real host plus a `data:` request; one host comes out.
- `missing-entries.har` — throws `InvalidDataException`.

Trim bodies. Keep the URLs, redirects, and `Location` headers that produced the host set.

### PR04-03

Package pin only after 1.0.0 is on the feed the suite already uses. OpenFileDialog filter `Capture or text (*.har;*.txt)|*.har;*.txt`. Valid HAR does not also scrape. Tab stays disabled on failure.

### PR04-04

Do not add a timeout box. Library default stands. Status bar `sent/total` is host index / host count.

### PR04-05

No test calls `LookupAsync`. Har tests do not need a network. Suite tests assert the VM does not enable the tab when `ReadFile` would have thrown — inject the result, do not read a fixture from the exe test unless the package is already referenced.

### PR04-06

Owner on the clone, against the original two files (not only the trimmed fixtures):

1. Cannot-reach HAR lists the three hosts. `q2valprod.services.idbs-cloud.com` is there.
2. SSO HAR lists the ten. CDN included.
3. Probe DNS against the lab resolver. Cancel mid-list stops it.
4. A row that says Resolved is not labeled Reachable.
5. Lookup and Dashboard still behave as v1.2.
6. A `.txt` dump of an email or a sheet lists the hosts in the prose and does not list `report.txt`. Probe DNS walks that list.

This plan does not mark the owner gate closed.

---

## Files this plan expects to touch

```
Vestigium.Helpers (or the two projects the owner created)
  src/Vestigium.Helpers.LogParser/…
  src/Vestigium.Helpers.LogParser.Har/…
  tests/Vestigium.Helpers.LogParser.Tests/…
  tests/Vestigium.Helpers.LogParser.Har.Tests/…

Vestigium.Suite.Network
  Directory.Build.props                                          pin 1.0.0
  src/Vestigium.Suite.Network.DnsIQ/DnsIqWindow.xaml             File item, HAR tab
  src/Vestigium.Suite.Network.DnsIQ/ViewModels/HarPageViewModel.cs   [NEW]
  src/Vestigium.Suite.Network.DnsIQ/ViewModels/HarHostRow.cs         [NEW]
  src/Vestigium.Suite.Network.DnsIQ/App.xaml.cs                  SetHarEnabled
```

Do not edit Helpers.Network. Do not edit Shell protocol code. Do not bump Themes or Charts.

When this plan finishes, move `PR04/` to `PR-Plans/Completed/PR04/` and point the queue README at the next slice.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No parser in the exe. No `ILogParser`. No TCP probe. No xlsx reader. Text scrape stays in LogParser. |
| Theodore | Status 0 entries still emit hosts. `report.txt` does not. Cancel does not throw. Oversize fails closed. Tests stay off the wire. |
| Simon | Resolved ≠ Reachable. Server IP is not a probe name. A valid HAR is read, not scraped. |

---

## Next action

PR04-01. Types and `TextHostReader` in `Vestigium.Helpers.LogParser`, pack 1.0.0. No HAR reader until that package exists.
