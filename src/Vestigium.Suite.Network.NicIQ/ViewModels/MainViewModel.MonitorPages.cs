using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<MonitorDetailRow> MonitorFacts { get; } = [];

    [ObservableProperty]
    private string _monitorHeadline = string.Empty;

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
        RefreshMonitorFacts();
    }

    partial void OnLiveChartChanged(FrameworkElement? value) => RefreshMonitorFacts();

    private void RefreshMonitorFacts()
    {
        var (headline, rows) = MonitorDetailCard.Build(ChartPage, SelectedMonitorNic, _ring);
        if (!string.Equals(MonitorHeadline, headline, StringComparison.Ordinal))
            MonitorHeadline = headline;

        var n = Math.Min(MonitorFacts.Count, rows.Count);
        for (var i = 0; i < n; i++)
        {
            if (!string.Equals(MonitorFacts[i].Label, rows[i].Label, StringComparison.Ordinal)
                || !string.Equals(MonitorFacts[i].Value, rows[i].Value, StringComparison.Ordinal))
            {
                MonitorFacts[i] = rows[i];
            }
        }

        while (MonitorFacts.Count > rows.Count)
            MonitorFacts.RemoveAt(MonitorFacts.Count - 1);
        for (var i = MonitorFacts.Count; i < rows.Count; i++)
            MonitorFacts.Add(rows[i]);
    }
}
