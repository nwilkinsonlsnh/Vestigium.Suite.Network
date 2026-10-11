using System.Collections.ObjectModel;
using System.Windows;
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
    public const int DefaultParallel = 10;
    public const int DefaultProbes = 5;
    public const int MinHops = 1;
    public const int MaxHopsLimit = 64;
    public const int MinProbes = 1;
    public const int MaxParallel = 30;
    public const int MaxProbes = 5;

    public BindFields Bind { get; } = new();

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public IReadOnlyList<string> Families { get; } = ["All", "IPv4", "IPv6"];

    public IReadOnlyList<AdapterChoice> Interfaces { get; }

    public ObservableCollection<HopRow> Hops { get; } = [];

    private CancellationTokenSource? _cts;
    private int _reachedTtl;
    private string? _resolved;
    private int _estimate;
    private readonly Dictionary<string, int> _seen = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<int> _pinging = [];

    public SettingsViewModel Settings { get; }

    public MainViewModel(SettingsViewModel settings)
    {
        Settings = settings;
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
    private MruEntry? _selectedMru;

    partial void OnTargetChanged(string value) => PinCommand.NotifyCanExecuteChanged();

    public bool IsPinned => Settings.IsSticky(Target);

    public string PinGlyph => IsPinned ? "Unpin" : "Pin";

    private bool CanPin() => Settings.CanPin(Target);

    [RelayCommand(CanExecute = nameof(CanPin))]
    private void Pin()
    {
        Settings.Pin(Target);
        OnPropertyChanged(nameof(IsPinned));
        OnPropertyChanged(nameof(PinGlyph));
        PinCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RunTraceCommand))]
    private string _target = "127.0.0.1";

    [ObservableProperty]
    private decimal _maxHops = DefaultMaxHops;

    [ObservableProperty]
    private decimal _parallel = DefaultParallel;

    [ObservableProperty]
    private decimal _probes = DefaultProbes;

    [ObservableProperty]
    private int _probeColumns = DefaultProbes;

    [ObservableProperty]
    private string _family = "All";

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
        var remembered = Target.Trim();
        Hops.Clear();
        _reachedTtl = 0;
        _resolved = null;
        _estimate = 0;
        _seen.Clear();
        _pinging.Clear();
        ProbeColumns = (int)Settings.Probes;
        Settings.Remember(remembered);
        if (!string.Equals(Target, remembered, StringComparison.Ordinal))
            Target = remembered;
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
            job.ProgressChanged += (_, p) => Post(p);
            var result = await job.RunAsync(token).ConfigureAwait(true);
            _resolved = result.ResolvedAddress ?? _resolved;
            FillFinal(result.Hops.Select(HopRow.From));
            TrimPastTarget(result.ResolvedAddress);
            SettleGaps();
            ProgressMax = Math.Max(ProgressMax, Hops.Count);
            Progress = ProgressMax;
            Reached = result.Reached ? "Yes" : "No";
            Protocol = result.ProbeProtocol.ToString();
            SetStatus(result.Status.ToString());
            IsBusy = false;
            _ = PingAfterAsync(token);
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
            var reply = result.Replies.FirstOrDefault(r => r.Status == IcmpEchoStatus.Success);
            _resolved = result.ResolvedAddress ?? reply?.Address;
            _estimate = HopEstimate.FromReplyTtl(reply?.Ttl ?? 0);
            return _estimate;
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

    private static (string Address, List<string> Times) Split(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || status == "Reached")
            return ("*", []);
        var parts = status.Split('|', 2);
        var address = string.IsNullOrWhiteSpace(parts[0]) ? "*" : parts[0];
        if (parts.Length < 2 || address == "*")
            return (address, []);
        var times = parts[1]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => long.TryParse(t, out var ms) && ms > 0 ? ms.ToString() : "<1 ms")
            .ToList();
        return (address, times);
    }

    private void Post(NetworkProgress progress)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            OnHop(progress);
            return;
        }

        dispatcher.BeginInvoke(() => OnHop(progress));
    }

    private void OnHop(NetworkProgress progress)
    {
        if (progress.Sequence < 1)
            return;
        if (_reachedTtl > 0 && progress.Sequence > _reachedTtl)
            return;
        var (address, times) = Split(progress.LastStatus);
        if (times.Count == 0 && progress.LastRoundtripMs is > 0)
            times = [progress.LastRoundtripMs.Value.ToString()];
        if (address != "*" && _seen.TryGetValue(address, out var first) && progress.Sequence != first)
        {
            _reachedTtl = Math.Min(first, progress.Sequence);
            TrimPastTarget();
            return;
        }

        if (SameHost(address))
            _reachedTtl = _reachedTtl == 0 ? progress.Sequence : Math.Min(_reachedTtl, progress.Sequence);
        if (_reachedTtl > 0 && progress.Sequence > _reachedTtl)
            return;
        var isTarget = SameHost(address);
        if (!isTarget && _estimate > 0 && progress.Sequence > _estimate + 1)
            return;
        if (address != "*")
            _seen[address] = progress.Sequence;

        var probes = address == "*" ? "No reply" : "";
        var row = new HopRow { Ttl = progress.Sequence, Address = address, Name = "", Probes = probes };
        if (times.Count > 0)
            row = row.WithTimes(times);
        Place(row);
        if (address != "*" && _pinging.Add(progress.Sequence))
            _ = PingOneAsync(progress.Sequence, address);
        if (_reachedTtl > 0)
            TrimPastTarget();
        Progress = Hops.Count(h => h.Probes != "Waiting");
        if (Hops.Count > ProgressMax)
            ProgressMax = Hops.Count;
    }

    private bool SameHost(string address)
        => !string.IsNullOrWhiteSpace(address)
           && !address.Equals("*", StringComparison.Ordinal)
           && (address.Equals(Target.Trim(), StringComparison.OrdinalIgnoreCase)
               || Same(address, _resolved));

    private async Task PingAfterAsync(CancellationToken token)
    {
        try
        {
            await PingDiscoveredAsync(token).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
        }
    }

    private async Task FillHolesAsync(CancellationToken token)
    {
        if (!Hops.Any(h => h.Address == "*"))
            return;

        SetStatus("Filling hops");
        if (!TryBuild(out var options, out _))
            return;
        options.ParallelHops = 1;
        options.ProbesPerHop = 1;
        SetFanOut(options, 1);
        var job = NetworkHelper.IcmpTrace(Target.Trim(), options);
        var result = await job.RunAsync(token).ConfigureAwait(true);
        foreach (var hop in result.Hops)
        {
            if (string.IsNullOrWhiteSpace(hop.Address))
                continue;
            var index = hop.Ttl - 1;
            if (index < 0 || index >= Hops.Count || Hops[index].Address != "*")
                continue;
            Hops[index] = HopRow.From(hop);
        }

        TrimPastTarget(result.ResolvedAddress);
    }

    private async Task PingDiscoveredAsync(CancellationToken token)
    {
        var count = Math.Clamp((int)Settings.Probes, MinProbes, MaxProbes);
        var jobs = Hops
            .Select((row, index) => (row, index))
            .Where(x => x.row.Address != "*" && !_pinging.Contains(x.row.Ttl))
            .Select(async x =>
            {
                var times = await PingAsync(x.row.Address, count, token).ConfigureAwait(true);
                Hops[x.index] = x.row.WithTimes(times);
            });
        await Task.WhenAll(jobs).ConfigureAwait(true);
    }

    private async Task PingOneAsync(int ttl, string address)
    {
        try
        {
            var count = Math.Clamp((int)Settings.Probes, MinProbes, MaxProbes);
            var times = await PingAsync(address, count, CancellationToken.None).ConfigureAwait(true);
            var index = ttl - 1;
            if (index < 0 || index >= Hops.Count || Hops[index].Ttl != ttl)
                return;
            Hops[index] = Hops[index].WithTimes(times);
        }
        catch (Exception)
        {
        }
    }

    private async Task<IReadOnlyList<string>> PingAsync(string address, int count, CancellationToken token)
    {
        try
        {
            var echo = NetworkHelper.IcmpEcho(address, new IcmpEchoOptions
            {
                Count = count,
                Timeout = TimeSpan.FromSeconds(2),
                InterfaceIndex = Bind.InterfaceIndex,
                SourceAddress = Bind.SourceAddress
            });
            var result = await echo.RunAsync(token).ConfigureAwait(true);
            return result.Replies.Take(count).Select(Time).ToArray();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return Enumerable.Repeat("*", count).ToArray();
        }
    }

    private static string Time(IcmpEchoReply reply)
    {
        if (reply.Status != IcmpEchoStatus.Success)
            return "*";
        return reply.RoundtripTimeMs > 0 ? reply.RoundtripTimeMs.ToString() : "<1 ms";
    }

    private void FillFinal(IEnumerable<HopRow> rows)
    {
        foreach (var row in rows.OrderBy(r => r.Ttl))
            PlaceFinal(row);
        SettleGaps();
    }

    private void PlaceFinal(HopRow row)
    {
        if (_reachedTtl > 0 && row.Ttl > _reachedTtl && !row.Reached && !SameHost(row.Address))
            return;
        while (Hops.Count < row.Ttl)
            Hops.Add(Gap(Hops.Count + 1));
        var index = row.Ttl - 1;
        if (index < Hops.Count && Hops[index].Ttl == row.Ttl)
            Hops[index] = row;
        else
            Hops.Add(row);
        if (row.Reached || SameHost(row.Address))
            _reachedTtl = _reachedTtl == 0 ? row.Ttl : Math.Min(_reachedTtl, row.Ttl);
    }

    private void Fill(IEnumerable<HopRow> rows)
    {
        foreach (var row in rows)
            Place(row);
        SettleGaps();
    }

    private void Place(HopRow row)
    {
        if (_reachedTtl > 0 && row.Ttl > _reachedTtl)
            return;
        var isTarget = SameHost(row.Address);
        if (!isTarget && _estimate > 0 && row.Ttl > _estimate + 1)
            return;
        while (Hops.Count < row.Ttl)
            Hops.Add(Gap(Hops.Count + 1));

        var index = row.Ttl - 1;
        if (index < Hops.Count && Hops[index].Ttl == row.Ttl)
            Hops[index] = row;
        else
            Hops.Add(row);
    }

    private void TrimPastTarget(string? resolved = null)
    {
        var end = Hops.FirstOrDefault(h => h.Reached || SameHost(h.Address) || Same(h.Address, resolved));
        if (end is null)
            return;
        _reachedTtl = end.Ttl;
        while (Hops.Count > end.Ttl)
            Hops.RemoveAt(Hops.Count - 1);
    }

    private static bool Same(string address, string? other)
        => !string.IsNullOrWhiteSpace(other)
           && address.Equals(other.Trim(), StringComparison.OrdinalIgnoreCase);

    private void SettleGaps()
    {
        for (var i = 0; i < Hops.Count; i++)
        {
            if (Hops[i].Probes == "Waiting")
                Hops[i] = Gap(Hops[i].Ttl, "No reply");
        }
    }

    private static HopRow Gap(int ttl, string probes = "Waiting")
        => HopRow.Gap(ttl, probes);

    private bool TryBuild(out IcmpTraceOptions options, out string? reject)
    {
        options = new IcmpTraceOptions();
        reject = null;
        if (string.IsNullOrWhiteSpace(Target))
        {
            reject = "Target is required";
            return false;
        }

        var hops = (int)Settings.MaxHops;
        if (hops < MinHops || hops > MaxHopsLimit)
        {
            reject = "Max hops is 1–64";
            return false;
        }

        var parallel = (int)Settings.Parallel;
        if (parallel < 1 || parallel > MaxParallel)
        {
            reject = "Parallel is 1–30";
            return false;
        }

        var probes = (int)Settings.Probes;
        if (probes < MinProbes || probes > MaxProbes)
        {
            reject = "Probes is 1–5";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(Settings.SelectedInterfaceId)
            && Settings.Adapters.All(a => !a.Id.Equals(Settings.SelectedInterfaceId, StringComparison.OrdinalIgnoreCase)))
        {
            reject = "That interface is not on this PC";
            return false;
        }

        var source = Settings.Source?.Trim();
        if (!string.IsNullOrWhiteSpace(source) && !IPAddress.TryParse(source, out _))
        {
            reject = "Source must be an IP address";
            return false;
        }

        Bind.InterfaceIndex = InterfaceIndexOf(Settings.SelectedInterfaceId) ?? 0;
        Bind.SourceAddress = string.IsNullOrWhiteSpace(source) ? null : source;
        options = new IcmpTraceOptions
        {
            MaxHops = hops,
            ProbesPerHop = probes,
            Family = MapFamily(Settings.Family),
            InterfaceIndex = Bind.InterfaceIndex,
            SourceAddress = Bind.SourceAddress
        };
        SetFanOut(options, parallel);
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

