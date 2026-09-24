using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    [ObservableProperty]
    private string _netBiosSummary = "NetBIOS names.";

    private void ApplyNetBiosStats()
    {
        var stats = NetworkHelper.GetNetBiosStats();
        var node = string.IsNullOrWhiteSpace(stats.NodeType) ? "unknown" : stats.NodeType;
        NetBiosSummary = $"Node {node}. Broadcast {stats.ResolvedByBroadcast}, WINS {stats.ResolvedByNameServer}. Registered broadcast {stats.RegisteredByBroadcast}, WINS {stats.RegisteredByNameServer}.";
    }
}
