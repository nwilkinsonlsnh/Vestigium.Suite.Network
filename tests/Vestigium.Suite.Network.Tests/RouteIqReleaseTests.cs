using System.IO;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqReleaseTests
{
    [Fact]
    public void Refresh_print_does_not_scan()
    {
        var refresh = Method(ViewModel("MainViewModel.cs"), "private Task RefreshRoutes()");
        Assert.DoesNotContain("ResolveLiveVendors", refresh);
        Assert.Contains("RouteMask", refresh);
        var neighbors = Method(ViewModel("MainViewModel.cs"), "private Task RefreshNeighbors()");
        Assert.Contains("NeighborMask", neighbors);
    }

    [Fact]
    public void Neighbor_fill_starts_rtt_and_the_probe_button_is_gone()
    {
        var load = Method(ViewModel("MainViewModel.cs"), "private async Task LoadNeighbors(");
        Assert.Contains("StartEnrich", load);
        var view = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ViewModel("MainViewModel.cs"))!, "..", "Views", "NeighborsView.xaml"));
        Assert.DoesNotContain("Probe", view);
        Assert.Contains("private async Task FillRtt", File.ReadAllText(ViewModel("MainViewModel.cs")));
    }

    [Fact]
    public void Open_after_refuses_a_non_xlsx_path()
    {
        var export = Method(ViewModel("RouteIqExport.cs"), "private void WriteExport(");
        var guard = export.IndexOf("EndsWith(\".xlsx\"", StringComparison.Ordinal);
        var start = export.IndexOf("Process.Start", StringComparison.Ordinal);
        Assert.True(guard > 0);
        Assert.True(start > guard);
        Assert.Contains("Export was written. It was not opened.", export);
    }

    [Fact]
    public void Failed_query_leaves_rows_visible()
    {
        var bars = File.ReadAllText(ViewModel("KqlBars.cs"));
        var compile = Method(ViewModel("KqlBars.cs"), "private KqlBoundQuery? Compile(");
        Assert.Contains("return null", compile);
        Assert.Contains("!compiled.Ok", compile);
        Assert.Contains("return _routeBound is null", bars);
        Assert.Contains("return _neighborBound is null", bars);
        Assert.Contains("return _connectionBound is null", bars);
    }

    private static string ViewModel(string name)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Vestigium.Suite.Network.RouteIQ", "ViewModels", name);
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException(name);
    }

    private static string Method(string path, string signature)
    {
        var text = File.ReadAllText(path);
        var start = text.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, signature);
        var open = text.IndexOf('{', start);
        var depth = 0;
        for (var i = open; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}') depth--;
            if (depth == 0)
                return text[start..(i + 1)];
        }

        throw new InvalidOperationException(signature);
    }

    private static int Count(string text, string token)
    {
        var count = 0;
        var from = 0;
        while ((from = text.IndexOf(token, from, StringComparison.Ordinal)) >= 0)
        {
            count++;
            from += token.Length;
        }

        return count;
    }
}
