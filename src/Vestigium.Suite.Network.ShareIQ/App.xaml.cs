using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.ShareIQ;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.ShareIQ);
        base.OnStartup(e);
    }
}
