using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    [ObservableProperty]
    private string _lmHostSummary = "LMHosts.";

    private void ApplyLmHostSummary(int count)
        => LmHostSummary = count == 0
            ? "No LMHosts rows. A missing file is normal."
            : count + " LMHosts row" + (count == 1 ? "" : "s") + ".";
}
