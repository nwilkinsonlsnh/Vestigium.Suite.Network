namespace Vestigium.Suite.Network.DnsIQ.ViewModels;

public sealed record AnswerRow(
    string Type,
    string Name,
    string Data,
    int Ttl);
