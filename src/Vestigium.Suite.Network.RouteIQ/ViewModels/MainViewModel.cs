using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
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

    public MainViewModel() => Refresh();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            var routes = NetworkHelper.GetRoutes();
            Routes.Clear();
            foreach (var route in routes)
                Routes.Add(route);
            Status = "Idle";
        }
        catch (Exception ex)
        {
            Status = $"Failed: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanProbe))]
    private void Probe()
    {
    }

    private bool CanProbe() => false;
}
