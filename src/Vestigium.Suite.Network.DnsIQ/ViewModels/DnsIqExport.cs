using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.ClosedXml;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel
{
    private bool _exportQuiet;

    [ObservableProperty]
    private bool _exportLookup = true;

    [ObservableProperty]
    private bool _exportCapture = true;

    [ObservableProperty]
    private bool _exportProbe = true;

    [ObservableProperty]
    private bool _exportOpenAfter;

    [ObservableProperty]
    private bool _exportOpenFolder;

    public string CapturePath { get; private set; } = string.Empty;

    public void RememberExportChecks(bool lookup, bool capture, bool probe, bool openAfter, bool openFolder)
    {
        _exportQuiet = true;
        ExportLookup = lookup;
        ExportCapture = capture;
        ExportProbe = probe;
        ExportOpenAfter = openAfter;
        ExportOpenFolder = openFolder;
        _exportQuiet = false;
        ExportSelectedCommand.NotifyCanExecuteChanged();
    }

    public void NotifyExport()
    {
        ExportSelectedCommand.NotifyCanExecuteChanged();
        ExportAllCommand.NotifyCanExecuteChanged();
    }

    private bool CanExport()
        => Answers.Count > 0 || Hosts.Count > 0 || Dashboard?.ProbeSamples.Count > 0;

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ExportSelected()
    {
        if (!ExportLookup && !ExportCapture && !ExportProbe)
        {
            Status = "Select a print to export.";
            return;
        }

        WriteExport(ExportLookup, ExportCapture, ExportProbe);
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ExportAll()
        => WriteExport(lookup: true, capture: true, probe: true);

    private void WriteExport(bool lookup, bool capture, bool probe)
    {
        var lookupRows = lookup ? Answers.ToList() : [];
        var captureRows = capture ? Hosts.ToList() : [];
        var probeRows = probe ? Dashboard?.ProbeSamples ?? [] : [];
        if (lookupRows.Count == 0 && captureRows.Count == 0 && probeRows.Count == 0)
        {
            Status = "Nothing loaded to export.";
            return;
        }

        var name = "DnsIQ-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xlsx";
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            FileName = name,
            RestoreDirectory = true,
            AddExtension = true,
            DefaultExt = ".xlsx",
            OverwritePrompt = true
        };
        try
        {
            var folder = ExportFolder();
            dialog.InitialDirectory = folder;
            if (dialog.ShowDialog() != true)
                return;
            var path = string.IsNullOrWhiteSpace(dialog.FileName) ? Path.Combine(folder, name) : dialog.FileName;
            using var book = WorkbookHelper.Create("Cover", "DnsIQ");
            var cover = new List<(string Key, object? Value)>
            {
                ("Host", Environment.MachineName),
                ("Operator", Environment.UserName),
                ("Taken", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")),
                ("Name", string.IsNullOrWhiteSpace(Name) ? "localhost" : Name.Trim()),
                ("Server", string.IsNullOrWhiteSpace(Server) ? "--" : Server.Trim()),
                ("Port", Port.ToString(CultureInfo.InvariantCulture)),
                ("Type", RecordType),
                ("Interface", InterfaceLabel()),
                ("Source", string.IsNullOrWhiteSpace(Bind.SourceAddress) ? "Any" : Bind.SourceAddress),
                ("Requests", RequestCount.ToString(CultureInfo.InvariantCulture)),
                ("Seconds", DurationSeconds.ToString(CultureInfo.InvariantCulture)),
                ("Capture file", string.IsNullOrWhiteSpace(CapturePath) ? "--" : CapturePath)
            };
            if (lookupRows.Count > 0)
                cover.Add(("Lookup", lookupRows.Count.ToString(CultureInfo.InvariantCulture)));
            if (captureRows.Count > 0)
                cover.Add(("Capture", captureRows.Count.ToString(CultureInfo.InvariantCulture)));
            if (probeRows.Count > 0)
                cover.Add(("Probe", probeRows.Count.ToString(CultureInfo.InvariantCulture)));
            book.Sheet("Cover").WriteTable(SheetTable.KeyValue("Field", "Value", cover, "Cover"));
            if (lookupRows.Count > 0)
                book.Sheet("Lookup").WriteTable(LookupTable(lookupRows));
            if (captureRows.Count > 0)
                book.Sheet("Capture").WriteTable(CaptureTable(captureRows));
            if (probeRows.Count > 0)
                book.Sheet("Probe").WriteTable(ProbeTable(probeRows));
            var saved = book.SaveAs(path);
            if (!File.Exists(saved))
            {
                Status = "Export did not write " + path;
                return;
            }

            Status = "Exported " + saved;
            if (ExportOpenAfter || ExportOpenFolder)
            {
                if (!string.Equals(saved, path, StringComparison.OrdinalIgnoreCase) || !saved.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    Status = "Export was written. It was not opened.";
                    return;
                }

                if (ExportOpenAfter)
                    Process.Start(new ProcessStartInfo(saved) { UseShellExecute = true });
                if (ExportOpenFolder)
                    Process.Start(new ProcessStartInfo(Path.GetDirectoryName(saved)!) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            Status = string.IsNullOrWhiteSpace(ex.Message) ? "Failed" : ex.Message;
        }
    }

    private string InterfaceLabel()
        => Interfaces.FirstOrDefault(item => item.Index == SelectedInterfaceIndex)?.Label ?? SelectedInterfaceIndex.ToString(CultureInfo.InvariantCulture);

    private static string ExportFolder()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(profile, "Desktop", "Vestigium", "Exports", "DnsIQ");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static SheetTable LookupTable(IReadOnlyList<AnswerRow> rows)
        => SheetTable.Create(
            ["Type", "Name", "Data", "Ttl"],
            rows.Select(row => (IReadOnlyList<object?>)[row.Type, row.Name, row.Data, row.Ttl]),
            "Lookup");

    private static SheetTable CaptureTable(IReadOnlyList<HarHostRow> rows)
        => SheetTable.Create(
            ["Host", "Ports", "Hits", "Sources", "DNS", "Error", "Answers"],
            rows.Select(row => (IReadOnlyList<object?>)[row.Host, row.Ports, row.Hits, row.Sources, row.Dns, row.Error, row.Answers]),
            "Capture");

    private static SheetTable ProbeTable(IReadOnlyList<double> samples)
        => SheetTable.Create(
            ["Index", "RttMs"],
            samples.Select((sample, index) => (IReadOnlyList<object?>)[index + 1, sample]),
            "Probe");
}
