using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record AdapterChoice(int Index, string Id, string Name, string Label);

public static class AdapterChoices
{
    public static AdapterChoice Any { get; } = new(0, string.Empty, "Any", "Any (0)");

    public static IReadOnlyList<AdapterChoice> From(IReadOnlyList<NetworkAdapter> adapters)
    {
        var list = new List<AdapterChoice> { Any };
        if (adapters is null)
            return list;

        var index = 1;
        foreach (var adapter in adapters)
        {
            var name = string.IsNullOrWhiteSpace(adapter.Name) ? adapter.Id : adapter.Name;
            list.Add(new AdapterChoice(index, adapter.Id, name, $"{name}  ({index})"));
            index++;
        }

        return list;
    }
}
