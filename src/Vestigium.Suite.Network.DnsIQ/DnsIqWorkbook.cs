using System.IO;
using Vestigium.Helpers.ClosedXml;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ;

public static class DnsIqWorkbook
{
    public static IReadOnlyList<string> CaptureHeaders { get; } =
        ["Host", "Ports", "Hits", "Sources", "DNS", "Error", "Category", "Answer"];

    public static bool MayOpen(string saved, string dialogPath)
        => string.Equals(saved, dialogPath, StringComparison.OrdinalIgnoreCase)
           && saved.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

    public static bool TryWrite(
        string path,
        IReadOnlyList<AnswerRow> lookup,
        IReadOnlyList<CaptureLine> capture,
        IReadOnlyList<double> probe,
        IReadOnlyList<(string Key, object? Value)> cover,
        out string? saved,
        out IReadOnlyList<string> sheets,
        out string? reject)
    {
        saved = null;
        sheets = [];
        reject = null;
        if (lookup.Count == 0 && capture.Count == 0 && probe.Count == 0)
        {
            reject = "Nothing loaded to export.";
            return false;
        }

        var names = new List<string> { "Cover" };
        if (lookup.Count > 0)
            names.Add("Lookup");
        if (capture.Count > 0)
            names.Add("Capture");
        if (probe.Count > 0)
            names.Add("Probe");

        using var book = WorkbookHelper.Create("Cover", "DnsIQ");
        book.Sheet("Cover").WriteTable(SheetTable.KeyValue("Field", "Value", cover, "Cover"));
        if (lookup.Count > 0)
            book.Sheet("Lookup").WriteTable(LookupTable(lookup));
        if (capture.Count > 0)
            book.Sheet("Capture").WriteTable(CaptureTable(capture));
        if (probe.Count > 0)
            book.Sheet("Probe").WriteTable(ProbeTable(probe));
        saved = book.SaveAs(path);
        if (!File.Exists(saved))
        {
            reject = "Export did not write " + path;
            return false;
        }

        sheets = names;
        return true;
    }

    private static SheetTable LookupTable(IReadOnlyList<AnswerRow> rows)
        => SheetTable.Create(
            ["Type", "Name", "Data", "Ttl"],
            rows.Select(row => (IReadOnlyList<object?>)[row.Type, row.Name, row.Data, row.Ttl]),
            "Lookup");

    private static SheetTable CaptureTable(IReadOnlyList<CaptureLine> rows)
        => SheetTable.Create(
            CaptureHeaders,
            rows.Select(row => (IReadOnlyList<object?>)[row.Host, row.Ports, row.Hits, row.Sources, row.Dns, row.Error, row.Category, row.Answer]),
            "Capture");

    private static SheetTable ProbeTable(IReadOnlyList<double> samples)
        => SheetTable.Create(
            ["Index", "RttMs"],
            samples.Select((sample, index) => (IReadOnlyList<object?>)[index + 1, sample]),
            "Probe");
}
