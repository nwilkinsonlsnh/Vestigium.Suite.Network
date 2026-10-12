using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.ClosedXml;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class ExportsViewModel : ObservableObject
{
    private readonly MainViewModel _trace;
    private readonly PathPingViewModel _path;

    public ExportsViewModel(MainViewModel trace, PathPingViewModel path)
    {
        _trace = trace;
        _path = path;
    }

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    [ObservableProperty] private bool _includeTrace = true;
    [ObservableProperty] private bool _includePath = true;
    [ObservableProperty] private bool _openFile = true;
    [ObservableProperty] private bool _openFolder;

    public string Folder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        "Vestigium", "Exports", "TraceIQ");

    [RelayCommand]
    private void Export()
    {
        if (!IncludeTrace && !IncludePath)
        {
            StatusBar?.Engine.PostImmediate("elapsed", new StatusBarUpdate { Text = "Pick TraceIQ or Path Ping." });
            return;
        }

        Directory.CreateDirectory(Folder);
        var file = Path.Combine(Folder, $"TraceIQ-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
        using var book = WorkbookHelper.Create("TraceIQ", "TraceIQ");
        if (IncludeTrace)
            WriteTrace(book);
        if (IncludePath)
            WritePath(book);
        book.SaveAs(file);

        StatusBar?.Engine.PostImmediate("elapsed", new StatusBarUpdate { Text = file });
        if (OpenFile)
            Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
        if (OpenFolder)
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{file}\"") { UseShellExecute = true });
    }

    private void WriteTrace(WorkbookSession book)
    {
        var count = Math.Clamp((int)_trace.Settings.Probes, 1, HopRow.MaxColumns);
        var headers = new List<string> { "TTL", "Status", "Address", "Name", "Reached" };
        headers.AddRange(Enumerable.Range(1, count).Select(i => $"RTT {i} (ms)"));
        var rows = _trace.Hops.Select(h =>
        {
            var cells = new List<object?> { h.Ttl, h.StatusText, h.Address, h.Name, h.Reached ? "Yes" : "No" };
            var values = new[] { h.P1, h.P2, h.P3, h.P4, h.P5, h.P6, h.P7, h.P8, h.P9, h.P10 };
            cells.AddRange(values.Take(count));
            return (IReadOnlyList<object?>)cells;
        }).ToList();
        book.Sheet("TraceIQ").WriteTable(SheetTable.Create(headers, rows, "TraceIQ"));
    }

    private void WritePath(WorkbookSession book)
    {
        var headers = new[] { "TTL", "Status", "Address", "Name", "Sent", "Received", "Loss", "Min", "Q1", "Median", "Q3", "Max" };
        var rows = _path.Rows.Select(r => (IReadOnlyList<object?>)new object?[]
        {
            r.Ttl, r.StatusText, r.Address, r.Name, r.Sent, r.Received, r.Loss, r.Min, r.Q1, r.Median, r.Q3, r.Max
        }).ToList();
        book.Sheet("Path Ping").WriteTable(SheetTable.Create(headers, rows, "PathPing"));

        var samples = new List<IReadOnlyList<object?>>();
        foreach (var row in _path.Rows)
        {
            var n = 1;
            foreach (var sample in row.Samples)
                samples.Add([row.Ttl, row.Address, n++, sample]);
        }
        book.Sheet("Path Ping Samples").WriteTable(
            SheetTable.Create(["TTL", "Address", "Sample", "RTT ms"], samples, "PathPingSamples"));
    }
}
