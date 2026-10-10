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

    public IReadOnlyList<int> Durations { get; } = Enumerable.Range(1, MaxSeconds / StepSeconds).Select(i => i * StepSeconds).ToList();

    public ObservableCollection<MonitorRow> Rows { get; } = [];

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

    public string ElapsedText => $"{Elapsed:0} / {Seconds}";

    private DispatcherTimer? _clock;

    public string? PipeName { get; private set; }

    private CancellationTokenSource? _read;
    private NamedPipeClientStream? _client;

    [RelayCommand]
    private async Task StartAsync()
    {
        if (Seconds < StepSeconds || Seconds > MaxSeconds || Seconds % StepSeconds != 0)
        {
            MonitorStatus = "Rejected";
            return;
        }

        BeginClock();
        var exe = FindWatchExe();
        if (exe is null)
        {
            MonitorStatus = "Watching. Exe was not in the output or the NuGet cache.";
            return;
        }

        PipeName = "Vestigium.Watch.Dns." + Guid.NewGuid().ToString("N");
        PlaceAbstractions(Path.GetDirectoryName(exe)!);
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                WorkingDirectory = Path.GetDirectoryName(exe)!,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = Source + " " + Seconds + " pipe:" + PipeName
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == UacDeclined)
        {
            MonitorStatus = "Watch was not started.";
            return;
        }

        _read = new CancellationTokenSource();
        if (!await ConnectAndReadAsync(PipeName, _read.Token).ConfigureAwait(false))
            MonitorStatus = "Pipe did not open.";
    }

    private void BeginClock()
    {
        _clock?.Stop();
        Elapsed = 0;
        OnPropertyChanged(nameof(ElapsedText));
        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clock.Tick += (_, _) =>
        {
            if (Elapsed >= Seconds)
            {
                _clock?.Stop();
                return;
            }

            Elapsed += 1;
            OnPropertyChanged(nameof(ElapsedText));
            PostElapsed();
        };
        _clock.Start();
        MonitorStatus = "Watching";
        PostElapsed();
    }

    [RelayCommand]
    private void Stop()
    {
        _clock?.Stop();
        _read?.Cancel();
        _client?.Dispose();
        MonitorStatus = "Stopped";
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

        var keyName = parsed.Name;
        var keyType = parsed.Type;
        var existing = Rows.FirstOrDefault(row =>
            string.Equals(row.Name, keyName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(row.Type, keyType, StringComparison.Ordinal));
        if (existing is null)
        {
            Rows.Add(ToRow(parsed));
            OnPropertyChanged(nameof(ResolvedText));
            return;
        }

        var index = Rows.IndexOf(existing);
        Rows[index] = ToRow(parsed);
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

        MonitorStatus = "Reading";
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
        StatusBar.Engine.PostImmediate("message", new StatusBarUpdate { Text = Elapsed + " of " + Seconds + " seconds" });
        StatusBar.Engine.PostImmediate("progress", new StatusBarUpdate
        {
            Progress = Seconds == 0 ? 0 : Math.Clamp(100.0 * Elapsed / Seconds, 0, 100),
            IsProgressVisible = !done,
            IsIndeterminate = false
        });
    }

    private static MonitorRow ToRow(MonitorLine line)
        => new()
        {
            Time = line.Time,
            Pid = line.Pid,
            Name = line.Name,
            Type = TypeName(line.Type),
            ResolverCount = line.ResolverCount,
            PacketCount = line.PacketCount,
            Total = line.Total
        };

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
        var candidates = new List<string>();
        var root = AppContext.BaseDirectory;
        candidates.Add(Path.Combine(root, "watch", name));
        candidates.Add(Path.Combine(root, name));
        if (Directory.Exists(root))
            candidates.AddRange(Directory.EnumerateFiles(root, name, SearchOption.AllDirectories));
        var cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "vestigium.helpers.watch.dns");
        if (Directory.Exists(cache))
            candidates.AddRange(Directory.EnumerateFiles(cache, name, SearchOption.AllDirectories).OrderByDescending(path => path));
        return candidates.FirstOrDefault(HasDependencies);
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
public sealed class MonitorRow
{
    public DateTimeOffset Time { get; init; }
    public int Pid { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
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
