using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vestigium.Helpers.Kql;
using Vestigium.Helpers.Network;

namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed partial class MainViewModel
{
    private readonly KqlSession _routeSession = KqlHelper.Create(KqlPack.Route);
    private readonly KqlSession _neighborSession = KqlHelper.Create(KqlPack.Neighbor);
    private readonly KqlSession _connectionSession = KqlHelper.Create(KqlPack.Connection);
    private readonly ObservableCollection<RouteIqQueryEntry> _noQueries = [];
    private KqlBoundQuery? _routeBound;
    private KqlBoundQuery? _neighborBound;
    private KqlBoundQuery? _connectionBound;
    private RouteIqSession? _queryBook;
    private bool _filtersArmed;

    public RouteIqSession? QueryBook
    {
        get => _queryBook;
        set
        {
            _queryBook = value;
            var source = value?.Queries ?? _noQueries;
            RouteMru = View(source, RouteIqSession.RouteTab);
            NeighborMru = View(source, RouteIqSession.NeighborTab);
            ConnectionMru = View(source, RouteIqSession.ConnectionTab);
            OnPropertyChanged(nameof(QueryBook));
            OnPropertyChanged(nameof(RouteMru));
            OnPropertyChanged(nameof(NeighborMru));
            OnPropertyChanged(nameof(ConnectionMru));
        }
    }

    public Action<string>? ReportQuery { get; set; }

    public System.ComponentModel.ICollectionView RouteMru { get; private set; } = View([], RouteIqSession.RouteTab);

    public System.ComponentModel.ICollectionView NeighborMru { get; private set; } = View([], RouteIqSession.NeighborTab);

    public System.ComponentModel.ICollectionView ConnectionMru { get; private set; } = View([], RouteIqSession.ConnectionTab);

    [ObservableProperty]
    private string _routeQuery = string.Empty;

    [ObservableProperty]
    private string _neighborQuery = string.Empty;

    [ObservableProperty]
    private string _connectionQuery = string.Empty;

    partial void OnRouteQueryChanged(string value) => Arm(_routeDelay);

    partial void OnNeighborQueryChanged(string value) => Arm(_neighborDelay);

    partial void OnConnectionQueryChanged(string value) => Arm(_connectionDelay);

    private void Arm(DispatcherTimer timer)
    {
        if (!_filtersArmed)
        {
            ArmFilters();
            _filtersArmed = true;
        }

        timer.Stop();
        timer.Start();
    }

    [RelayCommand]
    private void ClearRouteQuery() => RouteQuery = string.Empty;

    [RelayCommand]
    private void ClearNeighborQuery() => NeighborQuery = string.Empty;

    [RelayCommand]
    private void ClearConnectionQuery() => ConnectionQuery = string.Empty;

    [RelayCommand]
    private void CommitRouteQuery()
    {
        ApplyRoute();
        Remember(RouteIqSession.RouteTab, RouteQuery, _routeBound);
    }

    [RelayCommand]
    private void CommitNeighborQuery()
    {
        ApplyNeighbor();
        Remember(RouteIqSession.NeighborTab, NeighborQuery, _neighborBound);
    }

    [RelayCommand]
    private void CommitConnectionQuery()
    {
        ApplyConnection();
        Remember(RouteIqSession.ConnectionTab, ConnectionQuery, _connectionBound);
    }

    [RelayCommand]
    private void ApplyRouteQuery(string? text) => RouteQuery = text ?? string.Empty;

    [RelayCommand]
    private void ApplyNeighborQuery(string? text) => NeighborQuery = text ?? string.Empty;

    [RelayCommand]
    private void ApplyConnectionQuery(string? text) => ConnectionQuery = text ?? string.Empty;

    [RelayCommand]
    private void ToggleRouteSticky(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.SetSticky(entry, !entry.Sticky);
    }

    [RelayCommand]
    private void ToggleNeighborSticky(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.SetSticky(entry, !entry.Sticky);
    }

    [RelayCommand]
    private void ToggleConnectionSticky(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.SetSticky(entry, !entry.Sticky);
    }

    [RelayCommand]
    private void RemoveRouteQuery(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.Remove(entry);
    }

    [RelayCommand]
    private void RemoveNeighborQuery(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.Remove(entry);
    }

    [RelayCommand]
    private void RemoveConnectionQuery(RouteIqQueryEntry? entry)
    {
        if (entry is not null)
            QueryBook?.Remove(entry);
    }

    private void Remember(string tab, string text, KqlBoundQuery? bound)
    {
        if (bound is null || string.IsNullOrWhiteSpace(text))
            return;
        QueryBook?.Remember(tab, text);
    }

    private static System.ComponentModel.ICollectionView View(ObservableCollection<RouteIqQueryEntry> source, string tab)
    {
        var view = new CollectionViewSource { Source = source }.View;
        view.Filter = item => item is RouteIqQueryEntry entry && entry.Tab == tab;
        return view;
    }

    private KqlBoundQuery? Compile(string text, KqlSession session)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            ReportQuery?.Invoke(string.Empty);
            return null;
        }

        var compiled = KqlHelper.Compile(text, session);
        if (!compiled.Ok)
        {
            ReportQuery?.Invoke(compiled.Error?.ToString() ?? "query failed");
            return null;
        }

        ReportQuery?.Invoke(string.Empty);
        return compiled.Query;
    }

    private static void Refresh<T>(ObservableCollection<T> source, Predicate<object> filter)
    {
        var view = CollectionViewSource.GetDefaultView(source);
        view.Filter = filter;
        view.Refresh();
    }

    private bool RouteFilter(object item)
    {
        if (_routeBound is null || item is not NetworkRoute route)
            return _routeBound is null;
        return _routeBound.Matches(new KqlFixtureRow(_routeSession)
            .Set("route.destination", route.Destination)
            .Set("route.prefixlength", route.PrefixLength)
            .Set("route.subnetmask", route.Mask)
            .Set("route.gateway", route.Gateway)
            .Set("route.interfacename", route.InterfaceName)
            .Set("route.interfaceindex", route.InterfaceIndex)
            .Set("route.metric", route.Metric)
            .Set("route.protocol", route.Protocol));
    }

    private bool NeighborFilter(object item)
    {
        if (_neighborBound is null || item is not NeighborGridRow row)
            return _neighborBound is null;
        int? rtt = int.TryParse(row.RttMs, out var parsed) ? parsed : null;
        return _neighborBound.Matches(new KqlFixtureRow(_neighborSession)
            .Set("neighbors.address", row.Address)
            .Set("neighbors.class", row.ClassText)
            .Set("neighbors.macaddress", row.MacAddress)
            .Set("neighbors.interfacename", row.InterfaceName)
            .Set("neighbors.interfaceindex", row.InterfaceIndex)
            .Set("neighbors.state", row.State)
            .Set("neighbors.ismulticast", row.IsMulticast)
            .Set("neighbors.isbroadcast", row.IsBroadcast)
            .Set("neighbors.vendor", row.VendorText)
            .Set("neighbors.rtt", rtt)
            .Set("neighbors.isrouter", row.IsRouter));
    }

    private bool ConnectionFilter(object item)
    {
        if (_connectionBound is null || item is not ConnectionGridRow row)
            return _connectionBound is null;
        int? remotePort = int.TryParse(row.RemotePort, out var parsed) ? parsed : null;
        return _connectionBound.Matches(new KqlFixtureRow(_connectionSession)
            .Set("connections.status", row.Change)
            .Set("connections.protocol", row.Protocol)
            .Set("connections.local", row.LocalAddress)
            .Set("connections.localport", row.LocalPort)
            .Set("connections.remote", row.RemoteAddress)
            .Set("connections.remoteport", remotePort)
            .Set("connections.service", row.Service)
            .Set("connections.state", row.State)
            .Set("connections.process", row.Process)
            .Set("connections.time", row.TimeSeconds));
    }
}
