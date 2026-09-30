namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class MonitorFactColumn
{
    public MonitorFactColumn(params MonitorDetailRow[] rows)
    {
        Rows = rows;
    }

    public IReadOnlyList<MonitorDetailRow> Rows { get; }
}
