using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<MonitorFactColumn> MonitorFacts { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMonitorHeadlineTitle))]
    private string _monitorHeadlineTitle = string.Empty;

    [ObservableProperty]
    private string _monitorHeadline = string.Empty;

    public bool HasMonitorHeadlineTitle => !string.IsNullOrWhiteSpace(MonitorHeadlineTitle);

    public string LegendButtonText => ChartTheme.WatchLegend ? "Hide Legend" : "Show Legend";

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

    [RelayCommand]
    private void ToggleLegend()
    {
        var next = !ChartTheme.WatchLegend;
        if (Settings is not null)
            Settings.ShowLegend = next;
        else
            ApplyLegend(next);
        RefreshLegendButton();
    }

    public void RefreshLegendButton()
        => OnPropertyChanged(nameof(LegendButtonText));

    private void RaiseMonitorPages()
    {
        OnPropertyChanged(nameof(ThroughputPageOpen));
        OnPropertyChanged(nameof(PacketsPageOpen));
        OnPropertyChanged(nameof(IntegrityPageOpen));
        OnPropertyChanged(nameof(UtilizationPageOpen));
        OnPropertyChanged(nameof(CpuPageOpen));
        OnPropertyChanged(nameof(MemoryPageOpen));
        RefreshMonitorFacts();
        SyncWarm();
    }

    partial void OnLiveChartChanged(FrameworkElement? value)
    {
        RefreshMonitorFacts();
        SyncWarm();
    }

    private void RefreshMonitorFacts()
    {
        var (title, value, columns) = MonitorDetailCard.Build(ChartPage, SelectedMonitorNic, _ring);
        if (!string.Equals(MonitorHeadlineTitle, title, StringComparison.Ordinal))
            MonitorHeadlineTitle = title;
        if (!string.Equals(MonitorHeadline, value, StringComparison.Ordinal))
            MonitorHeadline = value;

        var n = Math.Min(MonitorFacts.Count, columns.Count);
        for (var i = 0; i < n; i++)
            MonitorFacts[i] = columns[i];
        while (MonitorFacts.Count > columns.Count)
            MonitorFacts.RemoveAt(MonitorFacts.Count - 1);
        for (var i = MonitorFacts.Count; i < columns.Count; i++)
            MonitorFacts.Add(columns[i]);
    }
}
