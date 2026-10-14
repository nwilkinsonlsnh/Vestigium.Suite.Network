# SysInfoIQ — Inventory Sections Catalog

**Companion:** [Requirements_v1.0.md](Requirements_v1.0.md)  
**Project:** `Vestigium.Suite.Network.SysInfoIQ`  
**Purpose:** Organize every section from the existing PowerShell collector into categories. Map each to pure .NET Framework / managed collection (System.Management, System.Net.NetworkInformation, System.Diagnostics, System.Security, etc.). No powershell.exe, no pwsh.exe, no wmic.exe.

Collection is local only. Elevation improves completeness (event logs, some firewall, full driver list) but is not required for base.

---

## Categories

### System / Identity
| Section | Sample file | .NET door | Notes |
|---|---|---|---|
| ComputerSystem | ComputerSystem.txt | `Win32_ComputerSystem` via ManagementObjectSearcher | Hostname, domain, role, manufacturer, model, memory, processors, username |
| OperatingSystem | OperatingSystem.txt | `Win32_OperatingSystem` | Caption, version, build, install date, last boot, architecture |
| BIOS | BIOS.txt | `Win32_BIOS` | Manufacturer, version, release date, serial |
| Baseboard | Baseboard.txt | `Win32_BaseBoard` | Manufacturer, product, serial |
| CPU | CPU.txt | `Win32_Processor` | Name, cores, logical, max clock, manufacturer |
| Memory | Memory.txt | `Win32_PhysicalMemory` | Capacity, speed, manufacturer, part number |
| CurrentIdentity | CurrentIdentity.txt | `WindowsIdentity.GetCurrent()` + token groups / privileges | SID, groups, privileges. Large. |
| LocalUsers | LocalUsers.txt | `Win32_UserAccount` or AccountManagement | Enabled, last logon, description |
| LocalGroups | LocalGroups.txt | `Win32_Group` | Built-in + custom |
| LocalAdministrators | LocalAdministrators.txt | Group membership query | Subset of groups |

### Storage
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| DiskDrives | DiskDrives.txt | `Win32_DiskDrive` | Model, size, interface, serial |
| LogicalDisks | LogicalDisks.txt | `Win32_LogicalDisk` | Drive letter, size, free, filesystem |
| Partitions | Partitions.txt | MSFT_Partition (Storage namespace) or Win32_DiskPartition | Offset, size, type, boot/system flags |
| Volumes | Volumes.txt | MSFT_Volume | Health, filesystem, remaining |

### Network
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| IPConfiguration | IPConfiguration.txt | `NetworkInterface.GetAllNetworkInterfaces()` + GetIPProperties | Adapters, addresses, DNS, gateways. Match ipconfig style |
| DNSConfiguration | DNSConfiguration.txt | `NetworkInterface.GetIPProperties().DnsAddresses` per adapter | Per-interface |
| NetworkInterfaceCards | NetworkInterfaceCards.txt | `NetworkInterface` + ManagementObject for advanced | MAC, speed, status, driver |
| Routes | Routes.txt | `IPGlobalProperties` + P/Invoke GetIpForwardTable or CIM | IPv4/IPv6 tables |
| ARPTable | ARPTable.txt | P/Invoke GetIpNetTable / GetIpNetTable2 | Large on busy machines |
| ListeningPorts | ListeningPorts.txt | `IPGlobalProperties.GetActiveTcpListeners()` + UDP + Process.GetProcessById | State, PID, process name |
| TCPConnections | TCPConnections.txt | `IPGlobalProperties.GetActiveTcpConnections()` | Local/remote, state, owning process |

### Security / Firewall
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| FirewallProfiles | FirewallProfiles.txt | CIM `MSFT_NetFirewallProfile` or NetSecurity | Domain/Private/Public states |
| FirewallRules | FirewallRules.txt | CIM `MSFT_NetFirewallRule` | Large. Filter by enabled/profile |
| Certificates | Certificates.txt | `X509Store` (My, Root, CA, etc.) | Thumbprints, subjects, expiry |
| SignedDrivers | SignedDrivers.txt | `Win32_PnPSignedDriver` | IsSigned, signer |

