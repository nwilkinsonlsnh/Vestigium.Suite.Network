using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject
{
    public Action? GoToNicIq { get; set; }

    public Action<bool>? DashboardAvailabilityChanged { get; set; }

    [RelayCommand]
    private void OpenNicIq() => GoToNicIq?.Invoke();

    public void Unlock() => DashboardAvailabilityChanged?.Invoke(true);
}
