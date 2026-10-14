using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoSummaryFormatterTests
{
    [Fact]
    public void Domain_role_1_is_member_workstation()
    {
        Assert.Equal("Member Workstation", SummaryFormatter.DomainRoleName(1));
    }

    [Fact]
    public void Product_type_1_is_work_station()
    {
        Assert.Equal("Work Station", SummaryFormatter.ProductTypeName(1));
    }

    [Fact]
    public void Uptime_is_days_hours_minutes()
    {
        var boot = new DateTime(2026, 10, 13, 8, 21, 47);
        var now = new DateTime(2026, 10, 14, 9, 23, 26);
        var text = SummaryFormatter.Uptime(boot, now);
        Assert.Equal("1 days, 1 hours, 1 minutes", text);
    }

    [Fact]
    public void Short_user_strips_domain()
    {
        Assert.Equal("s1108600", SummaryFormatter.ShortUser(@"qamr-accounts\s1108600"));
        Assert.Equal("s1108600", SummaryFormatter.ShortUser("s1108600"));
    }

    [Fact]
    public void Format_contains_locked_labels()
    {
        var text = SummaryFormatter.Format(
            "USOVE2WLU15197",
            "quintiles.net",
            1,
            1,
            "LENOVO",
            "21MDS1KD00",
            "PF59HVMZ",
            "Microsoft Windows 11 Enterprise",
            "64-bit",
            "10.0.26100",
            "26100",
            new DateTime(2026, 6, 5, 12, 24, 57),
            new DateTime(2026, 10, 13, 8, 21, 47),
            "AMD Ryzen 5 PRO 8540U w/ Radeon 740M Graphics",
            1,
            12,
            14.67,
            "s1108600",
            new DateTime(2026, 10, 14, 9, 23, 26),
            @"C:\Users\s1108600\Desktop\USOVE2WLU15197_Inventory_20261014_092324");

        Assert.Contains("WINDOWS SERVER INVENTORY SUMMARY", text);
        Assert.Contains("Domain Role:              1 (Member Workstation)", text);
        Assert.Contains("OS Product Type:          1 (Work Station)", text);
        Assert.Contains("Installed Memory (GB):    14.67", text);
        Assert.Contains("Current Uptime:           1 days, 1 hours, 1 minutes", text);
        Assert.Contains("Collection Time:          2026-10-14 09:23:26", text);
    }
}
