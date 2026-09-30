# PR03g-Memory — Requirements (NEW) `Vestigium.Helpers.SystemInfo.Memory`

**Package:** `Vestigium.Helpers.SystemInfo.Memory`  
**Depends on:** `Vestigium.Helpers.SystemInfo`  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers` — `src/Vestigium.Helpers.SystemInfo.Memory`  
**Evidence:** `MemoryHostFacts.cs`

## Goal

Replace `MemoryHostFacts`. Not the PDH series (that is `Helpers.PerfMon.Memory`).

## 0.1 surface

Physical total / available / in-use from `GlobalMemoryStatusEx`. Commit peak, kernel paged, kernel nonpaged from `GetPerformanceInfo` × page size. Raw values are bytes.

Door: `MemoryFacts.Read()`. Failed API → Unavailable on that API’s fields.

EVENTID 20000–20499.

NicIQ Memory card uses this snapshot **and** the PerfMon ring. One card, two packages.

Delete `MemoryHostFacts.cs` from NicIQ when this publishes.
