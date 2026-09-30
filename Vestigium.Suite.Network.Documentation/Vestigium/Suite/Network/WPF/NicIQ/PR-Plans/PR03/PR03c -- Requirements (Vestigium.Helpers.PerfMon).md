# PR03c — Requirements (`Vestigium.Helpers.PerfMon`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03C-PERFMON  
**Package:** `Vestigium.Helpers.PerfMon` 0.1.1  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**APPID:** `PerfMon`  
**EVENTID:** 17000–17499  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `src/Vestigium.Suite.Network.NicIQ/ViewModels/CachedPdhSource.cs`

## Goal

The default `ICounterSource` keeps PDH counters alive across samples so rate counters return a rate.

This version is not `perfmon.exe`, not a chart, not FileIo, and not a NicIQ-only source type.

## Defect

NicIQ comment on `CachedPdhSource`:

> The stock PerfMon source opens and disposes on every read, so rate counters stay at zero.

`SampleJob` already accepts `SampleJobOptions.Source`. The host implemented `ICounterSource` because the built-in one is wrong for `% Processor Time` and `Bytes Received/sec`. That is a library bug with a host workaround.

Unavailable stays Unavailable. Cache is not a license to invent 0.

## Must change in PerfMon core

### P-01 Cached local source is the default

`SampleJob` with no `Source` uses a source that:

- opens `PerformanceCounter` once per `CounterPath`
- primes on first read (rate counters need two observations)
- returns `SampleRecord.Unavailable` on InvalidOperation / missing instance / access denied
- disposes counters when the job / source is disposed

Name it in the public surface (`CachedPdhSource`, `LocalPdhSource`, whatever Alvin picks). One type. Host does not subclass it.

### P-02 Lifetime

Source lifetime follows the job, not the process, unless the host passes a long-lived source. Restarting the job for a new NIC instance must drop the old instance counters. No leaked `PerformanceCounter` on adapter switch.

### P-03 ListInstances

NicIQ `CachedPdhSource.ListInstances` wraps `PerformanceCounterCategory.GetInstanceNames` with a cap. If core already has `NetworkCounterCatalog.LiveInstances`, do not duplicate the API. If hosts need a generic lister, it lives on core once.

### P-04 Prime flag

NicIQ tracks `HasFreshPrime` so the first sample after open is not shown as a real 0 rate. Core should expose “this path is not primed” or suppress the first rate sample as Unavailable. Do not publish a fake 0 to mean “not primed.”

## Must not change

- Sample record shape.
- Satellite packages’ catalogs (those are PR03d).
- EVENTID range.
- Libraries still do not call `VestigiumLogger.Initialize`.

## Host after consume

Delete `CachedPdhSource.cs`. `SampleJob` construction in `MainViewModel` stops passing a host source unless tests need a fake.

## Acceptance

1. A one-second `SampleJob` on `Network Interface(*)\Bytes Received/sec` for a live instance produces a non-zero possibility after prime. Zero is allowed when the wire is quiet. Consecutive zeros on a busy NIC after prime is a fail.
2. Missing instance → Unavailable, not 0.
3. Dispose of the job disposes opened counters.
4. NicIQ no longer compiles `CachedPdhSource`.

## Tuesday-at-2am

Adapter rename / VPN flap: instance disappears mid-job. Next sample is Unavailable. Job does not throw through the dispatcher. Host status line already covers this; core must not throw.

## Parked

- Remote PDH.
- ETW.
- A Demo / CLI (forbidden on the 0.1 line).
