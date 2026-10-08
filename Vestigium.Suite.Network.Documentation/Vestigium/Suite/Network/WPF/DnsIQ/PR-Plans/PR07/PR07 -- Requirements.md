# DnsIQ — PR07 Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR07-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Live. Binding for this push until the implementation plan closes it.
**Date:** 8 October 2026
**Product papers (unchanged until fold):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)
**Prior slice:** [Completed/PR06](../Completed/PR06/PR06%20--%20Requirements.md). Shipped. Still marked Draft inside that folder. This file does not reopen the print.

**One sentence:** Close the capture/details drift and the holes that can pin the process or point the resolver at a server the operator did not choose.

**This version is not** a KQL bar, a reverse page, a paste box, DNSSEC, DoH, AXFR, a Check sheet, or a new chart.

---

## 0. Decision

| Call | Why |
|---|---|
| PR07 is not the KQL bar | PR06 parked it. Pins exist (`VestigiumKqlVersion` 1.0.4, `VestigiumQueryBarVersion` 1.0.3). DnsIQ does not reference them. No buyer in this push. |
| Details stays a window | Owner commits on 7 Oct opened `CaptureDetailsWindow` from capture double-click and from lookup rows. PR06's page-swap is superseded. Do not revert it. |
| Checks leave the code-behind | `CaptureDetailsView` calls `NetworkHelper.LookupAsync`. v1.2 D2 says code-behind does not. The check belongs on a view-model type. |
| Cap the fan-out | Pulse queues every request before any must finish. Details fires one PTR per answer with no cap. Both can pin the process and the resolver. |
| Cap the HAR read | `CaptureLoader.EntryErrors` does `File.ReadAllText` plus `JsonDocument.Parse` with no size gate, after `HarReader` already read the file. |
| Settings path stays | v1.2 locks `ProgramData\Vestigium\Settings\Diagnostics\DnsIQ`. Do not move it. Tighten the ACL on create. |
| Dashboard stays Probe-gated | v1.2 D11. `RunJobAsync` calls `Dashboard.Unlock()` after a successful Lookup prelude. That is a defect. |
| Mapped IPv6 PTR is IPv4 | `ReverseName` treats `::ffff:a.b.c.d` as `ip6.arpa`. The question that was sent is the wrong question. |
| Export open holds | `MayOpen` requires the saved path equal the dialog path and end in `.xlsx`. Do not widen `Process.Start`. |
| Check column stays off the sheet | PR06 parked it. A sheet of in-flight PTR is a lie. |

Rejected: KQL on Capture. Rejected: a shell reverse page. Rejected: auto-PTR from the Name box. Rejected: moving settings to LocalAppData. Rejected: writing the Check column. Rejected: reverting details to an in-page swap.

---

## 1. What this version is

The same DnsIQ window. Same Lookup, same pulse, same open-file probe, same one-line capture print.

| Surface | Today | PR07 |
|---|---|---|
| Details | `CaptureDetailsWindow` hosts `CaptureDetailsView`. View calls `NetworkHelper`. | Window stays. Checks run from a view-model. View binds. |
| Pulse | Up to 10_000 `LookupAsync` tasks queued with no in-flight cap. | In-flight cap 32. Timing of request 1 and request N unchanged. |
| Details checks | One PTR task per answer, plus A/AAAA, all at once. | Same questions. Cap 8 in flight. Close still cancels. |
| HAR errors | Second full-file read. No size gate. | One read. Refuse above 32 MB. Grid does not open. |
| Settings | `CreateDirectory` under ProgramData. Inherited ACL. | Directory created, then ACL is Administrators full, current user modify. Users are not granted modify. |
| Dashboard | Unlock on Lookup prelude success. | Unlock only after a Probe completes. |
| Reverse name | IPv4 → `in-addr.arpa`. IPv6 → `ip6.arpa`. Mapped stays IPv6. | IPv4-mapped IPv6 uses the IPv4 form. |
| Papers | Queue README still points at PR05 / PR06 live folders. Those folders are under Completed. | README points at this folder. |

---

## 2. Evidence

| # | Fact | Where |
|---|---|---|
| E1 | Queue README status is PR05, next slice PR05-05, PR06 drafted. Tree has both under `Completed/`. No live PR05 or PR06 folder. | `DnsIQ/PR-Plans/README.md`. Tree at `9fb82ca`. |
| E2 | PR06 required a view on the Capture page, not a dialog, not a shell item. Commits on 7 Oct opened a shared window from the capture grid and from lookup rows. | PR06 R06-02. `HarView.OnOpenDetails`. `CaptureDetailsWindow.Open`. |
| E3 | `CaptureDetailsView.RunAsync` builds `DnsLookupOptions` from `MainViewModel` and calls `NetworkHelper.LookupAsync`. | `Views/CaptureDetailsView.xaml.cs`. |
| E4 | `ProbeAnswersAsync` adds a task per request into `inflight` and only drains completed ones. `PulsePlan.MaxRequests` is 10_000. | `MainViewModel.cs`. `PulsePlan.cs`. |
| E5 | `EntryErrors` reads the whole HAR with `File.ReadAllText` after `Load` already called `HarReader.ReadFile`. `JsonException` returns an empty map. No length check. | `CaptureLoader.cs`. |
| E6 | Settings root is `CommonApplicationData\Vestigium\Settings\Diagnostics\DnsIQ`. `Save` calls `Directory.CreateDirectory` and writes `Server`, `Source`, `Port`. A second user who can replace that file redirects the next launch. | `DnsIqSettingsStore.cs`. Requirements_v1.2 §3. |
| E7 | `RunJobAsync` calls `Dashboard?.Unlock()` when the prelude succeeds, before the pulse branch. v1.2 D11 says Dashboard stays disabled until Probe completes. | `MainViewModel.RunJobAsync`. Requirements_v1.2 D11. |
| E8 | `ReverseName.TryPtr` branches on `AddressFamily.InterNetworkV6` with no mapped check. `IPAddress.TryParse("::ffff:1.2.3.4")` is IPv6. | `ReverseName.cs`. |
| E9 | Export open is `Process.Start` with `UseShellExecute` only after `DnsIqWorkbook.MayOpen`. | `DnsIqExport.cs`. `DnsIqWorkbook.MayOpen`. |
| E10 | Product paper still says three pages, protocol 1.2.0, and links `PR-Plans/PR03/`, which is not on disk. Pin in `Directory.Build.props` is Network 1.5.0. | `Requirements_v1.2.md`. `Directory.Build.props`. |

