# PingIQ — Requirements v1.2

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-SRS-001  
**Version:** 1.2  
**Status:** Rev 1 as-built lock  
**Date:** 27 September 2026  
**Supersedes (window):** [Archived/Requirements/Requirements_v1.1.md](Archived/Requirements/Requirements_v1.1.md). 1.0 / 1.1 stay on disk as history.  
**Project:** `Vestigium.Suite.Network.PingIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `PingIQ` (never `Network`)  
**Protocol:** `Vestigium.Helpers.Network` only. No `ping.exe`. No raw sockets in the host.  
**Chrome:** `Vestigium.Themes`, `Vestigium.Controls`, StatusBar, NumericUpDown, Converters  
**Dashboard numbers:** `Vestigium.Helpers.Analytics` population metrics, `Vestigium.Helpers.Charts`  
**Binding:** Helpers.Network wins on ICMP and DNS. Analytics computes after the run. This file wins on the window.

Rev 1 is closed for now. Multipath and hop walk stay TraceIQ.

---

## 1. What ships

One host window (`PingIqWindow` + `VestigiumShell`). Three pages. Two jobs. Persist. Dashboard after Ping or Probe.

| Job | Door | Rev 1 |
|---|---|---|
| Ping | `NetworkHelper.IcmpEcho` with `Count = 1` per shot | Sequential. Count and Delay from Settings → PingIQ. Writes every row. |
| Probe | `NetworkHelper.IcmpEcho` with `Count = 1` per shot | Shots fire on the duration clock. Replies may land after End gate. Writes every row. |
| DNS | `NetworkHelper.LookupAsync` A then AAAA | Hostnames resolve to an IP before ICMP. Default: once per run. |
| PathMtu / UdpProbe / Campaigns / ping.exe | — | Out. |

---

## 2. Window

### 2.1 Chrome

| Piece | Rule |
|---|---|
| Shell | `PingIqWindow`: File / View, HorizontalTab strip, `VestigiumShell` `ShowNav=False`, status bar. |
| Icon | `Assets/PingIQ.ico`. |
| Size | Restored size fits the primary work area, capped at 1100×980. Minimize and Maximize stay. Drag-resize does not grow past that restored cap. Maximize fills the work area. |
| Placement | Cold start centers on `SystemParameters.WorkArea`. |
| Pages | **PingIQ** · **Dashboard** · **Settings**. |
| Dashboard | Disabled until a Ping or Probe finishes. Then both chart pages unlock. |
| View | Status bar visible, Top / Bottom, Theme submenu. |
| ComboBoxItem | Host template. No `ItemsControl` ancestor walk for content alignment. |

### 2.2 PingIQ page

| Field | Rule |
|---|---|
| Target | Editable combo. Type a name or address. MRU list, most recent first. Default last used. Cap 1–25, default 10. |
| Ping / Probe / Cancel | One token. |
| Grid | Seq, Status, Address, RTT (ms), TTL, Hops, Detail. Every shot writes a row. Hops from TTL buckets 64 / 128 / 255. |
| Results expander | Closed until Ping or Probe. Status, sent/recv/lost, Analytics population line, gates (Probe). |

Count, Delay, Interface, Source are **not** on this page.

### 2.3 Settings tabs

**PingIQ · Probe · MRU · Theme**

| Page | Fields |
|---|---|
| PingIQ | Resolve hostname once (default on). Count 1–99, default 4. Delay (ms) 10–60000, default 1000. Interface closed combo (Any = 0). Source closed combo (Any + unicast; narrowed by interface). |
| Probe | Requests 1–10000, default **300**. Milliseconds = total send window, 100–600000, default **5000** (5 s). Defaults button restores 300 / 5000. Live plan line. No calculator. |
| MRU | Remember targets 1–25. List add / update / delete / clear. Same collection as the Target combo. |
| Theme | Palette. Status bar visible. Status bar dock. `CheckBox.Standard`. |

### 2.4 Dashboard

| Page | Content |
|---|---|
| Ping | RTT series after a finished Ping. |
| Probe | Curve, histogram, control (when Analytics can form limits) after a finished Probe. Each plot min height 240. Page scrolls if short. |

Numbers are **population** metrics on the finished set (`NumericSeries.Full`). Not running sample stats.

---

## 3. Protocol

### 3.1 Resolve

If Target is an IP, skip DNS.  
If Target is a name: `LookupAsync` A, then AAAA. Ping the first address.  
Resolve once (default): one lookup, every ICMP uses that IP.  
Resolve once off: lookup before each shot, still ping the IP.  
NXDOMAIN / no A or AAAA → stop. Do not start ICMP.

RTT is ICMP. DNS time is not mixed into RTT.

### 3.2 Ping

`n` shots, `n` from Settings Count. Wait Delay ms between starts. Each shot `IcmpEcho` Count=1, reply timeout 4000 ms. Bind InterfaceIndex / SourceAddress.

### 3.3 Probe

`PulsePlan`: spacing = DurationMs / Requests.  
DueAt(1) = 0. DueAt(i) = spacing × (i − 1).  
At each due time start an `IcmpEcho` job. Do **not** wait for that reply before the next due time.  
`Task.WhenAll` after the last send.  
First-shot hard fail may still abort (prelude rule).  
Cancel cancels the token.

### 3.4 Gates (Results)

| Gate | Meaning |
|---|---|
| Start | Clock zero. |
| End | Planned send window (DurationMs). |
| Last send | When the last echo was issued. |
| Last packet | When the last reply landed. |
| Return wait | Last packet − End, or zero. |

Last send should sit near End gate. Last packet may be later.

---

## 4. Persist

`%ProgramData%\Vestigium\Settings\Diagnostics\PingIQ\settings.json`

Theme, Count, Delay, Interface, Source, Requests, DurationMs (and Seconds = DurationMs/1000 for older files), MRU list and cap, ResolveOnce, status bar, chart legend flags.

Load: if DurationMs missing or out of range, Seconds × 1000.

---

## 5. Out of Rev 1

- ping.exe, raw ICMP in the host  
- Multipath / traceroute (TraceIQ)  
- Campaigns, PathMtu, UdpProbe  
- Probe calculators  
- Continuous ping (Count 0)

---

## 6. Rev 1 close

PR01 shipped the window. Later field fixes (resolve, MRU, duration clock, gates) are included in this lock. Next work is a new rev, not more 1.x page churn unless a defect blocks the two jobs.
