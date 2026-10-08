using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        Session?.Save();
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
        var captureRows = capture ? Lines.ToList() : [];
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
            var cover = CoverRows(lookupRows.Count, captureRows.Count, probeRows.Count);
            if (!DnsIqWorkbook.TryWrite(path, lookupRows, captureRows, probeRows, cover, out var saved, out _, out var reject) || saved is null)
            {
                Status = reject ?? "Nothing loaded to export.";
                return;
            }

            Status = "Exported " + saved;
            if (ExportOpenAfter || ExportOpenFolder)
            {
                if (!DnsIqWorkbook.MayOpen(saved, path))
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

    private List<(string Key, object? Value)> CoverRows(int lookup, int capture, int probe)
    {
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
        if (lookup > 0)
            cover.Add(("Lookup", lookup.ToString(CultureInfo.InvariantCulture)));
        if (capture > 0)
            cover.Add(("Capture", capture.ToString(CultureInfo.InvariantCulture)));
        if (probe > 0)
            cover.Add(("Probe", probe.ToString(CultureInfo.InvariantCulture)));
        return cover;
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
}
