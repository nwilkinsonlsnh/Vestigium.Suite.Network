namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class MonitorDetailRow
{
    public MonitorDetailRow(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }

    public string Value { get; }
}
