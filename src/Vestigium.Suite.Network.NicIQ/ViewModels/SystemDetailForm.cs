using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Win32;
using Vestigium.Helpers.SystemInfo.Cpu;
using MemoryInfo = Vestigium.Helpers.SystemInfo.Memory.MemoryFacts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class SystemDetailForm
{
    private const string Dash = "\u2014";

    public string Title { get; private set; } = "This computer";

    public string CopyText { get; private set; } = string.Empty;

    public string HostName { get; private set; } = Dash;

    public string OsName { get; private set; } = Dash;

    public string OsVersion { get; private set; } = Dash;

    public string DisplayVersion { get; private set; } = Dash;

    public string Edition { get; private set; } = Dash;

    public string Build { get; private set; } = Dash;

    public string BuildType { get; private set; } = Dash;

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

    public string TotalPhysical { get; private set; } = Dash;

    public string AvailablePhysical { get; private set; } = Dash;

    public string InUsePhysical { get; private set; } = Dash;

    public string Commit { get; private set; } = Dash;

    public string PageFile { get; private set; } = Dash;

    public static SystemDetailForm Read()
    {
        var windows = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var bios = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
        var cpu = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        var memory = MemoryInfo.Read();
        var host = CpuFacts.Host;
        var build = Text(windows?.GetValue("CurrentBuild")?.ToString());
        var ubr = windows?.GetValue("UBR")?.ToString();
        var form = new SystemDetailForm
        {
            Title = Environment.MachineName,
            HostName = Environment.MachineName,
            OsName = Text(windows?.GetValue("ProductName")?.ToString()),
            DisplayVersion = Text(windows?.GetValue("DisplayVersion")?.ToString()),
            Edition = Text(windows?.GetValue("EditionID")?.ToString()),
            Build = string.IsNullOrWhiteSpace(ubr) ? build : build + "." + ubr,
            OsVersion = Environment.OSVersion.VersionString,
            BuildType = Text(windows?.GetValue("CurrentType")?.ToString()),
            Owner = Text(windows?.GetValue("RegisteredOwner")?.ToString()),
            Organization = Text(windows?.GetValue("RegisteredOrganization")?.ToString()),
            ProductId = Text(windows?.GetValue("ProductId")?.ToString()),
            InstallDate = Install(windows?.GetValue("InstallDate")),
            BootTime = DateTimeOffset.Now.AddMilliseconds(-Environment.TickCount64).ToString("g", CultureInfo.CurrentCulture),
            Manufacturer = Text(bios?.GetValue("SystemManufacturer")?.ToString()),
            Model = Text(bios?.GetValue("SystemProductName")?.ToString()),
            SystemType = Environment.Is64BitOperatingSystem ? "x64-based PC" : "x86-based PC",
            Processor = ProcessorText(cpu, host),
            Bios = Text(bios?.GetValue("BIOSVersion")?.ToString()),
            WindowsDirectory = Text(Environment.GetFolderPath(Environment.SpecialFolder.Windows)),
            SystemDirectory = Text(Environment.SystemDirectory),
            Locale = CultureInfo.CurrentCulture.DisplayName,
            TimeZone = TimeZoneInfo.Local.DisplayName,
            Domain = DomainName(),
            TotalPhysical = Gb(memory.TotalBytes),
            AvailablePhysical = Gb(memory.AvailableBytes),
            InUsePhysical = Gb(memory.InUseBytes),
            Commit = memory.CommitLimitBytes.IsOk && memory.CommittedBytes.IsOk
                ? Gb(memory.CommittedBytes) + " of " + Gb(memory.CommitLimitBytes)
                : Dash,
            PageFile = PageFile()
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
        Line(text, "Host name", HostName);
        Line(text, "OS name", OsName);
        Line(text, "Edition", Edition);
        Line(text, "Version", DisplayVersion);
        Line(text, "Build", Build);
        Line(text, "OS version", OsVersion);
        Line(text, "Build type", BuildType);
        Line(text, "Owner", Owner);
        Line(text, "Organization", Organization);
        Line(text, "Product ID", ProductId);
        Line(text, "Install date", InstallDate);
        Line(text, "Boot time", BootTime);
        Line(text, "Windows directory", WindowsDirectory);
        Line(text, "System directory", SystemDirectory);
        Line(text, "Locale", Locale);
        Line(text, "Time zone", TimeZone);
        Line(text, "Domain", Domain);
        text.AppendLine();
        text.AppendLine("Computer");
        Line(text, "Manufacturer", Manufacturer);
        Line(text, "Model", Model);
        Line(text, "System type", SystemType);
        Line(text, "Processor", Processor);
        Line(text, "BIOS", Bios);
        text.AppendLine();
        text.AppendLine("Memory");
        Line(text, "Total physical", TotalPhysical);
        Line(text, "In use", InUsePhysical);
        Line(text, "Available", AvailablePhysical);
        Line(text, "Commit", Commit);
        Line(text, "Page file", PageFile);
        return text.ToString();
    }

    private static string ProcessorText(RegistryKey? cpu, Vestigium.Helpers.SystemInfo.Fact<CpuTopology> host)
    {
        var name = cpu?.GetValue("ProcessorNameString")?.ToString();
        var topology = host.IsOk ? $"{host.Value.Sockets} socket, {host.Value.Cores} cores, {host.Value.Logical} logical" : null;
        if (string.IsNullOrWhiteSpace(name))
            return Text(topology);
        return string.IsNullOrWhiteSpace(topology) ? name.Trim() : name.Trim() + "  " + topology;
    }

    private static string PageFile()
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

    private static string Gb(Vestigium.Helpers.SystemInfo.Fact<ulong> fact)
        => fact.IsOk ? (fact.Value / 1073741824d).ToString("0.0", CultureInfo.InvariantCulture) + " GB" : Dash;

    private static void Line(StringBuilder text, string label, string value)
        => text.AppendLine($"{label}  {value}");

    private static string Text(string? value)
        => string.IsNullOrWhiteSpace(value) ? Dash : value.Trim();
}
