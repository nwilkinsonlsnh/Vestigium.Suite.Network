# Published NuGet Packages — Vestigium

**Publisher:** Zazoo on nuget.org  
**Prefix:** `Vestigium.*`  
**Snapshot:** 2026-09-30  
**Count on the Zazoo profile listing:** 20 packages. NuGet search for “Vestigium” reported 21 hits; the profile page lists these 20 IDs. If a twenty-first ID appears, add it here before anyone treats a name as free.

This file is the room’s map. Use it before a new package is named, scoped, or published. A paragraph here is intent, not marketing. If a proposed package’s job already lives in one of these paragraphs, the answer is extend that package — not mint a sibling.

## How to use this file when proposing a new package

1. State the job in one sentence and what this version is not.
2. Scan the families below. Same domain = same package, even if the filter, schedule, or host is new.
3. If two packages could own it, that is a fight to settle here, not a reason to ship a third.
4. Libraries never call `VestigiumLogger.Initialize`. The host owns APPID and log directory. Helpers register catalogs into that host config.
5. Helpers compute or run jobs. Charts draw. Controls render chrome. Themes paint. Logging writes JSONL. Do not collapse those.
6. Hashing is not Encryption. Network is not Charts. FileIo is not `robocopy.exe`. PerfMon is not `perfmon.exe`. SystemInfo is not PerfMon. Those “not” lines are invariants.

**Families**

| Family | Owns | Does not own |
| --- | --- | --- |
| `Vestigium.Logging` | JSONL audit spine, EVENTID, flood, seal, WPF-safe subscribers | Payload documents, file copy, plots |
| `Vestigium.Helpers.*` | Jobs, numbers, inventory, hashes, JSON documents, PDH samples, machine-fact snapshots | Shell, themes, converters |
| `Vestigium.Helpers.Charts` | Drawing Analytics results on WPF / PNG | Computing UCL/CL/LCL or run rules |
| `Vestigium.Helpers.SystemInfo*` | OS snapshots: CPU topology/clocks/census, physical memory/pools | PDH series, adapter inventory, protocol jobs |
| `Vestigium.Themes` | Palettes + ThemeManager + control catalog look | New controls |
| `Vestigium.Converters` | IValueConverter catalog | Controls |
| `Vestigium.Controls*` | Shell and reusable WPF controls | Theming engine, helpers |

Known EVENTID reservations from published READMEs (do not reuse):

| Range | Owner |
| --- | --- |
| 10000+ | Host custom catalogs via Logging |
| 12500–12999 | FileIo |
| 13000–13070 | Hashing (used through 13070) |
| 13500–13999 | Json |
| 14500–14999 | Network (used through 14560) |
| 16500–16999 | Charts (used through 16545) |
| 17000–17499 | PerfMon core |
| 17500–17999 | PerfMon.Network |
| 18000–18499 | PerfMon.Cpu |
| 19000–19499 | SystemInfo core (proposed, unpublished) |
| 19500–19999 | SystemInfo.Cpu (proposed, unpublished) |
| 20000–20499 | SystemInfo.Memory (proposed, unpublished) |

Disk / Gpu / Memory **PerfMon** satellite ranges were not printed on the NuGet pages reviewed for this snapshot. Confirm in the Helpers requirements docs before allocating more.

---

## Foundation

### Vestigium.Logging — 1.7.1

Intent: this is the owned JSON Lines logger for Vestigium hosts and for anything outside the suite that agrees to the same contract — one JSON object per line for PowerBI, required EVENTID, LEVEL as severity and STATUS as outcome, flood suppression on a four-field identity, optional HMAC seal, and WPF-safe batched subscribers so the UI is never driven one line at a time. It exists because diagnostic modules log stack traces and HTTP bodies and pipe-delimited text splits those payloads. It is not a general logging facade over Serilog/NLog, not FileIo, not the JSON document helper, and not a place to dump payload bodies. Hosts call `VestigiumLogger.Initialize` with *their* APPID and bind lifetime; helper libraries register catalogs and otherwise no-op. Do not create another logger package. Extend the catalog or the host custom EVENTID space (10000+) instead.

