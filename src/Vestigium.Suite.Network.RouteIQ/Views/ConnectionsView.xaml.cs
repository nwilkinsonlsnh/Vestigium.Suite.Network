using System.Windows.Controls;
using Vestigium.Suite.Network.RouteIQ.ViewModels;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class ConnectionsView : UserControl
{
    public ConnectionsView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel host && host.Connections.Count == 0)
                await host.SnapshotConnectionsCommand.ExecuteAsync(null);
        };
    }
}
