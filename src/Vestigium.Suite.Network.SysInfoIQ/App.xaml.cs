using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Vestigium.Controls.DependencyInjection;
using Vestigium.Controls.Shell;
using Vestigium.Converters;
using Vestigium.Converters.DependencyInjection;
using Vestigium.Suite.Network.Shell;
using Vestigium.Themes;

namespace Vestigium.Suite.Network.SysInfoIQ;

public partial class App : Application
{
    public static ThemeManager Themes { get; } = new();

    public static IServiceProvider Services { get; private set; } = null!;

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
        chrome.Status.Message = "Idle";
        return window;
    }
}