### Vestigium.Helpers.Json — 1.0.1

Intent: System.Text.Json helpers for payload documents and JSONL that a host can open, snapshot, edit through pointer or dotted path, inspect as an RFC 6902 patch, then commit and save — pretty + camelCase for `.json`, one path, 64 KiB streams. APPID `Json`, EVENTID 13500–13999. It is not the audit logger (that is Logging), not FileIo, and it must not log payload bodies, field values, PEM, or Exception objects. Campaign stats and structured payloads belong here; append-only operational JSONL belonging to a job still goes through Logging. Do not stand up `Vestigium.Helpers.JsonPatch` or a second serializer package.

### Vestigium.Helpers.Hashing — 1.4.1

Intent: string and file hashing only — SHA-2, SHA-3, HMAC-SHA2/SHA3, KMAC, SHAKE, Argon2id (PHC), CRC-32/64, xxHash — hex default, `HashFile` streams so nobody `ReadAllBytes` a payload, fail-closed `VerifyPassword`, named EVENTIDs 13000–13070. APPID `Hashing`. Explicitly separate from Encryption. A new cipher, envelope, or key-wrap package is not this package; a new digest or MAC is this package. FileIo already depends on it for content identity. Do not duplicate hash APIs inside FileIo, Network, or Logging.

### Vestigium.Helpers.Analytics — 1.0.1

Intent: descriptive statistics, quartile bands, confidence intervals, Shewhart control limits, capability, and run rules over a finite numeric series (`NumericSeries`). It computes. It does not draw and it does not persist. Charts consumes the numbers; FileIo uses it for sizes and rates; Network uses it for P95 / billing math. Do not put UCL/CL/LCL calculation into Charts, and do not invent a second stats package because a host wants a different percentile. Missing APPID/EVENTID on the NuGet page does not make a new analytics package legitimate — register through `AnalyticsCatalog` on the existing one.

### Vestigium.Helpers.Charts — 1.0.7

Intent: ScottPlot wrapper that puts Analytics numbers on a WPF form (or a PNG via `ChartView.SavePng`). Analytics or the host supplies series, limits, and rules; Charts draws control charts and histograms. ScottPlot types stay internal. It does not compute UCL/CL/LCL, run rules, spec fences, or KDE points. APPID `Charts`, EVENTID 16500–16999. Not a plot API for Network. If the ask is “show the P95,” compute in Analytics and draw here. If the ask is “new chart kind for the same series,” extend Charts — do not mint `Vestigium.Helpers.Plots`.

---

## Jobs

### Vestigium.Helpers.FileIo — 1.1.2

Intent: validated file jobs whose behavior reference is robocopy and whose product is the record — recon into five size buckets, UniqueName (`.##`) as the default collision policy, `NameCap` so the original dest is never overwritten unless the caller picks Overwrite, Audit Mode that writes Would* decisions and touches no disk, Pause/Cancel that removes only files this job created, Analytics for sizes/rates, Hashing for content identity, ALCOA+ JSONL. APPID `FileIo`, EVENTID 12500–12999. It does not spawn `robocopy.exe`, does not `ReadAllBytes` a payload, and does not log payload bytes or Exception objects. Share probes in Network go through this package; they do not become a second file library.

### Vestigium.Helpers.Network — 1.3.4

