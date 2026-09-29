using System.Globalization;
using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed class MonitorSampleRow
{
    public MonitorSampleRow(SampleRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Source = source;
    }

    public SampleRecord Source { get; }

    public string Counter => Source.Counter;

    public string Instance => Source.Instance;

    public string Unit => Source.Unit;

    public string Status => Source.Status.ToString();

    public string Value => Source.Value is double number
        ? number.ToString("0.###", CultureInfo.InvariantCulture)
        : "—";

    public string Utc => Source.Utc.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
}
