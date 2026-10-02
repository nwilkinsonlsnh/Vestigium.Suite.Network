using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Helpers.Analytics;
using Vestigium.Helpers.Charts;
using Vestigium.Helpers.PerfMon;
using Vestigium.Helpers.PerfMon.Cpu;
using Vestigium.Helpers.PerfMon.Memory;
using Vestigium.Helpers.PerfMon.Network;
using Vestigium.Suite.Network.NicIQ.ViewModels;
using Vestigium.Suite.Network.NicIQ.Views;
using Vestigium.Suite.Network.Shell;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.NicIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    public static SettingsViewModel? Settings { get; private set; }

    public static NicIqSession? Session { get; private set; }

    public static VestigiumShell? Shell { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.NicIQ, cfg =>
        {
            AnalyticsCatalog.Register(cfg);
            ChartsCatalog.Register(cfg);
            PerfMonCatalog.Register(cfg);
            CpuPerfCatalog.Register(cfg);
            MemoryPerfCatalog.Register(cfg);
            NetworkPerfCatalog.Register(cfg);
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
        var window = new NicIqWindow(chrome);
        Shell = window.HostShell;

        window.HostShell.ApplySpec(new VestigiumShellSpec
        {
            Items =
            {
                new VestigiumNavItemSpec("NicIQ")
                {
                    Title = "NicIQ",
                    Subject = "Adapters",
                    Description = "Workstation adapters. Watch status. Double-click a row for detail."
                },
                new VestigiumNavItemSpec("Monitoring")
                {
                    Title = "Monitoring",
                    Subject = "Live counters",
                    Description = "Primary NIC first. Pick another active adapter to switch."
                },
                new VestigiumNavItemSpec("Settings")
            }
        });

        var store = new NicIqSettingsStore(NicIqSettingsStore.DefaultRoot);
        var session = new NicIqSession(store, Themes, chrome);
        Session = session;

        Settings = new SettingsViewModel(Themes, chrome) { Session = session };
        var nic = new MainViewModel
        {
            StatusBar = chrome.Status,
            Session = session,
            Settings = Settings
        };
        Settings.Host = nic;
        session.Attach(nic, Settings);
        nic.RefreshCommand.Execute(null);
        nic.StartMonitoring();

        var nicItem = window.HostShell["NicIQ"];
        if (nicItem is not null)
        {
            nicItem.Content = new NicIqView { DataContext = nic };
            window.HostShell.SelectedItem = nicItem;
        }

        var monitorItem = window.HostShell["Monitoring"];
        if (monitorItem is not null)
            monitorItem.Content = new MonitoringView { DataContext = nic };

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = Settings };

        window.Loaded += (_, _) =>
        {
            Shell = window.HostShell;
            nic.StartMonitoring();
        };
        window.Closed += (_, _) =>
        {
            session.Save();
            nic.StopMonitoring();
        };
        chrome.Status.Message = "Idle";
        chrome.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(VestigiumDefaultWindowViewModel.ShowStatusBar)
                or nameof(VestigiumDefaultWindowViewModel.Status))
                session.Save();
        };
        return window;
    }
}
