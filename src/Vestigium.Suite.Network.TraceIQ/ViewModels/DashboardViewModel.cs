using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Analytics;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private IReadOnlyList<PathPingRow> _rows = [];

    public DashboardViewModel(SettingsViewModel settings) => _settings = settings;
    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public ObservableCollection<DashboardPage> Pages { get; } = [];

    [ObservableProperty] private string _target = "";
    [ObservableProperty] private bool _ready;
    [ObservableProperty] private string _status = "Finish a Path Ping to open the dashboard.";
    [ObservableProperty] private DashboardPage? _selected;
    [ObservableProperty] private bool _steady;
    [ObservableProperty] private bool _iqr;
    public bool Legend
    {
        get => _settings.ShowLegend;
        set => _settings.ShowLegend = value;
    }

    [RelayCommand]
    private void ToggleLegend() => Legend = !Legend;

    [RelayCommand]
    private void ToggleSteady()
    {
        Steady = !Steady;
        Rebuild();
    }

    [RelayCommand]
    private void ToggleIqr()
    {
        Iqr = !Iqr;
        Rebuild();
    }

    public void Publish()
    {
        if (StatusBar is null)
            return;
        StatusBar.Message = Selected?.Summary ?? Status;
        StatusBar.Engine.PostImmediate("elapsed", new StatusBarUpdate { Text = "" });
    }

    [RelayCommand]
    private void Select(DashboardPage? page)
    {
        if (page is not null)
            Selected = page;
    }

    partial void OnSelectedChanged(DashboardPage? value)
    {
        foreach (var page in Pages)
            page.IsSelected = ReferenceEquals(page, value);
        Publish();
    }

    public void Rebuild()
    {
        if (_rows.Count == 0)
            return;
        Offer(Target, _rows);
    }

    public void ApplyLegend()
    {
        foreach (var page in Pages)
        {
            ChartView.SetLegendVisible(page.Chart, Legend);
            if (page.Control is not null)
                ChartView.SetLegendVisible(page.Control, Legend);
        }
    }

    public void Offer(string target, IReadOnlyList<PathPingRow> rows)
    {
        var graded = rows.Where(r => r.Samples.Count > 0).ToList();
        if (graded.Count == 0 || string.IsNullOrWhiteSpace(target))
            return;
        _rows = graded;
        Target = target;
        Pages.Clear();
        Pages.Add(DashboardPage.Overview(graded));
        foreach (var row in graded)
            Pages.Add(DashboardPage.Hop(row, Steady, Iqr));
        Selected = Pages[0];
        OnSelectedChanged(Selected);
        ApplyLegend();
        Publish();
        Ready = true;
        Status = $"{graded.Count} hops. Overview compares them. A tab per hop shows that hop only.";
    }
}

public sealed partial class DashboardPage : ObservableObject
{
    public string Header { get; init; } = "";
    public string Summary { get; init; } = "";
    public FrameworkElement Chart { get; init; } = null!;
    public FrameworkElement? Control { get; init; }
    public bool IsOverview { get; init; }

    [ObservableProperty] private bool _isSelected;

    public static DashboardPage Overview(IReadOnlyList<PathPingRow> rows)
    {
        var labels = rows.Select(r => $"TTL {r.Ttl}").ToArray();
        var medians = rows.Select(Median).ToArray();
        var chart = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Column,
            Title = "Median RTT by hop",
            Series = [new ChartSeries { Name = "Median", Y = medians, Labels = labels }],
            Options = ChartTheme.Apply(new ChartOptions { YLabel = "RTT ms", ShowLegend = false, Height = 360 })
        });
        return new DashboardPage
        {
            Header = "Overview",
            Summary = "Median of each hop. Not the whole Path Ping as one population.",
            Chart = chart,
            IsOverview = true
        };
    }

    public static DashboardPage Hop(PathPingRow row, bool steady, bool iqr)
    {
        var samples = row.Samples;
        if (steady && samples.Count > 1 && samples.Distinct().Count() > 1)
        {
            var series0 = NumericSeries.From(samples.Select(s => (decimal)s), "band");
            var limits = series0.ControlLimits();
            samples = samples.Where(s => (double)s >= limits.Lower && (double)s <= limits.Upper).ToArray();
            if (samples.Count == 0)
                samples = row.Samples;
        }
        if (iqr && samples.Count > 3)
        {
            var sorted = samples.OrderBy(s => s).ToArray();
            long At(double p) => sorted[(int)Math.Round((sorted.Length - 1) * p)];
            var lo = At(0.25);
            var hi = At(0.75);
            var body = sorted.Where(s => s >= lo && s <= hi).ToArray();
            if (body.Length > 0)
                samples = body;
        }
        var series = NumericSeries.From(samples.Select(s => (decimal)s), $"ttl-{row.Ttl}");
        var chart = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Histogram,
            Title = "Histogram",
            Source = series,
            Options = ChartTheme.Apply(new ChartOptions { YLabel = "Count", XLabel = "RTT ms", ShowLegend = true, ShowEcdf = true, Height = 280 })
        });
        var ys = row.Samples.Select(s => (double)s).ToArray();
        var xs = Enumerable.Range(1, ys.Length).Select(i => (double)i).ToArray();
        ControlLimits? limits = samples.Distinct().Count() > 1 ? series.ControlLimits() : null;
        var control = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Control,
            Title = "RTT in order",
            Series = [new ChartSeries { Name = "RTT", X = xs, Y = ys }],
            Limits = limits,
            Options = ChartTheme.Apply(new ChartOptions { YLabel = "RTT ms", XLabel = "Sample", ShowLegend = true, Height = 280 })
        });
        var five = Five(row.Samples);
        return new DashboardPage
        {
            Header = $"TTL {row.Ttl}",
            Summary = $"{row.Address}   {row.Name}   Sent {row.Sent}   Received {row.Received}   Loss {row.Loss}   min {five.min}   Q1 {five.q1}   median {five.median}   Q3 {five.q3}   max {five.max}",
            Chart = chart,
            Control = control
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
