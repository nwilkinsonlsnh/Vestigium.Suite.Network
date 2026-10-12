using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.ClosedXml;
using Vestigium.Suite.Network.Shell;
using Vestigium.Suite.Network.TraceIQ.ViewModels;
using Vestigium.Suite.Network.TraceIQ.Views;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.TraceIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.TraceIQ, cfg =>
        {
            AnalyticsCatalog.Register(cfg);
            ChartsCatalog.Register(cfg);
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
        MainWindow = CreateMainWindow();
        MainWindow.Show();

        base.OnStartup(e);
    }

    private static Window CreateMainWindow()
    {
        var chrome = Services.GetRequiredService<VestigiumDefaultWindowViewModel>();
        var window = new TraceIqWindow(chrome);

        window.HostShell.ApplySpec(new VestigiumShellSpec
        {
            Items =
            {
                new VestigiumNavItemSpec("TraceIQ")
                {
                    Title = "TraceIQ",
                    Subject = "One walk",
                    Description = "One target. One walk. Hop list."
                },
                new VestigiumNavItemSpec("Path Ping")
                {
                    Title = "Path Ping",
                    Subject = "Grade the path",
                    Description = "Enabled when a walk reaches the target."
                },
                new VestigiumNavItemSpec("Exports")
                {
                    Title = "Exports",
                    Subject = "Raw rows",
                    Description = "TraceIQ and Path Ping, as spreadsheets."
                },
                new VestigiumNavItemSpec("Dashboard")
                {
                    Title = "Dashboard",
                    Subject = "The grade",
                    Description = "Enabled when Path Ping finishes."
                },
                new VestigiumNavItemSpec("Settings")
                {
                    Title = "Settings",
                    Subject = "TraceIQ",
                    Description = "Knobs, MRU, theme."
                }
            }
        });

        var settings = new SettingsViewModel(Themes, chrome);
        settings.Load(TraceIqSettingsStore.Load());
        var trace = new MainViewModel(settings) { StatusBar = chrome.Status };
        if (!string.IsNullOrWhiteSpace(settings.LastTarget))
            trace.Target = settings.LastTarget;
        var path = new PathPingViewModel(settings) { StatusBar = chrome.Status };
        var dash = new DashboardViewModel(settings) { StatusBar = chrome.Status };
        var exports = new ExportsViewModel(trace, path) { StatusBar = chrome.Status };
        path.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(PathPingViewModel.Running))
                return;
            trace.GradeRunning = path.Running;
            trace.RefreshTrace();
        };
        trace.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(MainViewModel.IsBusy))
                return;
            path.WalkBusy = trace.IsBusy;
            path.RefreshRun();
        };
        settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.ShowLegend))
                dash.ApplyLegend();
            if (e.PropertyName == nameof(SettingsViewModel.SelectedThemeId))
                dash.Rebuild();
        };
        var dashItem = window.HostShell["Dashboard"];
        if (dashItem is not null)
        {
            dashItem.IsEnabled = false;
            dashItem.Content = new DashboardView { DataContext = dash };
        }
        path.Completed += (_, _) =>
        {
            dash.Offer(path.Target, path.Rows.ToList());
            if (dashItem is not null)
                dashItem.IsEnabled = dash.Ready;
        };
        var pathItem = window.HostShell["Path Ping"];
        if (pathItem is not null)
        {
            pathItem.IsEnabled = false;
            pathItem.Content = new PathPingView { DataContext = path };
        }
        var exportsItem = window.HostShell["Exports"];
        if (exportsItem is not null)
            exportsItem.Content = new ExportsView { DataContext = exports };
        trace.RouteReady += (_, _) =>
        {
            path.Offer(trace.Target, trace.Hops.ToList());
            if (pathItem is not null)
                pathItem.IsEnabled = path.Ready;
        };
        var item = window.HostShell["TraceIQ"];
        if (item is not null)
        {
            item.Content = new TraceView { DataContext = trace };
            window.HostShell.SelectedItem = item;
        }

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = settings };

        var elapsed = new StatusBarColumn
        {
            Key = "elapsed",
            Slot = StatusBarSlot.Center,
            Kind = StatusBarColumnKind.Text,
            Text = ""
        };
        chrome.Status.Engine.Columns.Insert(1, elapsed);
        chrome.Status.Message = "Idle";
        window.TabShown = key =>
        {
            if (key == "TraceIQ") trace.Publish();
            else if (key == "Path Ping") path.Publish();
            else if (key == "Dashboard") dash.Publish();
            else if (key == "Exports") chrome.Status.Message = "Exports";
        };
        return window;
    }
}
