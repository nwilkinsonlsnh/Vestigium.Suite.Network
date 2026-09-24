using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.PingIQ;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.PingIQ);
        base.OnStartup(e);
    }
}
