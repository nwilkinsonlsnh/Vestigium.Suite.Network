using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class DnsIqSettingsStoreTests
{
    [Fact]
    public void Missing_file_returns_defaults()
    {
        var root = NewRoot();
        var loaded = new DnsIqSettingsStore(root).Load();
        Assert.Equal("All", loaded.Type);
        Assert.Equal(53, loaded.Port);
        Assert.Equal(1000, loaded.Requests);
        Assert.Equal(60, loaded.Seconds);
        Assert.True(loaded.StatusBarVisible);
        Assert.Equal("Bottom", loaded.StatusBarDock);
        Assert.Null(loaded.Source);
        Assert.Equal(0, loaded.InterfaceIndex);
    }

    [Fact]
    public void Round_trip_writes_every_knob()
    {
        var root = NewRoot();
        var store = new DnsIqSettingsStore(root);
        store.Save(new DnsIqSettings
        {
            ThemeId = "Monokai",
            Server = "172.16.0.5",
            Type = "AAAA",
            InterfaceIndex = 2,
            Port = 5353,
            Requests = 200,
            Seconds = 180,
            StatusBarVisible = false,
            StatusBarDock = "Top",
            Source = "10.0.0.5"
        });

        var loaded = store.Load();
        Assert.Equal("Monokai", loaded.ThemeId);
        Assert.Equal("172.16.0.5", loaded.Server);
        Assert.Equal("AAAA", loaded.Type);
        Assert.Equal(2, loaded.InterfaceIndex);
        Assert.Equal(5353, loaded.Port);
        Assert.Equal(200, loaded.Requests);
        Assert.Equal(180, loaded.Seconds);
        Assert.False(loaded.StatusBarVisible);
        Assert.Equal("Top", loaded.StatusBarDock);
        Assert.Equal("10.0.0.5", loaded.Source);
        Assert.True(File.Exists(Path.Combine(root, "settings.json")));
    }

    [Fact]
    public void Corrupt_json_returns_defaults_without_throwing()
    {
        var root = NewRoot();
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"), "{ not json");
        var loaded = new DnsIqSettingsStore(root).Load();
        Assert.Equal(53, loaded.Port);
        Assert.Equal("All", loaded.Type);
    }

    [Fact]
    public void Default_root_is_programdata_diagnostics()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Vestigium", "Settings", "Diagnostics", "DnsIQ");
        Assert.Equal(expected, DnsIqSettingsStore.DefaultRoot);
    }

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "DnsIQ-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
