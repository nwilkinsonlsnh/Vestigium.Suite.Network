# ShareIQ — Requirements v1.0 (First and ten)

**Document ID:** VEST-SUITE-NETWORK-SHAREIQ-SRS-001  
**Version:** 1.0  
**Status:** Locked for the first shippable window  
**Date:** 24 September 2026  
**Project:** `Vestigium.Suite.Network.ShareIQ`  
**Kind:** WPF exe, `net10.0-windows`, MVVM  
**APPID:** `ShareIQ` (never `Network`)  
**Library:** `Vestigium.Helpers.Network` 1.2.0 (+ FileIo transitively)  
**Binding:** Helpers.Network Requirements v1.6 (lock 32: share campaigns, no password field). This file wins on the window.

First and ten is “can this box reach that UNC.” One path. One run. No credentials. No scheduler.

---

## 1. What ships

| Job | Library door | First-and-ten |
|---|---|---|
| One-shot campaign | `NetworkHelper.CreateShareCampaign(ShareCampaignOptions)` then `RunAsync` | **Required.** |
| Plan from FileIo analysis | `PlanShareProbe(FileIoDirectoryAnalysis, …)` | **Out.** Needs a FileIo recon object this host does not own yet. |
| Saved recipe / clock window | campaign JSON | **Out.** Second down. |

---

## 2. Window

| Field | Rule |
|---|---|
| Target | Required UNC or local path the library accepts. Trim. Blank → reject. Example `\\server\share`. |
| Probe | Builds options with that target only and runs the campaign once. Disabled while running. |
| Cancel | Cancels the token. |
| Status | Idle / Running / Success / Failed / Cancelled + library status text. |
| Log | Result summary only (status, label, error). No file listing dump this release. |

**Forbidden on this window:** password, username, domain, credential manager picker.

---

## 3. Behavior

| # | Rule |
|---|---|
| S1 | `App.OnStartup` calls `HostLog.Initialize(HostIds.ShareIQ)` before the window shows. Logs under `%ProgramData%\Vestigium\Logs\ShareIQ\`. |
| S2 | MVVM. Code-behind does not call `NetworkHelper`. |
| S3 | No password field. Not hidden. Not in options. Not in JSONL. |
| S4 | One in-flight run. |
| S5 | File work is FileIo through Network. No `robocopy.exe`. No `net use`. |
| S6 | Recipe/results path rules stay in the library (must resolve under the campaign root when a campaign dir is used). First ten may use the library default location. |
| S7 | Failed reachability is a status line, not a crash. |
| S8 | No echo, trace, DNS, or route UI. |

---

## 4. Acceptance

1. Window opens. APPID folder exists after first run.
2. Blank target does not start. |
3. No credential controls in XAML. |
4. Probe of a missing share returns Failed (or the library equivalent) without throwing out of the UI. |
5. Cancel stops a running job. |
6. No Plan button. No Save recipe button. No schedule controls.

---

## 5. Out of first and ten

| Item | Why later |
|---|---|
| `PlanShareProbe` | Requires `FileIoDirectoryAnalysis`. |
| Recipe persist + clock windows | Campaign scheduler story. |
| Copy/mirror verbs on the share | FileIo suite, not this first window. |
| Credentials | Permanent non-goal for this host. |

---

## 6. Stack

| Item | Value |
|---|---|
| IDE | Visual Studio 2026 |
| TFM | `net10.0-windows` |
| UI | WPF |
| Architecture | MVVM (`CommunityToolkit.Mvvm`) |
| Shell | `HostLog`, `HostIds.ShareIQ` |
| Protocol | `Vestigium.Helpers.Network` (+ FileIo via that door) |

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | First and ten. One UNC, one run, no password. |
