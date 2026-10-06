using System.Diagnostics;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.ClosedXml;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private bool _exportQuiet;

    [ObservableProperty]
    private bool _exportRoutes = true;

    [ObservableProperty]
    private bool _exportNeighbors = true;

    [ObservableProperty]
    private bool _exportConnections = true;

    [ObservableProperty]
    private bool _exportNetBios = true;

    [ObservableProperty]
    private bool _exportLmHosts = true;

    [ObservableProperty]
    private bool _exportOpenAfter;

    [ObservableProperty]
    private bool _exportOpenFolder;

    public void RememberExportChecks(bool routes, bool neighbors, bool connections, bool netbios, bool lmhosts, bool openAfter, bool openFolder)
    {
        _exportQuiet = true;
        ExportRoutes = routes;
        ExportNeighbors = neighbors;
        ExportConnections = connections;
        ExportNetBios = netbios;
        ExportLmHosts = lmhosts;
        ExportOpenAfter = openAfter;
        ExportOpenFolder = openFolder;
        _exportQuiet = false;
        QueryBook?.SetExports(routes, neighbors, connections, netbios, lmhosts, openAfter, openFolder);
        ExportSelectedCommand.NotifyCanExecuteChanged();
    }

    private bool CanExport() => _tablesReady && _connectionsReady;

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ExportSelected()
    {
        if (!ExportRoutes && !ExportNeighbors && !ExportConnections && !ExportNetBios && !ExportLmHosts)
        {
            Report("Select a print to export.");
            return;
        }

        WriteExport(ExportRoutes, ExportNeighbors, ExportConnections, ExportNetBios, ExportLmHosts);
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private void ExportAll()
        => WriteExport(routes: true, neighbors: true, connections: true, netbios: true, lmhosts: true);

    private void WriteExport(bool routes, bool neighbors, bool connections, bool netbios, bool lmhosts)
    {
        var folder = ExportFolder();
        var name = "RouteIQ-export-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xlsx";
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel workbook (*.xlsx)|*.xlsx",
            FileName = name,
            InitialDirectory = folder,
            RestoreDirectory = true,
            AddExtension = true,
            DefaultExt = ".xlsx",
            OverwritePrompt = true
        };
        if (dialog.ShowDialog() != true)
            return;

        var path = string.IsNullOrWhiteSpace(dialog.FileName) ? Path.Combine(folder, name) : dialog.FileName;
        try
        {
            using var book = WorkbookHelper.Create("Cover", "RouteIQ");
            var cover = new List<(string Key, object? Value)>
            {
                ("Host", Environment.MachineName),
                ("Operator", Environment.UserName),
                ("Taken", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz")),
                ("Route query", string.IsNullOrWhiteSpace(RouteQuery) ? "--" : RouteQuery),
                ("Neighbor query", string.IsNullOrWhiteSpace(NeighborQuery) ? "--" : NeighborQuery),
                ("Connection query", string.IsNullOrWhiteSpace(ConnectionQuery) ? "--" : ConnectionQuery)
            };
            if (routes)
                cover.Add(("Routes", (Ipv4Routes.Count + Ipv6Routes.Count).ToString(CultureInfo.InvariantCulture)));
            if (neighbors)
                cover.Add(("Neighbors", (Ipv4Neighbors.Count + Ipv6Neighbors.Count).ToString(CultureInfo.InvariantCulture)));
            if (connections)
                cover.Add(("Connections", Connections.Count.ToString(CultureInfo.InvariantCulture)));
            if (netbios)
                cover.Add(("NetBIOS", NetBiosNames.Count.ToString(CultureInfo.InvariantCulture)));
            if (lmhosts)
                cover.Add(("LMHOSTS", LmHosts.Count.ToString(CultureInfo.InvariantCulture)));
            book.Sheet("Cover").WriteTable(SheetTable.KeyValue("Field", "Value", cover, "Cover"));
            if (routes)
                book.Sheet("Routes").WriteTable(RouteTable());
            if (neighbors)
                book.Sheet("Neighbors").WriteTable(NeighborTable());
            if (connections)
                book.Sheet("Connections").WriteTable(ConnectionTable());
            if (netbios)
                book.Sheet("NetBIOS").WriteTable(NetBiosTable());
            if (lmhosts)
                book.Sheet("LMHOSTS").WriteTable(LmHostTable());
            var saved = book.SaveAs(path);
            if (!File.Exists(saved))
            {
                Report("Export did not write " + path);
                return;
            }

            Report("Exported " + saved);
            if (!ExportOpenAfter && !ExportOpenFolder)
                return;
            if (!string.Equals(saved, path, StringComparison.OrdinalIgnoreCase) || !saved.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                Report("Export was written. It was not opened.");
                return;
            }
            if (ExportOpenAfter)
                Process.Start(new ProcessStartInfo(saved) { UseShellExecute = true });
            if (ExportOpenFolder)
                Process.Start(new ProcessStartInfo(Path.GetDirectoryName(saved)!) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Report(ex.Message);
        }
    }

    private static string ExportFolder()
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var folder = Path.Combine(profile, "Desktop", "Vestigium", "Exports", "RouteIQ");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private SheetTable RouteTable()
    {
        var rows = new List<IReadOnlyList<object?>>();
        foreach (var row in Ipv4Routes)
            rows.Add(RouteCells("IPv4", row));
        foreach (var row in Ipv6Routes)
            rows.Add(RouteCells("IPv6", row));
        return SheetTable.Create(
            ["Family", "Destination", "PrefixLength", "Mask", "Gateway", "InterfaceName", "InterfaceIndex", "Metric", "Protocol"],
            rows,
            "Routes");
    }

    private static object?[] RouteCells(string family, NetworkRoute row)
        => [family, row.Destination, row.PrefixLength, row.Mask, row.Gateway, row.InterfaceName, row.InterfaceIndex, row.Metric, row.Protocol];

    private SheetTable NeighborTable()
    {
        var rows = new List<IReadOnlyList<object?>>();
        foreach (var row in Ipv4Neighbors)
            rows.Add(NeighborCells("IPv4", row));
        foreach (var row in Ipv6Neighbors)
            rows.Add(NeighborCells("IPv6", row));
        return SheetTable.Create(
            ["Family", "Address", "Class", "MacAddress", "InterfaceName", "InterfaceIndex", "State", "IsMulticast", "IsBroadcast", "Vendor", "RttMs", "IsRouter", "LastReachable"],
            rows,
            "Neighbors");
    }

    private static object?[] NeighborCells(string family, NeighborGridRow row)
        => [family, row.Address, row.ClassText, row.MacAddress, row.InterfaceName, row.InterfaceIndex, row.State, row.IsMulticast, row.IsBroadcast, row.VendorText, row.RttMs, row.IsRouter, row.LastReachableText];

    private SheetTable ConnectionTable()
        => SheetTable.Create(
            ["Status", "Protocol", "Local", "LocalPort", "Remote", "RemotePort", "Service", "State", "Process", "Time"],
            Connections.Select(row => (IReadOnlyList<object?>)[row.Change, row.Protocol, row.LocalAddress, row.LocalPort, row.RemoteAddress, row.RemotePort, row.Service, row.State, row.Process, row.TimeSeconds]),
            "Connections");

    private SheetTable NetBiosTable()
        => SheetTable.Create(
            ["IsCache", "Adapter", "NodeAddress", "Name", "Suffix", "SuffixName", "Type", "Status", "Address", "LifeSeconds"],
            NetBiosNames.Select(row => (IReadOnlyList<object?>)[row.IsCache, row.Adapter, row.NodeAddress, row.Name, row.Suffix, row.SuffixName, row.Type, row.Status, row.Address, row.LifeSeconds]),
            "NetBIOS");

    private SheetTable LmHostTable()
        => SheetTable.Create(
            ["Address", "Name", "Preload", "Domain", "MultiHome", "Include", "Raw"],
            LmHosts.Select(row => (IReadOnlyList<object?>)[row.Address, row.Name, row.Preload, row.Domain, row.MultiHome, row.IncludePath, row.Raw]),
            "LMHOSTS");
}
