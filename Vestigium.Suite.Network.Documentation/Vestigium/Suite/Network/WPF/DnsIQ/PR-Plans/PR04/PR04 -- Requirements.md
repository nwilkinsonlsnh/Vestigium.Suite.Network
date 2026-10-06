# PR04 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR04-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Draft
**Date:** 6 October 2026
**Product papers (unchanged until this PR lands):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)

This file is the **definition of done for PR04**. It is not a new product specification. When PR04 is accepted, fold the window delta into Requirements_v1.3 / Design_v1.3, then move this folder to `Completed/PR04/`.

Baseline is the PR03 exe: Lookup, Probe pulse, persist, closed combos, Dashboard charts.

**One sentence:** Open a Chrome HAR, list every host the browser had to reach, and ask DNS whether each name resolves.

**This version is not** a HAR analyzer. No waterfall, no timings, no cookies, no header dump, no body decode, no status-code story, no request replay, no TLS inspection.

---

## Why two libraries

DnsIQ does not grow a parser. `Vestigium.Helpers.Network` does not grow a parser. Nothing already in Helpers owns a capture file.

| Project | Owns | Does not own |
|---|---|---|
| `Vestigium.Helpers.LogParser` | Shared host model. Format id. Read result. | HAR JSON. Sockets. WPF. |
| `Vestigium.Helpers.LogParser.Har` | HAR 1.2 read. Map entries to that model. | DNS. UI. A second log format. |
| `Vestigium.Suite.Network.DnsIQ` | Open file. Grid. Probe loop. | JSON. A private host list type. |

Suite rule holds: DnsIQ takes **packages**, not a project reference into Helpers.

Checked 6 October 2026: neither project is in `Vestigium.Helpers` on GitHub. The owner has created the two projects. This PR packs them. It does not invent a third.

---

## Corpus (the two captures)

Both files are Chrome WebInspector HAR 1.2, host `USITHLWDU19031`, 24 July 2026. They are the acceptance corpus. Tests may use trimmed copies with the same hosts. Do not check the 2.5 MB SSO file into the suite repo.

`USITHLWDU19031_24JUL_SiteCannotBeReached.har` — 11 entries. `q2prod.idbs-cloud.com:8443` returns 200. Every call to `q2valprod.services.idbs-cloud.com` is `net::ERR_TIMED_OUT` (status 0, no server IP). That host is still in scope. A timeout is why we are here.

`USITHLWDU19031_SSO_SignInError.har` — 72 entries. SSO path reaches Auth0 and Microsoft login, then CDN assets. Errors include `ERR_CONNECTION_RESET` (20), `ERR_TIMED_OUT` (2), `ERR_ABORTED` (2). Those hosts stay in the list.

| Host | Cannot-reach | SSO | Probe? |
|---|---|---|---|
| `q2prod.idbs-cloud.com` (observed port 8443) | yes | yes | yes |
| `q2valprod.services.idbs-cloud.com` | yes | yes | yes |
| `quintiles.sharepoint.com` | yes | yes | yes |
| `idbs-q2valprod.us.auth0.com` | | yes | yes |
| `login.microsoftonline.com` | | yes | yes |
| `aadcdn.msftauth.net` | | yes | yes |
| `aadcdn.msauth.net` | | yes | yes |
| `static-resources.idbs-cloud.com` | | yes | yes |
| `idbs-themes.idbs-cloud.com` | | yes | yes |
| `cdn.auth0.com` | | yes | yes |

Server IPs in those files (`75.2.119.14`, `13.248.155.168`, `104.18.43.182`, and the rest) are observed peers. They are not probe names.

---

## Must change

### R04-01 Shared model (`Vestigium.Helpers.LogParser`)

`net10.0`. No WPF. No `System.Net.Sockets`. First package **1.0.0**.

Types, nothing else:

| Type | Role |
|---|---|
| `LogFormat` | `Unknown`, `Har`. One value is enough. |
| `LogHostSource` | Flags: `Request`, `Redirect`, `Location`, `Page`. |
| `LogHost` | Host (ASCII, lower, no trailing dot), ports seen, hit count, sources, `IsAddress` when the host is already an IP. |
| `LogReadResult` | Format, entry count, page count, hosts, warnings. |

No `ILogParser`. No plugin host. The second format does not exist. A shared bag of types is the seam.

### R04-02 HAR read (`Vestigium.Helpers.LogParser.Har`)

`net10.0`. References LogParser only. `System.Text.Json`. No Newtonsoft. First package **1.0.0**.

Door:

```
HarReader.Read(Stream)     → LogReadResult
HarReader.ReadFile(path)   → LogReadResult
```

Extract a host when it is the host of an absolute `http` or `https` URL in:

| Field | Source flag |
|---|---|
| `entry.request.url` | `Request` |
| `entry.response.redirectURL` | `Redirect` |
| response header `Location` (absolute URL only) | `Location` |
| `page.title` when it is an absolute http(s) URL | `Page` |

Rules:

