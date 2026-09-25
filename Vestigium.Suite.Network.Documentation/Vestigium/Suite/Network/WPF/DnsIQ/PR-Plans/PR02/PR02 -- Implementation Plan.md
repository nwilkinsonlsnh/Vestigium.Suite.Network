# DnsIQ — PR02 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR02  
**Host:** `Vestigium.Suite.Network.DnsIQ`  
**APPID:** `DnsIQ`  
**Status:** Live  
**Date:** 25 September 2026  
**Binding:** Requirements win on the window. This file wins on the slice. Helpers.Network 1.2.0 wins on protocol. Do not invent a second DNS client.

**Goal:** Make the window tell one story. One name. One Go. A status line that names the resolver, the rcode, and the time. A grid of records.

**Not:** A protocol library. Not Probe as a second dump of the same rows. Not DoH. Not AXFR. Not CSV. Not a chart. Not a server roster editor.

---

## Why this slice exists

PR01 shipped a usable window. Then the owner used it.

What we learned on the glass:

- Blank Server was not Pi-hole. A/AAAA went to `GetHostAddresses` (TTL 0). The adapter already listed `172.16.0.5`.
- Type All + an explicit server produced A, AAAA, MX, NS, TXT, SOA. That is the product.
- Lookup and Probe run the same `LookupAsync`. Two buttons painted the same grid. Status was the only tell (`NoError` vs `Answered`). That is fog.
- `Txt` / `Aaaa` / `Mx` were enum `ToString`, not wire names.

PR01 is first-and-ten. PR02 is the first week of use. Do not reopen AXFR because the grid works.

---

## Decision

| Call | Why |
|---|---|
| One command. Label **Lookup**. Remove the Probe button from this window. | Probe is `LookupAsync` plus a three-state map. The rcode already says timeout / refused / answered. |
| `ProbeDns` stays in Helpers. DnsIQ stops calling it. | Alive/dead belongs to a watch or a campaign host, not a record reader. |
| Type default stays **All**. Single type stays in the combo for isolation. | Owner asked for `cnn.com` → every first-and-ten type. |
| Empty Server binds the first non-loopback adapter DNS (IPv4 first). Already in `DnsIqInput.FirstConfiguredDns`. Keep it. | That is Pi-hole on this workstation (`172.16.0.5`). Do not go back to OS `GetHostAddresses` for the happy path. |
| Status line after a run: `rcode · server · ms` (and type count if All). | So you can see you hit Pi-hole without opening Wireshark. |
| Grid columns stay Type, Name, Data, Ttl. Display names stay A / AAAA / CNAME / MX / NS / PTR / TXT / SOA. Sort by Type then Name. | TXT pile is real. Sort beats a second list. |
| Failed still clears. Cancel still cancels the whole walk. One in-flight. | D3 / D8 do not move. |
| Papers move to Requirements 1.1 / Design 1.1 in this slice. | v1.0 required two buttons and default A. The window already left that paper. Write it down. |

### Rejected

| Idea | Why out |
|---|---|
| One button that runs Lookup and Probe and shows both | Two statuses, one grid. Same fog with more text. |
| Probe stays, gridless | Honest, extra chrome. Owner asked how to tell them apart. Removing the twin is cheaper. |
| Type ANY on the wire | Public resolvers ignore or refuse 255. All is eight questions, not ANY. |
| `LookupManyAsync` | That API is many names, same type. Wrong door. |
| Auto-fill the Server box with `172.16.0.5` | Status line is enough. The box stays the override. Empty still means “use adapter DNS”. |
| CSV / favorites / history | After the status line does not lie. |
| Pack Helpers just to get `GetOsDnsServers` | Host already reads `GetAdapters().DnsServers` on 1.2.0. Pin stays 1.2.0 until owner packs a Network bump. |

---

## Window map (after PR02)

Top: Name, Server (optional override), Type (`All` default + eight), Interface, Source, **Lookup**, Cancel.  
Middle: Status — Idle / Running / `NoError · 172.16.0.5 · 84 ms · 8 types` / Failed / Cancelled.  
Bottom: read-only grid, sorted.

No Probe button. No timeout box. No port box. No chart.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR02-01 | Requirements 1.1 + Design 1.1. One Lookup. All default. Empty Server = adapter DNS. Status carries server and ms. | Open |
| 2 | PR02-02 | Remove Probe command and button. Status formatter. Keep All walk on Lookup only. | Open |
| 3 | PR02-03 | Sort grid. Keep display names. No new columns. | Open |
| 4 | PR02-04 | Host tests: All accepts; empty server binds a parseable IP or null; Probe command gone from VM. No wire. | Open |
| 5 | PR02-05 | Owner: `cnn.com` + blank Server hits Pi-hole Query Log for more than A. Status shows `172.16.0.5`. No Probe button. | Owner |

### PR02-01

Edit `Requirements_v1.0.md` in place to 1.1 (same file, version bump + document control row) **or** add `Requirements_v1.1.md` and point the queue at it. Prefer a 1.1 file so 1.0 stays the first-and-ten lock. Design the same way.

Must say:

- Probe is out of this window.
- Type default All.
- Empty Server = first configured adapter DNS, not `GetHostAddresses`.
- Status includes resolver IP and elapsed when the library returns them.

### PR02-02

`MainWindow.xaml`: delete Probe.  
`MainViewModel`: delete `ProbeAsync` / `_probeJob`. Cancel the token only.  
Status: after each walk, take `result.Server` (or options.Server) and `Elapsed` from the last successful call, or sum elapsed across the All walk. Do not invent a stopwatch in the host if the library already measured — for All, a host `Stopwatch` around the loop is acceptable.

### PR02-03

After the walk, order `Answers` by Type then Name then Data. Do it in the VM. No ICollectionView required.

### PR02-04

Keep `DnsIqInputTests`. Add: Type All sets `AllTypes`. Empty server is null or an IP. Do not assert a specific Pi-hole address. Do not call `LookupAsync`.

### PR02-05

Owner on the clone. This agent does not mark PR02 closed.

---

## Files this plan expects to touch

```
Vestigium/.../WPF/DnsIQ/Requirements_v1.1.md          [NEW]
Vestigium/.../WPF/DnsIQ/Design_v1.1.md                [NEW]
src/Vestigium.Suite.Network.DnsIQ/MainWindow.xaml
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MainViewModel.cs
tests/Vestigium.Suite.Network.Tests/DnsIqInputTests.cs
```

Do not edit Shell. Do not bump package pins in this slice. Do not add Probe back “because v1.0 said so” — v1.1 is the window paper for this slice.

When this plan finishes, move the whole `PR02/` folder to `PR-Plans/Completed/PR02/` and idle the queue README. Move `PR01/` under `Completed/` at the same time if it is still sitting live.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No second grid. No Probe button leftover. No timeout box. |
| Theodore | Cancel still kills an All walk. Tests stay off the wire. |
| Simon | Status server is the resolver we sent to, not a pretty name like `pi.hole`. |

---

## Next action

PR02-01. Write Requirements 1.1 and Design 1.1 so the window paper matches the product the owner already asked for.
