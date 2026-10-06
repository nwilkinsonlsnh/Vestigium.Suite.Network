# RouteIQ — PR04 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR04
**Version:** PR04
**Status:** Live. Step 2 done.
**Date:** 5 October 2026
**Binding:** `PR04 -- Requirements.md`. This file wins on order. Requirements win on the window.

One pass per file. A later step that reopens a file from an earlier step is rework. Do not do it.

---

## 0. Done

Step 1 closed 5 October 2026. Requirements accepted with the owner path.
Step 2 closed 5 October 2026. RouteIQ pins come from props. No version bump. Sibling hosts not retargeted.

Not done: steps 3–6.

---

## 0.1 Mind change

Old belief: settings should move to LocalAppData because ProgramData is shared.
What arrived: owner decision. The folder is `C:\ProgramData\Vestigium\Settings\RouteIQ`.
New position: that path is the store. The gap is `Diagnostics` in the current path, and a save that swallows the error.
Out: LocalAppData. A dual read. A fallback store.

Dissent, one paragraph. A default ProgramData directory is writable by other local users, so a shared box can still plant theme, MRU queries, and the vendor opt-in. Owner accepted that. This slice does not add an ACL and does not elevate. If the folder cannot be written, the status line says so.

---

## 1. Order

Each step names the files it may touch. A file not in the row is out of that step.

| Step | Closes | Files | Exit |
|---|---|---|---|
| 1 | G1, G2 | This folder. Keeper. | Done. Requirements and this plan are the live papers. v1.0 stays historical. |
| 2 | G7 | `Directory.Build.props`. `RouteIQ.csproj`. | Done. QueryBar 1.0.3, ClosedXml 1.0.1, Kql 1.0.4, DI 10.0.12 come from props. No version bump. Sibling hosts not retargeted. |
| 3 | G5, S3 | `RouteIqSettingsStore.cs`. `SettingsViewModel.cs`. New `RouteIqSettingsStoreTests.cs`. | File is `%ProgramData%\Vestigium\Settings\RouteIQ\settings.json`. One-time copy from `Settings\Diagnostics\RouteIQ`. No second read. Save and load failures return a reason. `LiveVendorLookup` exists, default false. |
| 4 | G3, S1, G4, S2, R5–R7 | `MainViewModel.cs`. `NeighborsView.xaml`. `SettingsView.xaml`. `SettingsViewModel.cs` only for the toggle already added in step 3. | Refresh prints and applies packed OUI. It does not call `LookupOuiAsync` or `Ping`. Probe is one selected row or one typed address. Live vendor runs only when the setting is on, and the page shows `https://api.macvendors.com/{oui}`. |
| 5 | G8, S4, S5 | `RouteIqExport.cs`. `HelpView.xaml.cs`. | Open-after starts only the dialog path ending in `.xlsx`. Help starts only `https`. Help export topic says the workbook is local and names the columns that leave the grid. |
| 6 | G6, R11, R13 | `tests/Vestigium.Suite.Network.Tests`. | Store tests already exist from step 3. This step adds probe-scope, refresh-does-not-scan, xlsx guard, and KQL-miss tests. No network. |

SettingsViewModel is listed in step 3 and step 4 on purpose. Step 3 adds the field and persist. Step 4 adds the checkbox and the URL text only. Do not build the checkbox in step 3 and move it in step 4.

---

## 2. Step 2 — pins

Closed 5 October 2026. Commit `59ef1b7`.

| Property | Pin |
|---|---|
| `VestigiumQueryBarVersion` | 1.0.3 |
| `VestigiumClosedXmlVersion` | 1.0.1 |
| `VestigiumKqlVersion` | 1.0.4 |
| `MicrosoftExtensionsDiVersion` | 10.0.12 |

Props was 10.0.0. The host already referenced 10.0.12. Props now matches the host. That is a correction, not a bump. RouteIQ uses `$(...)`. PingIQ, NicIQ, and DnsIQ still hardcode DI 10.0.12. They were not edited.

---

## 3. Step 3 — settings file

`RouteIqSettingsStore.DefaultRoot` becomes:

