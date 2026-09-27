namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public static class ProbeGates
{
    public static string Format(TimeSpan start, TimeSpan end, TimeSpan lastSend, TimeSpan lastPacket)
    {
        var wait = lastPacket > end ? lastPacket - end : TimeSpan.Zero;
        return $"Start gate {Stamp(start)}   End gate {Stamp(end)}   Last send {Stamp(lastSend)}   Last packet {Stamp(lastPacket)}   Return wait {Stamp(wait)}";
    }

    private static string Stamp(TimeSpan value)
        => $"{value.TotalSeconds:0.000}s";
}