- Case-fold. Strip one trailing dot. `Uri` host (punycode, bracketed IPv6 already handled).
- Deduplicate on host. Union ports. Union sources. Hit count = number of `request.url` hits. A host seen only on a redirect still appears, hit count 0.
- Status 0, `net::ERR_*`, 302, 404, and 400 still yield a host. Failure is not a reason to drop the name.
- Drop `data:`, `blob:`, `about:`, `chrome:`, empty, and relative `Location`.
- Do not emit `serverIPAddress` as a host.
- IP-literal host (`http://75.2.119.14/`) is listed with `IsAddress=true`. DNS probe skips it.
- `log.version` other than `1.2` → warning, still read `entries` if present.
- Missing `log` or `log.entries` → throw `InvalidDataException` with a fixed message. Do not return an empty success.
- UTF-8. BOM allowed.
- File over 64 MB → reject before parse. The corpus is 2.5 MB. The cap is a guard.

### R04-03 DnsIQ opens the file

- File menu: **Open HAR…** (`*.har`). No drag-drop. No last-path persist.
- New top tab **HAR**, same HorizontalTab strip. Disabled until a file parses with zero throw. Lookup does not unlock it. Probe pulse does not unlock it.
- Grid, read-only: Host, Ports, Hits, Sources, DNS, Answers.
- Empty host list is a successful parse. Status: `No hosts`. Grid empty. Probe does nothing.
- Bad file: status line = the exception message. Grid cleared. Tab stays disabled. No throw out of the UI.
- Opening a second file replaces the grid and clears prior DNS columns.

Not on this page: the Lookup answer grid, Requests, Seconds, charts.

### R04-04 DNS probe uses the live knobs

Button **Probe DNS** on the HAR page. **Cancel** uses the same in-flight token as Lookup (one job in the process).

For each host where `IsAddress` is false, sequential:

1. `NetworkHelper.LookupAsync` type `A`, then type `AAAA`, same Server / Port / Interface / Source the DnsIQ page already has.
2. Do not call `ProbeDns`. Do not run the eight-type Lookup. Do not start the pulse.

| DNS column | When |
|---|---|
| `Resolved` | A or AAAA returned at least one answer. Answers column = those records, short. |
| `NxDomain` | Both lookups came back NxDomain (or one NxDomain and the other empty of answers, neither timeout). |
| `TimedOut` | Either lookup timed out and neither returned an answer. |
| `Refused` | Refused and no answer. |
| `Failed` | Exception, or no server configured. |
| `Skipped` | `IsAddress`. Answers column = the literal. |

Status bar during the loop: `sent/total` over **hosts**, not over the pulse Requests value.

Wording: **Resolved is not Reachable.** The cannot-reach capture is the proof. `q2prod` answered HTTP from `75.2.119.14` while `q2valprod.services` never completed a socket. A green DNS row does not mean the site loads.

### R04-05 Tests

LogParser.Har tests, off the wire, against trimmed corpus fixtures:

- Cannot-reach file yields the three hosts above, including `q2valprod.services.idbs-cloud.com`, and port 8443 on `q2prod`.
- SSO file yields the ten hosts above.
- Neither file yields a server IP as a probe name.
- A `data:` URL in an otherwise valid HAR does not become a host.
- Missing `log.entries` throws.
- DnsIQ host tests do not open a socket and do not parse HAR JSON themselves.

---

## Must not change in PR04

- Lookup / Probe pulse behavior, timing, and Dashboard charts
- Helpers.Network protocol surface
- Persist file shape, except nothing new is written
- Spoofing a source address
- CSV export, DoH, AXFR
- Other suite hosts

---

## Acceptance

1. Open the cannot-reach HAR. Grid has `q2prod.idbs-cloud.com` (8443), `q2valprod.services.idbs-cloud.com`, `quintiles.sharepoint.com`. No IP in the Host column.
2. Open the SSO HAR. Grid has the ten hosts in the corpus table. CDN hosts are present. Server IPs are absent.
3. Probe DNS walks the list on the current Server / Port / Interface / Source. Cancel stops the walk. Lookup and pulse cannot run at the same time.
4. An IP-literal host shows Skipped and is not sent to `LookupAsync`.
5. A truncated or non-HAR file sets the status line and does not throw.
6. `dotnet test` for the Har project is green. Suite host tests stay green and off the wire.

---

## Parked (not required to close PR04)

- Waterfall, timings, status-code rollup, cookies, header viewer
- Filter to "page title host only" or hide CDN
- TCP / TLS reachability on the observed port (8443). Different question. Different tool.
- Parallel lookups
- Last-file persist, drag-drop, multi-file
- Fiddler SAZ, ETL, netlog
- `ILogParser` until a second format exists

---

## Decision

| Call | Why |
|---|---|
| All http(s) hosts, including failed requests and CDN | The SSO break is on Auth0 / login / CDN, not only on the ELN host. Dropping status 0 drops the cannot-reach name. |
| SharePoint stays | It is in the capture. A "primary page" filter is a guess. The owner can ignore the row. |
| Parser does not probe | LogParser must stay usable without a NIC and without Helpers.Network. |
| A + AAAA only | The question is "does the name resolve." Eight types is the Lookup button. |
| No `ILogParser` | One format. An interface with one implementation is a stunt. |

### Rejected

| Idea | Why out |
|---|---|
| Parse HAR inside DnsIQ | Next format would fork the exe. Owner already split the projects. |
| Put HAR in Helpers.Network | Network is the protocol. A capture file is not a DNS message. |
| Probe only the page-title host | Hides `login.microsoftonline.com` and both `aadcdn` hosts. |
| Treat server IP as a name to resolve | It is already an address. Asking DNS is the wrong question. |
| New reachability probe (TCP 8443) | Owner asked for DNS. The timeout in the corpus may be path, proxy, or firewall. Do not smuggle that in. |
