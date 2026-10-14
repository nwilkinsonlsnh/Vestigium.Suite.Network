namespace Vestigium.Suite.Network.SysInfoIQ.ViewModels;

public sealed class InventorySection
{
    public required string Name { get; init; }
    public required string FileName { get; init; }
    public required string Content { get; init; }
    public bool IsError { get; init; }
}
