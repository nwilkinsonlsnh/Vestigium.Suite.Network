using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    [ObservableProperty]
    private string _lmHostSummary = "LMHOSTS.";

    private void ApplyLmHostSummary(int count)
        => LmHostSummary = count == 0
            ? "No LMHOSTS rows. A missing file is normal."
            : count + " LMHOSTS row" + (count == 1 ? "" : "s") + ".";
}