### Software / Runtime
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| InstalledSoftware | InstalledSoftware.txt | Registry Uninstall keys (HKLM/HKCU 32/64) or `Win32_Product` (slow) | Prefer registry for speed |
| Hotfixes | Hotfixes.txt | `Win32_QuickFixEngineering` | KB, installed on |
| WindowsFeatures | WindowsFeatures.txt | Dism API or CIM `MSFT_OptionalFeature` | Enabled features |
| ODBCDrivers | ODBCDrivers.txt | Registry `SOFTWARE\ODBC\ODBCINST.INI` | Drivers and DSNs |
| EnvironmentVariables | EnvironmentVariables.txt | `Environment.GetEnvironmentVariables()` | Process + machine + user |
| SQLServices | SQLServices.txt | ServiceController filtered by name | MSSQL* services |

### Processes / Services / Tasks
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| Processes | Processes.txt | `Process.GetProcesses()` | Name, ID, memory, start time, user (where available) |
| Services | Services.txt | `ServiceController.GetServices()` | Name, display, status, start type |
| ScheduledTasks | ScheduledTasks.txt | Task Scheduler COM (ITaskService) or Microsoft.Win32.TaskScheduler | Path, state, last run |
| Shares | Shares.txt | `Win32_Share` | Name, path, type |

### Events / Diagnostics
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| SystemErrors_Last7Days | SystemErrors_Last7Days.txt | `EventLog` (System, Error, time filter) | Limited without elevation |
| ApplicationErrors_Last7Days | ApplicationErrors_Last7Days.txt | `EventLog` (Application) | Same |
| SystemInformation | SystemInformation.txt | Synthesized from Win32_OperatingSystem + ComputerSystem + BIOS + Processor + Memory + NetworkInterface + Hotfixes | systeminfo.exe style text. Do not launch systeminfo.exe or msinfo32. |
| Drivers | Drivers.txt | `Win32_SystemDriver` or PnP | Loaded drivers |

### Summary / Manifest
| Section | Sample | .NET door | Notes |
|---|---|---|---|
| Summary | Summary.txt | Computed from ComputerSystem + OperatingSystem + BIOS + Processor + Memory | **Exact format locked below.** |
| InventoryManifest | InventoryManifest.txt | Directory listing of written files | Name, length, last write |

#### Summary block (locked to sample)

```
WINDOWS SERVER INVENTORY SUMMARY
================================
Hostname:                 {DNSHostName}
Domain:                   {Domain}
Domain Role:              {DomainRole} ({RoleName})
OS Product Type:          {ProductType} ({ProductTypeName})
Manufacturer:             {Manufacturer}
Model:                    {Model}
Serial Number:            {BIOS.SerialNumber}
Operating System:         {OS.Caption}
OS Architecture:          {OS.OSArchitecture}
Version:                  {OS.Version}
Build Number:             {OS.BuildNumber}
Install Date:             {OS.InstallDate:MM/dd/yyyy HH:mm:ss}
Last Boot Time:           {OS.LastBootUpTime:MM/dd/yyyy HH:mm:ss}
Current Uptime:           {N} days, {N} hours, {N} minutes
Processor:                {Processor.Name}
Processor Sockets:        {ComputerSystem.NumberOfProcessors}
Logical Processors:       {ComputerSystem.NumberOfLogicalProcessors}
Installed Memory (GB):    {TotalPhysicalMemory / 1024^3:F2}
Current User:             {UserName short form}
Collection Time:          {yyyy-MM-dd HH:mm:ss}
Inventory Location:       {full path to export folder}
```

Domain Role map (from sample): 1 = Member Workstation.  
OS Product Type map: 1 = Work Station.  
Uptime is days/hours/minutes only (no seconds). Memory is TotalPhysicalMemory converted to GB with two decimals.

---

## Collection rules

- Prefer System.Management (WMI) for Win32_* and MSFT_* classes.
- Prefer System.Net.NetworkInformation for adapters, TCP/UDP listeners/connections.
- Prefer System.Diagnostics.EventLog and Process / ServiceController for runtime.
- Prefer Registry and X509Store for software and certs.
- P/Invoke only where no managed equivalent exists (ARP, some route tables, some firewall).
- One section failure writes an error file and continues.
- All output UTF-8 text matching the sample layout where practical.
- No external process launches.

---

## PR sequencing (after PR01)

- PR02: Storage + Network basics (IP, DNS, adapters)
- PR03: Processes, Services, ScheduledTasks
- PR04: Users/Groups/Identity + Firewall profiles
- PR05: Events (7-day errors) + Hotfixes + InstalledSoftware (registry)
- PR06: Remaining (ARP, Routes, full firewall rules, drivers, certs, features)
- Later: Excel export, section selection, elevation prompt

PR01 locks base (ComputerSystem, OS, BIOS, Processor, Memory) + chrome + export skeleton. This catalog drives everything after.
