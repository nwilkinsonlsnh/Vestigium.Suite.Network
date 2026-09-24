# PR03h — Requirements (NicIQ adapter details)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03H-DETAILS  
**Host:** `Vestigium.Suite.Network.NicIQ`  
**Status:** Recorded. Not started.  
**Date:** 2 October 2026  

## Goal

The monitor details pane stays a three-row glance. A detail form holds the rest of the adapter inventory. No new package.

## Pane

The NIC page title is the active adapter name. CPU and memory keep their titles. No selection stays `Network`.

Bottom right is a link labeled `Adapter details`. Click opens the form. Double-click on the adapter row opens the same form. The link is the visual aid. Double-click is the shortcut.

## Card rows

| Row | Wi-Fi | Ethernet |
| ---: | :--- | :--- |
| 1 | Send, receive, PHY and band | Send, receive, link speed |
| 2 | SSID, signal | MAC, status |
| 3 | IPv4, domain | IPv4, gateway |

## Form

One window. Title is the adapter name. Blocks: Identity, Link, Addresses, Driver. Two columns. Empty is a dash.

Wi-Fi adds BSSID, receive rate, transmit rate, and security only when the type is `Wireless80211`. Security is the algorithm, cipher, and 802.1X flag in one cell. Ethernet does not render that block.

## Must not happen

- Profile name on the card or the form.
- Keys, PSK, or certificates.
- The form calling `wlanapi`.
- A hole reserved for channel, frequency, or dBm.
- A `Vestigium.Helpers.Wireless` package.
