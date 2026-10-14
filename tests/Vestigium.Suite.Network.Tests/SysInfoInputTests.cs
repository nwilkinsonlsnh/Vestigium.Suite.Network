using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class SysInfoInputTests
{
    [Fact]
    public void Empty_path_is_rejected()
    {
        var ok = SysInfoInput.TryOutputFolder("", out _, out var reject);
        Assert.False(ok);
        Assert.Equal("Output folder is required.", reject);
    }

    [Fact]
    public void Relative_path_is_rejected()
    {
        var ok = SysInfoInput.TryOutputFolder("Desktop\\out", out _, out var reject);
        Assert.False(ok);
        Assert.Equal("Output folder must be an absolute path.", reject);
    }

    [Fact]
    public void Absolute_path_is_accepted()
    {
        var ok = SysInfoInput.TryOutputFolder(@"C:\Users\me\Desktop", out var folder, out var reject);
        Assert.True(ok);
        Assert.Null(reject);
        Assert.Equal(@"C:\Users\me\Desktop", folder);
    }
}
