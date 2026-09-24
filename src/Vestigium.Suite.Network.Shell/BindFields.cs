using CommunityToolkit.Mvvm.ComponentModel;

namespace Vestigium.Suite.Network.Shell;

public sealed partial class BindFields : ObservableObject
{
    [ObservableProperty]
    private int _interfaceIndex;

    [ObservableProperty]
    private string? _sourceAddress;
}
