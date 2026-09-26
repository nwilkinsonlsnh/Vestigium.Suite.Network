using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Vestigium.Suite.Network.DnsIQ.Views;
using Vestigium.Suite.Network.Shell;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.DnsIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    public static SettingsViewModel? Settings { get; private set; }

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

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        MainWindow = CreateMainWindow();
        MainWindow.Show();

        base.OnStartup(e);
    }

    private static Window CreateMainWindow()
    {
        var chrome = Services.GetRequiredService<VestigiumDefaultWindowViewModel>();
        var window = new VestigiumDefaultWindow(chrome)
        {
            Title = "DnsIQ"
        };

        window.HostShell.ApplySpec(new VestigiumShellSpec
        {
            Items =
            {
                new VestigiumNavItemSpec("DnsIQ")
                {
                    Title = "DnsIQ",
                    Subject = "Lookup and pulse",
                    Description = "One name. Lookup writes records. Probe is the resolver pulse."
                },
                new VestigiumNavItemSpec("Dashboard"),
                new VestigiumNavItemSpec("Settings")
            }
        });

        var dns = new MainViewModel { StatusBar = chrome.Status };
        var dnsItem = window.HostShell["DnsIQ"];
        if (dnsItem is not null)
        {
            dnsItem.Content = new DnsIqView { DataContext = dns };
            window.HostShell.SelectedItem = dnsItem;
        }

        var dash = window.HostShell["Dashboard"];
        if (dash is not null)
            dash.Content = new DashboardView { DataContext = new DashboardViewModel() };

        Settings = new SettingsViewModel(Themes, chrome);
        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = Settings };

        chrome.Status.Message = "Idle";
        return window;
    }
}
