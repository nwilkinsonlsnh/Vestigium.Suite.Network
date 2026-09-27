using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WatchEmpty))]
    private bool _hasWatchData;

    [ObservableProperty]
    private FrameworkElement? _watchChart;

    public Action? GoToNicIq { get; set; }

    public Action<bool>? DashboardAvailabilityChanged { get; set; }

    public bool WatchEmpty => !HasWatchData;

    [RelayCommand]
    private void OpenNicIq() => GoToNicIq?.Invoke();

    public void Unlock() => DashboardAvailabilityChanged?.Invoke(true);

    public void ShowWatch(IReadOnlyList<long> speedsBitsPerSecond)
    {
        Unlock();
        try
        {
            if (speedsBitsPerSecond.Count == 0)
            {
                WatchChart = null;
                HasWatchData = false;
                return;
            }

            var scale = LinkSpeed.ScaleFor(speedsBitsPerSecond);
            var values = speedsBitsPerSecond.Select(v => LinkSpeed.ToUnit(v, scale.Divisor)).ToList();
            var series = NumericSeries.From(values, scale.SeriesName);
            WatchChart = TryChart(() => ChartView.Line(
                series,
                ChartTheme.Options($"Link speed ({scale.Unit})", "Sample", scale.Unit)));
            HasWatchData = WatchChart is not null;
        }
        catch (Exception)
        {
            WatchChart = null;
            HasWatchData = false;
        }
    }

    private static FrameworkElement? TryChart(Func<FrameworkElement> build)
    {
        try
        {
            return ChartTheme.Paint(build());
        }
        catch (Exception)
        {
            return null;
        }
    }
}
