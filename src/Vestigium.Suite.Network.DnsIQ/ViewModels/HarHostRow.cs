using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.LogParser;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class HarHostRow : ObservableObject
{
    public HarHostRow(LogHost host)
    {
        Host = host.Host;
        Ports = host.Ports.Count == 0 ? "" : string.Join(", ", host.Ports.OrderBy(p => p));
        Hits = host.HitCount;
        Sources = host.Sources.ToString();
        IsAddress = host.IsAddress;
        Error = host.Error;
    }

    public string Host { get; }

    public string Ports { get; }

    public int Hits { get; }

    public string Sources { get; }

    [ObservableProperty]
    private string _dns = "";

    [ObservableProperty]
    private string _answers = "";

    [ObservableProperty]
    private string _error = "";

    public bool IsAddress { get; }

    public IReadOnlyList<string> AnswerItems { get; private set; } = [];

    public void SetAnswerItems(IReadOnlyList<string> items)
        => AnswerItems = items ?? [];

    partial void OnAnswersChanged(string value)
    {
        AnswerItems = string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(", ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
