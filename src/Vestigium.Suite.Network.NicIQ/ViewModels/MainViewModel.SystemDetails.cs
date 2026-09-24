namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    public bool ShowSystemDetails => ChartPage is MonitorChartPages.Cpu or MonitorChartPages.Memory;
}
