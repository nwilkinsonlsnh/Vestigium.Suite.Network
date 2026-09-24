# RouteIQ — PR03 Implementation Plan

**Document ID:** VEST-SUITE-NETWORK-ROUTEIQ-PLN-PR03  
**Version:** PR03  
**Status:** Accepted. Step 5 done. PR03 closed.  
**Date:** 4 October 2026  
**Binding:** `PR03 -- Requirements.md`.

There is no step 6. There is no design paper.

---

## 0. Done

Step 1 closed 4 October 2026. Documents Accepted.

Not done: nothing in this plan.

---

## 1. Order

| Step | Where | Exit |
|---|---|---|
| 1 | This folder | Done. Documents Accepted. |
| 2 | `src/Vestigium.Suite.Network.RouteIQ/Assets/RouteIQ.ico` | Done. 16, 24, 32, 48, 64, 128, 256. Node, one arrow in, two out. `ApplicationIcon` and `Resource` set. |
| 3 | `MainWindow.xaml` | Done. Help is a right-docked `RadioButton.HorizontalTab`, group `RouteIqHelpNav`. Work tabs stay in `RouteIqMainNav`. A work-tab click closes Help. |
| 4 | `Views/HelpView` | Done. Two columns. `RadioButton.VerticalNav`, ten topics, Overview selected. Document swaps. Help sets `VestigiumShell.Content` and clears it on leave, so the status bar stays. |
| 5 | Topic resources | Done. Ten pages. Columns match the grids. About reads referenced assembly versions. License reads the output LICENSE, with the repo text as fallback. |

All five steps are this repo. `src/Vestigium.Suite.Network.RouteIQ`.

---

## 2. Watch

| Watch | Failure |
|---|---|
| Icon | A PNG committed as the icon. Or the table mark, which fails at 16px. |
| Strip | Help appended to `NavItems`. That puts it left, in the work group. |
| Selection | Help and Routes sharing a group name. Checking Help clears the work tab. |
| Theme | A FlowDocument with `Foreground="Black"`. Dark packs fail. |
| About | Hand-typed versions. They rot the day a package moves. |
| License | A rewritten MIT paragraph. The repo file is the text. |
| Scope | Markdig, `DocumentViewer`, an About dialog beside the About topic, a Helpers.RouteIQ package. |

---

## 3. Out of this plan

Write-route form. Settings moved to the right rail. A Logs topic. A markdown pipeline. Closing PR01. Publishing anything.

---

## 4. Document control

| Version | Date | Change |
|---|---|---|
| PR03 | 4 Oct 2026 | Plan written. Step 1 Accepted. Steps 2–5 not started. Moved here from the Helpers document store. |
| PR03 | 4 Oct 2026 | Step 2. Icon wired. |
| PR03 | 4 Oct 2026 | Step 3. Trailing Help tab. Page is step 4. |
| PR03 | 4 Oct 2026 | Step 4. Help view. Bodies are stubs for step 5. |
| PR03 | 4 Oct 2026 | Step 5. Pages filled. Plan closed. |