Intent: workstation inventory and protocol jobs for diagnostic hosts (PingIQ, DnsIQ, TraceIQ, and kin) — adapters, snapshot, connections, routes, neighbors, ICMP echo/trace/pathping, single-host TCP/UDP probe, DNS probe, adapter watch, counter sample, path MTU, prefix math, echo/share campaigns, P95 billing via Analytics, share probes via FileIo, campaign stats via Json. APPID `Network`, EVENTID 14500–14999. Not a CLI. Not `ping.exe` / `tracert.exe` / `pathping.exe`. Not a plot package and it will not grow a plot API. No port sweep. No credential logging. OUI completeness is a caller URL; the embedded snapshot is a stub, not the IEEE registry. Route writes need admin. Do not create `Vestigium.Helpers.Ping`, `Dns`, `Trace`, or `Share` — those jobs already live here. Drawing results is Charts. PDH adapter rates are PerfMon.Network, not this package. Machine CPU/RAM snapshots are SystemInfo, not this package.

---

## PerfMon

PerfMon is one contract with satellite counter sources. A new counter family is a satellite under `Vestigium.Helpers.PerfMon.*`, not a root helper and not a chart package. All of these are 0.1.1, net10.0-windows, no Demo project. Missing categories are `Unavailable`, never a fake zero. None of them call `VestigiumLogger.Initialize`. None of them are `perfmon.exe`. None of them plot.

### Vestigium.Helpers.PerfMon — 0.1.1

Intent: shared performance sample contract, job runner, and counter source. APPID `PerfMon`, EVENTID 17000–17499. Satellites depend on this. If the sample shape or runner changes, it changes here once.

### Vestigium.Helpers.PerfMon.Cpu — 0.1.1

Intent: processor PDH samples. APPID `PerfMon.Cpu`, EVENTID 18000–18499. Do not fold CPU counters into Network or into a host. Topology, clock MHz, and process/thread/handle census are SystemInfo.Cpu, not this package.

### Vestigium.Helpers.PerfMon.Network — 0.1.1

Intent: adapter PDH rates and errors. APPID `PerfMon.Network`, EVENTID 17500–17999. Different from `Vestigium.Helpers.Network`, which owns protocol jobs and inventory. Rates live here; ICMP/DNS/share jobs live there.

### Vestigium.Helpers.PerfMon.Disk — 0.1.1

Intent: physical and logical disk PDH samples. Not FileIo. FileIo copies and records files; this package samples disks.

### Vestigium.Helpers.PerfMon.Memory — 0.1.1

Intent: commit, available, and machine memory **samples**. Physical total / in-use snapshots and kernel pools are SystemInfo.Memory, not a junk drawer here.

### Vestigium.Helpers.PerfMon.Gpu — 0.1.1

Intent: GPU engine and adapter memory the OS exposes. If the OS does not expose it, the sample is Unavailable — do not invent a vendor SDK inside this package without an explicit owner decision.

---

## SystemInfo (named, not yet on nuget.org)

SystemInfo is one contract with satellites, same shape as PerfMon. Snapshots, not a 1 s PDH clock. Projects live in `Vestigium.Helpers`. IDs are `Vestigium.Helpers.SystemInfo*` — never a root `Vestigium.SystemInfo` prefix. No Demo / CLI on 0.1. Unavailable, never a fake zero. None of them call `VestigiumLogger.Initialize`. None of them plot. None of them list adapters.

### Vestigium.Helpers.SystemInfo — unpublished

Intent: shared snapshot records and catalog hook. APPID `SystemInfo`, EVENTID 19000–19499 proposed. Satellites depend on this.

### Vestigium.Helpers.SystemInfo.Cpu — unpublished

Intent: processor topology, current/max MHz, process/thread/handle census. APPID `SystemInfo.Cpu`, EVENTID 19500–19999 proposed. Not `% Processor Time`.

### Vestigium.Helpers.SystemInfo.Memory — unpublished

Intent: physical total / available / in-use, commit peak, paged / nonpaged pools. APPID `SystemInfo.Memory`, EVENTID 20000–20499 proposed. Not the PDH series.

Do not create `Vestigium.Helpers.SystemInfo.Network`. Adapters and WLAN stay on Helpers.Network. Rates stay on PerfMon.Network.

