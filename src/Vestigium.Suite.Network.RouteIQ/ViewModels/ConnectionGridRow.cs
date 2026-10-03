namespace Vestigium.Suite.Network.RouteIQ.ViewModels;

public sealed record ConnectionGridRow(
    string Change,
    string Mark,
    string Protocol,
    string LocalAddress,
    int LocalPort,
    string RemoteAddress,
    string RemotePort,
    string State,
    string Process,
    int TimeSeconds);
