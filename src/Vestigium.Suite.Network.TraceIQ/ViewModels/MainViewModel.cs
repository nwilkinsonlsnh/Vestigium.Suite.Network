using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public BindFields Bind { get; } = new();

    [ObservableProperty]
    private string _target = "127.0.0.1";

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _log = string.Empty;

    [RelayCommand]
    private async Task RunTraceAsync()
    {
        Status = "Running";
        try
        {
            var job = NetworkHelper.IcmpTrace(Target, new IcmpTraceOptions
            {
                MaxHops = 8,
                ProbesPerHop = 1,
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            });
            var result = await job.RunAsync().ConfigureAwait(true);
            Status = result.Status.ToString();
            Log = string.Join(Environment.NewLine, result.Hops.Select(h => $"{h.Ttl,2}  {h.Address ?? "*"}"));
        }
        catch (Exception ex)
        {
            Status = "Failed";
            Log = ex.Message;
        }
    }
}