---

## WPF chrome

These packages exist so PingIQ / DnsIQ / TraceIQ / HttpIQ / ProbeHost share one shell language. New chrome that is “another Vestigium window” belongs in Controls. New palette belongs in Themes. New IValueConverter belongs in Converters. A one-off control that is not reusable across hosts does not get a `Vestigium.Controls.*` package.

### Vestigium.Themes — 1.0.2

Intent: modular WPF theming for the suite — one control language, many palettes, runtime swap without rebuilding the host. One nupkg, eleven DLLs (core, catalog, nine v1 palettes: Light Blue, Dark Mode, Terminal, Solarized Dark, Standard WPF, Monokai, Sublime/Mariana, Dracula, Nord). Hosts register only the palettes they will switch; NuGet does not call `Register`. Initialize before the first window parses. A new palette is a new DLL inside this pack, not `Vestigium.Themes.Nord2`.

### Vestigium.Converters — 1.0.0

Intent: DI-driven WPF `IValueConverter` catalog (CONV-01–CONV-42) so a host registers one catalog, assigns `VestigiumConverterHost.ServiceProvider`, and resolves converters from XAML without a static resource per converter. Visibility, boolean, formatting, numeric, collection, multi-binding, and domain brushes/descriptions for ping/DNS/HTTP/trace/port/loss — domain converters take enum or string and do not reference host assemblies. Convert never throws on the dispatcher. Do not add converters inside Controls or inside a host if they are suite-reusable.

### Vestigium.Controls — 1.0.0

Intent: shared WPF shell, default Vestigium form (`VestigiumDefaultWindow`: menu + empty client + status bar), and DI host for the suite. Hosts consume; this package does not ship the diagnostic tools. Child control packages are separate so a host can take NumericUpDown without the whole inspector. Do not create a second shell package.

### Vestigium.Controls.StatusBar — 1.0.0

Intent: bindable WPF status bar with Left/Center/Right slots and Top or Bottom dock. Default dock is Bottom. If the ask is “status text on the shell,” this is the control.

### Vestigium.Controls.PropertiesGrid — 1.0.0

Intent: WPF property inspector (Win11-shaped) with multi-select, collections, reset, and nested expand. Not a DataGrid. Not a form layout engine.

### Vestigium.Controls.NumericUpDown — 1.0.1

Intent: WPF decimal spinner with Immediate or Deferred commit, signed/unsigned, and Round/Floor/Ceiling snap. If the ask is “a number box with ticks,” extend this.

### Vestigium.Controls.UnderConstruction — 1.0.0

Intent: WPF placeholder page and overlay with title, subject, and multi-line description. The default window’s menu items resolve here until a host replaces them. Not a dialog framework.

---

## Intentionally unpublished (named so nobody “fills the gap” by accident)

- **Encryption** — Hashing’s README reserves this as a separate domain. Do not sneak ciphers into Hashing.
- **A plot API on Network** — rejected in Network 1.3.x. Draw with Charts.
- **robocopy.exe wrapper** — FileIo is the record, not a process spawn.
- **IEEE OUI registry package** — Network’s packed table is a stub; live lookup is a caller URL.
- **PerfMon Demo / CLI** — forbidden on the 0.1 line.
- **Per-host converter or theme packages** — catalog and palettes are already suite-wide.
- **`Vestigium.SystemInfo` root prefix** — the family is `Vestigium.Helpers.SystemInfo*`.
- **`Vestigium.Helpers.SystemInfo.Network`** — adapters and WLAN stay on Helpers.Network.

---

## Decision rule for the next package

Facts about what is already published sit in this file. Shape of a new helper still goes Alvin → Dave. “We need a package for X” is not a destination until X fails the domain test above.

**This version of the catalog is not:** a substitute for each package’s requirements doc, a download dashboard, or permission to republish an old version number as if it grew new surface.
