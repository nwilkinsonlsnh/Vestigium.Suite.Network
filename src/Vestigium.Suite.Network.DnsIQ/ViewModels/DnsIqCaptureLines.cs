using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel
{
    public ObservableCollection<CaptureLine> Lines { get; } = [];

    public void AttachCaptureLines()
    {
        Hosts.CollectionChanged += (_, change) =>
        {
            if (change.NewItems is not null)
            {
                foreach (HarHostRow row in change.NewItems)
                    row.PropertyChanged += (_, _) => RebuildLines();
            }

            RebuildLines();
        };
        RebuildLines();
    }

    public void RebuildLines()
    {
        Lines.Clear();
        foreach (var line in CaptureLines.From(Hosts))
            Lines.Add(line);
    }

    [RelayCommand]
    private Task LookupLine(CaptureLine? line)
    {
        if (line is null || IsBusy)
            return Task.CompletedTask;
        Name = line.Host;
        SelectDns?.Invoke();
        return LookupAsync();
    }

    [RelayCommand]
    private Task LookupLineAndProbe(CaptureLine? line)
    {
        if (line is null || IsBusy)
            return Task.CompletedTask;
        Name = line.Host;
        SelectDns?.Invoke();
        return ProbeAsync();
    }
}
