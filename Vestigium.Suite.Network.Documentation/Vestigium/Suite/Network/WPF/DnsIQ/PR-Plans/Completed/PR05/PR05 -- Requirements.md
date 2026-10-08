# PR05 — Requirements

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PR05-REQ
**Host:** `Vestigium.Suite.Network.DnsIQ`
**Status:** Draft
**Date:** 7 October 2026
**Product papers (unchanged until this PR lands):** [Requirements_v1.2.md](../../Requirements_v1.2.md), [Design_v1.2.md](../../Design_v1.2.md)

This file is the **definition of done for PR05**. It is not a new product specification. When PR05 is accepted, fold the window delta into Requirements_v1.3 / Design_v1.3, then move this folder to `Completed/PR05/`.

Baseline is the shipped exe after PR04: Lookup, Probe pulse, persist, Dashboard charts, Capture (HAR or `.txt` dump, DNS column, context Lookup / Lookup + Probe).

**One sentence:** Give the operator a Help reader that explains the page they are on, and an Exports tab that writes the prints already loaded to one local workbook.

**This version is not** a shared Help library, a chart exporter, a second parser, a settings redesign, or a change to Lookup, pulse, or Capture behavior.

---

## Why this slice

RouteIQ already has the two doors. Help is a right-aligned tab on the page strip. The page is a topic rail and a reader. Each topic says what that print is, what it is not, how to read a row, and when to use it. Exports is a shell tab. It writes the loaded prints. It does not read the stack again.

DnsIQ collects the same kind of record and has neither door. The operator can run a lookup, a pulse, and a capture probe, and cannot take the rows with them or read what a DNS column means without leaving the window.

Help and Exports are the suite standard from here. The standard is the contract below. The control stays in the host. A shared `Vestigium.Controls.Help` has no second buyer in this slice.

---

## Suite standard (locked by this host)

Later hosts copy this. They do not invent a third shape.

| Door | Rule |
|---|---|
| Help | Right-aligned `RadioButton.HorizontalTab`, same group as the page strip. Not a `MenuItem`. Not a shell nav item. Constructed on first check. |
| Reader | Topic rail (`RadioButton.VerticalNav`, 220). Optional section strip. `FlowDocumentScrollViewer`. Theme brushes. No search box. |
| Copy | One topic per print the operator is looking at. Each data topic: what it is, what it is not, how to read a row, when to use it, columns. Overview names every tab and says the tool does not change the thing it prints. |
| Links | `https` only. A `file:` or script URI does not launch. |
| License | Its own topic. The MIT text, not a paraphrase. |
| Exports | Shell nav item, after the data tabs, before Settings. Caption. One checkbox per print. Open file after. Open folder after. Export writes the checks. Export All ignores the checks. Cover is always written. |
| Workbook | Dialog opens in `Desktop\Vestigium\Exports\{APPID}`. Writes the loaded prints. Does not re-query. Does not re-read a file. Stays on this machine. Status line names the path. Help says what the file contains. |
| Open-after | `.xlsx` only, and only the path the dialog returned. Folder open is that directory only. |
| Checks | Remembered in the host settings file. Missing keys default the prints on and the open flags off. |

Rejected for the standard: a Help menu of one-line tooltips, a CSV beside the workbook, chart images in the workbook, a compare-two-files page.

---

## Must change

### R05-01 Help chrome

`DnsIqWindow` gains a Help tab on the right of the page strip, same radio group as DnsIQ / Capture / Dashboard / Settings (`DnsIqMainNav`).

- First check builds `HelpView`. Later checks show the same instance.
- Uncheck returns to the shell page that was selected. Help is not a `VestigiumNavItemSpec`.
- Layout matches RouteIQ: rail, optional section strip, reader. Foreground and background from `Vestigium.Brushes.Text.Primary` and `Vestigium.Brushes.Surface.Window`.
- About may link `https://github.com/nwilkinsonlsnh/Vestigium.Suite.Network`. No other scheme.

### R05-02 Help topics

Topics, in this order: Overview, Lookup, Probe, Capture, Dashboard, Exports, Settings, Glossary, About, License.

Glossary has section chips: Tabs, Lookup, Capture, Probe. No port catalog. DnsIQ does not name services.

The reader must contain these claims. Wording can be tighter. The claim cannot move to a tooltip, and it cannot be dropped.

