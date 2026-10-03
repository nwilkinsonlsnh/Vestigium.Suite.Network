# RouteIQ — PR01a Chrome

**Status:** Landed  
**Date:** 3 October 2026  
**Owner override of PR01.**

PR01 locked one window, two lists, no suite shell, and a Probe button. The owner changed the window.

## Decision

Shell pages: Routes, Neighbors, Settings. No Dashboard. No File menu. No View menu. Theme is chosen and saved on Settings. Status-bar visible and dock save with it, because the menu that used to own them is gone.

Palette file: `%ProgramData%\Vestigium\Settings\Diagnostics\RouteIQ\settings.json`.

Packages on the exe, versions from `Directory.Build.props`: Themes, Controls, StatusBar, Converters. Not PerfMon. Not Charts. Not SystemInfo.

## Probe

Probe button is off the window. Refresh prints `GetRoutes` and `GetNeighbors`. It does not call `ProbeNeighbor`.

Dissent: those are different doors. Refresh prints the cache. Probe resolves one address and may ARP. A neighbor that is not already cached will not appear because Refresh ran. `RouteIqInput.TryParseProbe` stays for the host tests. The button does not.

## Not

NicIQ duration, chart window, adapter filters, theme menu.
