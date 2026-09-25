# DnsIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.DnsIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `DnsIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0  
**Binding:** Helpers.Network Requirements v1.6 wins on protocol. This file wins on the window.

First and ten means the smallest window a lab tech can use on Tuesday. One name. One optional server. Answers or an rcode. Then we punt everything else.

---

## 1. What ships

One main window. Two jobs from the library. No second window. No campaign. No plot.

| Job | Library door | First-and-ten |
|---|---|---|
| Lookup | `NetworkHelper.LookupAsync(name, DnsLookupOptions)` | **Required.** Default button. |
| Probe | `NetworkHelper.ProbeDns(name, …)` | **Required.** Second button. Answered / Refused / TimedOut. |
| Lookup many | `LookupManyAsync` | **Out.** One name this release. |

---

## 2. Window

| Field | Rule |
|---|---|
| Name | Required. Trim. Blank → reject, do not call the library. Placeholder may be `localhost`. |
| Server | Optional. Empty = OS resolver / library default. If set, must parse as IPv4 or IPv6. Garbage server → reject before send. |
| Type | Optional. Default **A**. First-and-ten types: **A, AAAA, CNAME, MX, NS, PTR, TXT, SOA**. Anything else is out. |
| Interface index | Optional integer. Empty or `0` = not pinned. Negative → reject. Do not rewrite `0` to `1`. |
| Source address | Optional. Empty = not pinned. If set, must parse as an IP. |
| Lookup | Runs `LookupAsync`. Disabled while a job is running. |
| Probe | Runs `ProbeDns`. Disabled while a job is running. |
| Cancel | Cancels the in-flight token. Status becomes Cancelled. |
| Status | Idle / Running / rcode / Answered / Refused / TimedOut / Failed / Cancelled. |
| Results | Read-only list. One row per answer: Type, Name, Data, TTL when the library returns it. Empty answers + success rcode still show the rcode. |
| Error | Library throw or reject message in a status line. No message box required this release. |

Layout is one form + one list. No tab control. No chart host.

---

## 3. Behavior

| # | Rule |
|---|---|
| D1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.DnsIQ)` before the window shows. JSONL lands under `%ProgramData%\Vestigium\Logs\DnsIQ\`. |
| D2 | MVVM. `MainViewModel` owns state. Code-behind does not call `NetworkHelper`. |
| D3 | One in-flight job. Second click is ignored until the first finishes or cancel. |
| D4 | Bind fields pass through on both jobs. Shell does not apply bind; the library does. |
| D5 | Wire peer is the queried server only (library lock 28). The host does not accept answers from a different IP. |
| D6 | PTR: if the name looks like an address, the host may send it as typed. It does not invent an `.in-addr.arpa` rewrite this release unless the library already does. |
| D7 | Timeout is the library default unless the host exposes Timeout. First-and-ten may omit a timeout box and use the library default. |
| D8 | Failed lookup is a result, not a crash. Catch, set Status = Failed, keep the last good list or clear it — pick **clear**. |
| D9 | No `nslookup.exe`, no `dig`, no `Resolve-DnsName`. |
| D10 | Consume Network / Analytics / Charts / Logging as package references. Charts is referenced by Shell; DnsIQ first-and-ten **does not draw**. |

---

## 4. Acceptance (this release is done when)

1. Window opens, APPID folder exists after first run.
2. Blank name does not send.
3. `localhost` Lookup returns at least a status line without throwing out of the UI.
4. Probe on a silent or refused name returns Answered, Refused, or TimedOut — not an unhandled exception.
5. Negative interface index is rejected in the host or by `EgressBind.Validate` before send.
6. Cancel stops a running lookup.
7. No HTTP client, no OUI button, no zone-transfer button, no server list walker.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| `LookupManyAsync` | Second name is a second click, not a list this release. |
| DoH / DoT | Library does not speak HTTPS DNS. |
| AXFR / IXFR | Zone transfer UI is denied. |
| Recursive NS walk | That is a scanner. |
| Server roster / DHCP option-15 editor | Nice, not Tuesday. |
| History / favorites / export CSV | After the list works. |
| Charts of lookup RTT | No series owner yet. |
| Campaign / scheduler | PingIQ / ShareIQ only. |
| Echo, trace, pathping | Wrong host. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.DnsIQ`, `BindFields` |
| Protocol | `Vestigium.Helpers.Network` only |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. Lookup + Probe. One name. |
