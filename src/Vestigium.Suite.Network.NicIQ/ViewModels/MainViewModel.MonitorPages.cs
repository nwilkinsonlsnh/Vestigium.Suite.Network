namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    public bool CpuPageOpen
    {
        get => ChartPage == MonitorChartPages.Cpu;
        set { if (value) ChartPage = MonitorChartPages.Cpu; }
    }

    public bool MemoryPageOpen
    {
        get => ChartPage == MonitorChartPages.Memory;
        set { if (value) ChartPage = MonitorChartPages.Memory; }
    }

    private void RaiseMonitorPages()
    {
        OnPropertyChanged(nameof(ThroughputPageOpen));
        OnPropertyChanged(nameof(PacketsPageOpen));
        OnPropertyChanged(nameof(IntegrityPageOpen));
        OnPropertyChanged(nameof(UtilizationPageOpen));
        OnPropertyChanged(nameof(CpuPageOpen));
        OnPropertyChanged(nameof(MemoryPageOpen));
    }
}
