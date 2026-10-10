using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.Shell;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    public const int DefaultMaxHops = 30;
    public const int DefaultProbes = 1;
    public const int MinHops = 1;
    public const int MaxHopsLimit = 64;
    public const int MinProbes = 1;
    public const int MaxProbes = 10;

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public IReadOnlyList<string> Families { get; } = ["All", "IPv4", "IPv6"];

    public IReadOnlyList<AdapterChoice> Interfaces { get; }

    private CancellationTokenSource? _cts;

    public MainViewModel()
    {
        try
        {
            Interfaces = AdapterChoices.From(NetworkHelper.GetAdapters());
        }
        catch (Exception)
        {
            Interfaces = AdapterChoices.From([]);
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunTraceCommand))]
    private string _target = "127.0.0.1";

    [ObservableProperty]
    private decimal _maxHops = DefaultMaxHops;

    [ObservableProperty]
    private decimal _probesPerHop = DefaultProbes;

    [ObservableProperty]
    private string _family = "All";

    [ObservableProperty]
    private int _selectedInterfaceIndex;

    [ObservableProperty]
    private string _source = string.Empty;

    [ObservableProperty]
    private string _status = "Idle";

    [ObservableProperty]
    private string _log = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunTraceCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    private bool CanTrace() => !IsBusy && !string.IsNullOrWhiteSpace(Target);
    private bool CanCancel() => IsBusy;

    [RelayCommand(CanExecute = nameof(CanTrace))]
    private async Task RunTraceAsync()
    {
        if (!TryBuild(out var options, out var reject))
        {
            SetStatus(reject ?? "Failed");
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        IsBusy = true;
        SetStatus("Running");
        try
        {
            var job = NetworkHelper.IcmpTrace(Target.Trim(), options);
            var result = await job.RunAsync(token).ConfigureAwait(true);
            SetStatus(result.Status.ToString());
            Log = string.Join(Environment.NewLine, result.Hops.Select(h => $"{h.Ttl,2}  {h.Address ?? "*"}"));
        }
        catch (OperationCanceledException)
        {
            SetStatus("Cancelled");
        }
        catch (Exception ex)
        {
            SetStatus("Failed");
            Log = ex.Message;
        }
        finally
        {
            IsBusy = false;
            _cts.Dispose();
            _cts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
    }

    private bool TryBuild(out IcmpTraceOptions options, out string? reject)
    {
        options = new IcmpTraceOptions();
        reject = null;
        if (string.IsNullOrWhiteSpace(Target))
        {
            reject = "Target is required";
            return false;
        }

        var hops = (int)MaxHops;
        if (hops < MinHops || hops > MaxHopsLimit)
        {
            reject = "Max hops is 1–64";
            return false;
        }

        var probes = (int)ProbesPerHop;
        if (probes < MinProbes || probes > MaxProbes)
        {
            reject = "Probes per hop is 1–10";
            return false;
        }

        if (SelectedInterfaceIndex < 0)
        {
            reject = "Interface index cannot be negative";
            return false;
        }

        var source = Source?.Trim();
        if (!string.IsNullOrWhiteSpace(source) && !IPAddress.TryParse(source, out _))
        {
            reject = "Source must be an IP address";
            return false;
        }

        Bind.InterfaceIndex = SelectedInterfaceIndex;
        Bind.SourceAddress = string.IsNullOrWhiteSpace(source) ? null : source;
        options = new IcmpTraceOptions
        {
            MaxHops = hops,
            ProbesPerHop = probes,
            Family = MapFamily(Family),
            InterfaceIndex = Bind.InterfaceIndex,
            SourceAddress = Bind.SourceAddress
        };
        return true;
    }

    private static RouteFamily MapFamily(string family)
        => family switch
        {
            "IPv4" => RouteFamily.Pv4,
            "IPv6" => RouteFamily.Pv6,
            _ => RouteFamily.All
        };

    private void SetStatus(string value)
    {
        Status = value;
        if (StatusBar is not null)
            StatusBar.Message = value;
    }
}

public sealed record AdapterChoice(int Index, string Label);

public static class AdapterChoices
{
    public static AdapterChoice Any { get; } = new(0, "Any (0)");

    public static IReadOnlyList<AdapterChoice> From(IReadOnlyList<NetworkAdapter> adapters)
    {
        var list = new List<AdapterChoice> { Any };
        if (adapters is null)
            return list;

        var index = 1;
        foreach (var adapter in adapters)
        {
            var name = string.IsNullOrWhiteSpace(adapter.Name) ? adapter.Id : adapter.Name;
            list.Add(new AdapterChoice(index, $"{name}  ({index})"));
            index++;
        }

        return list;
    }
}
