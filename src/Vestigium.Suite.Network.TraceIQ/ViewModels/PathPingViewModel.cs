using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Controls.StatusBar;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.TraceIQ.ViewModels;

public partial class PathPingViewModel : ObservableObject
{
    private readonly SettingsViewModel _settings;
    private CancellationTokenSource? _cts;
    private readonly Stopwatch _elapsed = new();
    private readonly DispatcherTimer _ticker;
    private int _done;
    private int _total;

    public PathPingViewModel(SettingsViewModel settings)
    {
        _settings = settings;
        _settings.PropertyChanged += OnSettingsChanged;
        _ticker = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _ticker.Tick += (_, _) => SetElapsed(Format(_elapsed.Elapsed));
    }

    public event EventHandler? Completed;
    public VestigiumStatusBarViewModel? StatusBar { get; set; }
    public ObservableCollection<PathPingRow> Rows { get; } = [];

    [ObservableProperty] private string _target = "";
    [ObservableProperty] private bool _ready;
    [ObservableProperty] private bool _running;
    [ObservableProperty] private string _status = "Run a trace that reaches the target.";

    public string LossFill => _settings.LossFill;
    public string LossInk => _settings.LossInk;

    public bool CanRun => Ready && !Running && !string.IsNullOrWhiteSpace(Target);
    public bool CanCancel => Running;

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(SettingsViewModel.LossFill) or nameof(SettingsViewModel.LossInk))
        {
            OnPropertyChanged(nameof(LossFill));
            OnPropertyChanged(nameof(LossInk));
        }
    }

    public void Offer(string target, IReadOnlyList<HopRow> hops)
    {
        if (Running)
            return;
        var valid = hops.Where(h => !string.IsNullOrWhiteSpace(h.Address) && h.Address != "*").ToList();
        if (valid.Count == 0 || string.IsNullOrWhiteSpace(target))
            return;
        Target = target;
        Rows.Clear();
        foreach (var hop in valid)
            Rows.Add(new PathPingRow { Ttl = hop.Ttl, Address = hop.Address, Name = hop.Name, WalkMisses = Misses(hop) });
        Ready = true;
        Status = $"{Rows.Count} hops from the walk. Path Ping grades those hops.";
        Notify();
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task RunAsync()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        Running = true;
        Notify();
        var level = SaturationLevel.Find(_settings.PathSaturation);
        var interval = level.Level == "Custom"
            ? Math.Clamp(1000 / Math.Max(1, (int)_settings.PathPingsPerSecond), 1, 60000)
            : level.IntervalMs;
        var width = Math.Clamp((int)_settings.PathWidth, 1, 20);
        var seconds = Math.Clamp((int)_settings.PathSeconds, 1, 180);
        var dead = Math.Clamp((int)_settings.PathDeadAfter, 3, 30);
        var count = Math.Max(1, seconds * 1000 / interval);
        _done = 0;
        _total = Rows.Count * count;
        Status = $"Grading  0/{_total}";
        SetProgress(0);
        _elapsed.Restart();
        _ticker.Start();
        SetElapsed(Format(_elapsed.Elapsed));
        var gate = new SemaphoreSlim(width);
        try
        {
            var tasks = Rows.Select(row => GradeAsync(row, count, interval, dead, gate, token)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(true);
            if (token.IsCancellationRequested)
                Status = "Cancelled";
            else
            {
                Status = "Done";
                Completed?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
        }
        finally
        {
            _ticker.Stop();
            SetElapsed(Format(_elapsed.Elapsed));
            SetMessage("Idle");
            HideProgress();
            Running = false;
            Notify();
            _cts.Dispose();
            _cts = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cts?.Cancel();

    private async Task GradeAsync(PathPingRow row, int count, int intervalMs, int dead, SemaphoreSlim gate, CancellationToken token)
    {
        await gate.WaitAsync(token).ConfigureAwait(true);
        using var hop = CancellationTokenSource.CreateLinkedTokenSource(token);
        try
        {
            if (row.WalkMisses >= dead)
            {
                await Dispatch(() => row.Skip(row.WalkMisses)).ConfigureAwait(true);
                Tick(count);
                return;
            }

            // Count is the contract (rate × duration). Do not pass MaxDuration:
            // the engine waits for each reply before the interval, so a wall-clock
            // cap made slower hops send fewer packets. Dead After stops silent hops.
            var job = NetworkHelper.IcmpEcho(row.Address, new IcmpEchoOptions
            {
                Count = count,
                Interval = TimeSpan.FromMilliseconds(intervalMs),
                Timeout = TimeSpan.FromSeconds(2),
                AllowBurst = intervalMs < 200
            });
            job.ProgressChanged += (_, p) =>
            {
                Tick(1);
                var sample = p.LastStatus == nameof(IcmpEchoStatus.Success) ? p.LastRoundtripMs : (long?)null;
                _ = Dispatch(() => row.Note(p.Sent, p.Received, sample, p.LastStatus));
                if (p.Received == 0 && row.WalkMisses + p.Sent >= dead)
                    hop.Cancel();
            };
            var result = await job.RunAsync(hop.Token).ConfigureAwait(true);
            var skipped = result.Received == 0 && row.WalkMisses + result.Sent >= dead;
            await Dispatch(() => row.Apply(result, skipped)).ConfigureAwait(true);
            if (result.Sent < count)
                Tick(count - result.Sent);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            await Dispatch(row.Close).ConfigureAwait(true);
        }
        finally
        {
            gate.Release();
        }
    }

    private void Tick(int n)
    {
        var done = Interlocked.Add(ref _done, n);
        var total = Math.Max(1, _total);
        var pct = Math.Clamp(done * 100.0 / total, 0, 100);
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            Show(done, pct);
        else
            dispatcher.Invoke(() => Show(done, pct));
    }

    private void Show(int done, double pct)
    {
        Status = $"Grading  {Math.Min(done, _total)}/{_total}";
        SetMessage(Status);
        SetProgress(pct);
    }

    private void SetMessage(string value)
    {
        if (StatusBar is not null)
            StatusBar.Message = value;
    }

    private void SetProgress(double pct)
    {
        StatusBar?.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = pct,
            IsProgressVisible = true
        });
    }

    private void HideProgress()
    {
        StatusBar?.Engine.PostImmediate("progress", new StatusBarUpdate { IsProgressVisible = false, Progress = 0 });
    }

    private string _lastElapsed = "";

    public void Publish()
    {
        if (StatusBar is null)
            return;
        StatusBar.Message = Status;
        StatusBar.Engine.PostImmediate("elapsed", new StatusBarUpdate { Text = _lastElapsed });
    }

    private void SetElapsed(string value)
    {
        _lastElapsed = value;
        StatusBar?.Engine.PostImmediate("elapsed", new StatusBarUpdate { Text = value });
    }

    private static string Format(TimeSpan elapsed)
        => $"{(int)elapsed.TotalMinutes} min {elapsed.Seconds} sec {(int)(elapsed.TotalMilliseconds % 1000)} ms";

    private static int Misses(HopRow hop)
    {
        var cells = new[] { hop.P1, hop.P2, hop.P3, hop.P4, hop.P5, hop.P6, hop.P7, hop.P8, hop.P9, hop.P10 };
        return cells.Count(c => c == "*");
    }

    private static Task Dispatch(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return Task.CompletedTask;
        }
        return dispatcher.InvokeAsync(action).Task;
    }

    private void Notify()
    {
        OnPropertyChanged(nameof(CanRun));
        OnPropertyChanged(nameof(CanCancel));
        RunCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }
}

