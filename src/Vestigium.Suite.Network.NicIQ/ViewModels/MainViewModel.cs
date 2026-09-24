using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _log = string.Empty;

    public MainViewModel() => Refresh();

    [RelayCommand]
    private void Refresh()
    {
        try
        {
            var adapters = NetworkHelper.GetAdapters();
            Log = string.Join(Environment.NewLine, adapters.Select(a => $"{a.Name}  {a.OperationalStatus}  {a.Id}"));
        }
        catch (Exception ex)
        {
            Log = ex.Message;
        }
    }
}
