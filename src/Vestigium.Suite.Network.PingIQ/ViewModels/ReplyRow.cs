namespace Vestigium.Suite.Network.PingIQ.ViewModels;

public sealed record ReplyRow(
    int Sequence,
    string Status,
    string? Address,
    long RttMs,
    int Ttl,
    string? Detail);
