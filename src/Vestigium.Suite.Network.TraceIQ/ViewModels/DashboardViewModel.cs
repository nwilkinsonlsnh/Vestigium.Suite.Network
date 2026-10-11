using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public ObservableCollection<DashboardPage> Pages { get; } = [];

    [ObservableProperty] private string _target = "";
    [ObservableProperty] private bool _ready;
    [ObservableProperty] private string _status = "Finish a Path Ping to open the dashboard.";
    [ObservableProperty] private DashboardPage? _selected;

    public void Offer(string target, IReadOnlyList<PathPingRow> rows)
    {
        var graded = rows.Where(r => r.Samples.Count > 0).ToList();
        if (graded.Count == 0 || string.IsNullOrWhiteSpace(target))
            return;
        Target = target;
        Pages.Clear();
        Pages.Add(DashboardPage.Overview(graded));
        foreach (var row in graded)
            Pages.Add(DashboardPage.Hop(row));
        Selected = Pages[0];
        Ready = true;
        Status = $"{graded.Count} hops. Overview compares them. A tab per hop shows that hop only.";
    }
}

public sealed class DashboardPage
{
    public string Header { get; init; } = "";
    public string Summary { get; init; } = "";
    public FrameworkElement Chart { get; init; } = null!;
    public FrameworkElement? Ecdf { get; init; }
    public bool IsOverview { get; init; }

    public static DashboardPage Overview(IReadOnlyList<PathPingRow> rows)
    {
        var labels = rows.Select(r => $"TTL {r.Ttl}").ToArray();
        var medians = rows.Select(Median).ToArray();
        var chart = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Column,
            Title = "Median RTT by hop",
            Series = [new ChartSeries { Name = "Median", Y = medians, Labels = labels }],
            Options = new ChartOptions { YLabel = "RTT ms", ShowLegend = false, Height = 360 }
        });
        return new DashboardPage
        {
            Header = "Overview",
            Summary = "Median of each hop. Not the whole Path Ping as one population.",
            Chart = chart,
            IsOverview = true
        };
    }

    public static DashboardPage Hop(PathPingRow row)
    {
        var series = NumericSeries.From(row.Samples.Select(s => (decimal)s), $"ttl-{row.Ttl}");
        var chart = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Histogram,
            Title = "Histogram",
            Source = series,
            Options = new ChartOptions { YLabel = "Count", XLabel = "RTT ms", ShowLegend = false, Height = 360 }
        });
        var ecdf = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Ecdf,
            Title = "ECDF",
            Source = series,
            Options = new ChartOptions { YLabel = "Share", XLabel = "RTT ms", ShowLegend = false, Height = 360 }
        });
        var five = Five(row.Samples);
        return new DashboardPage
        {
            Header = $"TTL {row.Ttl}",
            Summary = $"{row.Address}   {row.Name}   Sent {row.Sent}   Received {row.Received}   Loss {row.Loss}   min {five.min}   Q1 {five.q1}   median {five.median}   Q3 {five.q3}   max {five.max}",
            Chart = chart,
            Ecdf = ecdf
        };
    }

    private static double Median(PathPingRow row)
    {
        var s = row.Samples;
        return s.Count == 0 ? 0 : s[s.Count / 2];
    }

    private static (long min, long q1, long median, long q3, long max) Five(IReadOnlyList<long> samples)
    {
        long At(double p) => samples[(int)Math.Round((samples.Count - 1) * p)];
        return (At(0), At(0.25), At(0.5), At(0.75), At(1));
    }
}
