# RouteIQ — PR04 Requirements (Revision 2)

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-SRS-PR04
**Version:** PR04
**Status:** Live. Binding for Revision 2 until an implementation plan closes the slice.
**Date:** 5 October 2026
**Project:** `Vestigium.Suite.Network.RouteIQ`
**Kind:** WPF exe, `net10.0-windows`, MVVM
**APPID:** `RouteIQ` (never `Network`)
**Binding:** This file wins on the Revision 2 window and on the holes below. `Requirements_v1.0.md` remains the historical first-and-ten lock. It does not describe the host that shipped. Helpers.Network owns protocol. `Directory.Build.props` owns package pins. A plan that invents a door the library does not own is wrong.

Revision 2 is the same host, made honest enough to hand to someone else. It is not a second product.

---

## 0. Decision

| Call | Why |
|---|---|
| PR04 is hardening, not a feature release | The window already prints routes, neighbors, connections, NetBIOS, and LMHOSTS, and it already has KQL, export, settings, watch, and Help. Adding tabs now hides the holes. |
| Live vendor lookup is off unless the operator turns it on | Refresh calls `LookupOuiAsync`. The library default URL is `https://api.macvendors.com/{oui}`. That sends the OUI off the box on every refresh that misses the packed registry. A lab host does not get to do that by default. |
| Neighbor ICMP is a command, not a side effect of Refresh | `ProbeNeighbors` echoes every unicast neighbor (pool 4) after every print. First-and-ten said one address, operator started. An open on a lab VLAN is not a scan. |
| Settings live at `C:\ProgramData\Vestigium\Settings\RouteIQ` | Owner decision, 5 Oct 2026. The gap is the extra `Diagnostics` segment, not ProgramData. LocalAppData is out. |
| No route write in this revision | Library still denies the default route. Admin UX was deferred on purpose. Do not smuggle Add / Change / Remove back in as "production." |
| No elevation | No `app.manifest`. Stay `asInvoker`. Do not add `requireAdministrator` to make a print "work." |

Rejected: Option C write form. Default-route button. Neighbor flush. Saved route sets. Markdig / a document pipeline. A Logs topic. Charts. A Helpers.RouteIQ package. Publishing the exe.

---

## 1. What Revision 2 is

The host that exists after PR01 and PR03, with the gaps in section 2 and the holes in section 3 closed.

| Surface | Today | PR04 |
|---|---|---|
| Routes | IPv4 and IPv6 grids from `GetRoutes` | Keep. Failure keeps the last lists. |
| Neighbors | Grids from `GetNeighbors`, packed OUI, then live OUI, then ICMP | Packed OUI stays. Live OUI and ICMP leave Refresh. |
| Connections | Snapshot plus timed watch, process name on the row | Keep. Watch stays 5–180 seconds. Copy and export say they include process name. |
| NetBIOS / LMHOSTS | Local print | Keep. Empty is allowed. |
| KQL | Three bars. Bad compile reports and does not hide rows | Keep that fail-open display. Add a test so a later edit cannot flip it. |
| Export | xlsx, operator picks the path, optional open-after | Keep. Open-after only for the path the dialog returned, extension `.xlsx`. |
| Settings | Theme, status bar, OUI pool, MRU, marks, queries | `%ProgramData%\Vestigium\Settings\RouteIQ\settings.json`. Save failure is visible. |
| Help | Right-rail tab, ten topics, About reads assembly versions | Keep. Navigate only `https`. |
| Write form | Absent | Stays absent. |

---

## 2. Production gaps (this slice)

These are the backlog. They are in PR04 because a production handoff that skips them is a lie.

| # | Gap | Evidence | Exit |
|---|---|---|---|
| G1 | Window paper is first-and-ten. The exe is not. | `Requirements_v1.0.md` / `Design_v1.0.md` say one window, two lists, one probe. The exe has seven work tabs. | This file is the window paper for Revision 2. Do not silently rewrite v1.0. |
| G2 | Queue keeper still says PR03 is live. | `PR-Plans/README.md` links `PR03/`. PR01 and PR03 are under `Completed/`. There is no PR02. | Keeper points at PR04 only. |
| G3 | Live OUI on Refresh. | `MainViewModel.ResolveLiveVendors` calls `NetworkHelper.LookupOuiAsync` with default options. Library URL is `https://api.macvendors.com/{oui}`. | Off by default. Packed lookup only until the operator opts in. Opt-in shows the URL. Pool stays 1–20. |
| G4 | ICMP scan on Refresh. | `ProbeNeighbors` pings every `CanPing` neighbor after print. | Refresh does not ping. A Probe command pings the selected row, or one typed address. Not the table. |
| G5 | Settings folder is wrong, and save is silent. | `DefaultRoot` is `%ProgramData%\Vestigium\Settings\Diagnostics\RouteIQ`. `Save` / `Load` swallow every exception. | `%ProgramData%\Vestigium\Settings\RouteIQ\settings.json`. One-time copy if the new file is missing and the old file exists. Then stop reading the old path. Load failure keeps defaults and reports. Save failure reports. No empty catch. |
| G6 | Tests cover the parser, not the host. | `RouteIqInputTests` and `HostIds`. Nothing for settings path, OUI opt-in, probe scope, export extension, KQL miss. | Tests in `Vestigium.Suite.Network.Tests` for the locks in section 4. Not a stack suite. |
| G7 | Three packages pinned in the csproj, not props. | `Vestigium.Controls.QueryBar` 1.0.3, `Vestigium.Helpers.ClosedXml` 1.0.1, `Vestigium.Helpers.Kql` 1.0.4. DI is 10.0.12 in the csproj and 10.0.0 in props. | Versions live in `Directory.Build.props`. Host csproj uses the property. Do not bump for sport. |
| G8 | Help and export launch through the shell with no scheme check. | `Process.Start` + `UseShellExecute` on help URIs and on the saved workbook. | Help: `https` only. Export open-after: the dialog path, `.xlsx` only. Anything else does not start. |

