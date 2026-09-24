using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.ProbeHost;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.ProbeHost);
        base.OnStartup(e);
    }
}
