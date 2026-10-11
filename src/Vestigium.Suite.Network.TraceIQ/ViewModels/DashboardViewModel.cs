using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public ObservableCollection<DashboardHop> Hops { get; } = [];

    [ObservableProperty] private string _target = "";
    [ObservableProperty] private bool _ready;
    [ObservableProperty] private string _status = "Finish a Path Ping to open the dashboard.";

    public void Offer(string target, IReadOnlyList<PathPingRow> rows)
    {
        var graded = rows.Where(r => r.Samples.Count > 0).ToList();
        if (graded.Count == 0 || string.IsNullOrWhiteSpace(target))
            return;
        Target = target;
        Hops.Clear();
        foreach (var row in graded)
            Hops.Add(DashboardHop.From(row));
        Ready = true;
        Status = $"{Hops.Count} hops. One box per hop, in TTL order.";
    }
}

public sealed class DashboardHop
{
    public int Ttl { get; init; }
    public string Address { get; init; } = "";
    public string Name { get; init; } = "";
    public string Loss { get; init; } = "";
    public string Sent { get; init; } = "";
    public string Received { get; init; } = "";
    public FrameworkElement Chart { get; init; } = null!;

    public static DashboardHop From(PathPingRow row)
    {
        var series = NumericSeries.From(row.Samples.Select(s => (decimal)s), $"ttl-{row.Ttl}");
        var chart = ChartView.From(new ChartSpec
        {
            Kind = ChartKind.Box,
            Title = $"TTL {row.Ttl}",
            Source = series,
            Options = new ChartOptions
            {
                YLabel = "RTT ms",
                BoxWhisker = BoxWhiskerKind.FiveNumber,
                ShowLegend = false,
                Height = 180
            }
        });
        return new DashboardHop
        {
            Ttl = row.Ttl,
            Address = row.Address,
            Name = row.Name,
            Loss = row.Loss,
            Sent = row.Sent.ToString(),
            Received = row.Received.ToString(),
            Chart = chart
        };
    }
}
