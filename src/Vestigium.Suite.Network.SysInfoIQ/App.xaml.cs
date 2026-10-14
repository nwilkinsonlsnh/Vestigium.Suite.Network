using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Suite.Network.Shell;
using Vestigium.Suite.Network.SysInfoIQ.ViewModels;
using Vestigium.Suite.Network.SysInfoIQ.Views;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.SysInfoIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

    public static SettingsViewModel? Settings { get; private set; }

    public static SysInfoSession? Session { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        HostLog.Initialize(HostIds.SysInfoIQ);

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
        var window = new SysInfoIqWindow(chrome);

        window.HostShell.ApplySpec(new VestigiumShellSpec
        {
            Items =
            {
                new VestigiumNavItemSpec("SysInfo")
                {
                    Title = "SysInfo",
                    Subject = "Inventory",
                    Description = "Collect, organize, and export a local system inventory."
                },
                new VestigiumNavItemSpec("Settings")
            }
        });

        var store = new SysInfoSettingsStore(SysInfoSettingsStore.DefaultRoot);
        var session = new SysInfoSession(store, Themes, chrome);
        Session = session;

        Settings = new SettingsViewModel(Themes, chrome) { Session = session };
        session.Attach(Settings);

        var main = new MainViewModel
        {
            StatusBar = chrome.Status,
            Settings = Settings
        };

        var sysItem = window.HostShell["SysInfo"];
        if (sysItem is not null)
        {
            sysItem.Content = new SysInfoView { DataContext = main };
            window.HostShell.SelectedItem = sysItem;
        }

        var settingsItem = window.HostShell["Settings"];
        if (settingsItem is not null)
            settingsItem.Content = new SettingsView { DataContext = Settings };

        chrome.Status.Message = "Idle";
        return window;
    }
}
