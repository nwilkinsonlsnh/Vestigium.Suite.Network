using Vestigium.Helpers.LogParser;
using Vestigium.Helpers.Network;
using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

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

    [Fact]
    public void PR04_05_five_hundred_hits_are_one_row_and_address_is_skipped()
    {
        var repeated = Enumerable.Range(0, 500).Select(_ => new LogHost("login.microsoftonline.com", null, 1, LogHostSource.Url, false)).ToArray();
        var unique = CaptureLoader.Unique(repeated);
        Assert.Single(unique);
        Assert.Equal(500, unique[0].HitCount);

        var skipped = CaptureProbe.Map(true, "75.2.119.14", null, null);
        Assert.Equal("Skipped", skipped.Dns);
        Assert.Equal("75.2.119.14", skipped.Answers);

        var resolved = CaptureProbe.Map(false, "q2prod.idbs-cloud.com", Answer("10.0.0.1"), Answer("::1"));
        Assert.Equal("Resolved", resolved.Dns);
        Assert.Contains("10.0.0.1", resolved.Answers);
    }

    private static DnsLookupResult Answer(string data)
        => new("q2prod.idbs-cloud.com", DnsRecordType.A, "8.8.8.8", DnsRcode.NoError, false, false, TimeSpan.Zero, [new DnsRecord(DnsRecordType.A, "q2prod.idbs-cloud.com", 30, data)]);
}
