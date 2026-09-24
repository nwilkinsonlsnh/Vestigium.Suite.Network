# DnsIQ — Requirements v1.0

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-SRS-001  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.DnsIQ`  
**Kind:** WPF exe. APPID `DnsIQ`.  
**Library jobs:** `LookupAsync`, `LookupManyAsync`, `ProbeDns`.

## Intent

One name, optional server. Show answers or the rcode. Not a recursive scanner.

## Locks

| # | Lock |
|---|---|
| 1 | Startup calls `HostLog.Initialize(HostIds.DnsIQ)`. |
| 2 | Bind fields apply when the library job accepts them. |
| 3 | Wire peer is the queried server only. |
| 4 | `ProbeDns` is Answered / Refused / TimedOut. |
| 5 | No HTTP. No OUI. |

## Non-goals

Zone transfer UI. Port sweep. HTTP reachability.
