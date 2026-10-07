using System.Text.Json;
using Vestigium.Helpers.LogParser;
using Vestigium.Helpers.LogParser.Har;
using Vestigium.Helpers.LogParser.Url;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public static class CaptureLoader
{
    public static LogReadResult Load(string path)
    {
        if (path.EndsWith(".har", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return HarReader.ReadFile(path);
            }
            catch (JsonException)
            {
                return UrlReader.ReadFile(path);
            }
        }

        return UrlReader.ReadFile(path);
    }

    public static IReadOnlyList<LogHost> Unique(IReadOnlyList<LogHost> hosts)
    {
        var order = new List<string>();
        var map = new Dictionary<string, Bucket>(StringComparer.OrdinalIgnoreCase);
        foreach (var host in hosts)
        {
            if (!map.TryGetValue(host.Host, out var row))
            {
                row = new Bucket(host.Host, host.IsAddress);
                map.Add(host.Host, row);
                order.Add(host.Host);
            }
            row.Hits += host.HitCount;
            row.Sources |= host.Sources;
            foreach (var port in host.Ports)
                row.Ports.Add(port);
        }

        var unique = new LogHost[order.Count];
        for (var i = 0; i < order.Count; i++)
        {
            var row = map[order[i]];
            unique[i] = new LogHost(row.Host, row.Ports, row.Hits, row.Sources, row.IsAddress);
        }
        return unique;
    }

    private sealed class Bucket(string host, bool isAddress)
    {
        public string Host { get; } = host;
        public bool IsAddress { get; } = isAddress;
        public int Hits { get; set; }
        public LogHostSource Sources { get; set; }
        public SortedSet<int> Ports { get; } = [];
    }
}
