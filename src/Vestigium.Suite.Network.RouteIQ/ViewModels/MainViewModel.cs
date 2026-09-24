using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _log = string.Empty;

    public MainViewModel() => Refresh();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            var routes = NetworkHelper.GetRoutes();
            Log = string.Join(Environment.NewLine, routes.Select(r => $"{r.Destination}/{r.PrefixLength} via {r.Gateway}"));
        }
        catch (Exception ex)
        {
            Log = ex.Message;
        }
    }
}
