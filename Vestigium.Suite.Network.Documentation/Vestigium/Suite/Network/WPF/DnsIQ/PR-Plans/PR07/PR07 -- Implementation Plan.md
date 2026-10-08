# DnsIQ — PR07 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR07
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live. PR07-03 done. Settings ACL is next.
**Date:** 8 October 2026
**Binding:** [PR07 -- Requirements.md](PR07%20--%20Requirements.md) wins on this cut. Requirements_v1.2 wins on Lookup, the pulse shape, the settings path, and the Dashboard gate. PR06 wins on the capture print and on Skipped. This file wins on order.

**Goal:** One push that stops the process pin, the redirected resolver, and the code-behind lookup, without a new page.

**Not:** The KQL bar. Not a Check sheet. Not a settings-path move. Not a product-paper fold.

---

## Starting point

| Piece | Today | Required |
|---|---|---|
| Details | Window over a view that calls `NetworkHelper`. | Window stays. View binds. Check type owns the token. |
| Pulse | Unbounded in-flight list. | Cap 32. |
| Details fan-out | One task per answer. | Cap 8. |
| HAR | `ReadAllText` after `HarReader.ReadFile`. | Size gate 32 MB. One parse. |
| Settings | Inherited ProgramData ACL. | Administrators full, current user modify. |
| Dashboard | `Unlock` on prelude success. | Unlock only after Probe completes. |
| Reverse | Mapped IPv6 uses `ip6.arpa`. | Mapped uses `in-addr.arpa`. |
| Queue | README links folders that are Completed. | README links this folder. |

---

## Decision

Fix the holes. Do not add a surface.

| Call | Why |
|---|---|
| `DetailsCheck` in ViewModels | PR06 already named it and never landed it. The view is the wrong owner. |
| Caps are constants on the type that loops | 32 on the pulse. 8 on the details check. Not a setting. |
| 32 MB before any HAR parse | Cheap reject. A bigger file is not a diagnostic, it is a pin. |
| ACL on the directory, not a new path | v1.2 locked the path. Moving it splits two files. |
| `Unlock` moves to the pulse-complete branch | One call site. Lookup prelude must not reach it. |
| `IPAddress.IsIPv4MappedToIPv6` then `MapToIPv4` | The question name has to be the question that is sent. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| KQL on the capture grid this PR | Requirements §0. Packages without a buyer. |
| In-page details again | Owner already shipped the window. |
| Cap as a Settings field | Another knob. No operator asked. |
| LocalAppData settings | Breaks the v1.2 path and the shared-lab read. |
| Drop `EntryErrors` | The Error column is the HAR `_error` print. Keep it. Stop the second full read. |

---

## Types

| Type | Role |
|---|---|
| `DetailsCheck` | Side token. Cap 8. A/AAAA confirm for a name. PTR for an address. Writes Check and Question. Does not write the grid. |
| `ReverseName` | Existing. Mapped IPv6 takes the IPv4 branch. |
| `CaptureLoader` | Existing. Size gate. Errors from the parsed document. |

No new package. No new shell item. No new sheet.

---

## Behavior

1. Double-click still opens `CaptureDetailsWindow`. The view calls `DetailsCheck`, not `NetworkHelper`.
2. Pulse waits for a free slot before the next `LookupAsync` when 32 are in flight. Due-time slip stays.
3. Open capture checks length before parse. At or over 32 MB, status and return. No grid replace.
4. First settings save creates the directory and replaces the inherited Users-modify ACE.
5. Lookup success updates the answer grid and does not enable Dashboard.
6. Details on `::ffff:1.2.3.4` shows `4.3.2.1.in-addr.arpa` and sends that name.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR07-01 | `DetailsCheck`. View stops calling `NetworkHelper`. Close cancels. | Done. `0731412`. Cap is still 02. |
| 2 | PR07-02 | Pulse in-flight cap 32. Details cap 8. | Done. Cancelled wait is not Failed. |
| 3 | PR07-03 | HAR 32 MB gate. One parse for errors. | Done. Cap does not clear the grid. |
| 4 | PR07-04 | Settings directory ACL. Path unchanged. | |
| 5 | PR07-05 | Dashboard `Unlock` only after Probe completes. Mapped PTR. | |
| 6 | PR07-06 | Queue README. PR06 status line Closed. Host tests off the wire. | |
| 7 | PR07-07 | Owner gate on the clone. | |

### PR07-01

Move the check loop out of `CaptureDetailsView.xaml.cs`. The view sets ItemsSource and listens for column changes. `Lookup` and `Lookup + Probe` stay commands on `MainViewModel`.

Done. `DetailRow` and `DetailsCheck` own the token and the column writes. The view has no `NetworkHelper` call. Close and a new `Show` cancel. Fan-out is still unbounded. That is PR07-02.

### PR07-02

Done. Pulse waits for a slot at 32, then sends. Due time still slips if the slot is late. Details waits at 8, including the name confirm. A cancelled wait throws and does not write Failed.

### PR07-03

Done. A `.har` at 32 MB sets status and returns. The grid stays. Text is not this gate. `EntryErrors` is gone. `Unique` keeps the error `HarReader` already parsed.

### PR07-04

After `CreateDirectory`, set the directory ACL. Do not fail the launch if the ACL set fails on a locked existing folder. Status on save, not a throw through startup. Existing world-writable folder is noted, not silently treated as fixed.

### PR07-05

Delete the prelude `Unlock`. Call it where the pulse summary is posted. `ReverseName` maps before the nibble loop.

### PR07-06

README status is this plan. Tests: mapped arpa name, cap constants exist, HAR gate rejects a stub length, Dashboard flag stays false when only Lookup ran. No socket.

### PR07-07

Owner on the clone:

1. Lookup a name. Dashboard stays disabled.
2. Probe finishes. Dashboard enables.
3. Open a small HAR. Error column still fills. A file over the cap does not.
4. Double-click an address. Question name is the arpa form. Window still scrolls.
5. Settings folder ACL is not Users-modify.

This plan does not mark the owner gate closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DetailsCheck.cs          [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/ReverseName.cs
src/Vestigium.Suite.Network.DnsIQ/ViewModels/CaptureLoader.cs
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DnsIqSettingsStore.cs
src/Vestigium.Suite.Network.DnsIQ/ViewModels/MainViewModel.cs
  pulse cap, Unlock call site
src/Vestigium.Suite.Network.DnsIQ/Views/CaptureDetailsView.xaml.cs
  no NetworkHelper

Vestigium.Suite.Network.Documentation/.../DnsIQ/PR-Plans/README.md
Vestigium.Suite.Network.Documentation/.../DnsIQ/PR-Plans/Completed/PR06/PR06 -- Requirements.md
  status Draft → Closed

tests/… mapped arpa, caps, HAR gate, dashboard flag
```

Do not add KQL package references. Do not edit Helpers.Network. Do not move the settings path.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No new page. No second writer. Checks do not take the main job token. |
| Theodore | Close cancels. A failed check does not fail the page. A HAR over the cap does not replace the grid. ACL failure does not kill startup. |
| Simon | Open-file probe still skips addresses. Lookup still does not reverse. Dashboard stays dark until Probe. The arpa name on the line is the name that was sent, including mapped. |

---

## Next action

PR07-04. Settings directory ACL. Path unchanged. KQL stays parked.
