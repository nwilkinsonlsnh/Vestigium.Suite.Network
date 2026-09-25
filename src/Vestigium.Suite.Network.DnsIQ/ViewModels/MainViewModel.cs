using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public BindFields Bind { get; } = new();

    public IReadOnlyList<string> RecordTypes => DnsIqInput.RecordTypes;

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
        if (!DnsIqInput.TryCreate(
                Name,
                Server,
                RecordType,
                Bind.InterfaceIndex,
                Bind.SourceAddress,
                out var query,
                out var reject))
        {
            Status = reject ?? "Failed";
            return;
        }

        Status = "Running";
        Answers.Clear();
        try
        {
            var result = await NetworkHelper.LookupAsync(query!.Name, query.Options).ConfigureAwait(true);
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
