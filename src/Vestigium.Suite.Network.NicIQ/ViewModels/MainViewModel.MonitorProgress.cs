using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Analytics;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private bool _centerReady;
    private DateTimeOffset _runStartedUtc;
    private TimeSpan _heldElapsed;
    private bool _reviewImported;
    private TimeSpan _reviewSpan;
    private string _lastProgressText = string.Empty;

    private void BeginMonitorClock()
    {
        _reviewImported = false;
        _reviewSpan = TimeSpan.Zero;
        _heldElapsed = TimeSpan.Zero;
        _runStartedUtc = DateTimeOffset.UtcNow;
        PublishMonitorProgress();
    }

    private void HoldMonitorClock()
    {
        _heldElapsed = ElapsedNow();
        PublishMonitorProgress();
    }

    private void ReviewImportedClock(IReadOnlyList<MonitorReadingSeries> series)
    {
        _reviewImported = true;
        _reviewSpan = SpanOf(series);
        PublishMonitorProgress();
    }

    private TimeSpan ElapsedNow()
    {
        if (_reviewImported)
            return _reviewSpan;
        if (MonitorPaused)
            return _heldElapsed <= TimeSpan.Zero ? ElapsedFromStart() : _heldElapsed;
        return ElapsedFromStart();
    }

    private TimeSpan ElapsedFromStart()
    {
        if (_runStartedUtc == default)
            return TimeSpan.Zero;
        var span = DateTimeOffset.UtcNow - _runStartedUtc;
        return span < TimeSpan.Zero ? TimeSpan.Zero : span;
    }

    private void PublishMonitorProgress()
    {
        EnsureCenterColumn();
        var elapsed = ElapsedNow();
        var collected = Math.Min(_ring.MaxDepth(), MonitorRing.ArchiveSeconds);
        string text;
        if (_reviewImported)
            text = $"Review  {FormatClock(elapsed)}  ·  {collected} samples";
        else if (MonitorPaused)
            text = $"Paused  {FormatClock(elapsed)}  ·  {collected}/{MonitorRing.ArchiveSeconds} s";
        else
            text = $"Monitoring  {FormatClock(elapsed)}  ·  {collected}/{MonitorRing.ArchiveSeconds} s";

        if (string.Equals(_lastProgressText, text, StringComparison.Ordinal))
            return;
        _lastProgressText = text;
        StatusBar?.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Text = text,
            IsProgressVisible = false
        });
    }

    private void EnsureCenterColumn()
    {
        if (_centerReady || StatusBar is null)
            return;

        var column = StatusBar.Engine.Columns.FirstOrDefault(c => string.Equals(c.Key, "progress", StringComparison.OrdinalIgnoreCase))
            ?? StatusBar.Engine.Columns.FirstOrDefault(c => c.Slot == StatusBarSlot.Center);
        if (column is null)
            return;

        column.Kind = StatusBarColumnKind.Text;
        column.Slot = StatusBarSlot.Center;
        column.Width = StatusBarColumnWidth.Auto;
        column.IsProgressVisible = false;
        column.IsLiveRegion = false;
        _centerReady = true;
    }

    private static TimeSpan SpanOf(IReadOnlyList<MonitorReadingSeries> series)
    {
        DateTimeOffset? first = null;
        DateTimeOffset? last = null;
        var points = 0;
        foreach (var row in series)
        {
            if (row.Points is null)
                continue;
            foreach (var point in row.Points)
            {
                points++;
                if (point.At is not { } at)
                    continue;
                if (first is null || at < first) first = at;
                if (last is null || at > last) last = at;
            }
        }

        if (first is not null && last is not null && last >= first)
            return last.Value - first.Value;
        return TimeSpan.FromSeconds(Math.Max(points == 0 ? 0 : 1, Math.Min(points, MonitorRing.ArchiveSeconds)));
    }

    private static string FormatClock(TimeSpan value)
    {
        if (value < TimeSpan.Zero)
            value = TimeSpan.Zero;
        var total = (int)Math.Floor(value.TotalSeconds);
        var hours = total / 3600;
        var minutes = total % 3600 / 60;
        var seconds = total % 60;
        return hours > 0
            ? string.Create(null, $"{hours}:{minutes:00}:{seconds:00}")
            : string.Create(null, $"{minutes}:{seconds:00}");
    }
}
