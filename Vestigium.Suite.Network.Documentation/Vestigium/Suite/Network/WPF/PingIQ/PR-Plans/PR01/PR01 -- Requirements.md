# PR01 — Requirements

**Document ID:** VEST-SUITE-NETWORK-PINGIQ-PR01-REQ  
**Host:** `Vestigium.Suite.Network.PingIQ`  
**Status:** Live  
**Date:** 26 September 2026  
**Product papers:** [Requirements_v1.1.md](../../Requirements_v1.1.md), [Design_v1.1.md](../../Design_v1.1.md)

This file is the **definition of done for PR01**. It is not a second product specification. When PR01 is accepted, v1.1 is the live window lock. Move this folder to `Completed/PR01/`.

Baseline is the stub exe: `HostLog.Initialize(HostIds.PingIQ)`, Target + Echo, Status + `Log` text box. No chrome. No grid. No cancel. No persist.

---

## Source (what it is)

**Not spoofing.** Spoofing is putting a source IP on the wire that this machine does not own. PingIQ will not do that.

**Source** is an optional **local bind address**. Empty / **Any** = Windows picks. A picked address = bind before the echo leaves (`ping -S`).

| Field | Question |
|---|---|
| Target | Which host do we echo? |
| Interface | Which NIC / index? |
| Source | Which of *this PC's* addresses does the packet leave from? |

Addresses come only from `NetworkHelper.GetAdapters()` → `UnicastAddresses`. The user cannot type an IP.

---

## Must land

### R01-01 Chrome

`PingIqWindow` + `VestigiumShell`. File / View. HorizontalTab **PingIQ | Dashboard | Settings**. Primary-monitor center. `Assets/PingIQ.ico`. ThemeManager before parse. View → Theme one check. No nav-indent items.

### R01-02 Echo window

Replace `Log`. Target, Count (default 4), Timeout (default 4000 ms), Interface combo, Echo, Cancel, summary, reply grid. `PingIqInput` rejects blank target, Count outside 1–60, Timeout outside 10–60000 ms.

### R01-03 Persist + Settings

`%ProgramData%\Vestigium\Settings\Diagnostics\PingIQ\settings.json`. Settings pages PingIQ / Probe / Theme. Source closed combo on Settings → PingIQ. Requests / Seconds on Settings → Probe only.

### R01-04 Probe prelude + pulse

Probe = one Echo, then N `Count = 1` echoes over X seconds. Same DueAt / slip math as DnsIQ `PulsePlan`. Failed Echo does not pulse. Pulse does not append rows.

### R01-05 Dashboard

Echo pie from last grid. Probe curve + histogram + control after pulse. Dashboard tab disabled until Probe completes. Charts 1.0.5 detached window. No ScottPlot in the host.

### R01-06 Host tests

Off the wire. `PingIqInput` + settings store temp root + `HostIds.PingIQ`. Do not call `IcmpEcho` from tests.

---

## Must not change in PR01

- Helpers.Network protocol surface
- Package pins in `Directory.Build.props`
- Spoofing a source IP this machine does not own
- `CreateEchoCampaign`, PathMtu, UdpProbe, Count = 0
- TTL / Buffer / DF checkboxes
- Other suite hosts
- Live curve during pulse
- Pulse history across process restarts

---

## Acceptance

Requirements v1.1 §5. Owner gate is PR01-07. This agent does not mark Rev 1 closed.

---

## Parked (not required to close PR01)

- Live curve while Probe runs
- Last-pulse JSON next to settings
- Loss sparkline on the status bar
- Continuous ping
