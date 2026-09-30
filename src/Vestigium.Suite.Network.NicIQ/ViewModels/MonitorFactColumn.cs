namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class MonitorFactColumn
{
    public MonitorFactColumn(IReadOnlyList<MonitorDetailRow> rows)
    {
        Rows = rows;
    }

    public IReadOnlyList<MonitorDetailRow> Rows { get; }
}
