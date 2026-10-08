using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.DnsIQ.Views;

public partial class CaptureDetailsView : UserControl
{
    private CaptureLine? _line;
    private MainViewModel? _host;
    private CancellationTokenSource? _checks;

    public CaptureDetailsView()
    {
        InitializeComponent();
    }

    public event EventHandler? CloseRequested;

    public void Show(MainViewModel host, CaptureLine line, IReadOnlyList<CaptureLine> lines)
    {
        CancelChecks();
        _host = host;
        _line = line;
        TitleBlock.Text = Title(line);
        var rows = lines.Select(item => new DetailRow(item.Category, item.Answer)).ToList();
        Lines.ItemsSource = rows;
        _checks = new CancellationTokenSource();
        _ = RunAsync(host, line, rows, _checks.Token);
    }

    public void CancelChecks()
    {
        try { _checks?.Cancel(); }
        catch (ObjectDisposedException) { }
        _checks?.Dispose();
        _checks = null;
    }

    private async Task RunAsync(MainViewModel host, CaptureLine line, IReadOnlyList<DetailRow> rows, CancellationToken token)
    {
        var options = new DnsLookupOptions
        {
            Server = string.IsNullOrWhiteSpace(host.Server) ? null : host.Server.Trim(),
            Port = (int)host.Port,
            InterfaceIndex = host.Bind.InterfaceIndex,
            SourceAddress = string.IsNullOrWhiteSpace(host.Bind.SourceAddress) ? null : host.Bind.SourceAddress
        };

        var work = new List<Task>();
        if (!string.Equals(line.Category, CaptureLines.Address, StringComparison.Ordinal))
            work.Add(ConfirmHostAsync(line.Host, options, token));
        foreach (var row in rows)
            work.Add(CheckRowAsync(row, options, token));

        try
        {
            await Task.WhenAll(work).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task ConfirmHostAsync(string name, DnsLookupOptions options, CancellationToken token)
    {
        try
        {
            var a = LookupAsync(name, options, DnsRecordType.A, token);
            var aaaa = LookupAsync(name, options, DnsRecordType.Aaaa, token);
            await Task.WhenAll(a, aaaa).ConfigureAwait(false);
            var check = FormatHost(await a.ConfigureAwait(false), await aaaa.ConfigureAwait(false));
            SetTitle(check);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            SetTitle("Failed");
        }
    }

    private async Task CheckRowAsync(DetailRow row, DnsLookupOptions options, CancellationToken token)
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

    private static Task<DnsLookupResult> LookupAsync(string name, DnsLookupOptions options, DnsRecordType type, CancellationToken token)
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

    private void Set(DetailRow row, string check, string question)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            row.Check = check;
            row.Question = question;
            return;
        }

        dispatcher.BeginInvoke(() =>
        {
            row.Check = check;
            row.Question = question;
        }, DispatcherPriority.Background);
    }

    private void SetTitle(string check)
    {
        var dispatcher = Application.Current?.Dispatcher;
        void Apply()
        {
            if (_line is null)
                return;
            TitleBlock.Text = Title(_line) + " \u00b7 " + check;
        }

        if (dispatcher is null || dispatcher.CheckAccess())
            Apply();
        else
            dispatcher.BeginInvoke(Apply, DispatcherPriority.Background);
    }

    private static string Title(CaptureLine line)
        => string.IsNullOrWhiteSpace(line.Dns) ? line.Host : line.Host + " \u00b7 " + line.Dns;

    private void OnClose(object sender, RoutedEventArgs e)
    {
        CancelChecks();
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnLookup(object sender, RoutedEventArgs e)
    {
        if (_host is null || _line is null)
            return;
        _host.LookupLineCommand.Execute(_line);
    }

    private void OnLookupAndProbe(object sender, RoutedEventArgs e)
    {
        if (_host is null || _line is null)
            return;
        _host.LookupLineAndProbeCommand.Execute(_line);
    }

    private sealed class DetailRow : INotifyPropertyChanged
    {
        private string _check;
        private string _question;

        public DetailRow(string category, string answer)
        {
            Category = category;
            Answer = answer;
            _check = ReverseName.TryPtr(answer, out var question) ? "Pending" : "";
            _question = question;
        }

        public string Category { get; }
        public string Answer { get; }

        public string Check
        {
            get => _check;
            set
            {
                if (_check == value)
                    return;
                _check = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Check)));
            }
        }

        public string Question
        {
            get => _question;
            set
            {
                if (_question == value)
                    return;
                _question = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Question)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
