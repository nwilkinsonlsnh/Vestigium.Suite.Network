# DnsIQ — PR02b Pulse

**Parent:** [PR02 -- Implementation Plan.md](PR02%20--%20Implementation%20Plan.md)  
**Slices:** PR02-06 · PR02-07 · PR02-08  
**Date:** 25 September 2026

This file is the resolver pulse. Chrome must already exist (PR02a). Parent locks win.

---

## Jobs

| Button | Question | Grid |
|---|---|---|
| Lookup | What records does this name have? | Writes it. Clears on Failed. |
| Probe | How does this resolver take N bursts over X seconds? | Does not touch it. |

Same `DnsIqInput`. Same adapter DNS when Server is empty. Same Type combo.

---

## PR02-06 — DnsIQ tab knobs

Add two `VestigiumNumericUpDown` controls on the DnsIQ tab, next to Probe:

| Knob | Bind | Default | Min | Max | Increment |
|---|---|---|---|---|---|
| Bursts (N) | int | 10 | 1 | 60 | 1 |
| Seconds (X) | int | 10 | 1 | 60 | 1 |

Unsigned. Immediate. Settings defaults seed these on first load.

Lookup status formatter (one shot or All walk):

`{rcode} · {server} · {ms} ms` plus type count when All.

Sort the grid by Type, then Name, after Lookup. Display names stay A / AAAA / CNAME / MX / NS / PTR / TXT / SOA.

---

## PR02-07 — Probe loop

Host loop. One `CancellationTokenSource`. Do not add a Network campaign type.

```
spacing = N == 1 ? 0 : X / (N - 1)   // seconds, burst 1 at t=0, burst N at t=X
for i in 1..N
    wait until t >= (i-1)*spacing    // if late, start now (slip)
    run one burst (WhenAll if All)
    push status bar: pulse i/N · server · this-burst max ms
    if cancel: stop, Left = Cancelled, keep grid
summary: answered/timeout/refused · min/med/max ms · N/X bursts per second
```

### One burst

- Type = AAAA (or any single) → one `LookupAsync` (or `ProbeDns` if you only need three-state; prefer `LookupAsync` so elapsed/rcode/server stay consistent with Lookup).
- Type = All → eight `LookupAsync` with `Task.WhenAll`. One type timeout does not cancel the other seven.
- Do not `AppendAnswers`.

### Counts

Per burst and over the run: sent, answered (`NoError` or `NxDomain` still count as answered), timeout, refused. Min / median / max of elapsed on **answered** queries. Timeouts do not go into the median as 0.

### Status bar during pulse

| Slot | Text |
|---|---|
| Left | `pulse {i}/{N}` |
| Center | server IP |
| Right | `{maxMs} ms` this burst |

End line example:

`10/10 · 172.16.0.5 · med 14 ms · 1.0 burst/s · 0 timeout`

---

## PR02-08 — tests (no wire)

Keep `DnsIqInputTests`. Add pulse-plan tests in the host test project:

- N=0 or N=61 rejects before the loop.
- X=0 or X=61 rejects.
- N=1 → spacing 0.
- N=10, X=10 → spacing 10/9 seconds (assert the formula, not a sleep).
- Type All → `AllTypes` true (already exists).

Do not call `LookupAsync` in tests. Do not assert `172.16.0.5`.

---

## Done when

- Lookup still fills and sorts the grid.
- Probe for 10 bursts / 10 s updates the bar each burst and leaves the grid alone.
- All + Probe hits Pi-hole as parallel eights, then a gap, then another eight — not a 80-query flood with no pause.
- Cancel stops the wait and the in-flight burst.
