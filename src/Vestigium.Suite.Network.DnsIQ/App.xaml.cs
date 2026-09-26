using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Suite.Network.Shell;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.DnsIQ);

        Themes.Register(ThemeDefinition.FromPack(
            "LightBlue", "Light Blue", "Vestigium.Themes.LightBlue", isDark: false,
            "Default Vestigium diagnostic chrome."));
        Themes.Register(ThemeDefinition.FromPack(
            "DarkMode", "Dark Mode", "Vestigium.Themes.DarkMode", isDark: true,
            "Low-glare dark surfaces."));
        Themes.Initialize(this, "LightBlue");

        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        services.AddVestigiumControls();
        Services = services.BuildServiceProvider();
        VestigiumConverterHost.ServiceProvider = Services;

        base.OnStartup(e);
    }
}
