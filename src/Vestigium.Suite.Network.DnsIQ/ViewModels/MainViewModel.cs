using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public BindFields Bind { get; } = new();

    public IReadOnlyList<string> RecordTypes { get; } =
    [
        "A", "AAAA", "CNAME", "MX", "NS", "PTR", "TXT", "SOA"
    ];

    public ObservableCollection<AnswerRow> Answers { get; } = [];

    [ObservableProperty]
    private string _name = "localhost";

    [ObservableProperty]
    private string _server = string.Empty;

    [ObservableProperty]
    private string _recordType = "A";

    [ObservableProperty]
    private string _status = "Idle";

    [RelayCommand]
    private async Task LookupAsync()
    {
        Status = "Running";
        Answers.Clear();
        try
        {
            var result = await NetworkHelper.LookupAsync(Name, new DnsLookupOptions
            {
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            }).ConfigureAwait(true);
            Status = result.Rcode.ToString();
            foreach (var answer in result.Answers)
            {
                Answers.Add(new AnswerRow(
                    answer.Type.ToString(),
                    answer.Name,
                    answer.Data,
                    answer.Ttl));
            }
        }
        catch (Exception ex)
        {
            Status = "Failed";
            Answers.Clear();
            Status = string.IsNullOrWhiteSpace(ex.Message) ? "Failed" : $"Failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task ProbeAsync() => Task.CompletedTask;

    [RelayCommand]
    private void Cancel()
    {
    }
}
