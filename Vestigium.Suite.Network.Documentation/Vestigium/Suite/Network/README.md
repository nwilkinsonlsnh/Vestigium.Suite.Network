# Overview v1.0

**Document ID:** VEST-SUITE-NETWORK-OVW-001  
**Location:** `Vestigium/Suite/Network/README.md`  
**Status:** Draft — structure locked, owner goal open  
**Date:** 25 September 2026

This file is the briefing. It tells a new session (or a tired one) what this repository is, where the two trees live, and which paper to open next.

It is **not** Requirements. It is **not** Design. It is **not** a PR plan.  
Host papers: [WPF/README.md](WPF/README.md).

---

## 1. Owner lock

Fill or correct this block. Facts below it follow the repo as of this date. The goal sentence is the only thing this file is allowed to argue about.

| Field | Value |
|---|---|
| Repo | `nwilkinsonlsnh/Vestigium.Suite.Network` |
| Solution | `Vestigium.Suite.Network.slnx` |
| Company | Wilkinson Business |
| **Goal** | WPF diagnostic hosts that consume `Vestigium.Helpers.Network`. Lab tech, Tuesday, one job per window. |
| **Not** | The protocol library. That stays in [Vestigium.Helpers](https://github.com/nwilkinsonlsnh/Vestigium.Helpers). |

Owner: rewrite **Goal** / **Not** if that sentence is wrong. Do not grow this file into a feature list to avoid rewriting one line.

---

## 2. What you opened

Two products share one solution. They must not be confused.

| Piece | Disk | Solution Explorer |
|---|---|---|
| Documentation project | `Vestigium.Suite.Network.Documentation/` | `/Documentation/` |
| Hosts + Shell | `src/Vestigium.Suite.Network.<Name>/` | `/Suite/Network/WPF/` |
| Tests | `tests/Vestigium.Suite.Network.Tests/` | `/Tests/` |

The docs project mirrors a **namespace path** so papers sit under `Vestigium\Suite\Network\WPF\<Host>`.  
The WPF projects do **not** live under that path. They live under `src\`. Same names. Different roots.

Open `Vestigium.Suite.Network.slnx` in Visual Studio 2026. That is the door.

---

## 3. Documentation project

**Project:** `Vestigium.Suite.Network.Documentation`  
**Kind:** `net10.0` markdown holder. Not WPF. Not packable. No compile items.  
**Job:** Keep the paper tree visible in Solution Explorer and on GitHub.

```
Vestigium.Suite.Network.Documentation/
  Vestigium.Suite.Network.Documentation.csproj
  Vestigium/Suite/Network/
    README.md                          this overview
    WPF/
      README.md                        host-paper index
      <Host>/
        Requirements_v1.0.md           the window (or chrome, for Shell)
        Design_v1.0.md                 class and window map
        PR-Plans/
          README.md                    queue keeper
          Completed/                   finished plans (needs a file or Git drops it)
```

`<Host>` is one of: `Shell`, `PingIQ`, `TraceIQ`, `DnsIQ`, `NicIQ`, `RouteIQ`, `ProbeHost`, `ShareIQ`.

A `.md` file is what Git keeps. Empty `Completed/` or `PR01/` folders in the `.csproj` `<Folder Include>` list are Solution Explorer only until a file lands inside them.

### Paper precedence

| Fight | Winner |
|---|---|
| Protocol fact | Helpers.Network Requirements v1.6 |
| Window / chrome | that host's `Requirements_v1.0.md` |
| Class and window map | that host's `Design_v1.0.md` |
| What we build this week | a live `PRnn -- Implementation Plan.md` |

Design is not a second lock table. A plan does not reopen Requirements.

---

## 4. WPF projects and Shell

**TFM:** `net10.0-windows`  
**UI:** WPF  
**Shape:** MVVM (`CommunityToolkit.Mvvm`)  
**Consume:** packages pinned in `Directory.Build.props` — Network 1.2.0, Analytics 1.0.1, Charts 1.0.1, Logging 1.7.1.  
**No** project reference to `Vestigium.Helpers`. Package only.

| Project | Kind | APPID | Owns (library doors) | First and ten |
|---|---|---|---|---|
| `Vestigium.Suite.Network.Shell` | class library | none | `HostIds`, `HostLog`, `BindFields` | Shared startup. Not a second `NetworkHelper`. |
| `Vestigium.Suite.Network.PingIQ` | exe | `PingIQ` | Echo, later PathMtu / UdpProbe / campaigns | Four echoes. Reply list. Cancel. |
| `Vestigium.Suite.Network.TraceIQ` | exe | `TraceIQ` | Trace, later Pathping | One target. One walk. Hop list. |
| `Vestigium.Suite.Network.DnsIQ` | exe | `DnsIQ` | Lookup, ProbeDns | One name. Lookup + Probe. |
| `Vestigium.Suite.Network.NicIQ` | exe | `NicIQ` | Adapters, WatchAdapter, later SampleCounters | Which NIC, is it up, how fast. |
| `Vestigium.Suite.Network.RouteIQ` | exe | `RouteIQ` | Routes, neighbors, ProbeNeighbor | Print + one probe. No default-route write. |
| `Vestigium.Suite.Network.ProbeHost` | exe | `ProbeHost` | GetSnapshot | One dump. Four lists. Read-only. |
| `Vestigium.Suite.Network.ShareIQ` | exe | `ShareIQ` | Share campaign via Network + FileIo | One UNC. One run. No password. |
| `Vestigium.Suite.Network.Tests` | test | — | HostIds / bind / host contracts | Not a protocol test suite. |

APPID is the host name. Never `Network`. Never `Shell`.  
Each exe calls `HostLog.Initialize(HostIds.<ThatHost>)` before the window shows. JSONL lands under `%ProgramData%\Vestigium\Logs\<APPID>\`.

Disk for code:

```
src/Vestigium.Suite.Network.Shell/
src/Vestigium.Suite.Network.PingIQ/
...
tests/Vestigium.Suite.Network.Tests/
```

---

## 5. Rules that do not move

- Protocol library is Helpers. This repo hosts windows.
- Shell does not wrap `NetworkHelper`.
- No port sweep. No default-route write. No HTTP client.
- Charts stay on the host that owns a series. Network does not plot.
- First and ten is the shippable window. Host "owns" later doors without shipping them yet.

---

## 6. Start here

1. This file — what the repo is.
2. [WPF/README.md](WPF/README.md) — which host paper to open.
3. That host's `Requirements_v1.0.md` — what the window must do.
4. That host's `Design_v1.0.md` — what types exist.
5. That host's `PR-Plans/README.md` — whether a slice is live.
6. `src/Vestigium.Suite.Network.<Host>/` — the project you actually change.

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 25 Sep 2026 | First briefing. Two trees. Owner goal left editable. |
