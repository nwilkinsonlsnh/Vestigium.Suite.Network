using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Helpers.ClosedXml;
using Vestigium.Helpers.Kql;
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
        HostLog.Initialize(HostIds.RouteIQ, cfg =>
        {
            KqlLoggingCatalog.Register(cfg);
            ClosedXmlCatalog.Register(cfg);
        });

        Themes.RegisterSuiteV1();
        Themes.Initialize(this, "LightBlue");

        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        services.AddVestigiumControls();
        Services = services.BuildServiceProvider();
        VestigiumConverterHost.ServiceProvider = Services;

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        var window = CreateMainWindow();
        MainWindow = window;
        window.ShowSplash("Opening", 5);
        window.Show();
        window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => _ = Warm(window));

        base.OnStartup(e);
    }

    private static async Task Warm(MainWindow window)
    {
        var shell = window.HostShell;
        var steps = new (string Name, string Label, double Percent)[]
        {
            ("RouteIQ", "Routes", 15),
            ("Neighbors", "Neighbors", 30),
            ("Connections", "Connections", 45),
            ("NetBIOS", "NetBIOS", 60),
            ("LMHosts", "LMHOSTS", 70),
            ("Exports", "Exports", 80),
            ("Settings", "Settings", 90)
        };
        foreach (var step in steps)
            await window.Warm(step.Label, step.Percent, shell[step.Name]);

        window.ShowSplash("Prints", 95);
        var host = shell["RouteIQ"]?.Content is System.Windows.FrameworkElement view
            ? view.DataContext as ViewModels.MainViewModel
            : null;
        for (var i = 0; i < 80 && host is { PrintsReady: false }; i++)
            await Task.Delay(100);
        if (shell["RouteIQ"] is { } routes)
            shell.SelectedItem = routes;
        window.HideSplash();
    }

    private static MainWindow CreateMainWindow()
    {
        var chrome = Services.GetRequiredService<VestigiumDefaultWindowViewModel>();
        var window = new MainWindow(chrome);
        chrome.Status.Engine.Columns.Insert(1, StatusBarColumns.Create(StatusBarColumnKind.Text, column =>
        {
            column.Key = "query";
            column.Slot = StatusBarSlot.Center;
            column.Text = string.Empty;
        }));

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
                new VestigiumNavItemSpec("LMHosts")
                {
                    Title = "LMHosts",
                    Subject = "LMHOSTS file",
                    Description = "Read the system LMHOSTS file."
                },
                new VestigiumNavItemSpec("Exports")
                {
                    Title = "Exports",
                    Subject = "Workbook",
                    Description = "Write the checked prints to one workbook."
                },
                new VestigiumNavItemSpec("Settings")
            }
        });

        var store = new RouteIqSettingsStore(RouteIqSettingsStore.DefaultRoot);
        var session = new RouteIqSession(store, Themes, chrome);
        var settings = new SettingsViewModel(Themes, chrome) { Session = session };
        var host = new MainViewModel
        {
            Marks = settings.Marks,
            QueryBook = session,
            ReportStatus = text => chrome.Status.Message = text,
            ReportQuery = text => chrome.Status.Engine.PostImmediate("query", new StatusBarUpdate
            {
                Text = text,
                Icon = string.IsNullOrWhiteSpace(text) ? StatusBarIconKind.None : StatusBarIconKind.Error
            }),
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
            OuiPoolSize = () => session.OuiPoolSize,
            LiveVendorLookup = () => session.Current.LiveVendorLookup
        };
        session.Attach(settings);
        host.WatchSeconds = session.WatchSeconds;
        host.ExportRoutes = session.ExportRoutes;
        host.ExportNeighbors = session.ExportNeighbors;
        host.ExportConnections = session.ExportConnections;
        host.ExportNetBios = session.ExportNetBios;
        host.ExportLmHosts = session.ExportLmHosts;
        host.ExportOpenAfter = session.ExportOpenAfter;
        host.ExportOpenFolder = session.ExportOpenFolder;

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

        var lmhosts = window.HostShell["LMHosts"];
        if (lmhosts is not null)
            lmhosts.Content = new LmHostsView { DataContext = host };

        var exports = window.HostShell["Exports"];
        if (exports is not null)
            exports.Content = new ExportsView { DataContext = host };

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = settings };

        window.Closed += (_, _) => session.Save();
        chrome.Status.Message = string.Empty;
        return window;
    }
}
