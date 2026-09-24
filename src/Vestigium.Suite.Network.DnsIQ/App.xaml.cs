using System.Windows;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.DnsIQ);
        base.OnStartup(e);
    }
}
