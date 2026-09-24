# PR03f — Requirements (`Vestigium.Controls` / `StatusBar` / `NumericUpDown` / `UnderConstruction`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03F-CONTROLS  
**Packages:** `Vestigium.Controls` 1.0.0, `Vestigium.Controls.StatusBar` 1.0.0, `Vestigium.Controls.NumericUpDown` 1.0.1, `Vestigium.Controls.UnderConstruction` 1.0.0  
**Repo:** `nwilkinsonlsnh/Vestigium.Controls`  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `ThemeChrome.cs`, `App.xaml`, `Views/PageViewport.cs` (copied in NicIQ, DnsIQ, PingIQ)

## Goal

Suite controls paint from Vestigium tokens without each host walking the visual tree or reprinting TargetType styles.

This version is not a theming engine and not a second shell package.

## Split with Themes (do not blur)

| Owns | Package |
|---|---|
| Tokens, palettes, native WPF catalog styles | `Vestigium.Themes` (PR03b) |
| Vestigium controls and shell | `Vestigium.Controls*` |
| Host window jobs | NicIQ / DnsIQ / PingIQ |

Themes v1.0 refused to style custom assemblies. That is why the hosts grew `ThemeChrome`. The fix is on the control, using the same token strings.

## Must change

### F-01 StatusBar binds tokens

`VestigiumStatusBar` default template uses:

- Background → `Vestigium.Brushes.Surface.StatusBar` (fallback `Surface.Card`)
- Foreground → `Vestigium.Brushes.Text.Primary`
- Border → `Vestigium.Brushes.Stroke.Subtle`

No host `FindVisualChildren<Border>`. Delete `ThemeChrome.cs` from NicIQ, DnsIQ, and PingIQ together.

### F-02 NumericUpDown / UnderConstruction

Those packages ship default styles that DynamicResource the same tokens NicIQ set in `App.xaml` (Card / Window / Text.Primary / Stroke.Subtle). Hosts drop the Application.Resources copies.

Do not add those styles to `Vestigium.Themes.Controls` — that would smuggle suite controls into the native catalog.

### F-03 PageViewport

Three identical types force a page to the shell ScrollViewer height. If the shell should own “page fills the client,” put one helper on `Vestigium.Controls` / shell. If it is a host layout quirk, keep one copy in Shell (`Vestigium.Suite.Network.Shell`) — not three.

Preferred: Shell. Not a fourth control package.

### F-04 No second shell

`Vestigium.Controls` already ships `VestigiumDefaultWindow`. NicIQ keeps using it.

## Must not change

- PropertiesGrid.
- A NicIQ-only control package for the monitoring cards.
- Implicit TargetType styles inside Themes.Controls.

## Host after consume

Delete `ThemeChrome.cs`. Slim `App.xaml` to nothing theme-specific. One viewport helper or none.

## Acceptance

1. Theme switch updates StatusBar, NumericUpDown, and UnderConstruction without `ThemeChrome`.
2. Three hosts lose their private chrome binders in one consume slice.
3. `Vestigium.Controls.Theme` does not appear on nuget.org.

## Parked

Monitor detail cards and adapter grid stay host views. They are not reusable across PingIQ.
