# PR04 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR04-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Draft
**Date:** 6 October 2026
**Product papers (unchanged until this PR lands):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)

This file is the **definition of done for PR04**. It is not a new product specification. When PR04 is accepted, fold the window delta into Requirements_v1.3 / Design_v1.3, then move this folder to `Completed/PR04/`.

Baseline is the PR03 exe: Lookup, Probe pulse, persist, closed combos, Dashboard charts.

**One sentence:** Open a Chrome HAR, or any UTF-8 text dump saved as `.txt` or `.har`, list every host in it, and ask DNS whether each name resolves.

**This version is not** a HAR analyzer. No waterfall, no timings, no cookies, no header dump, no body decode, no status-code story, no request replay, no TLS inspection. The text door is a scrape, not a document parser.

---

## Why two libraries

DnsIQ does not grow a parser. `Vestigium.Helpers.Network` does not grow a parser. Nothing already in Helpers owns a capture file.

| Project | Owns | Does not own |
|---|---|---|
| `Vestigium.Helpers.LogParser` | Shared host model. Text scrape of URLs and domain names. | HAR JSON. Sockets. WPF. A Word/Excel parser. |
| `Vestigium.Helpers.LogParser.Har` | HAR 1.2 read. Map entries to that model. | DNS. UI. The text scrape. A second log format. |
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
| `LogFormat` | `Unknown`, `Har`, `Text`. |
| `LogHostSource` | Flags: `Request`, `Redirect`, `Location`, `Page`, `Text`. |
| `LogHost` | Host (ASCII, lower, no trailing dot), ports seen, hit count, sources, `IsAddress` when the host is already an IP. |
| `LogReadResult` | Format, entry count, page count, hosts, warnings. |
| `TextHostReader` | Scan UTF-8 text for URLs and domain names. The dump / email / spreadsheet door. |

No `ILogParser`. No plugin host. HAR is structured. Text is a scrape. That is two doors, not a framework.

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

- File menu: **Open capture…** Filter `*.har;*.txt`, plus HAR-only and text-only. No drag-drop. No last-path persist. No paste box. The owner pastes the email or the sheet into a `.txt` and opens that file.
- Route: `.har` whose content is a HAR object → `HarReader`. Anything else that is UTF-8 text, including `.txt` and a `.har` that is not JSON, → `TextHostReader`. A valid HAR is not scraped a second time.
- New top tab **HAR**, same HorizontalTab strip. Disabled until a file parses with zero throw. Lookup does not unlock it. Probe pulse does not unlock it. The tab name stays HAR. Text rows are the same grid.
- Grid, read-only: Host, Ports, Hits, Sources, DNS, Answers.
- Empty host list is a successful parse. Status: `No hosts`. Grid empty. Probe does nothing.
- Bad file (oversize, binary, undecodable): status line = the exception message. Grid cleared. Tab stays disabled. No throw out of the UI.
- Opening a second file replaces the grid and clears prior DNS columns.

Not on this page: the Lookup answer grid, Requests, Seconds, charts. Not a hidden keystroke. The owner called this an easter egg. Hiding it breaks the dump workflow, so the door is the file filter, not a secret.

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

### R04-06 Text scrape (`TextHostReader` in LogParser)

This is the dump door. A copied email, a spreadsheet saved as `.txt`, a notes file, or a `.har` that is not JSON. Same probe as R04-04. Same 64 MB cap. UTF-8, BOM allowed.

Door:

```
TextHostReader.Read(Stream)    → LogReadResult   Format = Text
TextHostReader.ReadFile(path)  → LogReadResult
```

Pull a host from:

| Hit | Source flag | Notes |
|---|---|---|
| `http://` or `https://` URL | `Text` | Same host/port rules as HAR. Drop `data:`, `blob:`, `about:`, `chrome:`. |
| `mailto:` or `name@host` | `Text` | Host is the part after `@`. |
| Bare domain | `Text` | Two or more labels. Each label 1–63, `[a-z0-9-]`, no leading or trailing hyphen. Last label is letters, length 2–24. |
| Bare IPv4 | `Text` | `IsAddress=true`. Probe skips it. |
| `localhost` | `Text` | No dot. Still a host. |

Reject, do not emit:

- A token whose last label is a file extension: `txt`, `csv`, `tsv`, `xlsx`, `xls`, `json`, `har`, `log`, `xml`, `pdf`, `png`, `jpg`, `jpeg`, `gif`, `dll`, `exe`, `config`, `md`. `report.txt` in a sheet is a filename, not a zone.
- A token with a label that is only digits, unless the whole token is an IPv4.
- Single-letter labels (`e.g.`, `U.S.`).

Hit count = times that host was seen. Ports only from URLs that carried one. No public-suffix list. No Word, no xlsx, no HTML parser. If it is not UTF-8 text in a `.txt` or a failed `.har`, it is out.

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
6. `dotnet test` for the Har project and the LogParser text tests is green. Suite host tests stay green and off the wire.
7. A `.txt` that is a pasted email or a sheet, containing `https://q2prod.idbs-cloud.com:8443/` and `user@q2valprod.services.idbs-cloud.com` and the bare name `login.microsoftonline.com`, yields those three hosts. `notes.txt` in the same file does not. Probe DNS runs on that list the same way it runs on a HAR.

---

## Parked (not required to close PR04)

- Waterfall, timings, status-code rollup, cookies, header viewer
- Filter to "page title host only" or hide CDN
- TCP / TLS reachability on the observed port (8443). Different question. Different tool.
- Parallel lookups
- Last-file persist, drag-drop, multi-file
- Fiddler SAZ, ETL, netlog, `.xlsx`, `.docx`, `.csv` as a typed format (save the sheet as `.txt`)
- `ILogParser` until a third door exists
- A paste box. The file is the intake.

---

## Decision

| Call | Why |
|---|---|
| All http(s) hosts, including failed requests and CDN | The SSO break is on Auth0 / login / CDN, not only on the ELN host. Dropping status 0 drops the cannot-reach name. |
| SharePoint stays | It is in the capture. A "primary page" filter is a guess. The owner can ignore the row. |
| Parser does not probe | LogParser must stay usable without a NIC and without Helpers.Network. |
| A + AAAA only | The question is "does the name resolve." Eight types is the Lookup button. |
| No `ILogParser` | Two doors, one result shape. An interface still has no second implementation that needs swapping. |
| Text scrape lives in LogParser, not behind a keystroke | Owner's intake is a dump, a pasted email, or a sheet saved as `.txt`. A hidden gesture fails that. File filter is the easter egg. |
| Valid HAR wins over the scrape | Structured read keeps port 8443 and the source flags. Scraping a HAR would also work and would be worse. |

### Rejected

| Idea | Why out |
|---|---|
| Parse HAR inside DnsIQ | Next format would fork the exe. Owner already split the projects. |
| Scrape a valid HAR instead of reading it | Loses ports and throws away the field map the corpus was built to prove. |
| Open `.xlsx` natively | Owner already said the sheet is saved as `.txt`. ClosedXml in this slice is a stunt. |
| Put HAR in Helpers.Network | Network is the protocol. A capture file is not a DNS message. |
| Probe only the page-title host | Hides `login.microsoftonline.com` and both `aadcdn` hosts. |
| Treat server IP as a name to resolve | It is already an address. Asking DNS is the wrong question. |
| New reachability probe (TCP 8443) | Owner asked for DNS. The timeout in the corpus may be path, proxy, or firewall. Do not smuggle that in. |
