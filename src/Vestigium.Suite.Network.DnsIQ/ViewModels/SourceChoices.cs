using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record SourceChoice(string? Address, string Label);

public static class SourceChoices
{
    public static SourceChoice Any { get; } = new(null, "Any");

    public static IReadOnlyList<SourceChoice> From(
        IReadOnlyList<NetworkAdapter> adapters,
        int interfaceIndex = 0)
    {
        var list = new List<SourceChoice> { Any };
        if (adapters is null || adapters.Count == 0)
            return list;

        IEnumerable<NetworkAdapter> set = adapters;
        if (interfaceIndex > 0 && interfaceIndex <= adapters.Count)
            set = [adapters[interfaceIndex - 1]];
        else if (interfaceIndex > adapters.Count)
            return list;

        foreach (var adapter in set)
        {
            var name = string.IsNullOrWhiteSpace(adapter.Name) ? adapter.Id : adapter.Name;
            foreach (var address in adapter.UnicastAddresses)
            {
                if (string.IsNullOrWhiteSpace(address.Address))
                    continue;
                list.Add(new SourceChoice(address.Address, $"{address.Address}  {name}"));
            }
        }

        return list;
    }

    public static SourceChoice Resolve(IReadOnlyList<SourceChoice> items, string? persisted)
    {
        if (string.IsNullOrWhiteSpace(persisted) || items is null || items.Count == 0)
            return Any;

        return items.FirstOrDefault(i => i.Address == persisted) ?? Any;
    }
}
