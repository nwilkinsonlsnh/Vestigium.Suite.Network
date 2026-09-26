using System.Net;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed record PingIqQuery(string Target, IcmpEchoOptions Options);

public static class PingIqInput
{
    public const int DefaultCount = 4;
    public const int MinCount = 1;
    public const int MaxCount = 60;
    public const int DefaultTimeoutMs = 4000;
    public const int MinTimeoutMs = IcmpEchoOptions.MinTimeoutMs;
    public const int MaxTimeoutMs = IcmpEchoOptions.MaxTimeoutMs;

    public static bool TryCreate(
        string? target,
        decimal count,
        decimal timeoutMs,
        int interfaceIndex,
        string? sourceAddress,
        out PingIqQuery? query,
        out string? reject)
    {
        query = null;
        reject = null;

        var trimmed = target?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            reject = "Target is required.";
            return false;
        }

        var n = (int)decimal.Truncate(count);
        if (n is < MinCount or > MaxCount)
        {
            reject = $"Count must be {MinCount}–{MaxCount}.";
            return false;
        }

        var ms = (int)decimal.Truncate(timeoutMs);
        if (ms is < MinTimeoutMs or > MaxTimeoutMs)
        {
            reject = $"Timeout must be {MinTimeoutMs}–{MaxTimeoutMs} ms.";
            return false;
        }

        if (interfaceIndex < 0)
        {
            reject = "Interface index cannot be negative.";
            return false;
        }

        var source = sourceAddress?.Trim();
        if (string.IsNullOrEmpty(source))
            source = null;
        else if (!IPAddress.TryParse(source, out _))
        {
            reject = "Source must be an IPv4 or IPv6 address.";
            return false;
        }

        query = new PingIqQuery(
            trimmed,
            new IcmpEchoOptions
            {
                Count = n,
                Timeout = TimeSpan.FromMilliseconds(ms),
                InterfaceIndex = interfaceIndex,
                SourceAddress = source
            });
        return true;
    }
}
