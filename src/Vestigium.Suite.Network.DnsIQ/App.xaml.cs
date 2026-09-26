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

    public static DnsIqSession? Session { get; private set; }

    public static VestigiumShell? Shell { get; set; }

    public static void SetDashboardEnabled(bool enabled)
    {
        void Apply()
        {
            var item = Shell?["Dashboard"];
            if (item is not null)
                item.IsEnabled = enabled;

            if (Shell?.SelectItemCommand is IRelayCommand relay)
                relay.NotifyCanExecuteChanged();
            else
                CommandManager.InvalidateRequerySuggested();
        }

        if (Current is null)
        {
            Apply();
            return;
        }

        if (Current.Dispatcher.CheckAccess())
            Apply();
        else
            Current.Dispatcher.Invoke(Apply);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.DnsIQ, cfg =>
        {
            AnalyticsCatalog.Register(cfg);
            ChartsCatalog.Register(cfg);
        });

        ThemeCatalog.RegisterAll(Themes);
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
        var window = new DnsIqWindow(chrome);
        Shell = window.HostShell;

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

        var store = new DnsIqSettingsStore(DnsIqSettingsStore.DefaultRoot);
        var session = new DnsIqSession(store, Themes, chrome);
        Session = session;

        Settings = new SettingsViewModel(Themes, chrome) { Session = session };
        var dash = new DashboardViewModel();
        var dns = new MainViewModel
        {
            StatusBar = chrome.Status,
            Session = session,
            Dashboard = dash
        };
        session.Attach(dns, Settings);

        var dnsItem = window.HostShell["DnsIQ"];
        if (dnsItem is not null)
        {
            dnsItem.Content = new DnsIqView { DataContext = dns };
            window.HostShell.SelectedItem = dnsItem;
        }

        var dashItem = window.HostShell["Dashboard"];
        if (dashItem is not null)
        {
            dashItem.Content = new DashboardView { DataContext = dash };
            dashItem.IsEnabled = false;
        }

        dash.GoToDnsIq = () =>
        {
            var item = window.HostShell["DnsIQ"];
            if (item is not null)
                window.HostShell.SelectedItem = item;
        };
        dash.DashboardAvailabilityChanged = SetDashboardEnabled;

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = Settings };

        window.Loaded += (_, _) => Shell = window.HostShell;

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
