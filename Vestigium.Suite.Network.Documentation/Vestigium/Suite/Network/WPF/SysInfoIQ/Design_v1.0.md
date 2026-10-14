# SysInfoIQ — Design v1.0

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `src/Vestigium.Suite.Network.SysInfoIQ`

Window and class map for the first usable exe. Requirements 1.0 win on the window and inventory scope.

---

## Window

`SysInfoIqWindow` + `VestigiumShell` (`ShowNav=False`)

```
menu  File (Exit)
      View (bar visible, bar Top/Bottom, Theme submenu — one check)
icon  Assets/SysInfoIQ.ico
place primary WorkArea, centered, Manual startup
nav   HorizontalTab  SysInfo | Settings | Help
client
  SysInfo
    Collect / Cancel
    Summary card (always)
    Sections list / grids
  Settings
    indented HorizontalTab  General | Theme
    General: output folder, elevation note
    Theme: palette, bar visible, dock
  Help
    static copy
status bar    message | progress | detail | clock
```

Child tab strips use left margin 20. Selected top tab is the only page name.

---

## Types

| Type | Role |
|---|---|
| `App` | HostLog → Themes → DI → load settings → window. Static `Themes`, `Settings`, `Session`, `Shell`. |
| `ThemeCatalog` | Registers the Vestigium.Themes palettes. |
| `SysInfoIqWindow` | Chrome. Centers on primary work area. Builds View → Theme. No collection. |
| `MainViewModel` | Collect command, Cancel, Summary, Sections, status bar posts. |
| `SettingsViewModel` | General / Theme pages. Output folder, palette, bar. |
| `SysInfoInput` | Pure reject / path rules. No CIM. |
| `SysInfoSettings` / `SysInfoSettingsStore` | DTO + load/save. Root path injectable. |
| `SysInfoSession` | Load/save glue. |
| `InventorySection` | Name, FileName, Content or rows. |
| `Collector` | CIM / managed queries for base sections. Returns sections or error records. Full section map in SectionsCatalog.md. |

No PowerShell. No wmic. No `ISystemInfoService` façade that wraps the whole collector.

---

## Persist

File: `%ProgramData%\Vestigium\Settings\Diagnostics\SysInfoIQ\settings.json`

Load after Themes.Initialize. Save on change. Tests pass a temp directory into the store.

---

## Flow

### Collect

1. Validate output path.
2. Create timestamped folder.
3. Run base collectors (ComputerSystem, OS, BIOS, Processor, Memory).
4. Build Summary text.
5. Display in UI.
6. Write files (or stage for Export).
7. On section failure, write error file and continue.

### Export

Explicit button or automatic at end of Collect. Writes the folder + manifest.

---

## Pins

| Package | Version |
|---|---|
| Themes | (Directory.Build.props) |
| Controls / StatusBar | (Directory.Build.props) |
| Converters | (Directory.Build.props) |
| Helpers (FileIo / ClosedXML if used) | (Directory.Build.props) |
| Logging | via HostLog |

---

## Out of this design

Full section list. Remote. Charts. Excel until text export is proven.
