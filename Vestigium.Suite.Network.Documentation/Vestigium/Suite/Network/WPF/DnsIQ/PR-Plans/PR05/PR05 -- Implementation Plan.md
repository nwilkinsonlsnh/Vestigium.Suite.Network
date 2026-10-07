# PR05 — Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-DNSIQ-PLAN-PR05
**Host:** `Vestigium.Suite.Network.DnsIQ`
**APPID:** `DnsIQ`
**Status:** Live
**Date:** 7 October 2026
**Binding:** [PR05 -- Requirements.md](PR05%20--%20Requirements.md) wins on this slice. Requirements_v1.2 wins on the existing window until this PR folds. ClosedXml 1.0.1 wins on the workbook call. This file wins on order.

**Goal:** Add the RouteIQ Help reader and Exports workbook to DnsIQ, with DnsIQ copy and DnsIQ sheets.

**Not:** A shared Help control. Not a change to Lookup, pulse, or Capture. Not a chart export. Not a settings-folder move. Not Help on the other hosts.

---

## Starting point

| Piece | Today | Required |
|---|---|---|
| DnsIQ strip | DnsIQ, Capture, Dashboard, Settings. File → Open capture. | Help tab on the right. Exports between Dashboard and Settings. |
| Help | None. | Topic rail + reader. Ten topics. Claims in R05-02. |
| Export | None. ClosedXml 1.0.1 is pinned in `Directory.Build.props` and used by RouteIQ. DnsIQ does not reference it. | Package reference. Cover + Lookup + Capture + Probe. |
| Settings | `Settings\Diagnostics\DnsIQ\settings.json`. | Four new keys. Same file. |
| RouteIQ | HelpView + ExportsView + `RouteIqExport`. | Read-only pattern. Do not edit RouteIQ in this slice. |

---

## Decision

Copy the chrome. Write new copy. Do not lift RouteIQ types into DnsIQ.

| Call | Why |
|---|---|
| Help stays out of `VestigiumShell` | RouteIQ already proved the right-aligned tab. A shell item would put Help in the nav walk and the splash. DnsIQ has no splash. It still does not belong in the spec. |
| Lazy on first check | The reader is text. Building it at startup buys nothing. |
| Export writes from the view-models already on screen | `Answers`, `Hosts`, and the RTT list. A second read would disagree with the grid. |
| Hold the RTT list on `DashboardViewModel` | `ShowProbe` already receives it and drops it. Keep the list. Do not persist it. |
| ClosedXml package, not a project reference | Suite rule. Pin is already 1.0.1. |
| Checks in the existing session store | A second file is a second load path. |

### Rejected alternatives

| Idea | Why out |
|---|---|
| Shared Help project this slice | Moves RouteIQ or forks the reader. Parked. |
| Help as a File menu item | Hides the topic rail. The owner pointed at RouteIQ's tab. |
| Probe DNS button to match the old PR04 plan | The shipped page does not have one. Help must not document a control that is not there. |
| Export chart PNG | Charts paint a reading. The sheet is the samples. |
| Re-run Lookup inside Export | The file would not be the print on screen. |

---

## Library doors this slice may call

Existing, unchanged:

```
WorkbookHelper.Create(coverName, appName)
book.Sheet(name).WriteTable(SheetTable)
book.SaveAs(path)

NetworkHelper is not called from Export or Help.
```

DnsIQ project gains:

```
PackageReference Vestigium.Helpers.ClosedXml  $(VestigiumClosedXmlVersion)
```

No new package. No Helpers edit.

---

## Types to land

| Type | Project | Role |
|---|---|---|
| `HelpView` | DnsIQ | Rail, sections, reader. Copy of the RouteIQ chrome, not a link to it. |
| `HelpTopic` | DnsIQ | Title, document, sections. Local. Do not reuse RouteIQ's type. |
| `DnsIqExport` | DnsIQ | Partial on `MainViewModel`, or a small writer the VM calls. One write path. |
| `DashboardViewModel.ProbeSamples` | DnsIQ | The `IReadOnlyList<double>` from the last `ShowProbe`. Empty until a pulse returns times. |

`App.CreateMainWindow` gains the Exports nav item and the Help tab hook. Code-behind still does not call `NetworkHelper` or `WorkbookHelper`.

---

## Window map

```
strip   DnsIQ | Capture | Dashboard | Exports | Settings          Help (right)
client
  Help
    rail: Overview, Lookup, Probe, Capture, Dashboard, Exports, Settings, Glossary, About, License
    reader: FlowDocument
  Exports
    caption
    Lookup, Capture, Probe
    Open file after saving, Open folder after saving
    Export, Export All
```

Capture, Lookup, and Dashboard markup stay. File → Open capture stays.