| Topic | Must say |
|---|---|
| Overview | DnsIQ asks a resolver and prints the answer. It does not change DNS, edit a zone, shell to `nslookup.exe`, or use HTTP DNS. The pages are DnsIQ, Capture, Dashboard, Exports, Settings. Help is this page. A row that says Resolved is not Reachable. |
| Lookup | Blank Name means `localhost` and is not persisted. Server, Port, Type, Interface are the query. Lookup writes Type, Name, Data, Ttl. A failed Lookup clears the grid. One job in the process. Cancel stops it. |
| Probe | Probe is that Lookup, then a pulse of N lookups over X seconds. The prelude fills the grid. The pulse does not append rows. Requests and Seconds live on Settings → Probe (default 1000 and 60). The status bar during the pulse is `sent/total`. Dashboard stays disabled until a Probe finishes. Lookup alone does not unlock it. NxDomain counts as answered. Timeout and Refused are counts only. |
| Capture | Open capture reads a HAR, or a UTF-8 `.txt`, or a `.har` that is not JSON. Columns are Host, Ports, Hits, Sources, DNS, Error, Answers. A file that parses starts the DNS probe. The row menu is Lookup, or Lookup + Probe, for that host. DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Skipped is an address and is not sent. Resolved is not Reachable. This page is not a HAR analyzer: no waterfall, no cookies, no header dump, no replay. |
| Dashboard | Lookup tab is the type mix of the last answer grid. Probe tab is the RTT curve, the histogram, and the control chart when Analytics returns fences. Empty state points back to DnsIQ. The charts are a reading of the run. They are not a second measurement. |
| Exports | Writes the prints already loaded. Does not query again. Does not re-read the file. Cover is always written. A checked print with rows becomes a sheet. The workbook stays on this machine. It contains the name, the answers, the capture hosts, and the probe times. It is not uploaded. |
| Settings | Settings → DnsIQ holds the Port seed and Source. Source is the local bind address, not spoofing. Settings → Probe holds Requests and Seconds. Settings → Theme holds the palette and the status bar. The file is `settings.json` under ProgramData. Name is not stored. Pulse samples are not stored. |
| About | Assembly version. Copyright (c) 2026 Nathaniel Wilkinson. MIT. The public source is Vestigium.Suite.Network. |
| License | The MIT notice from the repo `LICENSE`. |

Glossary terms cover the words on the page: Lookup, Probe, Capture, Host, Hits, Sources, Resolved, Skipped, Prelude, Pulse, Cover, Export, Export All, Source. A term that is not on the window does not get a paragraph.

### R05-03 Exports tab

Shell item **Exports**, after Dashboard, before Settings.

| Piece | Rule |
|---|---|
| Caption | One workbook. Cover is always written. Export writes the checked prints. Export All writes every print. The dialog opens in `Desktop\Vestigium\Exports\DnsIQ`. |
| Checks | Lookup, Capture, Probe. Open file after saving. Open folder after saving. |
| Export | Checked prints that have rows. Nothing checked → status `Select a print to export.` Nothing loaded → status `Nothing loaded to export.` Do not write a cover-only file. |
| Export All | All three prints that have rows. Ignores the checks. Same empty rule. |
| Buttons | Off until at least one print has rows. |
| Dialog | `DnsIQ-export-yyyyMMdd-HHmmss.xlsx`. Filter `Excel workbook (*.xlsx)\|*.xlsx`. Initial directory created if missing. |
| Writer | `WorkbookHelper.Create("Cover", "DnsIQ")` from `Vestigium.Helpers.ClosedXml` 1.0.1 (already pinned). Package reference on the DnsIQ project. No project reference. |
| Open-after | `UseShellExecute` only when the saved path ends with `.xlsx` and is the path the dialog returned. Otherwise status `Export was written. It was not opened.` |
| Status | `Exported {path}` on success. Exception message on failure. No throw out of the UI. |

Sheets:

| Sheet | When | Columns |
|---|---|---|
| Cover | Always, if any sheet has rows | Host, Operator, Taken, Name, Server, Port, Type, Interface, Source, Requests, Seconds, Capture file (or `--`), and the count of each sheet written. |
| Lookup | Answer grid has rows | Type, Name, Data, Ttl. |
| Capture | Host grid has rows | Host, Ports, Hits, Sources, DNS, Error, Answers. |
| Probe | The RTT list handed to the dashboard has rows | Index, RttMs. |

