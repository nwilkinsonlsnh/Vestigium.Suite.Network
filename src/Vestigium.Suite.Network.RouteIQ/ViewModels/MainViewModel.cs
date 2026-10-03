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
    private string _probeAddress = string.Empty;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _probeLine = string.Empty;

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
        RaiseCanExecute();
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
            RaiseCanExecute();
        }
    }

    [RelayCommand(CanExecute = nameof(CanProbe))]
    private async Task Probe()
    {
        if (_busy)
            return;

        if (!RouteIqInput.TryParseProbe(ProbeAddress, out var address, out var reason))
        {
            if (reason is not null)
                Status = $"Failed: {reason}";
            return;
        }

        _busy = true;
        RaiseCanExecute();
        Status = "Running";
        try
        {
            var result = await Task.Run(() => NetworkHelper.ProbeNeighbor(address!.ToString())).ConfigureAwait(true);
            if (result.Found && !string.IsNullOrWhiteSpace(result.MacAddress))
            {
                ProbeLine = $"{result.Address} {result.MacAddress}";
                Status = "Idle";
            }
            else
            {
                ProbeLine = result.Address;
                Status = "Failed";
            }
        }
        catch (Exception ex)
        {
            Status = $"Failed: {ex.Message}";
        }
        finally
        {
            _busy = false;
            RaiseCanExecute();
        }
    }

    private bool CanRefresh() => !_busy;

    private bool CanProbe() => !_busy && !string.IsNullOrWhiteSpace(ProbeAddress);

    partial void OnProbeAddressChanged(string value) => ProbeCommand.NotifyCanExecuteChanged();

    private void RaiseCanExecute()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        ProbeCommand.NotifyCanExecuteChanged();
    }

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
