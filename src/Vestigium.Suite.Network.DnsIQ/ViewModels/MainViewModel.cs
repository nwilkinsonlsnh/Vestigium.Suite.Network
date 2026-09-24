using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public BindFields Bind { get; } = new();

    [ObservableProperty]
    private string _name = "localhost";

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _log = string.Empty;

    [RelayCommand]
    private async Task RunLookupAsync()
    {
        Status = "Running";
        try
        {
            var result = await NetworkHelper.LookupAsync(Name, new DnsLookupOptions
            {
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            }).ConfigureAwait(true);
            Status = result.Rcode.ToString();
            Log = string.Join(Environment.NewLine, result.Answers.Select(a => $"{a.Type} {a.Name} {a.Data}"));
            if (string.IsNullOrWhiteSpace(Log))
                Log = result.Rcode.ToString();
        }
        catch (Exception ex)
        {
            Status = "Failed";
            Log = ex.Message;
        }
    }
}