---

## 3. Must change

### R07-01 Checks are not a view concern

`CaptureDetailsView` does not call `NetworkHelper`. A view-model type owns the token, the cap, and the column writes. The view binds Check and Question. Close and a new `Show` cancel the previous token. Main Cancel still does not have to cancel details. The capture grid still does not change.

### R07-02 Fan-out has a ceiling

Pulse keeps at most 32 lookups in flight. A request waits for a slot. Due time still slips if late. Request 1 is still at t=0. Request N is still at t=X when N>1.

Details keeps at most 8 checks in flight for one host. Pending stays the first Check value. A throw on one line is Failed on that line. The others keep running.

### R07-03 HAR open has a size gate

A `.har` at or above 32 MB does not open. Status says why. No second full-file read on the accept path. `EntryErrors` reads from the document already parsed, or it does not run. A non-HAR text scrape is unchanged.

### R07-04 Settings directory is not inherited modify

Path stays `C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`. On create, ACL is Administrators full control and the current user modify. Inherited Users-modify is removed on that directory. Corrupt file still falls back to defaults. No secrets in the file.

### R07-05 Dashboard unlock is the pulse

`Unlock` runs only after a Probe completes. A Lookup, including a failed one, does not enable Dashboard. A cancelled pulse does not enable it.

### R07-06 Mapped address asks the IPv4 question

`ReverseName` maps IPv4-mapped IPv6 to the four-octet `in-addr.arpa` name. A real IPv6 address still uses `ip6.arpa`. A non-address still returns false. The question name on the details line is the name that was sent.

### R07-07 Queue paper matches the tree

`PR-Plans/README.md` points at this folder. It does not link a live PR05 or PR06 folder. Completed PR06 status in that file is Closed, not Draft.

---

## 4. Must not change

- Lookup on the DnsIQ tab. An address typed there is still a forward question.
- Open-file probe. An address is still Skipped and is not sent.
- Capture print: one line per answer. Export Capture columns.
- Details as one shared window. Not a shell item.
- `MayOpen` and the xlsx-only shell open.
- Helpers.Network, LogParser, ClosedXml. No pin move.
- KQL packages. Not referenced.

---

## 5. Failure

| Case | Required behavior |
|---|---|
| HAR ≥ 32 MB | No grid. Status names the cap. Process stays up. |
| Pulse at 10_000, resolver slow | Never more than 32 in flight. Cancel still stops the wait. |
| Details host with 40 answers | Never more than 8 PTR in flight. Close cancels the rest. |
| Settings file replaced with a bad server string | Next launch uses it. That is the operator file. A second account must not be able to write it after R07-04. |
| `GetNeighbors`-class throw does not apply | This host does not read the neighbor table. |
| Mapped `::ffff:1.2.3.4` | Question is `4.3.2.1.in-addr.arpa`. Not an `ip6.arpa` name. |
| Lookup success | Dashboard nav stays disabled. |
| Export open of a non-xlsx | Refused. Status says the file was written and not opened. |

Tuesday-at-2am failure this slice exists to stop: a shared lab PC whose ProgramData settings were rewritten, so the next DnsIQ launch sends the pulse at a resolver the operator did not pick; and a 200 MB HAR that pins the process on the second parse.

---

## 6. Acceptance

1. Details window still opens from a capture line and from a lookup row. The view file has no `NetworkHelper` call.
2. A 10_000-request pulse does not hold more than 32 lookups.
3. A details host with more answers than 8 does not hold more than 8 checks.
4. A HAR at the cap does not open. A small HAR still fills the grid and the Error column.
5. New settings directory ACL does not grant Users modify.
6. Lookup does not enable Dashboard. A finished Probe does.
7. `::ffff:1.2.3.4` details question is the IPv4 arpa name.
8. Queue README links this folder. KQL packages are still not referenced by DnsIQ.

---

## 7. Out

| Item | Why |
|---|---|
| KQL bar | Named and refused in PR06. Packages are not the buyer. |
| Check column sheet | Export must not wait. Pending is not a print. |
| Reverse page on the shell | Lookup must not change meaning. |
| Paste box | No buyer. |
| Move settings off ProgramData | v1.2 path lock. ACL is the fix. |
| Product paper v1.3 fold | E10 is real. Not this push. Fold after this slice lands. |
| DNSSEC, DoH, AXFR | Out of v1.2. Still out. |
| Pin bumps | Network is already 1.5.0. This slice does not need a new package. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR07 | 8 Oct 2026 | Cut opened. Not the KQL bar. Details window kept. Checks leave the view. Fan-out and HAR capped. Settings ACL tightened. Dashboard gate restored. Mapped PTR fixed. |
