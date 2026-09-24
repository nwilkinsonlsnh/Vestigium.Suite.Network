using System.IO;
using System.Text.Json;
using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class RouteIqSettingsStoreTests
{
    [Fact]
    public void Default_root_is_program_data_settings_routeiq()
    {
        var tail = Path.Combine("Vestigium", "Settings", "RouteIQ");
        Assert.EndsWith(tail, RouteIqSettingsStore.DefaultRoot);
        Assert.DoesNotContain("Diagnostics", RouteIqSettingsStore.DefaultRoot);
        Assert.EndsWith(Path.Combine("Vestigium", "Settings", "Diagnostics", "RouteIQ", "settings.json"), RouteIqSettingsStore.LegacyFilePath);
    }

    [Fact]
    public void New_file_has_live_vendor_lookup_off()
    {
        var loaded = new RouteIqSettingsStore(NewRoot(), legacyFilePath: Missing()).Load();
        Assert.True(loaded.Ok);
        Assert.False(loaded.Settings.LiveVendorLookup);
    }

    [Fact]
    public void Missing_field_deserializes_live_vendor_lookup_off()
    {
        var root = NewRoot();
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"), """{"ThemeId":"LightBlue"}""");
        var loaded = new RouteIqSettingsStore(root, legacyFilePath: Missing()).Load();
        Assert.True(loaded.Ok);
        Assert.Equal("LightBlue", loaded.Settings.ThemeId);
        Assert.False(loaded.Settings.LiveVendorLookup);
    }

    [Fact]
    public void Copy_once_then_stops_reading_the_old_file()
    {
        var root = NewRoot();
        var legacy = Path.Combine(NewRoot(), "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(legacy)!);
        File.WriteAllText(legacy, JsonSerializer.Serialize(new RouteIqSettings { ThemeId = "FromOld", LiveVendorLookup = true }));

        var store = new RouteIqSettingsStore(root, legacy);
        var first = store.Load();
        Assert.True(first.Ok);
        Assert.Equal("FromOld", first.Settings.ThemeId);
        Assert.True(first.Settings.LiveVendorLookup);
        Assert.True(File.Exists(store.FilePath));

        File.WriteAllText(legacy, JsonSerializer.Serialize(new RouteIqSettings { ThemeId = "Changed" }));
        var second = store.Load();
        Assert.True(second.Ok);
        Assert.Equal("FromOld", second.Settings.ThemeId);
        Assert.True(second.Settings.LiveVendorLookup);
    }

    [Fact]
    public void Bad_file_returns_defaults_and_a_reason()
    {
        var root = NewRoot();
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"), "{ not json");
        var loaded = new RouteIqSettingsStore(root, legacyFilePath: Missing()).Load();
        Assert.False(loaded.Ok);
        Assert.False(loaded.Settings.LiveVendorLookup);
        Assert.Contains("could not be read", loaded.Reason);
    }

    [Fact]
    public void Save_failure_returns_a_reason()
    {
        var blocked = Path.Combine(NewRoot(), "not-a-directory");
        Directory.CreateDirectory(Path.GetDirectoryName(blocked)!);
        File.WriteAllText(blocked, "file");
        var saved = new RouteIqSettingsStore(blocked, legacyFilePath: Missing()).Save(new RouteIqSettings());
        Assert.False(saved.Ok);
        Assert.Contains("not saved", saved.Reason);
    }

    private static string NewRoot() => Path.Combine(Path.GetTempPath(), "routeiq-settings", Guid.NewGuid().ToString("N"));

    private static string Missing() => Path.Combine(Path.GetTempPath(), "routeiq-settings", Guid.NewGuid().ToString("N"), "missing.json");
}
