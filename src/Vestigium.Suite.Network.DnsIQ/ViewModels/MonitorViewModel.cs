using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Vestigium.Controls.StatusBar;
using CommunityToolkit.Mvvm.Input;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed partial class MonitorViewModel : ObservableObject
{
    public const int DefaultSeconds = 10;
    public const int StepSeconds = 5;
    public const int MaxSeconds = 180;
    public const int UacDeclined = 1223;

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public VestigiumStatusBarViewModel? StatusBar { get; set; }

    public Func<string, Task>? Lookup { get; set; }

    public Func<string, Task>? LookupAndProbe { get; set; }

    [RelayCommand]
    private Task LookupSelected(MonitorAggregate? row)
        => row is null || string.IsNullOrWhiteSpace(row.Name) || Lookup is null ? Task.CompletedTask : Lookup(row.Name);

    [RelayCommand]
    private Task LookupAndProbeSelected(MonitorAggregate? row)
        => row is null || string.IsNullOrWhiteSpace(row.Name) || LookupAndProbe is null ? Task.CompletedTask : LookupAndProbe(row.Name);

    public IReadOnlyList<int> Durations { get; } = Enumerable.Range(1, MaxSeconds / StepSeconds).Select(i => i * StepSeconds).ToList();

    public ObservableCollection<MonitorAggregate> Rows { get; } = [];

    private readonly List<MonitorLine> _lines = [];
    private bool _dirty;
    private DispatcherTimer? _flush;
    private bool _timed;

    public IReadOnlyList<MonitorLine> LinesFor(string name)
        => _lines.Where(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();

    [ObservableProperty]
    private string _source = "Both";

    [ObservableProperty]
    private int _seconds = DefaultSeconds;

    [ObservableProperty]
    private string _monitorStatus = "Idle";

    [ObservableProperty]
    private string _unseen = "";

    [ObservableProperty]
    private double _elapsed;

    public string ElapsedText => _timed ? $"{Elapsed:0} / {Seconds}" : $"{Elapsed:0}";

    private DispatcherTimer? _clock;

    public string? PipeName { get; private set; }

    private CancellationTokenSource? _read;
    private NamedPipeClientStream? _client;

    [RelayCommand]
    private Task StartAsync() => LaunchAsync(timed: false);

    [RelayCommand]
    private Task WatchAsync() => LaunchAsync(timed: true);

    private async Task LaunchAsync(bool timed)
    {
        if (timed && (Seconds < StepSeconds || Seconds > MaxSeconds || Seconds % StepSeconds != 0))
        {
            MonitorStatus = "Rejected";
            return;
        }

        Stop();
        _timed = timed;
        _lines.Clear();
        Rows.Clear();
        OnPropertyChanged(nameof(ResolvedText));
        BeginClock(timed);
        var exe = FindWatchExe();
        if (exe is null)
        {
            MonitorStatus = "Watching. Exe was not in the output or the NuGet cache.";
            return;
        }

        MonitorStatus = "Launching " + exe;
        PipeName = "Vestigium.Watch.Dns." + Guid.NewGuid().ToString("N");
        PlaceAbstractions(Path.GetDirectoryName(exe)!);
        var duration = timed ? Seconds : MaxSeconds;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                WorkingDirectory = Path.GetDirectoryName(exe)!,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = Source + " " + duration + " pipe:" + PipeName
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == UacDeclined)
        {
            MonitorStatus = "Watch was not started.";
            return;
        }

        _read = new CancellationTokenSource();
        if (!await ConnectAndReadAsync(PipeName, _read.Token).ConfigureAwait(false))
            MonitorStatus = "Pipe did not open. " + exe;
    }

    private void BeginClock(bool timed)
    {
        _clock?.Stop();
        Elapsed = 0;
        OnPropertyChanged(nameof(ElapsedText));
        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clock.Tick += (_, _) =>
        {
            Elapsed += 1;
            OnPropertyChanged(nameof(ElapsedText));
            PostElapsed();
            if (timed && Elapsed >= Seconds)
            {
                _clock?.Stop();
                Stop();
            }
        };
        _clock.Start();
        MonitorStatus = timed ? "Watching" : "Running";
        PostElapsed();
        EnsureFlush();
    }

    [RelayCommand]
    private void Stop()
    {
        _clock?.Stop();
        _flush?.Stop();
        _read?.Cancel();
        _client?.Dispose();
        if (_dirty)
            RebuildAggregates();
        MonitorStatus = "Stopped";
        PostElapsed(done: true);
    }

    public void Apply(string line)
    {
        MonitorLine? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<MonitorLine>(line, Json);
        }
        catch (JsonException)
        {
            MonitorStatus = "Bad row";
            return;
        }

        if (parsed is null)
            return;

        if (string.Equals(parsed.Status, "Unseen", StringComparison.OrdinalIgnoreCase))
        {
            Unseen = parsed.Answers;
            return;
        }

        if (string.Equals(parsed.Status, "Failed", StringComparison.OrdinalIgnoreCase))
        {
            MonitorStatus = string.IsNullOrWhiteSpace(parsed.Answers) ? "Failed" : parsed.Answers;
            return;
        }

        if (string.IsNullOrWhiteSpace(parsed.Name))
            return;

        parsed.Type = TypeName(parsed.Type);
        _lines.Add(parsed);
        _dirty = true;
    }

    private void EnsureFlush()
    {
        if (_flush is not null)
            return;
        _flush = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _flush.Tick += (_, _) =>
        {
            if (!_dirty)
                return;
            RebuildAggregates();
        };
        _flush.Start();
    }

    private void RebuildAggregates()
    {
        _dirty = false;
        var groups = _lines
            .GroupBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var latestByType = g
                    .GroupBy(l => l.Type, StringComparer.Ordinal)
                    .Select(tg => tg.OrderByDescending(l => l.Time).First())
                    .ToList();
                return new MonitorAggregate
                {
                    Time = g.Max(l => l.Time),
                    Name = g.Key,
                    TypeCount = latestByType.Count,
                    ResolverCount = latestByType.Sum(l => l.ResolverCount),
                    PacketCount = latestByType.Sum(l => l.PacketCount),
                    Total = latestByType.Sum(l => l.Total)
                };
            })
            .OrderByDescending(a => a.Time)
            .ToList();

        Rows.Clear();
        foreach (var agg in groups)
            Rows.Add(agg);
        OnPropertyChanged(nameof(ResolvedText));
    }

    private async Task<bool> ConnectAndReadAsync(string name, CancellationToken token)
    {
        var client = new NamedPipeClientStream(".", name, PipeDirection.In, PipeOptions.Asynchronous);
        _client = client;
        try
        {
            return await ReadAsync(client, name, token).ConfigureAwait(false);
        }
        finally
        {
            client.Dispose();
            if (ReferenceEquals(_client, client))
                _client = null;
        }
    }

    private async Task<bool> ReadAsync(NamedPipeClientStream client, string name, CancellationToken token)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                await client.ConnectAsync(500).ConfigureAwait(false);
                break;
            }
            catch (TimeoutException)
            {
            }
        }

        if (!client.IsConnected)
            return false;

        MonitorStatus = _timed ? "Reading" : "Running";
        using var reader = new StreamReader(client, Encoding.UTF8);
        try
        {
            while (!token.IsCancellationRequested && await reader.ReadLineAsync(token).ConfigureAwait(false) is string line)
                OnUi(() => Apply(line));
        }
        catch (OperationCanceledException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }

        if (!token.IsCancellationRequested)
            MonitorStatus = "Ended";
        PostElapsed(done: true);
        return true;
    }

    public string ResolvedText => Rows.Count == 1 ? "1 name resolved" : Rows.Count + " names resolved";

    private void PostElapsed(bool done = false)
    {
        if (StatusBar is null)
            return;
        var text = _timed ? Elapsed + " of " + Seconds + " seconds" : Elapsed + " seconds";
        StatusBar.Engine.PostImmediate("message", new StatusBarUpdate { Text = text });
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = !_timed || Seconds == 0 ? 0 : Math.Clamp(100.0 * Elapsed / Seconds, 0, 100),
            IsProgressVisible = _timed && !done,
            IsIndeterminate = !_timed && !done
        });
    }

    private static void OnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.BeginInvoke(action, DispatcherPriority.Background);
    }

    public static string? FindWatchExe()
    {
        var name = "Vestigium.Helpers.Watch.Dns.exe";
        var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "vestigium.helpers.watch.dns");
        if (Directory.Exists(cache))
        {
            var pinned = Directory.EnumerateFiles(cache, name, SearchOption.AllDirectories)
                .Where(path => path.Contains("1.0.15", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(path => path)
                .FirstOrDefault();
            if (pinned is not null && HasDependencies(pinned))
                return pinned;

            var any = Directory.EnumerateFiles(cache, name, SearchOption.AllDirectories)
                .OrderByDescending(path => path)
                .FirstOrDefault();
            if (any is not null && HasDependencies(any))
                return any;
        }

        var root = AppContext.BaseDirectory;
        var local = new[] { Path.Combine(root, "watch", name), Path.Combine(root, name) }
            .Concat(Directory.Exists(root) ? Directory.EnumerateFiles(root, name, SearchOption.AllDirectories) : [])
            .FirstOrDefault(HasDependencies);
        return local;
    }

    private static bool HasDependencies(string exe)
    {
        var dir = Path.GetDirectoryName(exe);
        return dir is not null && File.Exists(exe);
    }

    private static void PlaceAbstractions(string dir)
    {
        var dest = Path.Combine(dir, "Microsoft.Extensions.DependencyInjection.Abstractions.dll");
        if (File.Exists(dest))
            return;
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "microsoft.extensions.dependencyinjection.abstractions");
        if (!Directory.Exists(root))
            return;
        var source = Directory.EnumerateFiles(root, "Microsoft.Extensions.DependencyInjection.Abstractions.dll", SearchOption.AllDirectories).FirstOrDefault();
        if (source is not null)
            File.Copy(source, dest, overwrite: true);
    }

    private static string TypeName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";
        return raw.Trim() switch
        {
            "1" => "A",
            "2" => "NS",
            "5" => "CNAME",
            "6" => "SOA",
            "12" => "PTR",
            "15" => "MX",
            "16" => "TXT",
            "28" => "AAAA",
            "33" => "SRV",
            "64" => "SVCB",
            "65" => "HTTPS",
            "255" => "ANY",
            "257" => "CAA",
            _ => raw.Trim().ToUpperInvariant()
        };
    }

}

public sealed class MonitorAggregate
{
    public DateTimeOffset Time { get; init; }
    public string Name { get; init; } = "";
    public int TypeCount { get; init; }
    public int ResolverCount { get; init; }
    public int PacketCount { get; init; }
    public int Total { get; init; }
}

public sealed class MonitorLine
{
    public DateTimeOffset Time { get; set; }
    public int Pid { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Answers { get; set; } = "";
    public int ResolverCount { get; set; }
    public int PacketCount { get; set; }
    public int Total { get; set; }
}
