using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private bool _busy;

    public ObservableCollection<NetworkRoute> Ipv4Routes { get; } = [];

    public ObservableCollection<NetworkRoute> Ipv6Routes { get; } = [];

    public ObservableCollection<NetworkNeighbor> Ipv4Neighbors { get; } = [];

    public ObservableCollection<NetworkNeighbor> Ipv6Neighbors { get; } = [];

    public Action<string>? ReportStatus { get; set; }

    public MainViewModel() => _ = Refresh();

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task Refresh()
    {
        if (_busy)
            return;

        _busy = true;
        RaiseCanExecute();
        Report(string.Empty);
        try
        {
            var snapshot = await Task.Run(Load).ConfigureAwait(true);
            Replace(Ipv4Routes, snapshot.Ipv4);
            Replace(Ipv6Routes, snapshot.Ipv6);
            Replace(Ipv4Neighbors, snapshot.Ipv4Neighbors);
            Replace(Ipv6Neighbors, snapshot.Ipv6Neighbors);
            Report(string.Empty);
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
        finally
        {
            _busy = false;
            RaiseCanExecute();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void Copy()
    {
        try
        {
            Clipboard.SetText(FormatTables());
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopy))]
    private void CopyNeighbors()
    {
        try
        {
            Clipboard.SetText(FormatNeighbors());
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    private bool CanRefresh() => !_busy;

    private bool CanCopy() => !_busy;

    private void RaiseCanExecute()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        CopyCommand.NotifyCanExecuteChanged();
        CopyNeighborsCommand.NotifyCanExecuteChanged();
    }

    private void Report(string text) => ReportStatus?.Invoke(text);

    private static (IReadOnlyList<NetworkRoute> Ipv4, IReadOnlyList<NetworkRoute> Ipv6, IReadOnlyList<NetworkNeighbor> Ipv4Neighbors, IReadOnlyList<NetworkNeighbor> Ipv6Neighbors) Load()
    {
        var ipv4 = NetworkHelper.GetRoutes(RouteFamily.Pv4);
        var ipv6 = NetworkHelper.GetRoutes(RouteFamily.Pv6);
        var neighbors = NetworkHelper.GetNeighbors();
        var v4 = neighbors.Where(row => row.Family == AddressFamily.InterNetwork).ToArray();
        var v6 = neighbors.Where(row => row.Family == AddressFamily.InterNetworkV6).ToArray();
        return (ipv4, ipv6, v4, v6);
    }

    private string FormatTables()
    {
        var text = new StringBuilder();
        AppendRoutes(text, "IPv4 Route Table", Ipv4Routes);
        AppendRoutes(text, "IPv6 Route Table", Ipv6Routes);
        return text.ToString();
    }

    private string FormatNeighbors()
    {
        var text = new StringBuilder();
        AppendNeighbors(text, "IPv4 Neighbor Cache", Ipv4Neighbors);
        AppendNeighbors(text, "IPv6 Neighbor Cache", Ipv6Neighbors);
        return text.ToString();
    }

    private static void AppendNeighbors(StringBuilder text, string title, IReadOnlyList<NetworkNeighbor> rows)
    {
        if (text.Length > 0)
            text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Address              MAC                Interface            State");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Address, 21));
            text.Append(Pad(row.MacAddress, 19));
            text.Append(Pad(row.InterfaceName, 21));
            text.AppendLine(row.State);
        }

        if (rows.Count == 0)
            text.AppendLine("None");
    }

    private static void AppendRoutes(StringBuilder text, string title, IReadOnlyList<NetworkRoute> rows)
    {
        if (text.Length > 0)
            text.AppendLine();
        text.AppendLine(title);
        text.AppendLine("Destination          Prefix  Mask             Gateway            Interface           Index  Metric  Protocol");
        foreach (var row in rows)
        {
            text.Append(Pad(row.Destination, 21));
            text.Append(Pad(row.PrefixLength.ToString(), 8));
            text.Append(Pad(row.Mask, 17));
            text.Append(Pad(row.Gateway, 19));
            text.Append(Pad(row.InterfaceName, 20));
            text.Append(Pad(row.InterfaceIndex.ToString(), 7));
            text.Append(Pad(row.Metric.ToString(), 8));
            text.AppendLine(row.Protocol);
        }

        if (rows.Count == 0)
            text.AppendLine("None");
    }

    private static string Pad(string? value, int width)
    {
        var text = value ?? string.Empty;
        return text.Length >= width ? text + " " : text.PadRight(width);
    }

    private static void Replace<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
