using System.Net;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public static class RouteIqInput
{
    public static bool TryMapFamily(string? label, out RouteFamily family, out string? reason)
    {
        switch (label?.Trim())
        {
            case "All":
                family = RouteFamily.All;
                reason = null;
                return true;
            case "IPv4":
                family = RouteFamily.Pv4;
                reason = null;
                return true;
            case "IPv6":
                family = RouteFamily.Pv6;
                reason = null;
                return true;
            default:
                family = RouteFamily.All;
                reason = "Family must be All, IPv4, or IPv6.";
                return false;
        }
    }

    public static bool TryParseProbe(string? text, out IPAddress? address, out string? reason)
    {
        var trimmed = text?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            address = null;
            reason = null;
            return false;
        }

        if (!IPAddress.TryParse(trimmed, out var parsed))
        {
            address = null;
            reason = "Address must be a single IPv4 or IPv6 address.";
            return false;
        }

        address = parsed;
        reason = null;
        return true;
    }
}
