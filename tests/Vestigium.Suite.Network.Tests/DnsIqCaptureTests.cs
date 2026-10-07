using Vestigium.Suite.Network.DnsIQ.ViewModels;

namespace Vestigium.Suite.Network.Tests;

public sealed class DnsIqCaptureTests
{
    [Fact]
    public void PR04_04_text_dump_loads_and_failed_open_clears()
    {
        var path = Path.Combine(Path.GetTempPath(), "dnsiq-" + Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, "login.microsoftonline.com notes.txt");
        try
        {
            var vm = new MainViewModel();
            var enabled = false;
            vm.HarAvailabilityChanged = value => enabled = value;
            vm.LoadCapture(path);
            Assert.True(enabled);
            Assert.Contains(vm.Hosts, h => h.Host == "login.microsoftonline.com");
            Assert.DoesNotContain(vm.Hosts, h => h.Host == "notes.txt");
            Assert.Equal("", vm.Hosts[0].Dns);

            vm.LoadCapture(path + ".missing");
            Assert.False(enabled);
            Assert.Empty(vm.Hosts);
            Assert.False(string.IsNullOrWhiteSpace(vm.Status));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
