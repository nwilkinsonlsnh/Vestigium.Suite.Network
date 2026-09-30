using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Keeps a rolling archive. Live charts grow with the collected span, up to the window.</summary>
public sealed class MonitorRing
{
    public const int DefaultArchiveSeconds = 300;
    public const int MinArchiveSeconds = 180;
    public const int MaxArchiveSeconds = 600;

    public static int ArchiveSeconds { get; private set; } = DefaultArchiveSeconds;
    public static int Cap => ArchiveSeconds;

    private readonly Dictionary<string, List<Observation>> _rows =
        new(StringComparer.OrdinalIgnoreCase);

    public static int ClampArchive(int seconds)
    {
        var minutes = (int)Math.Round(seconds / 60d, MidpointRounding.AwayFromZero);
        if (minutes < MinArchiveSeconds / 60)
            minutes = MinArchiveSeconds / 60;
        if (minutes > MaxArchiveSeconds / 60)
            minutes = MaxArchiveSeconds / 60;
        return minutes * 60;
    }

    public static int ApplyArchive(int seconds)
    {
        ArchiveSeconds = ClampArchive(seconds);
        return ArchiveSeconds;
    }

    public void Clear()
    {
        foreach (var list in _rows.Values)
            list.Clear();
    }

    public void Trim()
    {
        foreach (var list in _rows.Values)
        {
            if (list.Count > ArchiveSeconds)
                list.RemoveRange(0, list.Count - ArchiveSeconds);
        }
    }

    public void Add(SampleRecord sample)
    {
        ArgumentNullException.ThrowIfNull(sample);
        if (sample.Value is not double raw || double.IsNaN(raw) || double.IsInfinity(raw))
            return;

        var name = sample.Counter.Trim();
        if (name.Length == 0)
            return;

        if (!_rows.TryGetValue(name, out var list))
        {
            list = [];
            _rows[name] = list;
        }

        list.Add(new Observation((decimal)raw, sample.Utc));
        if (list.Count > ArchiveSeconds)
            list.RemoveRange(0, list.Count - ArchiveSeconds);
    }

    public IReadOnlyList<Observation> Of(string counter)
    {
        if (!_rows.TryGetValue(counter, out var list) || list.Count == 0)
            return [];
        return list.ToArray();
    }

    public int MaxDepth()
    {
        var max = 0;
        foreach (var list in _rows.Values)
        {
            if (list.Count > max)
                max = list.Count;
        }

        return max;
    }

    public IReadOnlyList<MonitorReadingSeries> Snapshot()
    {
        var rows = new List<MonitorReadingSeries>();
        foreach (var pair in _rows.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (pair.Value.Count == 0)
                continue;
            rows.Add(new MonitorReadingSeries(
                pair.Key,
                pair.Value.Select(o => new MonitorReadingPoint(o.Value, o.At)).ToArray()));
        }

        return rows;
    }

    public void Replace(IEnumerable<MonitorReadingSeries> series)
    {
        ArgumentNullException.ThrowIfNull(series);
        Clear();
        foreach (var set in series)
        {
            var name = set.Counter?.Trim() ?? string.Empty;
            if (name.Length == 0 || set.Points is null || set.Points.Count == 0)
                continue;
            var list = new List<Observation>();
            foreach (var point in set.Points)
            {
                if (point is null)
                    continue;
                list.Add(new Observation(point.Value, point.At));
                if (list.Count > ArchiveSeconds)
                    list.RemoveRange(0, list.Count - ArchiveSeconds);
            }

            if (list.Count > 0)
                _rows[name] = list;
        }
    }
}
