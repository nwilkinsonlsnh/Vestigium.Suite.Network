using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Helpers.PerfMon;

namespace Vestigium.Suite.Network.NicIQ.ViewModels;

public sealed partial class MonitorSampleRow : ObservableObject
{
    public MonitorSampleRow(SampleRecord source)
    {
        Apply(source);
    }

    public SampleRecord Source { get; private set; } = null!;

    [ObservableProperty]
    private string _counter = string.Empty;

    [ObservableProperty]
    private string _instance = string.Empty;

    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _value = "\u2014";

    [ObservableProperty]
    private string _utc = string.Empty;

    public void Apply(SampleRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Source = source;
        Counter = source.Counter;
        Instance = source.Instance;
        Unit = source.Unit;
        Status = source.Status.ToString();
        Value = source.Value is double number
            ? number.ToString("0.###", CultureInfo.InvariantCulture)
            : "\u2014";
        Utc = source.Utc.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }
}