Probe samples are held in memory for the export. They are not added to `settings.json`. A second Probe replaces the list. Lookup does not clear it. Opening a new capture does not clear it.

The workbook does not contain chart images. It does not contain the raw HAR.

### R05-04 Persist

Same file. Do not move it. v1.2 locked `C:\ProgramData\Vestigium\Settings\Diagnostics\DnsIQ\settings.json`. RouteIQ's folder fight is not reopened here.

New keys: `exportLookup`, `exportCapture`, `exportProbe`, `exportOpenAfter`, `exportOpenFolder`.

Missing or corrupt file still means defaults. Default the three prints on, both open flags off. A check change saves with the rest of the session. No secrets.

### R05-05 Tests

Host tests, off the wire:

- Help topic titles are the ten names in R05-02, in that order.
- Overview text contains `nslookup` and says the tool does not shell to it.
- Capture text contains `Resolved is not Reachable`.
- Probe text says the pulse does not append rows.
- Exports text says the workbook is not uploaded.
- License topic is non-empty.
- Export builder given lookup rows and an empty capture writes Cover and Lookup, and does not write Capture.
- Export builder given no rows does not write a file.
- A saved path that does not end in `.xlsx` is not opened.

No test calls `LookupAsync`. No test opens a socket.

---

## Must not change in PR05

- Lookup, Probe pulse, Capture parse, and the DNS column mapping
- Dashboard chart kinds and the Probe gate
- Helpers.Network, LogParser, LogParser.Har, LogParser.Url
- Settings folder
- Other suite hosts, including RouteIQ
- CSV, DoH, AXFR, chart export, a paste box

---

## Acceptance

1. Help sits on the right of the page strip. First open builds the reader. The ten topics are there. Capture says Resolved is not Reachable. Probe says the pulse does not append rows. Exports says the file stays on this machine.
2. A `file:` link in Help does not launch. The GitHub link is `https`.
3. Exports sits between Dashboard and Settings. Checks survive a restart. Open-after does not.
4. After a Lookup, Export writes Cover and Lookup only. After a Capture probe, Capture is a sheet and the DNS column is the column on screen. After a Probe, Probe is Index and RttMs. Export does not send another query.
5. Export with nothing loaded does not write a file. Open-after on a non-`.xlsx` path does not launch.
6. Lookup, pulse, Capture, and Dashboard still behave as they did before this PR.
7. `dotnet test` for the suite host is green and off the wire.

---

## Parked (not required to close PR05)

- `Vestigium.Controls.Help` or a shared export control
- Help search
- Port catalog
- Chart images, CSV, a second workbook format
- Compare two exports
- Persist of pulse samples
- Moving the settings file out of `Diagnostics\DnsIQ`
- Help and Exports on PingIQ, TraceIQ, NicIQ, ShareIQ

---

## Decision

| Call | Why |
|---|---|
| Copy RouteIQ's chrome into DnsIQ | The reader and the workbook already exist. A shared control is a third design with one caller. |
| Help is a right-aligned tab, not a menu | That is the RouteIQ door the owner called the Help menu. A dropdown of topics hides the page the operator is on. |
| Describe the shipped Capture page | PR04's plan still says a Probe DNS button. The exe opens the file, probes, and offers Lookup / Lookup + Probe on the row. Help follows the exe. |
| Probe sheet is Index, RttMs | That is the list the dashboard already received. Inventing rcode columns means storing a record this host does not keep. |
| No chart images | The charts are a reading. The rows are the record. |
| Settings folder stays | v1.2 locked it. This slice adds keys. |

### Rejected

| Idea | Why out |
|---|---|
| Extract Help into Controls in this PR | RouteIQ would have to move with it, or the suite has two readers. No buyer asked for the move. |
| Tooltip per column instead of topics | The owner asked for the RouteIQ breakdown. A tooltip does not say what the page is not. |
| Re-query on Export | The workbook would be a different print from the one on screen. |
| CSV beside xlsx | ClosedXml is already the suite writer. A second format is a second contract. |
| Export the HAR body | The grid is the print. The file the operator opened is already on disk. |
