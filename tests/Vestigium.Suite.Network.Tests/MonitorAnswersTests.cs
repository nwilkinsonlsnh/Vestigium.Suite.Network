using Vestigium.Suite.Network.DnsIQ.ViewModels;
using Xunit;

namespace Vestigium.Suite.Network.Tests;

public sealed class MonitorAnswersTests
{
    [Fact]
    public void Type_prefixes_and_semicolons_become_values()
    {
        var values = MonitorAnswers.Values(
            "type: 5 www-www.bing.com.trafficmanager.net;type: 5 www.bing.com.edgekey.net;type: 5 e86303.dscx.akamaiedge.net");
        Assert.Equal(
            ["www-www.bing.com.trafficmanager.net", "www.bing.com.edgekey.net", "e86303.dscx.akamaiedge.net"],
            values);
    }

    [Fact]
    public void Mixed_cname_and_addresses_split()
    {
        var values = MonitorAnswers.Values("type: 5 cdn.webflow.com;198.202.211.1;");
        Assert.Equal(["cdn.webflow.com", "198.202.211.1"], values);
    }

    [Fact]
    public void Bare_address_list_splits_and_drops_the_trailing_semicolon()
    {
        var values = MonitorAnswers.Values("3.170.42.2;3.170.42.38;3.170.42.76;3.170.42.40;");
        Assert.Equal(["3.170.42.2", "3.170.42.38", "3.170.42.76", "3.170.42.40"], values);
    }

    [Fact]
    public void A_single_value_stays_one_row()
    {
        Assert.Equal(["54.187.119.242"], MonitorAnswers.Values("54.187.119.242"));
    }
}
