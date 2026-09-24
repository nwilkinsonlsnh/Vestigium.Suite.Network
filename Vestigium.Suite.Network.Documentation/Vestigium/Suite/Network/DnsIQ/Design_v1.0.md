# DnsIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.DnsIQ`

## Window

Top: Name, Server, Type combo (A default; AAAA, CNAME, MX, NS, PTR, TXT, SOA), bind fields, Lookup, Probe, Cancel.  
Middle: Status.  
Bottom: answer grid — Type, Name, Data, Ttl.

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.DnsIQ)`. |
| `MainWindow` | DataContext only. |
| `MainViewModel` | Name, Server, RecordType, Bind, Status, `ObservableCollection<AnswerRow>`. Lookup + Probe + Cancel commands. |
| `AnswerRow` | Type, Name, Data, Ttl. |

## Flow

1. Reject blank name. If Server set, parse as IP.
2. Lookup → `LookupAsync(Name, DnsLookupOptions { … bind, server, type })`.
3. Probe → `ProbeDns(...)`. Status = Answered / Refused / TimedOut. Grid may stay empty.
4. Map `result.Answers` to rows. Empty answers keep the rcode on Status.
5. Failed → clear grid (Requirements D8).

## Out of this design

`LookupManyAsync`. DoH. AXFR. Server roster.
