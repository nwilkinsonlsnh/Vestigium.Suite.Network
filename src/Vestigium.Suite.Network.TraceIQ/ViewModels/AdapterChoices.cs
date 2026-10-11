using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed record AdapterChoice(string Id, string Label);

public static class AdapterChoices
{
    public static AdapterChoice Any { get; } = new("", "Any");

    public static IReadOnlyList<AdapterChoice> From(IReadOnlyList<NetworkAdapter> adapters)
    {
        var list = new List<AdapterChoice> { Any };
        if (adapters is null)
            return list;

        foreach (var adapter in adapters)
        {
            if (string.IsNullOrWhiteSpace(adapter.Id))
                continue;
            var name = string.IsNullOrWhiteSpace(adapter.Name) ? adapter.Description : adapter.Name;
            var index = adapter.InterfaceIndex?.ToString() ?? "-";
            list.Add(new AdapterChoice(adapter.Id, $"{name}  ({index})"));
        }

        return list;
    }
}
