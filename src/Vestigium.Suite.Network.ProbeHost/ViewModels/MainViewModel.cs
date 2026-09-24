using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.ProbeHost.ViewModels;

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
            var snap = NetworkHelper.GetSnapshot();
            Log = $"{snap.Workstation.HostName} adapters={snap.Workstation.Adapters.Count} routes={snap.Routes.Count} conns={snap.Connections.Count}";
        }
        catch (Exception ex)
        {
            Log = ex.Message;
        }
    }
}