---

## Behavior

### Help

1. Check builds `HelpView` once and docks it over the shell client, or hides the shell client and shows the reader. Uncheck reverses that. The selected shell item does not change.
2. Overview is selected.
3. Glossary opens on Tabs.
4. A link that is not `https` does not navigate.

### Export

1. Buttons off while `Answers`, `Hosts`, and `ProbeSamples` are all empty.
2. Export with no check → status, return.
3. Dialog. User cancel → no change.
4. Build Cover from the live knobs. Add a sheet only when that print has rows.
5. If every checked print is empty, do not save. Status `Nothing loaded to export.`
6. Save. Missing file → status, return.
7. Open-after and open-folder only after the `.xlsx` check in R05-03.
8. Check changes call `session.Save()`.

---

## Implementation table

| Order | ID | Do | State |
| ---: | :--- | :--- | :--- |
| 1 | PR05-01 | Help tab, `HelpView`, ten topics, https allow-list. No export yet. | |
| 2 | PR05-02 | ClosedXml reference. Exports tab. Cover, Lookup, Capture, Probe. Empty and open-after rules. | |
| 3 | PR05-03 | Four settings keys. Defaults. Round-trip. | |
| 4 | PR05-04 | Host tests in R05-05. Off the wire. | |
| 5 | PR05-05 | Owner gate on the clone. | |

### PR05-01

Copy the RouteIQ reader shape (`HelpView.xaml` rail and `FlowDocumentScrollViewer`). Do not copy RouteIQ prose. DnsIQ topics are R05-02. License text is read from the linked `LICENSE` the suite already copies to output, not retyped from memory.

### PR05-02

`DnsIqExport` mirrors `RouteIqExport` only in the dialog, the cover key/value sheet, and the open-after guard. Sheet columns are the DnsIQ grids. `ShowProbe` keeps the sample list it was given.

### PR05-03

Add the keys next to the existing session fields. Do not version the file. Missing keys are the defaults, not a corrupt file.

### PR05-04

Assert topic titles and the four claims in R05-05. Assert the writer with in-memory rows. Do not launch Excel in a test.

### PR05-05

Owner on the clone:

1. Help opens on the right. Capture topic says Resolved is not Reachable.
2. Lookup, then Export. Workbook has Cover and Lookup. No second query in the status line.
3. Open a capture, let the probe fill DNS, Export All. Capture sheet matches the grid, including Error.
4. Probe, then Export with Probe checked. Sheet is Index and RttMs. Dashboard still unlocks.
5. Restart. Checks are as left. Open-after is off unless it was saved on.
6. Lookup and Capture still share one in-flight job.

This plan does not mark the owner gate closed.

---

## Files this plan expects to touch

```
src/Vestigium.Suite.Network.DnsIQ/Vestigium.Suite.Network.DnsIQ.csproj
  PackageReference Vestigium.Helpers.ClosedXml

src/Vestigium.Suite.Network.DnsIQ/DnsIqWindow.xaml
  Help tab, right aligned

src/Vestigium.Suite.Network.DnsIQ/DnsIqWindow.xaml.cs
  lazy Help, show / hide

src/Vestigium.Suite.Network.DnsIQ/App.xaml.cs
  Exports nav item between Dashboard and Settings

src/Vestigium.Suite.Network.DnsIQ/Views/HelpView.xaml            [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/HelpView.xaml.cs        [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/ExportsView.xaml        [NEW]
src/Vestigium.Suite.Network.DnsIQ/Views/ExportsView.xaml.cs     [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DnsIqExport.cs     [NEW]
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DashboardViewModel.cs
  keep ProbeSamples
src/Vestigium.Suite.Network.DnsIQ/ViewModels/DnsIqSession.cs
  four keys

tests/… host tests for topic titles, claims, and the writer
```

Do not edit RouteIQ. Do not edit Helpers.Network. Do not edit LogParser. Do not bump ClosedXml.

When this plan finishes, move `PR05/` to `PR-Plans/Completed/PR05/`, fold the window delta into Requirements_v1.3 / Design_v1.3, and point the queue README at the next slice.

---

## What each watch

| Role | Watch |
|---|---|
| Alvin | No shared Help project. No chart PNG. No second writer. Export does not call `LookupAsync`. Help is not a shell item. |
| Theodore | Empty export writes nothing. Non-`.xlsx` open-after does not launch. Pulse-does-not-append and Resolved-is-not-Reachable are in the reader. Tests stay off the wire. |
| Simon | Help describes the shipped Capture page, not the PR04 button that was not built. Probe sheet is the RTT list, not a new measurement. Settings path does not move. |

---

## Next action

PR05-01. Help tab and the ten topics. Exports waits until the reader is in.
