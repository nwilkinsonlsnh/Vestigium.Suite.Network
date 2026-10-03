# NicIQ — Requirements v1.1 (Monitoring)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-SRS-001  
**Version:** 1.1  
**Status:** Locked for the monitoring slice  
**Date:** 29 September 2026  
**Project:** `src/Vestigium.Suite.Network.NicIQ`  
**Library:** `Vestigium.Helpers.PerfMon` 0.1.1 + `Vestigium.Helpers.PerfMon.Network` 0.1.1  
**Binding:** Helpers.PerfMon wins on PDH. Helpers.Network still wins on inventory and status watch. This file wins on the window.
**Superseded by:** [Requirements_v1.2.md](Requirements_v1.2.md) for the current window. This file stays the monitoring slice.

v1.0 list + detail + status watch stays. This version adds live counter sampling.

This version is not charts, not `_Total` as the default instance, and not a second inventory engine.

---

## 1. What ships

| Job | Door | Rule |
|---|---|---|
| List adapters | `NetworkHelper.GetAdapters()` | Unchanged. |
| Primary NIC | host pick | Lowest IPv4 metric among **Up** adapters that have a unicast address. Saved Id wins if that adapter is still Up. |
| Live instances | `NetworkCounterCatalog.LiveInstances("Network Interface")` | Map adapter Name/Description onto a PDH instance. Slash → underscore. |
| Sample | `SampleJob` + `NetworkCounterCatalog.Paths` | One-second jobs, cancel token from the window. |
| Counter list | `NetworkInterface.Counters` | Settings \ Monitoring add/remove. Persist in `settings.json`. |

---

## 2. Window

| Piece | Rule |
|---|---|
| Main tab | Header **Monitoring**. Key stays `NicIQ`. |
| Open | Refresh inventory, select primary (or saved) NIC, start sampling. |
| Active NIC | Combo of **Up** adapters. Changing it restarts the sample job. |
| Samples | Latest value per selected counter. Unavailable is "—", never a fake zero. |
| Settings \ Monitoring | Available list vs selected list. Add / Remove. Empty selected list falls back to Bytes Received/Sent/Total per sec. |
| Close | Cancel the monitor token. |

Status watch (`WatchAdapter`) stays a separate button. It does not own the sample clock.

---

## 3. Acceptance

1. Window opens on Monitoring and begins sampling without a click.
2. Combo lists Up adapters only.
3. Changing the combo changes the PDH instance and the sample rows.
4. Settings \ Monitoring add/remove changes the next sample set and persists.
5. Missing PDH instance is a status line, not a crash.
6. v1.0 Refresh / Watch / Cancel still work.

---

## Document control

| Version | Date | Change |
|---|---|---|
| 1.0 | 24 Sep 2026 | List + detail + watch. |
| 1.1 | 29 Sep 2026 | PerfMon sample on primary NIC. Settings counter list. |
| 1.2 | 2 Oct 2026 | Current window. See [Requirements_v1.2.md](Requirements_v1.2.md). This file is unchanged below the control table. |
