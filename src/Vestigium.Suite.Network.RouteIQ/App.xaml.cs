using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.RouteIQ;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.RouteIQ);
        base.OnStartup(e);
    }
}
