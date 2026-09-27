using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Controls.StatusBar;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _header = "NicIQ";

    [ObservableProperty]
    private string _caption = "Idle";

    public VestigiumStatusBarViewModel? StatusBar { get; set; }
}
