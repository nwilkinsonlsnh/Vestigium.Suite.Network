using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Vestigium.Helpers.SystemInfo;
using Vestigium.Helpers.SystemInfo.Cpu;
using MemoryInfo = Vestigium.Helpers.SystemInfo.Memory.MemoryFacts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class SystemDetailForm
{
    private const string Dash = "\u2014";
    private const string DisplayClass = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";

    public string Title { get; private set; } = "This computer";

    public string CopyText { get; private set; } = string.Empty;

    public string Now { get; private set; } = Dash;

    public string HostName { get; private set; } = Dash;

    public string OsName { get; private set; } = Dash;

    public string OsVersion { get; private set; } = Dash;

    public string DisplayVersion { get; private set; } = Dash;

    public string Edition { get; private set; } = Dash;

    public string Build { get; private set; } = Dash;

    public string BuildType { get; private set; } = Dash;

    public string Language { get; private set; } = Dash;

    public string Owner { get; private set; } = Dash;

    public string Organization { get; private set; } = Dash;

    public string ProductId { get; private set; } = Dash;

    public string InstallDate { get; private set; } = Dash;

    public string BootTime { get; private set; } = Dash;

    public string Manufacturer { get; private set; } = Dash;

    public string Model { get; private set; } = Dash;

    public string SystemType { get; private set; } = Dash;

    public string Processor { get; private set; } = Dash;

    public string Bios { get; private set; } = Dash;

    public string WindowsDirectory { get; private set; } = Dash;

    public string SystemDirectory { get; private set; } = Dash;

    public string Locale { get; private set; } = Dash;

    public string TimeZone { get; private set; } = Dash;

    public string Domain { get; private set; } = Dash;

    public string DirectX { get; private set; } = Dash;

    public string TotalPhysical { get; private set; } = Dash;

    public string AvailablePhysical { get; private set; } = Dash;

    public string InUsePhysical { get; private set; } = Dash;

    public string CommitPeak { get; private set; } = Dash;

    public string PageFile { get; private set; } = Dash;

    public string PageFileUsed { get; private set; } = Dash;

    public string PageFileAvailable { get; private set; } = Dash;

    public string DisplayName { get; private set; } = Dash;

    public string DisplayManufacturer { get; private set; } = Dash;

    public string DisplayChip { get; private set; } = Dash;

    public string DisplayMemory { get; private set; } = Dash;

    public string DisplayVram { get; private set; } = Dash;

    public string DisplayShared { get; private set; } = Dash;

    public static SystemDetailForm Read()
    {
        var windows = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var bios = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
        var cpu = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        var memory = MemoryInfo.Read();
        var host = CpuFacts.Host;
        var live = CpuFacts.Live();
        var display = ReadDisplay();
        var page = ReadPageUsage();
        var build = Text(windows?.GetValue("CurrentBuild")?.ToString());
        var ubr = windows?.GetValue("UBR")?.ToString();
        var form = new SystemDetailForm
        {
            Title = Environment.MachineName,
            Now = DateTimeOffset.Now.ToString("f", CultureInfo.CurrentCulture),
            HostName = Environment.MachineName,
            OsName = OsLine(windows),
            DisplayVersion = Text(windows?.GetValue("DisplayVersion")?.ToString()),
            Edition = Text(windows?.GetValue("EditionID")?.ToString()),
            Build = string.IsNullOrWhiteSpace(ubr) ? build : build + "." + ubr,
            OsVersion = Environment.OSVersion.VersionString,
            BuildType = Text(windows?.GetValue("CurrentType")?.ToString()),
            Language = CultureInfo.CurrentUICulture.DisplayName + " (Regional Setting: " + CultureInfo.CurrentCulture.DisplayName + ")",
            Owner = Text(windows?.GetValue("RegisteredOwner")?.ToString()),
            Organization = Text(windows?.GetValue("RegisteredOrganization")?.ToString()),
            ProductId = Text(windows?.GetValue("ProductId")?.ToString()),
            InstallDate = Install(windows?.GetValue("InstallDate")),
            BootTime = DateTimeOffset.Now.AddMilliseconds(-Environment.TickCount64).ToString("g", CultureInfo.CurrentCulture),
            Manufacturer = Text(bios?.GetValue("SystemManufacturer")?.ToString()),
            Model = Text(bios?.GetValue("SystemProductName")?.ToString()),
            SystemType = Environment.Is64BitOperatingSystem ? "x64-based PC" : "x86-based PC",
            Processor = ProcessorText(cpu, host, live),
            Bios = BiosText(bios),
            WindowsDirectory = Text(Environment.GetFolderPath(Environment.SpecialFolder.Windows)),
            SystemDirectory = Text(Environment.SystemDirectory),
            Locale = CultureInfo.CurrentCulture.DisplayName,
            TimeZone = TimeZoneInfo.Local.DisplayName,
            Domain = DomainName(),
            DirectX = DirectXVersion(),
            TotalPhysical = Mb(memory.TotalBytes),
            AvailablePhysical = Gb(memory.AvailableBytes),
            InUsePhysical = Gb(memory.InUseBytes),
            CommitPeak = Gb(memory.CommitPeakBytes),
            PageFile = ReadPageFile(),
            PageFileUsed = page.Used,
            PageFileAvailable = page.Available,
            DisplayName = display.Name,
            DisplayManufacturer = display.Manufacturer,
            DisplayChip = display.Chip,
            DisplayMemory = display.Total,
            DisplayVram = display.Vram,
            DisplayShared = display.Shared
        };
        form.CopyText = form.Format();
        return form;
    }

    private string Format()
    {
        var text = new StringBuilder();
        text.AppendLine(Title);
        text.AppendLine();
        text.AppendLine("Windows");
        Line(text, "Date/Time", Now);
        Line(text, "Host name", HostName);
        Line(text, "OS name", OsName);
        Line(text, "Edition", Edition);
        Line(text, "Version", DisplayVersion);
        Line(text, "Build", Build);
        Line(text, "Language", Language);
        Line(text, "DirectX", DirectX);
        Line(text, "Owner", Owner);
        Line(text, "Organization", Organization);
        Line(text, "Product ID", ProductId);
        Line(text, "Install date", InstallDate);
        Line(text, "Boot time", BootTime);
        text.AppendLine();
        text.AppendLine("Computer");
        Line(text, "Manufacturer", Manufacturer);
        Line(text, "Model", Model);
        Line(text, "System type", SystemType);
        Line(text, "Processor", Processor);
        Line(text, "BIOS", Bios);
        Line(text, "Domain", Domain);
        text.AppendLine();
        text.AppendLine("Memory");
        Line(text, "Total physical", TotalPhysical);
        Line(text, "In use", InUsePhysical);
        Line(text, "Available", AvailablePhysical);
        Line(text, "Commit peak", CommitPeak);
        Line(text, "Page file", PageFile);
        Line(text, "Page file used", PageFileUsed);
        Line(text, "Page file available", PageFileAvailable);
        text.AppendLine();
        text.AppendLine("Display");
        Line(text, "Name", DisplayName);
        Line(text, "Manufacturer", DisplayManufacturer);
        Line(text, "Chip", DisplayChip);
        Line(text, "Approx. total", DisplayMemory);
        Line(text, "VRAM", DisplayVram);
        Line(text, "Shared", DisplayShared);
        return text.ToString();
    }

    private static string OsLine(RegistryKey? windows)
    {
        var name = windows?.GetValue("ProductName")?.ToString();
        var build = windows?.GetValue("CurrentBuild")?.ToString();
        var bits = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var version = Environment.OSVersion.Version;
        var core = string.IsNullOrWhiteSpace(name) ? "Windows" : name.Trim();
        return string.IsNullOrWhiteSpace(build)
            ? $"{core} {bits}"
            : $"{core} {bits} ({version.Major}.{version.Minor}, Build {build})";
    }

    private static string ProcessorText(RegistryKey? cpu, Fact<CpuTopology> host, Fact<CpuLive> live)
    {
        var name = cpu?.GetValue("ProcessorNameString")?.ToString()?.Trim();
        var cpus = host.IsOk ? host.Value.Logical : Environment.ProcessorCount;
        var mhz = live.IsOk && live.Value.MaxMhz > 0 ? (int)live.Value.MaxMhz : RegistryMhz(cpu);
        var speed = mhz > 0 ? $", ~{mhz / 1000d:0.00}GHz" : string.Empty;
        var body = string.IsNullOrWhiteSpace(name) ? "Processor" : name;
        return $"{body} ({cpus} CPUs){speed}";
    }

    private static int RegistryMhz(RegistryKey? cpu)
        => cpu?.GetValue("~MHz") is int mhz ? mhz : 0;

    private static string BiosText(RegistryKey? bios)
    {
        var version = bios?.GetValue("BIOSVersion");
        if (version is string[] rows)
            return Text(string.Join(" ", rows.Where(static value => !string.IsNullOrWhiteSpace(value))));
        var release = bios?.GetValue("BIOSReleaseDate")?.ToString();
        var text = version?.ToString();
        return Text(string.IsNullOrWhiteSpace(release) ? text : $"{text} {release}");
    }

    private static string DirectXVersion()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\DirectX");
        var version = key?.GetValue("Version")?.ToString();
        return string.IsNullOrWhiteSpace(version) ? "DirectX 12" : "DirectX " + version.Trim();
    }

    private static (string Used, string Available) ReadPageUsage()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status) || status.TotalPageFile == 0)
            return (Dash, Dash);
        var available = status.AvailPageFile;
        var used = status.TotalPageFile > available ? status.TotalPageFile - available : 0;
        return (Mb(used), Mb(available));
    }

    private static (string Name, string Manufacturer, string Chip, string Total, string Vram, string Shared) ReadDisplay()
    {
        var root = Registry.LocalMachine.OpenSubKey(DisplayClass);
        if (root is null)
            return (Dash, Dash, Dash, Dash, Dash, Dash);
        foreach (var name in root.GetSubKeyNames())
        {
            if (name.Length != 4 || !name.All(char.IsDigit))
                continue;
            var key = root.OpenSubKey(name);
            var desc = key?.GetValue("DriverDesc")?.ToString();
            if (string.IsNullOrWhiteSpace(desc))
                continue;
            var vram = key?.GetValue("HardwareInformation.qwMemorySize");
            var bytes = vram is long wide ? (ulong)wide : vram is int narrow ? (ulong)narrow : 0ul;
            var shared = 0ul;
            var physical = MemoryInfo.Read().TotalBytes;
            if (physical.IsOk && physical.Value > bytes)
                shared = physical.Value - bytes;
            return (
                desc.Trim(),
                Text(key?.GetValue("ProviderName")?.ToString()),
                Text(desc),
                bytes == 0 ? Dash : Mb(bytes + shared),
                bytes == 0 ? Dash : Mb(bytes),
                shared == 0 ? Dash : Mb(shared));
        }

        return (Dash, Dash, Dash, Dash, Dash, Dash);
    }

    private static string ReadPageFile()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management");
        if (key?.GetValue("PagingFiles") is not string[] files || files.Length == 0)
            return Dash;
        return string.Join("; ", files.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private static string DomainName()
    {
        try
        {
            var domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            if (!string.IsNullOrWhiteSpace(domain))
                return domain.Trim();
        }
        catch (NetworkInformationException)
        {
        }

        return Text(Environment.UserDomainName);
    }

    private static string Install(object? value)
    {
        if (value is int seconds)
            return DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        if (value is long wide)
            return DateTimeOffset.FromUnixTimeSeconds(wide).ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        return Dash;
    }

    private static string Gb(Fact<ulong> fact)
        => fact.IsOk ? Gb(fact.Value) : Dash;

    private static string Gb(ulong bytes)
        => (bytes / 1073741824d).ToString("0.0", CultureInfo.InvariantCulture) + " GB";

    private static string Mb(Fact<ulong> fact)
        => fact.IsOk ? Mb(fact.Value) : Dash;

    private static string Mb(ulong bytes)
        => (bytes / 1048576d).ToString("0", CultureInfo.InvariantCulture) + " MB";

    private static void Line(StringBuilder text, string label, string value)
        => text.AppendLine($"{label}  {value}");

    private static string Text(string? value)
        => string.IsNullOrWhiteSpace(value) ? Dash : value.Trim();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx status);

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }
}
