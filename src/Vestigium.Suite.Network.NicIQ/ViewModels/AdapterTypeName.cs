using System.Net.NetworkInformation;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

/// <summary>
/// .NET <see cref="NetworkInterfaceType"/> does not name every IANA ifType.
/// Windows still reports those numbers, so ToString() becomes "53".
/// </summary>
internal static class AdapterTypeName
{
    public static string Format(NetworkInterfaceType type)
    {
        if (Enum.IsDefined(type))
            return type.ToString();

        return (int)type switch
        {
            53 => "Virtual",
            54 => "Multiplexor",
            _ => $"Other ({(int)type})"
        };
    }
}