public partial class PathPingRow : ObservableObject
{
    private static readonly Brush SuccessBrush = Freeze("#22C55E");
    private static readonly Brush WarningBrush = Freeze("#EAB308");
    private static readonly Brush NoAnswerBrush = Freeze("#EF4444");
    private static readonly Brush RefusedBrush = Freeze("#F97316");
    private static readonly Brush SkippedBrush = Freeze("#6B7280");

    private bool _sawError;

    [ObservableProperty] private int _ttl;
    [ObservableProperty] private string _address = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private int _sent;
    [ObservableProperty] private int _received;
    [ObservableProperty] private string _loss = "";
    public bool Lossy => Received > 0 && Sent > Received;
    [ObservableProperty] private string _min = "";
    [ObservableProperty] private string _q1 = "";
    [ObservableProperty] private string _median = "";
    [ObservableProperty] private string _q3 = "";
    [ObservableProperty] private string _max = "";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private Brush _statusBrush = Brushes.Transparent;
    public int WalkMisses { get; init; }
    public IReadOnlyList<long> Samples { get; private set; } = [];

    public void Note(int sent, int received, long? sample, string? lastStatus)
    {
        Sent = sent;
        Received = received;
        Loss = Math.Max(0, sent - received).ToString();
        OnPropertyChanged(nameof(Lossy));
        if (IsError(lastStatus))
            _sawError = true;
        Classify();
        if (sample is not { } ms)
            return;
        var list = Samples.ToList();
        list.Add(ms);
        list.Sort();
        Samples = list;
        var five = Five(Samples);
        Min = five.Min;
        Q1 = five.Q1;
        Median = five.Median;
        Q3 = five.Q3;
        Max = five.Max;
    }

    public void Apply(IcmpEchoResult result, bool dead)
    {
        Sent = result.Sent;
        Received = result.Received;
        Loss = result.Lost.ToString();
        OnPropertyChanged(nameof(Lossy));
        _sawError = result.Replies.Any(r => r.Status is not IcmpEchoStatus.Success and not IcmpEchoStatus.TimedOut);
        Samples = result.Replies
            .Where(r => r.Status == IcmpEchoStatus.Success)
            .Select(r => r.RoundtripTimeMs)
            .OrderBy(x => x)
            .ToArray();
        var five = Five(Samples);
        Min = five.Min;
        Q1 = five.Q1;
        Median = five.Median;
        Q3 = five.Q3;
        Max = five.Max;
        Classify();
    }

    public void Skip(int lost)
    {
        Sent = lost;
        Received = 0;
        Loss = lost.ToString();
        SetStatus("Skipped", SkippedBrush);
    }

    public void Close() => Classify();

    private void Classify()
    {
        if (Sent <= 0)
        {
            SetStatus("", Brushes.Transparent);
            return;
        }
        if (Received == Sent)
            SetStatus("Success", SuccessBrush);
        else if (Received > 0)
            SetStatus("Warning", WarningBrush);
        else if (_sawError)
            SetStatus("Refused", RefusedBrush);
        else
            SetStatus("No Answer", NoAnswerBrush);
    }

    private void SetStatus(string text, Brush brush)
    {
        StatusText = text;
        StatusBrush = brush;
    }

    private static bool IsError(string? status)
        => status is not null
           && status != nameof(IcmpEchoStatus.Success)
           && status != nameof(IcmpEchoStatus.TimedOut);

    private static Brush Freeze(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }

    private static (string Min, string Q1, string Median, string Q3, string Max) Five(IReadOnlyList<long> samples)
    {
        if (samples.Count == 0)
            return ("", "", "", "", "");
        return (At(samples, 0), At(samples, 0.25), At(samples, 0.5), At(samples, 0.75), At(samples, 1));
    }

    private static string At(IReadOnlyList<long> samples, double p)
        => samples[(int)Math.Round((samples.Count - 1) * p)].ToString();
}
