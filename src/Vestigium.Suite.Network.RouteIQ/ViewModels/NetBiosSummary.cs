using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    [ObservableProperty]
    private string _netBiosSummary = "NetBIOS names.";

    private void ApplyNetBiosStats(NetworkNetBiosStats stats)
    {
        var node = string.IsNullOrWhiteSpace(stats.NodeType) ? "unknown" : stats.NodeType;
        NetBiosSummary = $"Node {node}. Broadcast {stats.ResolvedByBroadcast}, WINS {stats.ResolvedByNameServer}. Registered broadcast {stats.RegisteredByBroadcast}, WINS {stats.RegisteredByNameServer}.";
    }
}

    private static async Task<T> WithinAsync<T>(Task<T> work, CancellationToken cancellation)
    {
        var cap = Task.Delay(TimeSpan.FromSeconds(8), cancellation);
        var done = await Task.WhenAny(work, cap).ConfigureAwait(false);
        if (done != work)
            throw new TimeoutException("NetBIOS did not return in 8 seconds.");
        return await work.ConfigureAwait(false);
    }

