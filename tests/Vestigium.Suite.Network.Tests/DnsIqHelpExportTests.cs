using System.IO;
using Vestigium.Suite.Network.DnsIQ;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Vestigium.Suite.Network.DnsIQ.Views;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DnsIqHelpExportTests
{
    [Fact]
    public void Help_topics_are_the_ten_names_in_order()
    {
        var titles = HelpCopy.Topics("1.0.0", ["MIT"]).Select(topic => topic.Title).ToArray();
        Assert.Equal(
            ["Overview", "Lookup", "Probe", "Hosts Viewer", "Dashboard", "Exports", "Settings", "Glossary", "About", "License"],
            titles);
    }

    [Fact]
    public void Overview_says_it_does_not_shell_to_nslookup()
    {
        var text = Text("Overview");
        Assert.Contains("nslookup", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shell to nslookup.exe", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Capture_says_resolved_is_not_reachable()
    {
        Assert.Contains("Resolved is not Reachable", Text("Hosts Viewer"), StringComparison.Ordinal);
    }

    [Fact]
    public void Probe_says_the_pulse_does_not_append_rows()
    {
        Assert.Contains("The pulse does not append rows", Text("Probe"), StringComparison.Ordinal);
    }

    [Fact]
    public void Exports_says_the_workbook_is_not_uploaded()
    {
        Assert.Contains("not uploaded", Text("Exports"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void License_topic_is_non_empty()
    {
        var topic = HelpCopy.Topics("1.0.0", ["Copyright (c) 2026 Nathaniel Wilkinson"]).Single(item => item.Title == "License");
        Assert.NotEmpty(topic.Lines);
        Assert.Contains("Copyright", string.Join(" ", topic.Lines), StringComparison.Ordinal);
    }

    [Fact]
    public void Lookup_rows_write_cover_and_lookup_not_capture()
    {
        var path = TempFile();
        var wrote = DnsIqWorkbook.TryWrite(
            path,
            [new AnswerRow("A", "lab.example", "10.0.0.8", 60)],
            [],
            [],
            [],
            [("Name", "lab.example")],
            out var saved,
            out var sheets,
            out var reject);

        Assert.True(wrote, reject);
        Assert.Equal(["Cover", "Lookup"], sheets);
        Assert.DoesNotContain("Capture", sheets);
        Assert.True(File.Exists(saved));
        File.Delete(saved!);
    }

    [Fact]
    public void Capture_sheet_is_one_line_per_answer()
    {
        Assert.Equal(
            ["Host", "Ports", "Hits", "Sources", "DNS", "Error", "Category", "Answer"],
            DnsIqWorkbook.CaptureHeaders);
        Assert.DoesNotContain("Answers", DnsIqWorkbook.CaptureHeaders);

        var path = TempFile();
        var lines = new[]
        {
            new CaptureLine("edge.example", "443", 2, "Request", "Resolved", "", "IPv4", "1.1.1.1"),
            new CaptureLine("edge.example", "443", 2, "Request", "Resolved", "", "IPv6", "2600:9000:27d1:4000:7:951d:7a80:93a1"),
            new CaptureLine("edge.example", "443", 2, "Request", "Resolved", "", "IPv4", "8.8.8.8")
        };
        var wrote = DnsIqWorkbook.TryWrite(path, [], lines, [], [], [("Capture", lines.Length)], out var saved, out var sheets, out var reject);
        Assert.True(wrote, reject);
        Assert.Equal(["Cover", "Capture"], sheets);
        Assert.True(File.Exists(saved));
        File.Delete(saved!);
    }

    [Fact]
    public void No_rows_does_not_write_a_file()
    {
        var path = TempFile();
        var wrote = DnsIqWorkbook.TryWrite(path, [], [], [], [], [], out var saved, out _, out var reject);
        Assert.False(wrote);
        Assert.Null(saved);
        Assert.Equal("Nothing loaded to export.", reject);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void A_path_that_is_not_xlsx_is_not_opened()
    {
        Assert.False(DnsIqWorkbook.MayOpen(@"C:\exports\DnsIQ-export.txt", @"C:\exports\DnsIQ-export.txt"));
        Assert.False(DnsIqWorkbook.MayOpen(@"C:\exports\other.xlsx", @"C:\exports\DnsIQ-export.xlsx"));
        Assert.True(DnsIqWorkbook.MayOpen(@"C:\exports\DnsIQ-export.xlsx", @"C:\exports\DnsIQ-export.xlsx"));
    }

    [Fact]
    public void Missing_export_keys_are_the_defaults()
    {
        var root = Path.Combine(Path.GetTempPath(), "DnsIQ-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"), """{"Type":"MX","Port":53}""");
        var loaded = new DnsIqSettingsStore(root).Load();
        Assert.True(loaded.ExportLookup);
        Assert.True(loaded.ExportCapture);
        Assert.True(loaded.ExportProbe);
        Assert.False(loaded.ExportOpenAfter);
        Assert.False(loaded.ExportOpenFolder);
        Assert.Equal("MX", loaded.Type);
    }

    private static string Text(string title)
    {
        var topic = HelpCopy.Topics("1.0.0", ["MIT"]).Single(item => item.Title == title);
        return string.Join("\n", topic.Lines);
    }

    private static string TempFile()
    {
        var folder = Path.Combine(Path.GetTempPath(), "DnsIQ-tests");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, Guid.NewGuid().ToString("N") + ".xlsx");
    }
}