```
Path.Combine(CommonApplicationData, "Vestigium", "Settings", "RouteIQ")
```

File name stays `settings.json`.

Copy once: if the new file is absent and `%ProgramData%\Vestigium\Settings\Diagnostics\RouteIQ\settings.json` exists, copy that file, then load the new path. Do not read the old path on the next launch. Do not delete the old file in this slice. PingIQ, NicIQ, and DnsIQ keep `Diagnostics`. This PR does not "align" them.

`Load` and `Save` stop swallowing. Return a result the session can put on the status line. Defaults on a bad file. No empty `catch`.

Add `LiveVendorLookup` to `RouteIqSettings`, default false. Persist it with the rest. A missing field in an old file deserializes false.

Test the path, the default, the one-time copy, and a save failure against a stand-in root. Do not hit ProgramData from the test.

---

## 4. Step 4 — Refresh stops scanning

One edit of `MainViewModel.Refresh`.

Today Refresh loads, applies packed OUI, then starts `ResolveLiveVendors` and `ProbeNeighbors`. Delete those two calls from Refresh. Packed OUI stays.

Probe becomes a command. Enabled when one neighbor row is selected, or when a single address parses. It pings that address only. `CanPing` stays: no loopback, no unspecified, no multicast. Disabled while a probe is running. Button on `NeighborsView`, next to Refresh. Do not put it on every tab.

Live vendor is the settings flag from step 3. When off, a packed miss stays `--`. When on, the existing pool (1–20) and the 1200 ms pace stay, and the call is still `LookupOuiAsync` with the library default URL. Settings page shows that URL next to the checkbox. Do not add a URL box. Do not add a proxy, a token, or a credential.

Do not log the full MAC. The library already logs the OUI on success.

---

## 5. Step 5 — launch allow-list and export disclosure

`RouteIqExport`: open-after runs only when the path is the path the dialog returned and the extension is `.xlsx`. Anything else reports and does not start.

`HelpView`: both `RequestNavigate` handlers start the process only when the scheme is `https`. Other schemes report and do not start.

Same Help edit: the export topic says the workbook stays on the machine and contains routes, neighbor MACs, process names, NetBIOS names, and LMHOSTS lines. No upload. No telemetry. Do not rewrite the other nine topics.

---

## 6. Step 6 — tests

| Test | Fails if |
|---|---|
| Store path | Root ends with `Vestigium\Settings\RouteIQ`, not `Diagnostics`. |
| Vendor default | New settings file has live lookup false. |
| Copy once | Second load does not read the old file. |
| Refresh seam | The print path does not call `LookupOuiAsync` or `Ping`. A flag or a test double is enough. No socket. |
| Probe scope | One address in, one ping. A grid is not the target. |
| Open-after | A non-xlsx path does not start. |
| KQL miss | A failed compile leaves the row filter showing rows. |

Existing `RouteIqInputTests` stay. This is not a stack suite.

---

## 7. Watch

| Watch | Failure |
|---|---|
| Path | A second store under LocalAppData "just in case." |
| Copy | Reading Diagnostics on every launch. |
| Siblings | Editing PingIQ, NicIQ, or DnsIQ in this PR. |
| Refresh | Leaving the live call behind a comment. |
| Probe | Walking the neighbor table from the new button. |
| Pins | Bumping QueryBar or Kql because the props file was open. |
| Help | A markdown pipeline. PR03 rejected it. |
| Elevation | An `app.manifest` with `requireAdministrator`. |

---

## 8. Out of this plan

Write-route form. Default-route button. Neighbor flush. Saved route sets. Charts. Publishing. Sibling settings paths. An ACL on ProgramData. Deleting the old Diagnostics file.

---

## Document control

| Version | Date | Change |
|---|---|---|
| PR04 | 5 Oct 2026 | Plan opened. Step 1 done. Settings path locked to `C:\ProgramData\Vestigium\Settings\RouteIQ`. |
| PR04 | 5 Oct 2026 | Step 2. QueryBar 1.0.3, ClosedXml 1.0.1, Kql 1.0.4, DI 10.0.12 live in props. RouteIQ csproj uses the properties. |
