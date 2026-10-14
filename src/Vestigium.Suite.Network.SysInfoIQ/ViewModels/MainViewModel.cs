using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;

namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _cts;

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public SettingsViewModel? Settings { get; set; }

    [ObservableProperty]
    private string _summary = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _lastExport = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemPageOpen))]
    [NotifyPropertyChangedFor(nameof(StoragePageOpen))]
    [NotifyPropertyChangedFor(nameof(NetworkPageOpen))]
    [NotifyPropertyChangedFor(nameof(SecurityPageOpen))]
    [NotifyPropertyChangedFor(nameof(SoftwarePageOpen))]
    [NotifyPropertyChangedFor(nameof(ProcessesPageOpen))]
    [NotifyPropertyChangedFor(nameof(EventsPageOpen))]
    private string _category = "System";

    public bool SystemPageOpen
    {
        get => Category == "System";
        set { if (value) Category = "System"; }
    }

    public bool StoragePageOpen
    {
        get => Category == "Storage";
        set { if (value) Category = "Storage"; }
    }

    public bool NetworkPageOpen
    {
        get => Category == "Network";
        set { if (value) Category = "Network"; }
    }

    public bool SecurityPageOpen
    {
        get => Category == "Security";
        set { if (value) Category = "Security"; }
    }

    public bool SoftwarePageOpen
    {
        get => Category == "Software";
        set { if (value) Category = "Software"; }
    }

    public bool ProcessesPageOpen
    {
        get => Category == "Processes";
        set { if (value) Category = "Processes"; }
    }

    public bool EventsPageOpen
    {
        get => Category == "Events";
        set { if (value) Category = "Events"; }
    }

    public ObservableCollection<InventorySection> Sections { get; } = new();

    [RelayCommand(CanExecute = nameof(CanCollect))]
    private async Task CollectAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        CollectCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
        Sections.Clear();
        Summary = "";
        LastExport = "";

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        try
        {
            Status("Collecting base…");
            var sections = await Task.Run(() => Collector.CollectBase(ct), ct).ConfigureAwait(true);
            foreach (var s in sections)
                Sections.Add(s);

            var outputRoot = Settings?.OutputFolder;
            if (string.IsNullOrWhiteSpace(outputRoot))
                outputRoot = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var now = DateTime.Now;
            var hostname = ExtractHostname(sections) ?? Environment.MachineName;
            var folderName = $"{hostname}_Inventory_{now:yyyyMMdd_HHmmss}";
            var location = Path.Combine(outputRoot, folderName);

            Summary = Collector.BuildSummary(Sections, location, now);

            Status("Writing export…");
            var written = await Task.Run(() => Exporter.Write(outputRoot, hostname, Summary, Sections.ToList(), now), ct)
                .ConfigureAwait(true);
            LastExport = written;
            Status("Idle");
        }
        catch (OperationCanceledException)
        {
            Status("Cancelled");
        }
        catch (Exception ex)
        {
            Summary = ex.Message;
            Status("Failed");
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
            CollectCommand.NotifyCanExecuteChanged();
            CancelCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private bool CanCollect() => !IsBusy;

    private void Status(string message)
    {
        if (StatusBar is not null)
            StatusBar.Message = message;
    }

    private static string? ExtractHostname(IReadOnlyList<InventorySection> sections)
    {
        var cs = sections.FirstOrDefault(s => s.Name == "ComputerSystem" && !s.IsError);
        if (cs is null)
            return null;
        foreach (var line in cs.Content.Split('\n'))
        {
            var idx = line.IndexOf(':');
            if (idx < 0)
                continue;
            var key = line[..idx].Trim();
            if (string.Equals(key, "DNSHostName", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "Name", StringComparison.OrdinalIgnoreCase))
            {
                var value = line[(idx + 1)..].Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        return null;
    }
}
