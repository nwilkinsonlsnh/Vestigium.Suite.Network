using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    public string Title { get; } = "Dashboard";

    public string Subject { get; } = "Resolver pulse charts";

    public string Description { get; } =
        "Not in this release. Pulse numbers stay on the DnsIQ page and the status bar.";
}
