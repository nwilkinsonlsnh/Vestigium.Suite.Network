# DnsIQ — PR02 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR02  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 25 September 2026  
**Binding:** Requirements win on the window. This file wins on the slice. Helpers.Network 1.2.0 wins on protocol. Do not invent a second DNS client.

**Goal:** Two buttons with two jobs. Lookup reads records. Probe says whether the resolver answered. Status names the resolver and the time. The grid belongs to Lookup only.

**Not:** A protocol library. Not Probe as a second dump of the same rows. Not DoH. Not AXFR. Not CSV. Not a chart. Not a server roster editor.

---

## Why this slice exists

PR01 shipped a usable window. Then the owner used it.

What we learned on the glass:

- Blank Server was not Pi-hole. A/AAAA went to `GetHostAddresses` (TTL 0). The adapter already listed `172.16.0.5`.
- Type All + an explicit server produced A, AAAA, MX, NS, TXT, SOA. That is the product.
- Lookup and Probe run the same `LookupAsync`. Two buttons painted the same grid. Status was the only tell (`NoError` vs `Answered`). That is fog.
- `Txt` / `Aaaa` / `Mx` were enum `ToString`, not wire names.
- Owner is fine keeping both buttons **if the jobs are different**.

PR01 is first-and-ten. PR02 is the first week of use. Do not reopen AXFR because the grid works.

---

## Decision

| Call | Why |
|---|---|
| Keep **Lookup** and **Probe**. Split the jobs. | Owner wants both. They only earn two buttons if the glass differs. |
| Lookup owns the grid. Status = `rcode · server · ms` (· type count if All). | Record reader. |
| Probe does **not** write the grid. Status = `Answered|Refused|TimedOut · server · ms`. Last Lookup rows stay put. | Reachability. NxDomain is still Answered (library map). |
| Probe All = one `ProbeDns` per first-and-ten type. Status: Answered if any type answered, else TimedOut, else Refused. Still no grid writes. | Same walk as Lookup, different surface. |
| Type default stays **All**. Single type stays in the combo. | Owner asked for `cnn.com` → every first-and-ten type. |
| Empty Server binds the first non-loopback adapter DNS (IPv4 first). Keep `FirstConfiguredDns`. | Pi-hole on this workstation (`172.16.0.5`). |
| Grid columns stay Type, Name, Data, Ttl. Wire names. Sort by Type then Name. | TXT pile is real. Sort beats a second list. |
| Failed on Lookup clears the grid. Probe never clears it. Cancel cancels the in-flight job. | D3 / D8 stay; Probe is not Failed-on-empty. |
| Papers: Requirements 1.1 / Design 1.1. | v1.0 defaulted Type to A and let both buttons fill the list. Write the split down. |

### Rejected

| Idea | Why out |
|---|---|
| Delete Probe | Owner wants the button if the job is distinct. |
| One button that runs both | Two statuses fighting one line. |
| Probe fills the same grid | That is PR01 fog. |
| Type ANY on the wire | Public resolvers ignore or refuse 255. All is eight questions. |
| `LookupManyAsync` | Many names, same type. Wrong door. |
| Auto-fill the Server box | Status line names the IP. Empty still means adapter DNS. |
| CSV / favorites / history | After the two jobs stop lying. |
| Pack Helpers this slice | Host already reads `GetAdapters().DnsServers` on 1.2.0. |

---

## Window map (after PR02)

Top: Name, Server (optional override), Type (`All` default + eight), Interface, Source, **Lookup**, **Probe**, Cancel.  
Middle: Status — Idle / Running / `NoError · 172.16.0.5 · 84 ms · 8 types` / `Answered · 172.16.0.5 · 12 ms` / Failed / Cancelled.  
Bottom: read-only grid, sorted. Written by Lookup only.

No timeout box. No port box. No chart.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR02-01 | Requirements 1.1 + Design 1.1. Two buttons. Split surfaces. All default. Adapter DNS. Status carries server and ms. | Open |
| 2 | PR02-02 | Probe stops writing `Answers`. Lookup status formatter. Probe status formatter. | Open |
| 3 | PR02-03 | Sort grid after Lookup. Keep display names. No new columns. | Open |
| 4 | PR02-04 | Host tests stay off the wire. All accepts. Empty server is null or an IP. | Open |
| 5 | PR02-05 | Owner: Lookup fills the grid. Probe changes Status only. Blank Server shows `172.16.0.5` on the line. Pi-hole log sees more than A on All. | Owner |

### PR02-01

Add `Requirements_v1.1.md` and `Design_v1.1.md`. Keep v1.0 as the first-and-ten lock. Point the queue at 1.1.

Must say:

- Lookup writes the grid. Probe does not.
- Type default All.
- Empty Server = first configured adapter DNS.
- Status includes resolver IP and elapsed.

### PR02-02

`MainViewModel.ProbeAnswersAsync`: run `ProbeDns`, set Status, do not call `AppendAnswers`.  
Lookup keeps `AppendAnswers`.  
Format Status from `result.Server` / options.Server and elapsed. All-walk may use a host `Stopwatch` around the loop.

### PR02-03

After Lookup, order `Answers` by Type then Name then Data in the VM.

### PR02-04

Keep `DnsIqInputTests`. Do not assert a specific Pi-hole address. Do not call `LookupAsync` or `ProbeDns`.

### PR02-05

Owner on the clone. This agent does not mark PR02 closed.

---

## Files this plan expects to touch

```
Vestigium/.../WPF/DnsIQ/Requirements_v1.1.md          [NEW]
Vestigium/.../WPF/DnsIQ/Design_v1.1.md                [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MainViewModel.cs
tests/Vestigium.Suite.Network.Tests/DnsIqInputTests.cs
```

Do not edit Shell. Do not bump package pins in this slice.

When this plan finishes, move the whole `PR02/` folder to `PR-Plans/Completed/PR02/` and idle the queue README. Move `PR01/` under `Completed/` at the same time if it is still sitting live.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | Probe must not refill the grid. No timeout box. |
| Theodore | Cancel still kills an All walk. Tests stay off the wire. |
| Simon | Status server is the resolver IP, not `pi.hole`. |

---

## Next action

PR02-01. Write Requirements 1.1 and Design 1.1 with the split surfaces.
