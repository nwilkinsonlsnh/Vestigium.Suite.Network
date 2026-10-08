using System.Windows;
using System.Windows.Threading;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed class DetailsCheck
{
    public const int MaxInFlight = 8;

    private CancellationTokenSource? _cts;

    public IReadOnlyList<DetailRow> Start(
        CaptureLine line,
        string? server,
        int port,
        int interfaceIndex,
        string? sourceAddress,
        IReadOnlyList<CaptureLine> lines,
        Action<string>? onHostCheck)
    {
        Cancel();
        _cts = new CancellationTokenSource();
        var rows = lines.Select(item => new DetailRow(item.Category, item.Answer)).ToList();
        _ = RunAsync(line, Options(server, port, interfaceIndex, sourceAddress), rows, onHostCheck, _cts.Token);
        return rows;
    }

    public void Cancel()
    {
        try { _cts?.Cancel(); }
        catch (ObjectDisposedException) { }
        _cts?.Dispose();
        _cts = null;
    }

    private static async Task RunAsync(
        CaptureLine line,
        DnsLookupOptions options,
        IReadOnlyList<DetailRow> rows,
        Action<string>? onHostCheck,
        CancellationToken token)
    {
        var inflight = new List<Task>();
        try
        {
            if (!string.Equals(line.Category, CaptureLines.Address, StringComparison.Ordinal))
            {
                await InFlightGate.WaitAsync(inflight, MaxInFlight, token).ConfigureAwait(false);
                inflight.Add(ConfirmHostAsync(line.Host, options, onHostCheck, token));
            }

            foreach (var row in rows)
            {
                token.ThrowIfCancellationRequested();
                await InFlightGate.WaitAsync(inflight, MaxInFlight, token).ConfigureAwait(false);
                inflight.Add(CheckRowAsync(row, options, token));
            }

            await Task.WhenAll(inflight).WaitAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task ConfirmHostAsync(
        string name,
        DnsLookupOptions options,
        Action<string>? onHostCheck,
        CancellationToken token)
    {
        try
        {
            var a = LookupAsync(name, options, DnsRecordType.A, token);
            var aaaa = LookupAsync(name, options, DnsRecordType.Aaaa, token);
            await Task.WhenAll(a, aaaa).ConfigureAwait(false);
            Report(onHostCheck, FormatHost(await a.ConfigureAwait(false), await aaaa.ConfigureAwait(false)));
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            Report(onHostCheck, "Failed");
        }
    }

    private static async Task CheckRowAsync(DetailRow row, DnsLookupOptions options, CancellationToken token)
    {
        var target = string.IsNullOrWhiteSpace(row.Answer) ? "" : row.Answer;
        if (!ReverseName.TryPtr(target, out var question))
        {
            Set(row, "", "");
            return;
        }

        Set(row, "Pending", question);
        try
        {
            var result = await LookupAsync(question, options, DnsRecordType.Ptr, token).ConfigureAwait(false);
            Set(row, FormatPtr(result), question);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            Set(row, "Failed", question);
        }
    }

    private static DnsLookupOptions Options(string? server, int port, int interfaceIndex, string? sourceAddress)
        => new()
        {
            Server = string.IsNullOrWhiteSpace(server) ? null : server.Trim(),
            Port = port,
            InterfaceIndex = interfaceIndex,
            SourceAddress = string.IsNullOrWhiteSpace(sourceAddress) ? null : sourceAddress
        };

    private static Task<DnsLookupResult> LookupAsync(
        string name,
        DnsLookupOptions options,
        DnsRecordType type,
        CancellationToken token)
        => NetworkHelper.LookupAsync(name, new DnsLookupOptions
        {
            Type = type,
            Server = options.Server,
            Port = options.Port,
            Timeout = options.Timeout,
            RecursionDesired = options.RecursionDesired,
            InterfaceIndex = options.InterfaceIndex,
            SourceAddress = options.SourceAddress
        }, token);

    private static string FormatHost(DnsLookupResult a, DnsLookupResult aaaa)
    {
        if (a.Answers.Count > 0 || aaaa.Answers.Count > 0)
            return "Name";
        if (a.Rcode is DnsRcode.Timeout || aaaa.Rcode is DnsRcode.Timeout)
            return "TimedOut";
        if (a.Rcode == DnsRcode.Refused || aaaa.Rcode == DnsRcode.Refused)
            return "Refused";
        if (a.Rcode == DnsRcode.NxDomain && aaaa.Rcode == DnsRcode.NxDomain)
            return "NxDomain";
        return "Failed";
    }

    private static string FormatPtr(DnsLookupResult result)
    {
        if (result.Rcode is DnsRcode.Timeout)
            return "TimedOut";
        if (result.Rcode == DnsRcode.Refused)
            return "Refused";
        if (result.Rcode == DnsRcode.NxDomain)
            return "NxDomain";
        if (result.Answers.Count > 0)
            return string.Join(", ", result.Answers.Select(answer => answer.Data));
        return "Failed";
    }

    private static void Set(DetailRow row, string check, string question)
        => OnUi(() =>
        {
            row.Check = check;
            row.Question = question;
        });

    private static void Report(Action<string>? onHostCheck, string check)
    {
        if (onHostCheck is null)
            return;
        OnUi(() => onHostCheck(check));
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
}
