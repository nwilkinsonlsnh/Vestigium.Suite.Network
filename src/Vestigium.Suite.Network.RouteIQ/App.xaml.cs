using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
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
                new VestigiumNavItemSpec("RouteIQ")
                {
                    Title = "RouteIQ",
                    Subject = "Route table",
                    Description = "IPv4 and IPv6 prints."
                },
                new VestigiumNavItemSpec("Neighbors")
                {
                    Title = "Neighbors",
                    Subject = "Neighbor cache",
                    Description = "Print the stack neighbor cache."
                },
                new VestigiumNavItemSpec("Connections")
                {
                    Title = "Connections",
                    Subject = "Connection table",
                    Description = "Snapshot, then watch added, dropped, and returned rows."
                },
                new VestigiumNavItemSpec("NetBIOS")
                {
                    Title = "NetBIOS",
                    Subject = "NetBIOS names",
                    Description = "Local names and the remote name cache."
                },
                new VestigiumNavItemSpec("LMHOSTS")
                {
                    Title = "LMHOSTS",
                    Subject = "LMHOSTS file",
                    Description = "Read the system LMHOSTS file."
                },
                new VestigiumNavItemSpec("Settings")
            }
        });

        var store = new RouteIqSettingsStore(RouteIqSettingsStore.DefaultRoot);
        var session = new RouteIqSession(store, Themes, chrome);
        var settings = new SettingsViewModel(Themes, chrome) { Session = session };
        var host = new MainViewModel
        {
            ReportStatus = text => chrome.Status.Message = text,
            ReportWatch = (left, total) =>
            {
                if (total <= 0 || left <= 0)
                {
                    chrome.Status.Message = string.Empty;
                    chrome.Status.Engine.PostImmediate("progress", new StatusBarUpdate { Progress = 0, IsProgressVisible = false });
                    return;
                }

                chrome.Status.Message = "Watch " + left + "s left";
                chrome.Status.Engine.PostImmediate("progress", new StatusBarUpdate
                {
                    Progress = (total - left) * 100d / total,
                    IsIndeterminate = false,
                    IsProgressVisible = true
                });
            },
            OuiPoolSize = () => session.OuiPoolSize
        };
        session.Attach(settings);

        var routes = window.HostShell["RouteIQ"];
        if (routes is not null)
        {
            routes.Content = new RoutesView { DataContext = host };
            window.HostShell.SelectedItem = routes;
        }

        var neighbors = window.HostShell["Neighbors"];
        if (neighbors is not null)
            neighbors.Content = new NeighborsView { DataContext = host };

        var connections = window.HostShell["Connections"];
        if (connections is not null)
            connections.Content = new ConnectionsView { DataContext = host };

        var netbios = window.HostShell["NetBIOS"];
        if (netbios is not null)
            netbios.Content = new NetBiosView { DataContext = host };

        var lmhosts = window.HostShell["LMHOSTS"];
        if (lmhosts is not null)
            lmhosts.Content = new LmHostsView { DataContext = host };

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = settings };

        window.Closed += (_, _) => session.Save();
        chrome.Status.Message = string.Empty;
        return window;
    }
}
