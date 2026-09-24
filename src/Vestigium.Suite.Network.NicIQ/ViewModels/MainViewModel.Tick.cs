using Vestigium.Helpers.PerfMon;
using Vestigium.Helpers.PerfMon.Cpu;
using Vestigium.Helpers.PerfMon.Memory;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private CachedPdhSource? _sampleSource;

    private CachedPdhSource SampleSource()
        => _sampleSource ??= new CachedPdhSource();

    private void DropSampleSource()
    {
        _sampleSource?.Dispose();
        _sampleSource = null;
    }

    private static IReadOnlyList<CounterPath> HostSamplePaths()
        => CpuPerf.UtilizationPaths().Concat(MemoryPerf.HostPaths()).ToArray();

    private SampleJobResult RunSample(IReadOnlyList<CounterPath> paths, CancellationToken token)
    {
        var job = new SampleJob(paths, new SampleJobOptions { Count = 1, Source = SampleSource() });
        return job.RunAsync(token).GetAwaiter().GetResult();
    }

    private SampleTick TakeTickSafe(string name, string description, IReadOnlyList<string> selected, CancellationToken token)
    {
        try
        {
            return TakeTick(name, description, selected, token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SampleTick(null, null, ex.Message, false);
        }
    }
}
