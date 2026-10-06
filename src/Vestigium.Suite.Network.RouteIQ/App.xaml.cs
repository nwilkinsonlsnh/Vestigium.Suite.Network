using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Helpers.ClosedXml;
using Vestigium.Helpers.Kql;
using Vestigium.Logging;
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
            RouteIqCatalog.Register(cfg);
        });
        RouteIqLog.HostStarted();
        VestigiumLogger.BindLifetime(this);
        VestigiumLogger.Flush();
        DispatcherUnhandledException += OnDispatcherUnhandled;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandled;

        Themes.RegisterSuiteV1();
        Themes.Initialize(this, "LightBlue");

        var services = new ServiceCollection();
        services.AddVestigiumConverters();
        services.AddVestigiumControls();
        Services = services.BuildServiceProvider();
        VestigiumConverterHost.ServiceProvider = Services;

        ShutdownMode = ShutdownMode.OnMainWindowClose;
        var window = CreateMainWindow(out var host);
        CenterOnPrimary(window);
        MainWindow = window;
        window.ShowSplash("Opening", 0);
        window.Show();
        var prints = host.BeginPrints();
        window.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => _ = FinishSplash(window, prints));

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        RouteIqLog.HostStopped();
        VestigiumLogger.Flush();
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            RouteIqLog.Fail(e.Exception);
            e.Handled = true;
        }
        catch (Exception)
        {
            e.Handled = false;
        }
    }

    private static void OnDomainUnhandled(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            RouteIqLog.Fail(ex);
    }

    private static async Task FinishSplash(MainWindow window, Task prints)
    {
        await Task.WhenAny(prints, Task.Delay(TimeSpan.FromSeconds(8))).ConfigureAwait(true);
        window.ShowSplash("Connections", 100);
        await window.WarmConnections().ConfigureAwait(true);
        if (window.HostShell["RouteIQ"] is { } routes)
            window.HostShell.SelectedItem = routes;
        window.HideSplash();
    }

    private static void CenterOnPrimary(Window window)
    {
        var area = SystemParameters.WorkArea;
        var width = Math.Min(window.Width, area.Width);
        var height = Math.Min(window.Height, area.Height);
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Width = width;
        window.Height = height;
        window.Left = area.Left + (area.Width - width) / 2;
        window.Top = area.Top + (area.Height - height) / 2;
    }

    private static MainWindow CreateMainWindow(out MainViewModel host)
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
        host = new MainViewModel
        {
            Marks = settings.Marks,
            QueryBook = session,
            ReportStatus = text => chrome.Status.Message = text,
            ReportSplash = (text, percent) => window.ShowSplash(text, percent),
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

        var routesView = new RoutesView { DataContext = host };
        var connectionsView = new ConnectionsView { DataContext = host };
        var deck = new Grid { DataContext = host };
        deck.Children.Add(routesView);
        deck.Children.Add(connectionsView);
        window.HoldPages(routesView, connectionsView);

        var routes = window.HostShell["RouteIQ"];
        if (routes is not null)
        {
            routes.Content = deck;
            window.HostShell.SelectedItem = routes;
        }

        var neighbors = window.HostShell["Neighbors"];
        if (neighbors is not null)
            neighbors.Content = new NeighborsView { DataContext = host };

        var connections = window.HostShell["Connections"];
        if (connections is not null)
            connections.Content = deck;

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
