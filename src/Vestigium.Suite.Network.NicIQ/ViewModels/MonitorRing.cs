using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>Last 60 finite samples per counter. Unavailable rows never become zero.</summary>
public sealed class MonitorRing
{
    public const int Cap = 60;

    private readonly Dictionary<string, List<Observation>> _rows =
        new(StringComparer.OrdinalIgnoreCase);

    public void Clear()
    {
        foreach (var list in _rows.Values)
            list.Clear();
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
        if (list.Count > Cap)
            list.RemoveRange(0, list.Count - Cap);
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
}