Not a gap: bad KQL already returns null and the filter shows the rows. Do not "fix" that into an empty grid.

Not this slice: Help is 672 lines of code-behind. Leave it. A markdown pipeline was rejected in PR03.

---

## 3. Security holes

| # | Hole | Why it matters | Fix |
|---|---|---|---|
| S1 | Unsolicited egress of neighbor OUIs | Refresh misses the packed registry and GETs `api.macvendors.com`. The library will also substitute `{mac}` if a caller changes the template. RouteIQ passes the MAC into `LookupOuiAsync`. | Default path is `LookupOuiPacked` only. Live call requires an explicit setting, default false. Log the miss locally. Do not log the full MAC on the success line if the library already logs OUI only — do not add a fuller line. |
| S2 | Unsolicited ICMP to the neighbor table | Opening the host probes the LAN. That trips monitors and is not the job the operator asked for. | Probe is a command. One target. Loopback, unspecified, and multicast stay excluded (`CanPing` rules stay). |
| S3 | Settings in the wrong folder | File is under `Settings\Diagnostics\RouteIQ`. Owner locked `Settings\RouteIQ`. A shared ProgramData file is still shared; that is accepted, not fixed by a second store. | Move the file. Copy once. Do not keep a dual read. Do not fall back to LocalAppData if the folder is not writable. Report it. |
| S4 | Shell execute | Help links and open-after are `UseShellExecute = true`. A later edit that binds a `file:` or script URI launches it. | Allow-list. `https` for Help. `.xlsx` for open-after. |
| S5 | Inventory leaves the box when the operator exports | Workbook contains routes, neighbor MACs, process names, NetBIOS names, LMHOSTS lines. | Not a bug. Status line and Help say the workbook is local and what it contains. No upload. No telemetry. |

Stay `asInvoker`. Connection process names are already on the row; do not hide them. Do not add a credential, a token, or a proxy setting to reach the vendor URL.

---

## 4. Locks

| # | Rule |
|---|---|
| R1 | `HostLog.Initialize(HostIds.RouteIQ)` still runs before the window. Logs stay under `%ProgramData%\Vestigium\Logs\RouteIQ\`. |
| R2 | Code-behind does not call `NetworkHelper`. |
| R3 | No `route.exe`, `netsh`, `arp`, or `ip`. |
| R4 | No Add / Change / Remove. No default-route control. Interface index `0` is not rewritten. |
| R5 | Refresh prints. It does not ping. It does not call `LookupOuiAsync`. |
| R6 | Live vendor lookup is a setting, default off. The settings page shows the URL that will be called. |
| R7 | Probe pings one address the operator named or one selected neighbor row. Disabled while a probe is running. |
| R8 | Settings path is `%ProgramData%\Vestigium\Settings\RouteIQ\settings.json`. A failed save is a status line, not a swallowed exception. |
| R9 | Export open-after starts only the dialog path when it ends in `.xlsx`. |
| R10 | Help navigation starts only `https` URIs. |
| R11 | A KQL compile failure reports the error and leaves the current rows visible. |
| R12 | Package versions for QueryBar, ClosedXml, Kql, and DI come from props. |
| R13 | New tests fail if R5, R6, R7, R8, R9, or R11 regress. They do not hit the network. |

---

## 5. Acceptance

1. Cold start prints routes, neighbors, connections, NetBIOS, and LMHOSTS without an outbound vendor request and without ICMP to the table.
2. Packed OUI still fills a known prefix (`00:00:0C` is Cisco in the packed tests). A miss stays `--` until opt-in.
3. Opt-in off is the default on a new settings file.
4. Probe of one selected neighbor returns an RTT or `--`. It does not walk the grid.
5. Settings file is `%ProgramData%\Vestigium\Settings\RouteIQ\settings.json`. The Diagnostics path is not read after the one-time copy. A folder that cannot be written surfaces a status line.
6. Export open-after does not start a non-xlsx path. Help does not start a non-https URI.
7. Bad KQL text does not clear the grid.
8. `dotnet test` for the new RouteIQ locks passes without a network.

---

## 6. Out

| Item | Why |
|---|---|
| Option C add / change / remove | Admin and deny UX. Own paper. Not Revision 2. |
| Default-route write | Library lock. Permanent non-goal. |
| Neighbor flush | Privileged. Not this down. |
| Persist a route set | Config management. Not this host. |
| Markdown help, About dialog, Logs topic | Rejected in PR03. |
| Charts | No buyer on this host. |
| Elevation | Does not fix the folder, and it makes a shared settings file worse. |
| Publishing | Not this slice. |
| LocalAppData settings | Owner rejected it. Path is ProgramData. |

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR04 | 5 Oct 2026 | Revision 2 open. Production gaps G1–G8. Security holes S1–S5. No write form. |
| PR04 | 5 Oct 2026 | Owner locked settings at `C:\ProgramData\Vestigium\Settings\RouteIQ`. LocalAppData out. |
