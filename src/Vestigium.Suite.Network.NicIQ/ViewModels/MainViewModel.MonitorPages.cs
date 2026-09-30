using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<MonitorFactColumn> MonitorFacts { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMonitorHeadlineTitle))]
    private string _monitorHeadlineTitle = string.Empty;

    [ObservableProperty]
    private string _monitorHeadline = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PauseButtonText))]
    [NotifyPropertyChangedFor(nameof(CanExportReadings))]
    [NotifyPropertyChangedFor(nameof(CanImportReadings))]
    private bool _monitorPaused;

    public bool HasMonitorHeadlineTitle => !string.IsNullOrWhiteSpace(MonitorHeadlineTitle);

    public string LegendButtonText => ChartTheme.WatchLegend ? "Hide Legend" : "Show Legend";

    public string PauseButtonText => MonitorPaused ? "Resume" : "Pause";

    public bool CanExportReadings => MonitorPaused && _ring.MaxDepth() > 0;

    public bool CanImportReadings => MonitorPaused;

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

    [RelayCommand]
    private void TogglePause()
    {
        if (MonitorPaused)
            ResumeMonitoring();
        else
            PauseMonitoring();
    }

    [RelayCommand(CanExecute = nameof(CanExportReadings))]
    private void ExportReadings()
    {
        if (!CanExportReadings)
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Export readings",
            Filter = "NicIQ readings (*.json)|*.json|All files (*.*)|*.*",
            FileName = "niciq-readings.json",
            AddExtension = true,
            DefaultExt = ".json"
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            MonitorReadingsIo.Write(dialog.FileName, new MonitorReadingsFile
            {
                NicName = SelectedMonitorNic?.Name,
                PdhInstance = MonitorInstance,
                Series = [.. _ring.Snapshot()]
            });
            Note("Idle", "Readings exported");
        }
        catch (Exception ex)
        {
            Note("Failed", ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanImportReadings))]
    private void ImportReadings()
    {
        if (!CanImportReadings)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Import readings",
            Filter = "NicIQ readings (*.json)|*.json|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var file = MonitorReadingsIo.Read(dialog.FileName);
            if (file is null || file.Series.Count == 0)
            {
                Note("Failed", "No readings in that file");
                return;
            }

            _ring.Replace(file.Series);
            PaintChart(force: true);
            RefreshMonitorFacts();
            RaiseReadingsCommands();
            Note("Idle", "Readings imported");
        }
        catch (Exception ex)
        {
            Note("Failed", ex.Message);
        }
    }

    public void RefreshLegendButton()
        => OnPropertyChanged(nameof(LegendButtonText));

    private void PauseMonitoring()
    {
        MonitorPaused = true;
        Note("Paused", "Monitoring held");
        RaiseReadingsCommands();
    }

    private void ResumeMonitoring()
    {
        _ring.Clear();
        LiveChart = null;
        ChartStrip = "Waiting for samples.";
        MonitorPaused = false;
        BeginWarm("Resuming monitor\u2026");
        PaintChart(force: true);
        Note("Idle", "Monitoring resumed");
        RaiseReadingsCommands();
    }

    private void RaiseReadingsCommands()
    {
        OnPropertyChanged(nameof(CanExportReadings));
        OnPropertyChanged(nameof(CanImportReadings));
        ExportReadingsCommand.NotifyCanExecuteChanged();
        ImportReadingsCommand.NotifyCanExecuteChanged();
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
        SyncWarm();
    }

    partial void OnLiveChartChanged(FrameworkElement? value)
    {
        RefreshMonitorFacts();
        SyncWarm();
        RaiseReadingsCommands();
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
