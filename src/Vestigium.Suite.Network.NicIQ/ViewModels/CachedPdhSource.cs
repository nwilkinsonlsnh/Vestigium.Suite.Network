using System.Diagnostics;
using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>
/// Keeps <see cref="PerformanceCounter"/> instances alive.
/// The stock PerfMon source opens and disposes on every read, so rate counters stay at zero.
/// </summary>
internal sealed class CachedPdhSource : ICounterSource, IDisposable
{
    private readonly Dictionary<string, PerformanceCounter> _live = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _primed = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();
    private bool _disposed;

    public bool HasFreshPrime { get; private set; }

    public int OpenedCount
    {
        get { lock (_gate) return _live.Count; }
    }

    public int PrimedCount
    {
        get { lock (_gate) return _primed.Count; }
    }

    public SampleRecord Read(CounterPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var key = Key(path);
            try
            {
                if (!_live.TryGetValue(key, out var counter))
                {
                    counter = Open(path);
                    _live[key] = counter;
                }

                var value = counter.NextValue();
                var first = _primed.Add(key);
                HasFreshPrime |= first;
                return SampleRecord.Ok(path, value);
            }
            catch (InvalidOperationException)
            {
                _primed.Add(key);
                return SampleRecord.Unavailable(path);
            }
            catch (ArgumentException)
            {
                _primed.Add(key);
                return SampleRecord.Unavailable(path);
            }
            catch (UnauthorizedAccessException)
            {
                _primed.Add(key);
                return SampleRecord.Unavailable(path);
            }
        }
    }

    public IReadOnlyList<string> ListInstances(string category, int cap)
    {
        if (cap <= 0 || string.IsNullOrWhiteSpace(category))
            return [];

        try
        {
            var cat = new PerformanceCounterCategory(category.Trim());
            if (cat.CategoryType == PerformanceCounterCategoryType.SingleInstance)
                return [];
            var names = cat.GetInstanceNames();
            return names.Length <= cap ? names : names.Take(cap).ToArray();
        }
        catch (InvalidOperationException)
        {
            return [];
        }
        catch (ArgumentException)
        {
            return [];
        }
    }

    public bool NeedsPrime(CounterPath path)
    {
        ArgumentNullException.ThrowIfNull(path);
        lock (_gate)
            return !_primed.Contains(Key(path));
    }

    public void ResetPrimeFlag() => HasFreshPrime = false;

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
            foreach (var counter in _live.Values)
            {
                try { counter.Dispose(); }
                catch (Exception) { }
            }

            _live.Clear();
            _primed.Clear();
        }
    }

    private static string Key(CounterPath path) => $"{path.Category}\u001f{path.Counter}\u001f{path.Instance}";

    private static PerformanceCounter Open(CounterPath path)
        => path.Instance.Length == 0
            ? new PerformanceCounter(path.Category, path.Counter, readOnly: true)
            : new PerformanceCounter(path.Category, path.Counter, path.Instance, readOnly: true);
}
