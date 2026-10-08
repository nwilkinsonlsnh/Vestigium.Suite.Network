using System.IO;
using Vestigium.Helpers.LogParser;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class CaptureLoaderCapTests
{
    [Fact]
    public void A_har_at_32_mb_does_not_replace_the_grid()
    {
        var path = Path.Combine(Path.GetTempPath(), "dnsiq-" + Guid.NewGuid().ToString("N") + ".har");
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            stream.SetLength(CaptureLoader.MaxHarBytes);

        try
        {
            var vm = new MainViewModel();
            vm.Hosts.Add(new HarHostRow(new LogHost("keep.example", null, 1, LogHostSource.Url, false)));
            vm.LoadCapture(path);

            Assert.Equal(CaptureLoader.HarCapStatus, vm.Status);
            Assert.Contains(vm.Hosts, host => host.Host == "keep.example");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void A_text_file_is_not_the_har_gate()
    {
        var path = Path.Combine(Path.GetTempPath(), "dnsiq-" + Guid.NewGuid().ToString("N") + ".txt");
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            stream.SetLength(CaptureLoader.MaxHarBytes);

        try
        {
            Assert.False(CaptureLoader.IsOverHarCap(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Entry_errors_come_from_the_har_pin_not_loghost()
    {
        var path = Path.Combine(Path.GetTempPath(), "dnsiq-" + Guid.NewGuid().ToString("N") + ".har");
        File.WriteAllText(path, "{\"log\":{\"version\":\"1.2\",\"creator\":{\"name\":\"t\",\"version\":\"1\"},\"entries\":[{\"request\":{\"method\":\"GET\",\"url\":\"https://edge.example/\"},\"_error\":\"net::ERR_FAILED\"}]}}");
        try
        {
            var errors = CaptureLoader.EntryErrors(path);
            Assert.Equal("net::ERR_FAILED", errors["edge.example"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void A_small_har_still_fills_the_error_column()
    {
        var path = Path.Combine(Path.GetTempPath(), "dnsiq-" + Guid.NewGuid().ToString("N") + ".har");
        File.WriteAllText(path, """
            {"log":{"version":"1.2","creator":{"name":"t","version":"1"},"entries":[{"request":{"method":"GET","url":"https://edge.example/"},"_error":"net::ERR_FAILED"}]}}
            """);
        try
        {
            var vm = new MainViewModel();
            vm.LoadCapture(path);

            var row = Assert.Single(vm.Hosts);
            Assert.Equal("edge.example", row.Host);
            Assert.Equal("net::ERR_FAILED", row.Error);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
