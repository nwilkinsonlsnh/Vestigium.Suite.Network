using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Controls.StatusBar;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
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
        HostLog.Initialize(HostIds.TraceIQ);

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
        return window;
    }
}
