using System.IO;
using Vestigium.Suite.Network.NicIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class NicIqSettingsStoreTests
{
    [Fact]
    public void Missing_file_returns_defaults()
    {
        var loaded = new NicIqSettingsStore(NewRoot()).Load();
        Assert.Null(loaded.ThemeId);
        Assert.Equal(10, loaded.DurationSeconds);
        Assert.True(loaded.IncludeDown);
        Assert.True(loaded.StatusBarVisible);
        Assert.Equal("Bottom", loaded.StatusBarDock);
    }

    [Fact]
    public void Round_trip_writes_every_knob()
    {
        var root = NewRoot();
        var store = new NicIqSettingsStore(root);
        store.Save(new NicIqSettings
        {
            ThemeId = "Monokai",
            DurationSeconds = 25,
            IncludeDown = false,
            StatusBarVisible = false,
            StatusBarDock = "Top"
        });

        var loaded = store.Load();
        Assert.Equal("Monokai", loaded.ThemeId);
        Assert.Equal(25, loaded.DurationSeconds);
        Assert.False(loaded.IncludeDown);
        Assert.False(loaded.StatusBarVisible);
        Assert.Equal("Top", loaded.StatusBarDock);
        Assert.True(File.Exists(Path.Combine(root, "settings.json")));
    }

    [Fact]
    public void Corrupt_json_returns_defaults_without_throwing()
    {
        var root = NewRoot();
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"), "{ not json");
        var loaded = new NicIqSettingsStore(root).Load();
        Assert.Equal(10, loaded.DurationSeconds);
        Assert.True(loaded.IncludeDown);
    }

    [Fact]
    public void Default_root_is_programdata_diagnostics()
    {
        var expected = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Vestigium", "Settings", "Diagnostics", "NicIQ");
        Assert.Equal(expected, NicIqSettingsStore.DefaultRoot);
    }

    [Fact]
    public void Clamp_duration_holds_one_to_sixty()
    {
        Assert.Equal(1m, NicIqSession.ClampDuration(0));
        Assert.Equal(60m, NicIqSession.ClampDuration(90));
        Assert.Equal(10m, NicIqSession.ClampDuration(10.9m));
    }

    private static string NewRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "NicIQ-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }
}
