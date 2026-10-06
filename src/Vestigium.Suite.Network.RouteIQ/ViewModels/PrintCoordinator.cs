namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public delegate Task PrintSource(PrintScope scope, CancellationToken cancellationToken);

public readonly record struct PrintProgress(string Source, int Finished, int Started, int Generation);

public readonly struct PrintScope
{
    private readonly PrintCoordinator _owner;

    internal PrintScope(PrintCoordinator owner, int generation)
    {
        _owner = owner;
        Generation = generation;
    }

    public int Generation { get; }

    public bool IsCurrent => _owner.IsCurrent(Generation);
}

public sealed class PrintCoordinator
{
    public const int SourceCount = 6;

    public static IReadOnlyList<string> Names { get; } =
    [
        "IPv4 routes",
        "IPv6 routes",
        "Neighbors",
        "Connections",
        "NetBIOS",
        "LMHOSTS"
    ];

    private readonly PrintSource[] _sources;
    private readonly Action<PrintProgress>? _progress;
    private readonly Action<string, Exception>? _fault;
    private readonly Action<int>? _started;
    private readonly Lock _gate = new();
    private Task? _inflight;
    private bool _again;
    private int _generation;

    public PrintCoordinator(
        PrintSource ipv4,
        PrintSource ipv6,
        PrintSource neighbors,
        PrintSource connections,
        PrintSource netBios,
        PrintSource lmHosts,
        Action<PrintProgress>? progress = null,
        Action<string, Exception>? fault = null,
        Action<int>? started = null)
    {
        _sources =
        [
            ipv4 ?? throw new ArgumentNullException(nameof(ipv4)),
            ipv6 ?? throw new ArgumentNullException(nameof(ipv6)),
            neighbors ?? throw new ArgumentNullException(nameof(neighbors)),
            connections ?? throw new ArgumentNullException(nameof(connections)),
            netBios ?? throw new ArgumentNullException(nameof(netBios)),
            lmHosts ?? throw new ArgumentNullException(nameof(lmHosts))
        ];
        _progress = progress;
        _fault = fault;
        _started = started;
    }

    public int Generation => Volatile.Read(ref _generation);

    public bool IsCurrent(int generation)
        => generation > 0 && generation == Generation;

    public Task Request()
    {
        lock (_gate)
        {
            _again = true;
            return _inflight ??= Pump();
        }
    }

    private async Task Pump()
    {
        while (true)
        {
            int generation;
            lock (_gate)
            {
                if (!_again)
                {
                    _inflight = null;
                    return;
                }

                _again = false;
                generation = Interlocked.Increment(ref _generation);
            }

            await RunOnce(generation).ConfigureAwait(false);
        }
    }

    private async Task RunOnce(int generation)
    {
        _started?.Invoke(generation);
        var finished = 0;
        var tasks = new Task[SourceCount];
        for (var i = 0; i < SourceCount; i++)
        {
            var index = i;
            tasks[i] = RunSource(Names[index], _sources[index], generation, () => Interlocked.Increment(ref finished));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RunSource(string name, PrintSource source, int generation, Func<int> markFinished)
    {
        try
        {
            await source(new PrintScope(this, generation), CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            try
            {
                _fault?.Invoke(name, ex);
            }
            catch (Exception)
            {
                // A host fault handler must not fail the join. The host logs that second throw.
            }
        }

        var done = markFinished();
        try
        {
            _progress?.Invoke(new PrintProgress(name, done, SourceCount, generation));
        }
        catch (Exception)
        {
            // Progress is a report. It does not fail the join.
        }
    }
}
