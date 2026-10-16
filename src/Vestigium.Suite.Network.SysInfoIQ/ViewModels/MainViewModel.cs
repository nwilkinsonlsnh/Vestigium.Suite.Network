using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;
    private NetworkSnapshot? _pendingNetwork;
    private SecuritySnapshot? _pendingSecurity;
    private SoftwareSnapshot? _pendingSoftware;
    private readonly HashSet<string> _filledNetwork = new();
    private readonly HashSet<string> _filledSecurity = new();
    private readonly HashSet<string> _filledSoftware = new();

    public MainViewModel()
    {
        InstalledView = CollectionViewSource.GetDefaultView(InstalledSoftware);
    }

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public SettingsViewModel? Settings { get; set; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _progressMessage = "Collecting…";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemPageOpen))]
    [NotifyPropertyChangedFor(nameof(StoragePageOpen))]
    [NotifyPropertyChangedFor(nameof(NetworkPageOpen))]
    [NotifyPropertyChangedFor(nameof(SecurityPageOpen))]
    [NotifyPropertyChangedFor(nameof(SoftwarePageOpen))]
    [NotifyPropertyChangedFor(nameof(ProcessesPageOpen))]
    [NotifyPropertyChangedFor(nameof(EventsPageOpen))]
    private string _category = "System";

    partial void OnCategoryChanged(string value)
    {
        if (value == "Network" && _pendingNetwork is not null)
            _ = FillNetworkSectionAsync(NetworkSection);
        if (value == "Security" && _pendingSecurity is not null)
            _ = FillSecuritySectionAsync(SecuritySection);
        if (value == "Software" && _pendingSoftware is not null)
            _ = FillSoftwareSectionAsync(SoftwareSection);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisksOpen))]
    [NotifyPropertyChangedFor(nameof(LogicalOpen))]
    [NotifyPropertyChangedFor(nameof(PartitionsOpen))]
    [NotifyPropertyChangedFor(nameof(VolumesOpen))]
    private string _storageSection = "Disks";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IpOpen))]
    [NotifyPropertyChangedFor(nameof(DnsOpen))]
    [NotifyPropertyChangedFor(nameof(NicsOpen))]
    [NotifyPropertyChangedFor(nameof(RoutesOpen))]
    [NotifyPropertyChangedFor(nameof(ArpOpen))]
    [NotifyPropertyChangedFor(nameof(ListenersOpen))]
    [NotifyPropertyChangedFor(nameof(TcpOpen))]
    private string _networkSection = "IP";

    partial void OnNetworkSectionChanged(string value)
    {
        if (Category == "Network" && _pendingNetwork is not null)
            _ = FillNetworkSectionAsync(value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProfilesOpen))]
    [NotifyPropertyChangedFor(nameof(RulesOpen))]
    [NotifyPropertyChangedFor(nameof(CertificatesOpen))]
    [NotifyPropertyChangedFor(nameof(DriversOpen))]
    private string _securitySection = "Profiles";

    partial void OnSecuritySectionChanged(string value)
    {
        if (Category == "Security" && _pendingSecurity is not null)
            _ = FillSecuritySectionAsync(value);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstalledOpen))]
    [NotifyPropertyChangedFor(nameof(HotfixesOpen))]
    [NotifyPropertyChangedFor(nameof(FeaturesOpen))]
    [NotifyPropertyChangedFor(nameof(OdbcOpen))]
    [NotifyPropertyChangedFor(nameof(EnvironmentOpen))]
    [NotifyPropertyChangedFor(nameof(SqlOpen))]
    private string _softwareSection = "Installed";

    partial void OnSoftwareSectionChanged(string value)
    {
        if (Category == "Software" && _pendingSoftware is not null)
            _ = FillSoftwareSectionAsync(value);
    }

    public bool SystemPageOpen { get => Category == "System"; set { if (value) Category = "System"; } }
    public bool StoragePageOpen { get => Category == "Storage"; set { if (value) Category = "Storage"; } }
    public bool NetworkPageOpen { get => Category == "Network"; set { if (value) Category = "Network"; } }
    public bool SecurityPageOpen { get => Category == "Security"; set { if (value) Category = "Security"; } }
    public bool SoftwarePageOpen { get => Category == "Software"; set { if (value) Category = "Software"; } }
    public bool ProcessesPageOpen { get => Category == "Processes"; set { if (value) Category = "Processes"; } }
    public bool EventsPageOpen { get => Category == "Events"; set { if (value) Category = "Events"; } }

    public bool DisksOpen { get => StorageSection == "Disks"; set { if (value) StorageSection = "Disks"; } }
    public bool LogicalOpen { get => StorageSection == "Logical"; set { if (value) StorageSection = "Logical"; } }
    public bool PartitionsOpen { get => StorageSection == "Partitions"; set { if (value) StorageSection = "Partitions"; } }
    public bool VolumesOpen { get => StorageSection == "Volumes"; set { if (value) StorageSection = "Volumes"; } }

    public bool IpOpen { get => NetworkSection == "IP"; set { if (value) NetworkSection = "IP"; } }
    public bool DnsOpen { get => NetworkSection == "DNS"; set { if (value) NetworkSection = "DNS"; } }
    public bool NicsOpen { get => NetworkSection == "NICs"; set { if (value) NetworkSection = "NICs"; } }
    public bool RoutesOpen { get => NetworkSection == "Routes"; set { if (value) NetworkSection = "Routes"; } }
    public bool ArpOpen { get => NetworkSection == "ARP"; set { if (value) NetworkSection = "ARP"; } }
    public bool ListenersOpen { get => NetworkSection == "Listeners"; set { if (value) NetworkSection = "Listeners"; } }
    public bool TcpOpen { get => NetworkSection == "TCP"; set { if (value) NetworkSection = "TCP"; } }

    public bool ProfilesOpen { get => SecuritySection == "Profiles"; set { if (value) SecuritySection = "Profiles"; } }
    public bool RulesOpen { get => SecuritySection == "Rules"; set { if (value) SecuritySection = "Rules"; } }
    public bool CertificatesOpen { get => SecuritySection == "Certificates"; set { if (value) SecuritySection = "Certificates"; } }
    public bool DriversOpen { get => SecuritySection == "Drivers"; set { if (value) SecuritySection = "Drivers"; } }

    public bool InstalledOpen { get => SoftwareSection == "Installed"; set { if (value) SoftwareSection = "Installed"; } }
    public bool HotfixesOpen { get => SoftwareSection == "Hotfixes"; set { if (value) SoftwareSection = "Hotfixes"; } }
    public bool FeaturesOpen { get => SoftwareSection == "Features"; set { if (value) SoftwareSection = "Features"; } }
    public bool OdbcOpen { get => SoftwareSection == "ODBC"; set { if (value) SoftwareSection = "ODBC"; } }
    public bool EnvironmentOpen { get => SoftwareSection == "Environment"; set { if (value) SoftwareSection = "Environment"; } }
    public bool SqlOpen { get => SoftwareSection == "SQL"; set { if (value) SoftwareSection = "SQL"; } }

    public ObservableCollection<InventorySection> Sections { get; } = new();

    public ObservableCollection<SummaryRow> SystemRows { get; } = new();
    public ObservableCollection<SummaryRow> ProcessRows { get; } = new();
    public ObservableCollection<SummaryRow> EventRows { get; } = new();

    public ObservableCollection<DiskDriveRow> DiskDrives { get; } = new();
    public ObservableCollection<LogicalDiskRow> LogicalDisks { get; } = new();
    public ObservableCollection<PartitionRow> Partitions { get; } = new();
    public ObservableCollection<VolumeRow> Volumes { get; } = new();

    public ObservableCollection<IpConfigRow> IpConfigs { get; } = new();
    public ObservableCollection<DnsConfigRow> DnsConfigs { get; } = new();
    public ObservableCollection<NicRow> Nics { get; } = new();
    public ObservableCollection<RouteRow> Routes { get; } = new();
    public ObservableCollection<ArpRow> Arp { get; } = new();
    public ObservableCollection<ListenerRow> Listeners { get; } = new();
    public ObservableCollection<TcpRow> Tcp { get; } = new();

    public ObservableCollection<FirewallProfileRow> FirewallProfiles { get; } = new();
    public ObservableCollection<FirewallRuleRow> FirewallRules { get; } = new();
    public ObservableCollection<CertificateRow> Certificates { get; } = new();
    public ObservableCollection<SignedDriverRow> SignedDrivers { get; } = new();

    public ObservableCollection<InstalledSoftwareRow> InstalledSoftware { get; } = new();
    public ICollectionView InstalledView { get; }
    public ObservableCollection<HotfixRow> Hotfixes { get; } = new();
    public ObservableCollection<FeatureRow> Features { get; } = new();
    public ObservableCollection<OdbcRow> Odbc { get; } = new();
    public ObservableCollection<EnvironmentRow> EnvironmentVars { get; } = new();
    public ObservableCollection<SqlServiceRow> SqlServices { get; } = new();

    [RelayCommand(CanExecute = nameof(CanCollect))]
    private async Task CollectAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        CollectCommand.NotifyCanExecuteChanged();
        Sections.Clear();
        SystemRows.Clear();
        DiskDrives.Clear();
        LogicalDisks.Clear();
        Partitions.Clear();
        Volumes.Clear();
        IpConfigs.Clear();
        DnsConfigs.Clear();
        Nics.Clear();
        Routes.Clear();
        Arp.Clear();
        Listeners.Clear();
        Tcp.Clear();
        FirewallProfiles.Clear();
        FirewallRules.Clear();
        Certificates.Clear();
        SignedDrivers.Clear();
        InstalledSoftware.Clear();
        Hotfixes.Clear();
        Features.Clear();
        Odbc.Clear();
        EnvironmentVars.Clear();
        SqlServices.Clear();
        _pendingNetwork = null;
        _pendingSecurity = null;
        _pendingSoftware = null;
        _filledNetwork.Clear();
        _filledSecurity.Clear();
        _filledSoftware.Clear();

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        try
        {
            Status("Collecting system…");
            var sections = await Task.Run(() => Collector.CollectBase(ct), ct).ConfigureAwait(true);
            foreach (var s in sections)
                Sections.Add(s);
            foreach (var row in BuildSystemRows(sections, DateTime.Now))
                SystemRows.Add(row);

            Status("Collecting storage…");
            var storage = await Task.Run(
                () => InventoryMap.ToStorage(Vestigium.Helpers.SystemInfo.SystemInventory.ReadStorage()),
                ct).ConfigureAwait(true);
            foreach (var r in storage.DiskDrives) DiskDrives.Add(r);
            foreach (var r in storage.LogicalDisks) LogicalDisks.Add(r);
            foreach (var r in storage.Partitions) Partitions.Add(r);
            foreach (var r in storage.Volumes) Volumes.Add(r);

            Status("Collecting network…");
            _pendingNetwork = await Task.Run(
                () => InventoryMap.ToNetwork(Vestigium.Helpers.SystemInfo.SystemInventory.ReadNetwork()),
                ct).ConfigureAwait(true);

            Status("Collecting security…");
            _pendingSecurity = await Task.Run(
                () => SecurityCollector.Collect(ct, name => Status($"Security / {name}")),
                ct).ConfigureAwait(true);

            Status("Collecting software…");
            _pendingSoftware = await Task.Run(
                () => SoftwareCollector.Collect(ct, name => Status($"Software / {name}")),
                ct).ConfigureAwait(true);

            if (Category == "Network")
                await FillNetworkSectionAsync(NetworkSection);
            if (Category == "Security")
                await FillSecuritySectionAsync(SecuritySection);
            if (Category == "Software")
                await FillSoftwareSectionAsync(SoftwareSection);

            Status("Idle (all sections ready)");
        }
        catch (OperationCanceledException)
        {
            Status("Cancelled");
        }
        catch (Exception ex)
        {
            SystemRows.Add(new SummaryRow { Field = "Error", Value = ex.Message });
            Status("Failed");
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
            CollectCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task FillNetworkSectionAsync(string section)
    {
        if (_pendingNetwork is null || _filledNetwork.Contains(section))
            return;
        var network = _pendingNetwork;
        switch (section)
        {
            case "IP":
                IpConfigs.Clear();
                AddAllSync(IpConfigs, network.Ip);
                break;
            case "DNS":
                DnsConfigs.Clear();
                AddAllSync(DnsConfigs, network.Dns);
                break;
            case "NICs":
                Nics.Clear();
                AddAllSync(Nics, network.Nics);
                break;
            case "Routes":
                Routes.Clear();
                AddAllSync(Routes, network.Routes);
                break;
            case "ARP":
                Arp.Clear();
                await AddAllYield(Arp, network.Arp);
                break;
            case "Listeners":
                Listeners.Clear();
                AddAllSync(Listeners, network.Listeners);
                break;
            case "TCP":
                Tcp.Clear();
                await AddAllYield(Tcp, network.Tcp);
                break;
        }
        _filledNetwork.Add(section);
    }

    private async Task FillSecuritySectionAsync(string section)
    {
        if (_pendingSecurity is null || _filledSecurity.Contains(section))
            return;
        var security = _pendingSecurity;
        switch (section)
        {
            case "Profiles":
                FirewallProfiles.Clear();
                AddAllSync(FirewallProfiles, security.Profiles);
                break;
            case "Rules":
                FirewallRules.Clear();
                await AddAllYield(FirewallRules, security.Rules);
                break;
            case "Certificates":
                Certificates.Clear();
                AddAllSync(Certificates, security.Certificates);
                break;
            case "Drivers":
                SignedDrivers.Clear();
                await AddAllYield(SignedDrivers, security.Drivers);
                break;
        }
        _filledSecurity.Add(section);
    }

    private async Task FillSoftwareSectionAsync(string section)
    {
        if (_pendingSoftware is null || _filledSoftware.Contains(section))
            return;
        var software = _pendingSoftware;
        switch (section)
        {
            case "Installed":
                InstalledSoftware.Clear();
                await AddAllYield(InstalledSoftware, software.Installed);
                break;
            case "Hotfixes":
                Hotfixes.Clear();
                AddAllSync(Hotfixes, software.Hotfixes);
                break;
            case "Features":
                Features.Clear();
                await AddAllYield(Features, software.Features);
                break;
            case "ODBC":
                Odbc.Clear();
                AddAllSync(Odbc, software.Odbc);
                break;
            case "Environment":
                EnvironmentVars.Clear();
                AddAllSync(EnvironmentVars, software.Environment);
                break;
            case "SQL":
                SqlServices.Clear();
                AddAllSync(SqlServices, software.Sql);
                break;
        }
        _filledSoftware.Add(section);
    }

    private static void AddAllSync<T>(ObservableCollection<T> target, IEnumerable<T> rows)
    {
        foreach (var row in rows)
            target.Add(row);
    }

    private static async Task AddAllYield<T>(ObservableCollection<T> target, IEnumerable<T> rows)
    {
        var batch = 0;
        foreach (var row in rows)
        {
            target.Add(row);
            if (++batch == 1000)
            {
                batch = 0;
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            }
        }
    }

    private bool CanCollect() => !IsBusy;

    private void Status(string message)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            ProgressMessage = message;
            if (StatusBar is not null)
                StatusBar.Message = message;
        }
        else
        {
            dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
            {
                ProgressMessage = message;
                if (StatusBar is not null)
                    StatusBar.Message = message;
            });
        }
    }

    private static IReadOnlyList<SummaryRow> BuildSystemRows(IReadOnlyList<InventorySection> sections, DateTime now)
    {
        var cs = sections.FirstOrDefault(s => s.Name == "ComputerSystem" && !s.IsError);
        var os = sections.FirstOrDefault(s => s.Name == "OperatingSystem" && !s.IsError);
        var bios = sections.FirstOrDefault(s => s.Name == "BIOS" && !s.IsError);
        var cpu = sections.FirstOrDefault(s => s.Name == "CPU" && !s.IsError);

        var hostname = Value(cs, "Hostname") ?? Environment.MachineName;
        var domain = Value(cs, "Domain") ?? "";
        var role = Int(cs, "DomainRole");
        var productType = Int(os, "ProductType");
        var manufacturer = Value(cs, "Manufacturer") ?? "";
        var model = Value(cs, "Model") ?? "";
        var serial = Value(bios, "SerialNumber") ?? "";
        var caption = Value(os, "Caption") ?? "";
        var arch = Value(os, "Architecture") ?? "";
        var version = Value(os, "Version") ?? "";
        var build = Value(os, "BuildNumber") ?? "";
        var install = Date(os, "InstallDate");
        var boot = Date(os, "LastBootUpTime");
        var processor = Value(cpu, "Name")?.Trim() ?? "";
        var sockets = Int(cs, "Sockets");
        var logical = Int(cs, "LogicalProcessors");
        var memBytes = Long(cs, "TotalPhysicalMemoryBytes");
        var memGb = memBytes / 1024d / 1024d / 1024d;
        var user = SummaryFormatter.ShortUser(Value(cs, "UserName"));
        if (string.IsNullOrWhiteSpace(user))
            user = SummaryFormatter.ShortUser(Environment.UserName);

        return
        [
            Row("Hostname", hostname),
            Row("Domain", domain),
            Row("Domain Role", $"{role} ({SummaryFormatter.DomainRoleName(role)})"),
            Row("OS Product Type", $"{productType} ({SummaryFormatter.ProductTypeName(productType)})"),
            Row("Manufacturer", manufacturer),
            Row("Model", model),
            Row("Serial Number", serial),
            Row("Operating System", caption),
            Row("OS Architecture", arch),
            Row("Version", version),
            Row("Build Number", build),
            Row("Install Date", install?.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) ?? ""),
            Row("Last Boot Time", boot?.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) ?? ""),
            Row("Current Uptime", SummaryFormatter.Uptime(boot, now)),
            Row("Processor", processor),
            Row("Processor Sockets", sockets.ToString(CultureInfo.InvariantCulture)),
            Row("Logical Processors", logical.ToString(CultureInfo.InvariantCulture)),
            Row("Installed Memory (GB)", memGb.ToString("F2", CultureInfo.InvariantCulture)),
            Row("Current User", user),
            Row("Collection Time", now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
        ];
    }

    private static SummaryRow Row(string field, string value) => new() { Field = field, Value = value };

    private static string? Value(InventorySection? section, string key)
    {
        if (section is null)
            return null;
        foreach (var line in section.Content.Split('\n'))
        {
            var idx = line.IndexOf(':');
            if (idx < 0)
                continue;
            var k = line[..idx].Trim();
            if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                return line[(idx + 1)..].Trim();
        }
        return null;
    }

    private static int Int(InventorySection? section, string key)
        => int.TryParse(Value(section, key), out var n) ? n : 0;

    private static long Long(InventorySection? section, string key)
        => long.TryParse(Value(section, key), out var n) ? n : 0;

    private static DateTime? Date(InventorySection? section, string key)
    {
        var raw = Value(section, key);
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        return DateTime.TryParseExact(raw, "yyyyMMddHHmmss.000000-000", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d
            : DateTime.TryParse(raw, out var parsed) ? parsed : null;
    }
}
