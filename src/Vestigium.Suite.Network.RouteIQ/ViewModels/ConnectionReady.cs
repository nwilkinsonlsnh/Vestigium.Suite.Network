using System.Windows;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private void MarkConnectionsReady()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(MarkConnectionsReady);
            return;
        }

        _connectionsReady = true;
        RaiseCanExecute();
    }
}
