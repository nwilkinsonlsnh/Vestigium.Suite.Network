using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.NicIQ;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.NicIQ);
        base.OnStartup(e);
    }
}
