using Vestigium.Helpers.PerfMon;
using Vestigium.Helpers.PerfMon.Network;
using PdhNic = Vestigium.Helpers.PerfMon.Network.NetworkInterface;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel
{
    private IReadOnlyList<CounterPath> SamplePaths(string instance, IReadOnlyList<string> counters)
        => NetworkCounterCatalog.Paths(PdhNic.Category, instance, counters).Concat(HostSamplePaths()).ToArray();
}
