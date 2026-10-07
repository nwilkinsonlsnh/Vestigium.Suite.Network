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
}
