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
        var wrote = DnsIqWorkbook.TryWrite(path, [], lines, [], [("Capture", lines.Length)], out var saved, out var sheets, out var reject);
        Assert.True(wrote, reject);
        Assert.Equal(["Cover", "Capture"], sheets);
        Assert.True(File.Exists(saved));
        File.Delete(saved!);
    }

