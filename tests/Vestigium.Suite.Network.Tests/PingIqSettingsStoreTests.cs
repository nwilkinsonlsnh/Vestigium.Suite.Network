using System.IO;
using Vestigium.Suite.Network.PingIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class PingIqSettingsStoreTests
{
    [Fact]
    public void Missing_file_returns_defaults()
    {
        var loaded = new PingIqSettingsStore(NewRoot()).Load();
        Assert.Equal(4, loaded.Count);
        Assert.Equal(4000, loaded.TimeoutMs);
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
        var store = new PingIqSettingsStore(root);
        store.Save(new PingIqSettings
        {
            ThemeId = "Monokai",
            Count = 8,
            TimeoutMs = 1500,
            InterfaceIndex = 2,
            Requests = 200,
            Seconds = 180,
            StatusBarVisible = false,
            StatusBarDock = "Top",
            Source = "10.0.0.5"
        });

        var loaded = store.Load();
        Assert.Equal("Monokai", loaded.ThemeId);
        Assert.Equal(8, loaded.Count);
        Assert.Equal(1500, loaded.TimeoutMs);
        Assert.Equal(2, loaded.InterfaceIndex);
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
        var loaded = new PingIqSettingsStore(root).Load();
        Assert.Equal(4000, loaded.TimeoutMs);
        Assert.Equal(4, loaded.Count);
    }

    [Fact]
    public void Default_root_is_programdata_diagnostics()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Vestigium", "Settings", "Diagnostics", "PingIQ");
        Assert.Equal(expected, PingIqSettingsStore.DefaultRoot);
    }

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "PingIQ-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
