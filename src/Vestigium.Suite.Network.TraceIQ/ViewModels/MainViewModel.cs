using System.Collections.ObjectModel;
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

    public ObservableCollection<HopRow> Hops { get; } = [];

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
    private string _reached = "";

    [ObservableProperty]
    private string _protocol = "";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private double _progressMax = 1;

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
        Hops.Clear();
        Reached = "";
        Protocol = "";
        Progress = 0;
        ProgressMax = 1;
        SetStatus("Estimating");
        try
        {
            var estimate = await EstimateAsync(token).ConfigureAwait(true);
            ProgressMax = estimate;
            SetStatus("Running");

            var job = NetworkHelper.IcmpTrace(Target.Trim(), options);
            job.ProgressChanged += (_, p) => OnHop(p);
            var result = await job.RunAsync(token).ConfigureAwait(true);
            Fill(result.Hops.Select(HopRow.From));
            SettleGaps();
            ProgressMax = Math.Max(ProgressMax, Hops.Count);
            Progress = ProgressMax;
            Reached = result.Reached ? "Yes" : "No";
            Protocol = result.ProbeProtocol.ToString();
            SetStatus(result.Status.ToString());
        }
        catch (OperationCanceledException)
        {
            SetStatus("Cancelled");
        }
        catch (Exception ex)
        {
            SetStatus(string.IsNullOrWhiteSpace(ex.Message) ? "Failed" : ex.Message);
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

    private async Task<int> EstimateAsync(CancellationToken token)
    {
        try
        {
            var echo = NetworkHelper.IcmpEcho(Target.Trim(), new IcmpEchoOptions
            {
                Count = 1,
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            });
            var result = await echo.RunAsync(token).ConfigureAwait(true);
            var ttl = result.Replies.FirstOrDefault(r => r.Status == IcmpEchoStatus.Success)?.Ttl ?? 0;
            return HopEstimate.FromReplyTtl(ttl);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return (int)MaxHops;
        }
    }

    private void OnHop(NetworkProgress progress)
    {
        if (progress.Sequence < 1)
            return;
        var address = string.IsNullOrWhiteSpace(progress.LastStatus) || progress.LastStatus == "Reached"
            ? "*"
            : progress.LastStatus;
        var probes = address == "*" ? "No reply" : "";
        Place(new HopRow { Ttl = progress.Sequence, Address = address, Name = "", Probes = probes });
        Progress = Hops.Count(h => h.Probes != "Waiting");
        if (Hops.Count > ProgressMax)
            ProgressMax = Hops.Count;
    }

    private void Fill(IEnumerable<HopRow> rows)
    {
        foreach (var row in rows)
            Place(row);
        SettleGaps();
    }

    private void Place(HopRow row)
    {
        while (Hops.Count < row.Ttl)
            Hops.Add(Gap(Hops.Count + 1));

        var index = row.Ttl - 1;
        if (index < Hops.Count && Hops[index].Ttl == row.Ttl)
            Hops[index] = row;
        else
            Hops.Add(row);
    }

    private void SettleGaps()
    {
        for (var i = 0; i < Hops.Count; i++)
        {
            if (Hops[i].Probes == "Waiting")
                Hops[i] = Gap(Hops[i].Ttl, "No reply");
        }
    }

    private static HopRow Gap(int ttl, string probes = "Waiting")
        => new() { Ttl = ttl, Address = "*", Name = "", Probes = probes };

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
            ProbesPerHop = 1,
            Family = MapFamily(Family),
            InterfaceIndex = Bind.InterfaceIndex,
            SourceAddress = Bind.SourceAddress
        };
        SetFanOut(options, probes);
        return true;
    }

    private static void SetFanOut(IcmpTraceOptions options, int width)
    {
        var fan = options.GetType().GetProperty("ParallelHops");
        if (fan is null || !fan.CanWrite)
            return;
        fan.SetValue(options, width);
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
        if (StatusBar is not null)
            StatusBar.Message = value;
    }
}

public static class HopEstimate
{
    public static int FromReplyTtl(int replyTtl)
    {
        if (replyTtl <= 0)
            return 8;
        var initial = replyTtl <= 64 ? 64 : replyTtl <= 128 ? 128 : 255;
        var hops = initial - replyTtl;
        return Math.Clamp(hops < 1 ? 1 : hops, 1, 64);
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

        foreach (var adapter in adapters)
        {
            var index = adapter.InterfaceIndex ?? 0;
            if (index < 1)
                continue;
            var name = string.IsNullOrWhiteSpace(adapter.Name) ? adapter.Id : adapter.Name;
            list.Add(new AdapterChoice(index, $"{name}  ({index})"));
        }

        return list;
    }
}
