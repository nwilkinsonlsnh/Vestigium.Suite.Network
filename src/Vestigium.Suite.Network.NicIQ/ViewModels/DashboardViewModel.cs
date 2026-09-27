using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    private CounterSampleResult? _last;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WatchEmpty))]
    private bool _hasWatchData;

    [ObservableProperty]
    private FrameworkElement? _receiveChart;

    [ObservableProperty]
    private FrameworkElement? _sendChart;

    [ObservableProperty]
    private FrameworkElement? _errorChart;

    public Action? GoToNicIq { get; set; }

    public Action<bool>? DashboardAvailabilityChanged { get; set; }

    public bool WatchEmpty => !HasWatchData;

    public bool ShowReceive { get; set; } = true;
    public bool ShowSend { get; set; } = true;
    public bool ShowErrors { get; set; }
    public bool ShowDiscards { get; set; }

    [RelayCommand]
    private void OpenNicIq() => GoToNicIq?.Invoke();

    public void Unlock() => DashboardAvailabilityChanged?.Invoke(true);

    public void ShowTraffic(CounterSampleResult result, bool receive, bool send, bool errors, bool discards)
    {
        _last = result;
        ShowReceive = receive;
        ShowSend = send;
        ShowErrors = errors;
        ShowDiscards = discards;
        Redraw();
    }

    public void Redraw()
    {
        if (_last is null || _last.Samples.Count < 2)
        {
            ReceiveChart = SendChart = ErrorChart = null;
            HasWatchData = false;
            return;
        }

        Unlock();
        var receive = new List<double>();
        var send = new List<double>();
        var faults = new List<double>();
        for (var i = 1; i < _last.Samples.Count; i++)
        {
            var prev = _last.Samples[i - 1];
            var cur = _last.Samples[i];
            var seconds = Math.Max(0.2, (cur.CapturedUtc - prev.CapturedUtc).TotalSeconds);
            var delta = CounterSampleEngineDelta(prev.Reading, cur.Reading);
            receive.Add(delta.BytesIn * 8d / seconds / 1000d);
            send.Add(delta.BytesOut * 8d / seconds / 1000d);
            faults.Add(delta.ErrorsIn + delta.ErrorsOut + (ShowDiscards ? delta.DiscardsIn + delta.DiscardsOut : 0));
        }

        ReceiveChart = ShowReceive ? TryChart(receive, "Receive (kbps)") : null;
        SendChart = ShowSend ? TryChart(send, "Send (kbps)") : null;
        ErrorChart = ShowErrors || ShowDiscards ? TryChart(faults, ShowDiscards ? "Errors + discards" : "Errors") : null;
        HasWatchData = ReceiveChart is not null || SendChart is not null || ErrorChart is not null;
    }

    public void ShowWatch(IReadOnlyList<long> speedsBitsPerSecond)
    {
        // kept so older calls compile if any remain
        Unlock();
    }

    private static CounterReading CounterSampleEngineDelta(CounterReading start, CounterReading end)
        => new(
            Math.Max(0, end.BytesIn - start.BytesIn),
            Math.Max(0, end.BytesOut - start.BytesOut),
            Math.Max(0, end.ErrorsIn - start.ErrorsIn),
            Math.Max(0, end.ErrorsOut - start.ErrorsOut),
            Math.Max(0, end.DiscardsIn - start.DiscardsIn),
            Math.Max(0, end.DiscardsOut - start.DiscardsOut));

    private static FrameworkElement? TryChart(IReadOnlyList<double> values, string title)
    {
        try
        {
            if (values.Count == 0)
                return null;
            var series = NumericSeries.From(values, title);
            return ChartTheme.Paint(ChartView.Line(series, ChartTheme.Options(title, "Sample", "kbps")));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
