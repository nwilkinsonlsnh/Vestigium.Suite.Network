# PR03b — Requirements (`Vestigium.Themes`)

**Document ID:** VEST-SUITE-NETWORK-NICIQ-PR03B-THEMES  
**Package:** `Vestigium.Themes` 1.0.2  
**Repo:** `nwilkinsonlsnh/Vestigium.Themes`  
**Status:** Draft for the extract  
**Date:** 30 September 2026  
**Evidence:** `ThemeCatalog.cs` in NicIQ, DnsIQ, PingIQ; `App.xaml` TargetType restyles; Themes Requirements v1.0 §2.2 / §5.4

## Goal

One register surface for the nine shipped palettes. Suite hosts stop copying `FromPack` tables. Theme switch keeps painting Vestigium controls that the catalog does not yet style.

This version is not a new palette, not `Vestigium.Themes.NicIQ`, and not a control library. New chrome still belongs in `Vestigium.Controls*`.

## What the host is doing that Themes already owns

`ThemeCatalog.RegisterAll` in three projects is the same nine lines as the Themes README table. Catalog: “Hosts register only the palettes they will switch; NuGet does not call `Register`.”

That rule stands. The defect is the table living in every exe.

`App.xaml` in NicIQ then applies `TargetType` styles to `VestigiumNumericUpDown` and `VestigiumUnderConstruction` because those assemblies do not bind `Vestigium.Brushes.*` themselves. Themes v1.0 out-of-scope said “custom control assemblies beyond restyling native WPF types.” So the **paint** belongs on the control packages (see PR03f). Themes still owns the **tokens** those controls bind.

## Must change in Themes

### T-01 Suite palette list

Ship a read-only list the host can register in one call, host-initiated:

```text
ThemeDefinitions.SuiteV1
```

or

```text
ThemeManager.RegisterSuiteV1()
```

Either shape is fine. Constraints:

- NuGet still does not call `Register` during package import.
- Host still chooses LightBlue vs a subset if a product only ships two palettes. Provide both “all nine” and the existing per-id `FromPack`.
- Ids, display names, assembly names, `isDark`, descriptions match README 1.0.2. Do not drift.

Delete the three host `ThemeCatalog.cs` files in the same consume commit.

### T-02 Token contract already has what charts need

Do not add keys for this extract if Series.1–6, Surface.Window / Card, Text.Primary, Stroke.Subtle, Accent.Primary, Status.* already exist (Themes Requirements §7). Confirm Series brushes are present in every palette. If a palette is missing Series.N, that is a Themes bug, not a NicIQ hex table.

### T-03 StatusBar token

`Vestigium.Brushes.Surface.StatusBar` already exists. `ThemeChrome` exists because `VestigiumStatusBar` does not use it. Themes does not grow a StatusBar control. PR03f binds the control to this token.

If Controls cannot take a Themes package reference, the bind is still `SetResourceReference(..., "Vestigium.Brushes.Surface.StatusBar")` by string. No new theme package.

### T-04 No implicit TargetType in Themes.Controls

Themes FR-S01 stands: omitting `Style=` yields stock WPF. Do not “fix” NicIQ by adding implicit styles in `Vestigium.Themes.Controls` for NumericUpDown. That would violate the locked catalog.

Suite controls paint via their own default styles using DynamicResource tokens (PR03f).

## Must not change

- Nine palettes, one nupkg, eleven DLLs.
- `Initialize` before the first window parses.
- ContractVersion 1.0 unless a required key is actually missing. Additive optional keys only if Series.* has a hole — then bump per Themes breaking-change policy.
- Demo remains a gallery.

## Host after consume

```csharp
Themes.RegisterSuiteV1();          // or foreach (var d in ThemeDefinitions.SuiteV1) Register(d);
Themes.Initialize(this, "LightBlue");
```

No `ThemeCatalog` class in any Suite.Network project.

## Acceptance

1. PingIQ, DnsIQ, and NicIQ register palettes from the Themes package list. Byte-identical ids to 1.0.2.
2. Switching Nord → LightBlue updates window chrome without a host visual-tree walk for stock catalog controls.
3. `Vestigium.Themes.Nord2` does not appear.
4. Pack still produces eleven product DLLs.

## Parked

- Chart color mapping helper — that is PR03a (`ChartOptions` from tokens), not a Themes type.
- Authoring UI.
- Tenth palette.
