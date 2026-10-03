using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Suite.Network.RouteIQ.ViewModels;
using Vestigium.Suite.Network.RouteIQ.Views;
using Vestigium.Suite.Network.Shell;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.RouteIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.RouteIQ);

        Themes.RegisterSuiteV1();
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
        var window = new MainWindow(chrome);

        window.HostShell.ApplySpec(new VestigiumShellSpec
        {
            Items =
            {
                new VestigiumNavItemSpec("Routes")
                {
                    Title = "Routes",
                    Subject = "Route table",
                    Description = "Print the stack route table. Family filters the print."
                },
                new VestigiumNavItemSpec("Neighbors")
                {
                    Title = "Neighbors",
                    Subject = "Neighbor cache",
                    Description = "Print the stack neighbor cache. Refresh does not resolve one address."
                },
                new VestigiumNavItemSpec("Settings")
            }
        });

        var store = new RouteIqSettingsStore(RouteIqSettingsStore.DefaultRoot);
        var session = new RouteIqSession(store, Themes, chrome);
        var settings = new SettingsViewModel(Themes, chrome) { Session = session };
        var host = new MainViewModel();
        session.Attach(settings);

        var routes = window.HostShell["Routes"];
        if (routes is not null)
        {
            routes.Content = new RoutesView { DataContext = host };
            window.HostShell.SelectedItem = routes;
        }

        var neighbors = window.HostShell["Neighbors"];
        if (neighbors is not null)
            neighbors.Content = new NeighborsView { DataContext = host };

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = settings };

        window.Closed += (_, _) => session.Save();
        chrome.Status.Message = "Idle";
        return window;
    }
}
