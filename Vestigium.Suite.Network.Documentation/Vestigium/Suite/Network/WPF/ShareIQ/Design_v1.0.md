# ShareIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.ShareIQ`

## Window

Target text box, Probe, Cancel, Status, read-only log.  
Zero credential controls — not `Visibility=Collapsed`, not present.

## Types

| Type | Role |
|---|---|
| `App` | `HostLog.Initialize(HostIds.ShareIQ)`. |
| `MainWindow` | Now needs a DataContext (`MainViewModel`). Today it is code-behind only; first-and-ten adds the VM. |
| `MainViewModel` | Target, Status, Log, Probe/Cancel. |

## Flow

1. Reject blank Target.
2. `CreateShareCampaign(new ShareCampaignOptions { Target = … })` — only the properties the library requires for a one-shot. No password properties.
3. `await campaign.RunAsync(token)`.
4. Status + short summary in Log.

`PlanShareProbe` is not called. It needs `FileIoDirectoryAnalysis`.

## Out of this design

Recipe path picker. Clock window. FileIo recon. Credential UI.
