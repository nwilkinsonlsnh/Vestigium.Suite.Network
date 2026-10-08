namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record CaptureLine(
    string Host,
    string Ports,
    int Hits,
    string Sources,
    string Dns,
    string Error,
    string Category,
    string Answer);
