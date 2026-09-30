# PR03e — Requirements (`Vestigium.Helpers.Network`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03E-NETWORK  
**Package:** `Vestigium.Helpers.Network` 1.3.4  
**Repo:** `nwilkinsonlsnh/Vestigium.Helpers`  
**APPID:** `Network`  
**EVENTID:** 14500–14999 (used through 14560 — allocate in the unused tail)  
**Status:** Parked pending owner  
**Date:** 30 September 2026  
**Evidence:** `src/Vestigium.Suite.Network.NicIQ/ViewModels/WirelessLink.cs`  
**Product paper:** NicIQ Requirements v1.0 §5 — “Wireless SSID / BSSID extras — Not on `NetworkAdapter`.”

## Goal

If SSID / PHY / quality become suite facts, they land on Network inventory — not a new package, not PerfMon.Network, not the exe forever by accident.

This version is not a plot API, not a port sweep, not credential logging, and not `wlanapi` wrapped as FileIo.

## Why this is a fight, not a sneak

Catalog: Network owns adapters, snapshot, connections.  
NicIQ v1.0: wireless extras are out of first-and-ten because they are not on `NetworkAdapter`.  
Host Rev1 shipped `WirelessLinkLookup` anyway (`wlanapi.dll`, current connection, quality buckets).

That is a product change to Network, or it stays a host private. It is not Charts. It is not Themes.

## Decision (Dave, default)

**Park.** Do not expand `NetworkAdapter` in the same breath as the Charts / Themes extract.

NicIQ keeps `WirelessLink.cs` until the owner says the adapter record includes wireless association.

If the owner says yes, the requirements below become the Network delta. If the owner says no, delete the card from a later NicIQ slice or keep it as host-only with a comment pointing here.

## If the owner expands Network

### N-01 Door

`NetworkHelper` returns wireless association on the adapter when Type is `Wireless80211` and a current connection exists. Missing WLAN service / non-Wi-Fi NIC / no association → empty / null, not a throw.

### N-02 Fields (minimum)

SSID, PHY name, signal quality 0–100. BSSID optional. Do not log the profile as a secret if it is only a name; do not log keys.

### N-03 Implementation

One P/Invoke surface inside Network. NicIQ deletes `WirelessLink.cs` and reads the adapter.

### N-04 Not PerfMon.Network

PDH adapter rates stay in PerfMon.Network. RSSI-as-PDH if it exists later is still PerfMon, not this door.

## Must not happen

- `Vestigium.Helpers.Wireless`
- `Vestigium.Helpers.Wlan`
- Copying `WirelessLink.cs` into Charts or Themes

## Acceptance (only if unparked)

1. `GetAdapters()` on a Wi-Fi box with an association fills SSID without NicIQ calling `wlanapi`.
2. Ethernet rows stay empty on wireless fields.
3. `wlanapi` missing → empty, status stays usable.
