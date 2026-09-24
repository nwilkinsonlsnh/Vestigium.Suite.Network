using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

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
    private async Task RunEchoAsync()
    {
        Status = "Running";
        try
        {
            var job = NetworkHelper.IcmpEcho(Target, new IcmpEchoOptions
            {
                Count = 4,
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            });
            var result = await job.RunAsync().ConfigureAwait(true);
            Status = result.Status.ToString();
            Log = $"{result.Status} sent={result.Sent} recv={result.Received} lost={result.Lost} avg={result.AverageMs} ms";
        }
        catch (Exception ex)
        {
            Status = "Failed";
            Log = ex.Message;
        }
    }
}
