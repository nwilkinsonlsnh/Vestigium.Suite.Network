using System.Collections.ObjectModel;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private bool _busy;

    public IReadOnlyList<string> Families { get; } = ["All", "IPv4", "IPv6"];

    public ObservableCollection<NetworkRoute> Routes { get; } = [];

    public ObservableCollection<NetworkNeighbor> Neighbors { get; } = [];

    [ObservableProperty]
    private string _family = "All";

    [ObservableProperty]
    private string _status = "Idle";

    public MainViewModel() => _ = Refresh();

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task Refresh()
    {
        if (_busy)
            return;

        if (!RouteIqInput.TryMapFamily(Family, out var family, out var reason))
        {
            Status = $"Failed: {reason}";
            return;
        }

        _busy = true;
        RefreshCommand.NotifyCanExecuteChanged();
        Status = "Running";
        try
        {
            var snapshot = await Task.Run(() => Load(family)).ConfigureAwait(true);
            Replace(Routes, snapshot.Routes);
            Replace(Neighbors, snapshot.Neighbors);
            Status = "Idle";
        }
        catch (Exception ex)
        {
            Status = $"Failed: {ex.Message}";
        }
        finally
        {
            _busy = false;
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanRefresh() => !_busy;

    private static (IReadOnlyList<NetworkRoute> Routes, IReadOnlyList<NetworkNeighbor> Neighbors) Load(RouteFamily family)
    {
        var routes = NetworkHelper.GetRoutes(family);
        var neighbors = Filter(NetworkHelper.GetNeighbors(), family);
        return (routes, neighbors);
    }

    private static IReadOnlyList<NetworkNeighbor> Filter(IReadOnlyList<NetworkNeighbor> rows, RouteFamily family)
    {
        if (family == RouteFamily.All)
            return rows;

        var want = family == RouteFamily.Pv4
            ? AddressFamily.InterNetwork
            : AddressFamily.InterNetworkV6;
        return rows.Where(row => row.Family == want).ToArray();
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
