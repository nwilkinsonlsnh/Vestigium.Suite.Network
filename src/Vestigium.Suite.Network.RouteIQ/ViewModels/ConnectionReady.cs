namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private void MarkConnectionsReady()
    {
        _connectionsReady = true;
        RaiseCanExecute();
    }
}
