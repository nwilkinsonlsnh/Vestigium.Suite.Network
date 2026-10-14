using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels.Rows;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public SettingsViewModel? Settings { get; set; }

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemPageOpen))]
    [NotifyPropertyChangedFor(nameof(StoragePageOpen))]
    [NotifyPropertyChangedFor(nameof(NetworkPageOpen))]
    [NotifyPropertyChangedFor(nameof(SecurityPageOpen))]
    [NotifyPropertyChangedFor(nameof(SoftwarePageOpen))]
    [NotifyPropertyChangedFor(nameof(ProcessesPageOpen))]
    [NotifyPropertyChangedFor(nameof(EventsPageOpen))]
    private string _category = "System";

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

    public ObservableCollection<InventorySection> Sections { get; } = new();

    public ObservableCollection<SummaryRow> SystemRows { get; } = new();
    public ObservableCollection<SummaryRow> SecurityRows { get; } = new();
    public ObservableCollection<SummaryRow> SoftwareRows { get; } = new();
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
            var storage = await Task.Run(() => StorageCollector.Collect(ct), ct).ConfigureAwait(true);
            foreach (var r in storage.DiskDrives) DiskDrives.Add(r);
            foreach (var r in storage.LogicalDisks) LogicalDisks.Add(r);
            foreach (var r in storage.Partitions) Partitions.Add(r);
            foreach (var r in storage.Volumes) Volumes.Add(r);

            Status("Collecting network…");
            var network = await Task.Run(() => NetworkCollector.Collect(ct), ct).ConfigureAwait(true);
            foreach (var r in network.Ip) IpConfigs.Add(r);
            foreach (var r in network.Dns) DnsConfigs.Add(r);
            foreach (var r in network.Nics) Nics.Add(r);
            foreach (var r in network.Routes) Routes.Add(r);
            foreach (var r in network.Arp) Arp.Add(r);
            foreach (var r in network.Listeners) Listeners.Add(r);
            foreach (var r in network.Tcp) Tcp.Add(r);

            var note = network.ArpTruncated || network.TcpTruncated
                ? "Idle (some tables capped)"
                : "Idle";
            Status(note);
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

    private bool CanCollect() => !IsBusy;

    private void Status(string message)
    {
        if (StatusBar is not null)
            StatusBar.Message = message;
    }

    private static IReadOnlyList<SummaryRow> BuildSystemRows(IReadOnlyList<InventorySection> sections, DateTime now)
    {
        var cs = sections.FirstOrDefault(s => s.Name == "ComputerSystem" && !s.IsError);
        var os = sections.FirstOrDefault(s => s.Name == "OperatingSystem" && !s.IsError);
        var bios = sections.FirstOrDefault(s => s.Name == "BIOS" && !s.IsError);
        var cpu = sections.FirstOrDefault(s => s.Name == "CPU" && !s.IsError);

        var hostname = Value(cs, "DNSHostName") ?? Value(cs, "Name") ?? Environment.MachineName;
        var domain = Value(cs, "Domain") ?? "";
        var role = Int(cs, "DomainRole");
        var productType = Int(os, "ProductType");
        var manufacturer = Value(cs, "Manufacturer") ?? "";
        var model = Value(cs, "Model") ?? "";
        var serial = Value(bios, "SerialNumber") ?? "";
        var caption = Value(os, "Caption") ?? "";
        var arch = Value(os, "OSArchitecture") ?? "";
        var version = Value(os, "Version") ?? "";
        var build = Value(os, "BuildNumber") ?? "";
        var install = Date(os, "InstallDate");
        var boot = Date(os, "LastBootUpTime");
        var processor = Value(cpu, "Name")?.Trim() ?? "";
        var sockets = Int(cs, "NumberOfProcessors");
        var logical = Int(cs, "NumberOfLogicalProcessors");
        var memBytes = Long(cs, "TotalPhysicalMemory");
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
        try
        {
            return System.Management.ManagementDateTimeConverter.ToDateTime(raw);
        }
        catch
        {
            return DateTime.TryParse(raw, out var d) ? d : null;
        }
    }
}
