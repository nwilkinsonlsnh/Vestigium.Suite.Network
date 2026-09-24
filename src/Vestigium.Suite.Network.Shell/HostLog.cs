using Vestigium.Helpers.Network;
using Vestigium.Logging;

namespace Vestigium.Suite.Network.Shell;

public static class HostLog
{
    public static void Initialize(string appId)
    {
        if (string.IsNullOrWhiteSpace(appId))
            throw new ArgumentException("Host APPID is required.", nameof(appId));

        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Vestigium",
            "Logs",
            appId);

        Directory.CreateDirectory(root);
        VestigiumLogger.Initialize(cfg =>
        {
            cfg.AppId = appId;
            cfg.LogDirectory = root;
            NetworkCatalog.Register(cfg);
        });
    }
}
