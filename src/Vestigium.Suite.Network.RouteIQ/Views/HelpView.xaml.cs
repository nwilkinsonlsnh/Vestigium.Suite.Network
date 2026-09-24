using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Vestigium.Suite.Network.RouteIQ.Views;

public partial class HelpView: UserControl
{
    public HelpView( )
    {
        Topics = new ObservableCollection<HelpTopic>(Build( ));
        InitializeComponent( );
        DataContext = this;
        Selected = Topics[0];
        Selected.IsSelected = true;
        Show(Selected);
    }

    public ObservableCollection<HelpTopic> Topics { get; }

    public HelpTopic? Selected { get; private set; }

    private void Show(HelpTopic topic)
    {
        Selected = topic;
        if (topic.Sections.Count == 0)
        {
            PageSections.Visibility = System.Windows.Visibility.Collapsed;
            PageSections.ItemsSource = null;
            Reader.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Bind(topic.Document);
            return;
        }

        PageSections.ItemsSource = topic.Sections;
        PageSections.Visibility = System.Windows.Visibility.Visible;
        Reader.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        var section = topic.Sections.FirstOrDefault(s => s.IsSelected) ?? topic.Sections[0];
        section.IsSelected = true;
        ShowPages(section);
    }

    private void Bind(FlowDocument document)
    {
        document.SetResourceReference(FlowDocument.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        document.SetResourceReference(FlowDocument.BackgroundProperty, "Vestigium.Brushes.Surface.Window");
        Reader.Document = document;
    }

    private IEnumerable<HelpTopic> Build( )
    {
        yield return Topic("Overview",
            "RouteIQ is a read-only print of the Windows network stack on this machine. It exists so an operator can see how the host believes it is connected, without opening a command window and without changing the stack to look at it.",
            "A route problem is usually a disagreement between two prints. One desk can reach a host. Another cannot. A name resolves, and the packet still leaves by the wrong gateway. RouteIQ is the local half of that argument. It shows the route table the stack will use, the neighbor cache it has already learned, the connections it is holding, the NetBIOS names it is advertising or has cached, and the LMHOSTS file it will consult. Those five prints are the record. Exports writes them to a workbook. Settings is where the window is adjusted. Help is this page. The window finishes those prints before the tabs answer a click.",
            "The RouteIQ tab is the forwarding table. It prints IPv4 and IPv6 separately, because they are separate tables. Each row is a destination, a prefix, a gateway, the interface that will carry it, a metric, and the protocol that installed the route. Refresh reads the stack again. Copy takes the print. The query bar narrows that print. It does not ask another machine.",
            "Neighbors is the cache behind those routes. It is the address the stack has already resolved on the local link, the hardware address it resolved to, the interface, and the state of that entry. Refresh reads the cache again and fills the round-trip time. There is no address box. The tab does not scan the network. It prints what the stack already holds, then asks the entries it has.",
            "Connections is the session table, and it is the only tab that watches. Refresh writes a baseline. Watch then holds that set for the window you set and marks rows that were already there, rows that appeared, rows that left, and rows that came back. The point is a change during the watch, not a second copy of the table.",
            "NetBIOS prints the names this host owns and the names it has cached, grouped by adapter. LMHosts reads the system file and shows the rows in it. It does not edit the file. Neither tab has a query bar. Exports writes the loaded prints. It does not read the stack again. Settings holds the palette, the length of the saved query list, and the status bar.",
            "RouteIQ does not install, change, or remove a route. The default route is not a control. The window does not shell out to route.exe, netsh, or arp. If the print is wrong, the correction is made elsewhere. This host is the record of what the stack said.",
            "That record is also written under %ProgramData%\\Vestigium\\Logs\\RouteIQ\\. The grids are the print you came for. The log is the copy that remains after the window closes.");
        yield return Topic("Routes",
            "The RouteIQ tab is the forwarding table. It is the set of destinations this host will try, the gateway it will hand each one to, and the interface that will carry it. IPv4 and IPv6 are printed separately, because they are separate tables. There is no family switch. Both are on the page.",
            "Refresh reads the stack again and replaces both prints. Copy takes the print. The query bar narrows this tab only. A query here does not change Neighbors or Connections, and it does not ask another machine.",
            Head("Read a row"), "Destination and PrefixLength are the network the row covers. Mask is that prefix written out. Gateway is the next hop. An on-link route has no next hop to send to. InterfaceName and InterfaceIndex are the adapter that will carry the packet. Metric is how the stack ranks this row against another row that covers the same destination. Protocol is what installed the route: local, static, DHCP, a routing protocol, or the stack itself.",
            "The default route is the row whose destination is 0.0.0.0 or ::, with a prefix length of 0. It is on the print when the stack has one. It is not a control. This tab does not add it, change it, or remove it.",
            Head("When to use it"), "Use the print when a packet leaves by the wrong gateway, when two interfaces both claim the same destination, or when a path works on one host and not on this one. The row with the longest matching prefix is the one the stack will use. Metric breaks a tie between two rows of the same length.",
            Head("Columns"), "Both tables: Destination, PrefixLength, Mask, Gateway, InterfaceName, InterfaceIndex, Metric, Protocol.");
        yield return Topic("Neighbors",
            "Neighbors is the cache behind the route table. It is the list of addresses this host has already resolved on the local link: the IP address, the hardware address it resolved to, the interface that learned it, and the state of that entry. IPv4 and IPv6 are printed separately. Both are on the page.",
            "Refresh reads the cache again and replaces both prints. It then asks the entries the cache already holds and fills the round-trip time. There is no address box. The tab does not scan the network, and it does not take an address to probe. Copy takes the print. The query bar narrows this tab only. A query here does not change Routes or Connections.",
            Head("Read a row"), "Address is the neighbor. Class is the address class. MacAddress is the hardware address resolved for it. InterfaceName and InterfaceIndex are the adapter that holds the entry. State is where that entry sits in the cache: reachable, stale, incomplete, and the other states the stack reports. IsMulticast and IsBroadcast mark those kinds of entry. Vendor is the name resolved from the hardware prefix, when the lookup has one. RTT (ms) is the round-trip time filled on refresh. A blank time means that entry was not asked, or did not answer.",
            "The IPv6 print adds two columns. IsRouter marks a neighbor the stack believes is a router. LastReachable is the last time that entry was confirmed.",
            Head("When to use it"), "Use the print when a gateway is on the route table and the host still cannot reach it, when an address resolves to the wrong hardware address, or when a neighbor that should be on the link is missing. A missing row means the stack has not learned it. It does not mean the host is down.",
            Head("Columns"), "IPv4: Address, Class, MacAddress, InterfaceName, InterfaceIndex, State, IsMulticast, IsBroadcast, Vendor, RTT (ms).",
            "IPv6 adds IsRouter and LastReachable.");
        yield return Topic("Connections",
            "Connections is the session table. It is the only tab that watches. Refresh writes a baseline of the connections the stack is holding. Watch then holds that set for the window you set, 5 to 180 seconds, and marks what changed. Copy takes the print. The query bar narrows this tab only. A query here does not change Routes or Neighbors.",
            "The point of the tab is the watch, not a second copy of the table. Refresh is the baseline. The marks are measured against that baseline, not against the last watch.",
            "Open is a row that was in the baseline. Added is a row that was not in the baseline and showed up during the watch. Dropped is a row that was present, then left. The row stays. Reopened is a row that came back after a drop. The row stays. Legend names the marks.",
            Head("Read a row"), "Protocol is TCP or UDP. Local and its port are this host. Remote and its port are the other end. Service is the common name for that port, when the port is known. State is the session state the stack reports. Process is the process that owns the row, when the stack will say. Time is seconds in the last watch, not the age of the connection.",
            Head("When to use it"), "Use the print when a service is listening and should not be, when a connection appears during a test and you need the row that arrived, or when a session leaves and you need to see that it left rather than watch the table shrink.",
            Head("Columns"), "Status, Protocol, Local, Port, Remote, Port, Service, State, Process, Time.");
        yield return Topic("NetBIOS",
            "NetBIOS is the name print. It shows the names this host owns and the names it has cached, grouped by adapter. Refresh reads the stack again. Copy takes the print. This tab has no query bar.",
            "IsCache separates the two kinds of row. A cache row is a name this host has learned. A row that is not cache is a name this host owns. Adapter and NodeAddress are where the name sits. Name is the NetBIOS name. Suffix and SuffixName are the service that name is registered for. Type is unique or group. Status is the registration state. Address is the address bound to a cache row. LifeSeconds is how long a cache row has left.",
            Head("When to use it"), "Use the print when a name resolves on one host and not on this one, or when this host is advertising a name it should not. A missing name means the stack has not learned it. It does not mean the other host is down.",
            Head("Columns"), "IsCache, Adapter, NodeAddress, Name, Suffix, SuffixName, Type, Status, Address, LifeSeconds.");
        yield return Topic("Exports",
            "Exports writes the prints already loaded. It does not read the stack again, and it does not write the filtered view. Cover is always written. A checked print becomes its own sheet. The buttons stay off until those prints are in.",
            "Export writes the checked prints. Export All writes every print and ignores the checks. The dialog opens in Desktop\\Vestigium\\Exports\\RouteIQ. The workbook stays on this machine. It contains routes, neighbor MACs, process names, NetBIOS names, and LMHOSTS lines. It is not uploaded. Open after saving opens that workbook only. The checks are remembered.",
            Head("Cover"), "Host, operator, and the time the file was taken. The query lines are what was on screen. The counts are the loaded prints. Connection Time on the Connections sheet is seconds in the last watch, not the age of the connection.",
            Head("When to use it"), "Use the workbook when another desk needs the print, or when a change request needs the rows the stack was holding. The file is the record. This tab does not compare two files.");
        yield return Topic("LMHosts",
            "LMHosts reads the system LMHOSTS file and shows the rows in it. The file keeps its Windows name. The tab is not an editor. Refresh reads the file again. Copy takes the print. This tab has no query bar. A missing file is normal. The print is empty, and the caption says so.",
            Head("Read a row"), "Address is the address the line names. Name is the name it maps. Preload marks a row the stack will load at start. Domain marks a domain row. MultiHome marks a name with more than one address. Include is a file this line pulls in. Raw is the line as it was read.",
            Head("When to use it"), "Use the print when a name resolves here and nowhere else, or when a hosts-style mapping is still in force after DNS says otherwise. The page shows the file. It does not apply it.",
            Head("Columns"), "Address, Name, Preload, Domain, MultiHome, Include, Raw.");
        yield return QueryTopic( );
        yield return GlossaryTopic( );
        yield return AboutTopic( );
        yield return Topic("License", LicenseLines( ));
    }

    private HelpTopic GlossaryTopic( )
    {
        var topic = Topic("Glossary",
            "The words this window uses. A short meaning, in the sense of this print.");
        topic.Sections.Add(Section("Tabs",
            Term("Route", "The forwarding table. Each row is a destination the stack will try, the gateway it will hand it to, and the interface that will carry it."),
            Term("Neighbor", "An address this host has already resolved on the local link. The hardware address, the interface, and the state of that entry."),
            Term("Connection", "A session the stack is holding. Local end, remote end, protocol, and state."),
            Term("NetBIOS", "A name service on the local network. This tab shows the names this host owns and the names it has cached, grouped by adapter. It is not DNS."),
            Term("LMHosts", "The tab that reads the name-to-address file. It is not an editor."),
            Term("LMHosts file", "The system file that maps a name to an address. The tab shows the rows in it. A missing file is normal. The print is empty."),
            Term("Export", "The checked prints, written to one workbook. Cover is always included."),
            Term("Export All", "Every print, written to one workbook. The checks are ignored."),
            Term("Cover", "The first sheet. Host, operator, time, the queries on screen, and the count of each print that was written."),
            Term("TCP", "Connection-oriented. A session is opened, the stack keeps state for it, and the row has a local port and a remote port."),
            Term("UDP", "Connectionless. A datagram is sent. The stack does not keep a session the way TCP does. The row can still show a local port and a remote port.")));
        topic.Sections.Add(Section("Route",
            Term("Destination", "The network the row covers."),
            Term("Prefix length", "How many bits of the destination are the network. A longer prefix is the more specific row."),
            Term("Subnet mask", "That prefix written out."),
            Term("Gateway", "The next hop. An on-link route has no next hop to send to."),
            Term("Interface index", "The number the stack uses for the adapter that will carry the packet."),
            Term("Metric", "How the stack ranks this row against another row that covers the same destination. Lower is preferred when the prefix length is the same."),
            Term("Protocol", "What installed the route: local, static, DHCP, or the stack. On Connections, protocol is TCP or UDP.")));
        topic.Sections.Add(Section("Watch",
            Term("Baseline", "The connection print written by Refresh. The marks are measured against it."),
            Term("Watch", "The window, 5 to 180 seconds, that marks what changed after the baseline."),
            Term("Open", "A row that was in the baseline."),
            Term("Added", "A row that was not in the baseline and showed up during the watch."),
            Term("Dropped", "A row that was present, then left. The row stays."),
            Term("Reopened", "A row that came back after a drop. The row stays."),
            Term("RTT", "Round-trip time, in milliseconds, filled on Neighbors refresh. The field is neighbors.rtt.")));
        var ports = Section("Ports");
        ports.Pages.Add(PortPage("1-110", 1, 110));
        ports.Pages.Add(PortPage("111-995", 111, 995));
        ports.Pages.Add(PortPage("Registered", 1024, 49151));
        ports.Pages[0].IsSelected = true;
        topic.Sections.Add(ports);
        topic.Sections[0].IsSelected = true;
        Wire(topic);
        return topic;
    }

    private static HelpTopic PortPage(string title, int from, int to)
    {
        var lines = new List<string>( );
        var seen = new HashSet<int>( );
        foreach (var row in ViewModels.ConnectionServices.Catalog)
        {
            if (row.Port < from || row.Port > to || !seen.Add(row.Port))
                continue;
            var names = string.Join(", ", ViewModels.ConnectionServices.Catalog.Where(item => item.Port == row.Port).Select(item => item.Name + " (" + item.Protocols + ")"));
            lines.Add("\x16" + row.Port + "|" + names);
        }

        return Section(title, lines.ToArray( ));
    }

    private HelpTopic AboutTopic( )
    {
        var version = Assembly.GetExecutingAssembly( ).GetName( ).Version?.ToString( ) ?? "unknown";
        var topic = Topic("About", "RouteIQ " + version + ".");
        topic.Sections.Add(Section("App",
            "RouteIQ " + version + ". A read-only print of the route table, the neighbor cache, the connection table, NetBIOS names, and the system LMHOSTS file. Exports writes those prints to a workbook.",
            "Copyright (c) 2026 Nathaniel Wilkinson. MIT License. The notice is the License topic.",
            "The public source for this host is Vestigium.Suite.Network.",
            Link("https://github.com/nwilkinsonlsnh/Vestigium.Suite.Network", "Vestigium.Suite.Network on GitHub")));
        topic.Sections.Add(Section("Vestigium",
            "Vestigium is the library set this host is built from. These are my published packages. The name opens the public package page.",
            Pack("Vestigium.Themes", "1.0.6", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Controls", "1.0.0", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Controls.StatusBar", "1.0.1", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Controls.NumericUpDown", "1.0.2", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Controls.QueryBar", "1.0.3", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Converters", "1.0.0", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Helpers.Kql", "1.0.3", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Logging", "1.7.1", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Helpers.Network", "1.4.5", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Helpers.Analytics", "1.0.1", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Helpers.Charts", "1.0.9", "Nathaniel Wilkinson", "MIT"),
            Pack("Vestigium.Helpers.ClosedXml", "1.0.1", "Nathaniel Wilkinson", "MIT"),
            "Logging and Network come through Shell. Analytics and Charts are Shell packages. This window does not plot. ClosedXml writes the Exports workbook."));
        topic.Sections.Add(Section("Referenced",
            "These two are not Vestigium.",
            Pack("Microsoft.Extensions.DependencyInjection", "10.0.12", "Microsoft", "MIT"),
            Pack("CommunityToolkit.Mvvm", "8.4.0", "Community Toolkit", "MIT")));
        topic.Sections[0].IsSelected = true;
        Wire(topic);
        return topic;
    }

    private static string[ ] LicenseLines( )
    {
        var path = Path.Combine(AppContext.BaseDirectory, "LICENSE");
        if (File.Exists(path))
        {
            var text = File.ReadAllText(path).Trim( );
            if (text.StartsWith("MIT License", StringComparison.Ordinal))
                return text.Split(new[ ] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        return FallbackLicense;
    }

    private static readonly string[ ] FallbackLicense =
    [
        "MIT License",
        "",
        "Copyright (c) 2026 Nathaniel Wilkinson",
        "",
        "Permission is hereby granted, free of charge, to any person obtaining a copy",
        "of this software and associated documentation files (the \"Software\"), to deal",
        "in the Software without restriction, including without limitation the rights",
        "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell",
        "copies of the Software, and to permit persons to whom the Software is",
        "furnished to do so, subject to the following conditions:",
        "",
        "The above copyright notice and this permission notice shall be included in all",
        "copies or substantial portions of the Software.",
        "",
        "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
        "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,",
        "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE",
        "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER",
        "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,",
        "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE",
        "SOFTWARE.",
    ];


    private HelpTopic QueryTopic( )
    {
        var topic = Topic("Query", "The query bar filters the print on this tab. Pick a section.");
        topic.Sections.Add(Section("Bar",
            "The bar is how you narrow the print in front of you. You type a comparison, and the grid keeps the rows that match. The rows that do not match leave the grid. They are not deleted from the stack. Clear the bar and the print comes back.",
            "RouteIQ, Neighbors, and Connections each have a bar, and each bar has its own fields. A query you write on Routes stays on Routes. It does not follow you to Neighbors. NetBIOS and LMHosts are prints without a bar. You read those grids whole.",
            "A query is three parts. A field, an operator, and a value. The field is a column on this tab. On Routes that is the destination, the gateway, the prefix, the interface, or the protocol that installed the route. On Neighbors it is the address, the hardware address, the state, or the round-trip time. On Connections it is the protocol, the port, the remote address, the process, or the watch mark. You can write the short name when the tab has only one column of that name: destination on Routes, protocol on Connections.",
            "The operator says how to compare. == keeps an exact value. BEGINS WITH keeps a prefix. CONTAINS keeps a fragment. A number uses >= or LTE. BETWEEN keeps a range, and the word inside it is AND. && and AND keep a row only when both sides match. || and OR keep a row when either side matches. AND is read before OR. Parentheses are how you say otherwise. The Simple, Common, and Advanced sections are those operators, with a query you can type.",
            "Enter saves the query on this tab's list. The next time you open the list, that query is there. Settings sets how long the list is. A saved query is still a filter for this tab. It does not become a second print."));
        topic.Sections.Add(Section("Simple",
            "One comparison. Each block is one query.",
            "An exact destination.",
            Code("route.destination == ipaddress(172.16.0.15)"),
            "A protocol the stack installed.",
            Code("route.protocol == netmgmt"),
            "A neighbor the cache still considers reachable.",
            Code("neighbors.state == reachable"),
            "A TCP session.",
            Code("connections.protocol == tcp"),
            "A local port.",
            Code("connections.localport == 443"),
            "An exact hardware address. ipaddress() is an address. macaddress() is a hardware address. A closed value can be written bare, or as route.protocol(local). A free string is quoted, or wrapped in string().",
            Code("neighbors.macaddress == macaddress(00:e0:4c:0f:31:b4)"),
            "An interface whose name contains a word.",
            Code("route.interfacename CONTAINS 'ethernet'"),
            "A process whose name contains a word.",
            Code("connections.process CONTAINS string(chrome)"),
            "== is exact. ipaddress(172) is not a complete address, so it will not compile on ==. Use BEGINS WITH for a prefix."));
        topic.Sections.Add(Section("Common",
            "A prefix, a fragment, a number, a range. Each block is one query.",
            "Destinations that start with 172.",
            Code("destination BEGINS WITH ipaddress(172)"),
            "A destination that contains the octet pair 16.0. 172.160 does not hit.",
            Code("route.destination CONTAINS ipaddress(16.0)"),
            "A gateway, and a prefix of at least 16.",
            Code("route.gateway == ipaddress(172.16.0.1) && route.prefixlength >= 16"),
            "A neighbor whose round-trip time is at least 50 ms. The field is neighbors.rtt.",
            Code("neighbors.rtt >= 50"),
            "A neighbor the stack believes is a router.",
            Code("neighbors.isrouter == true"),
            "A local port at or below 1000.",
            Code("connections.localport LTE 1000"),
            "A session the stack has held from 1 to 30 seconds. BETWEEN is inclusive. The word inside it is AND.",
            Code("connections.time BETWEEN 1 AND 30"),
            "A row the watch marked added.",
            Code("connections.status == added"),
            "A hardware fragment has to be whole octets. macaddress(4c:0f) is a fragment. macaddress(4c0) is not."));
        topic.Sections.Add(Section("Advanced",
            "AND binds tighter than OR. NOT binds tighter than AND. Parentheses are how you say otherwise.",
            Code("route.protocol == netmgmt || route.protocol == local && route.prefixlength >= 16"),
            "That is read as netmgmt, or (local and a prefix of at least 16). It is not (netmgmt or local) and a prefix of at least 16.",
            Code("(route.protocol == route.protocol.netmgmt || route.protocol == route.protocol.local) && route.prefixlength >= 16"),
            "The same rule on Connections. A TCP row on port 80 does not hit. A TCP row on 443 does. A UDP row on 53 does.",
            Code("(connections.protocol == tcp || connections.protocol == udp) && (connections.localport == 443 || connections.localport == 53)"),
            "NOT wraps a comparison. It does not replace the word with !.",
            Code("NOT (connections.state == listen) && connections.protocol == tcp"),
            "A watch filter is the same shape. Status is the mark from the watch. Remote is the other end.",
            Code("connections.status == added && (connections.remote BEGINS WITH ipaddress(10) || connections.remote BEGINS WITH ipaddress(172.16))")));
        topic.Sections.Add(Section("Syntax",
            "The words the bar uses.",
            Term("Field", "A column on this tab. route.destination is a route field. neighbors.address is a neighbor field. connections.localport is a connection field."),
            Term("Short name", "The column name when the tab has only one. destination on Routes. protocol on Connections."),
            Term("Value", "What the field is compared to. An address, a number, a closed word, or a string."),
            Term("Closed value", "A word the field already knows. tcp, reachable, netmgmt, added. It can be written bare, or as connections.protocol.tcp."),
            Term("==", "Exact. The row matches this value and no other."),
            Term("BEGINS WITH", "The value starts with this prefix. Use it for an incomplete address."),
            Term("CONTAINS", "The value includes this fragment."),
            Term(">= and LTE", "A number at or beyond the bound. LTE is at or below."),
            Term("BETWEEN", "A range, inclusive. The word inside it is AND."),
            Term("AND", "Both sides must match. && is the same word. AND is read before OR."),
            Term("OR", "Either side may match. || is the same word."),
            Term("NOT", "The comparison inside the parentheses does not match."),
            Term("Parentheses", "The order you meant, when AND and OR are in the same query."),
            Term("ipaddress()", "An address value. A complete address for ==. A prefix for BEGINS WITH."),
            Term("macaddress()", "A hardware address. A fragment has to be whole octets."),
            Term("string()", "A free string, when the value is not a closed word.")));
        topic.Sections[0].IsSelected = true;
        Wire(topic);
        return topic;
    }

    private void ShowPages(HelpTopic section)
    {
        if (section.Pages.Count == 0)
        {
            PortPages.Visibility = Visibility.Collapsed;
            PortPages.ItemsSource = null;
            Bind(section.Document);
            return;
        }

        PortPages.ItemsSource = section.Pages;
        PortPages.Visibility = Visibility.Visible;
        var page = section.Pages.FirstOrDefault(item => item.IsSelected) ?? section.Pages[0];
        page.IsSelected = true;
        Bind(page.Document);
    }

    private void Wire(HelpTopic topic)
    {
        foreach (var section in topic.Sections)
        {
            section.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(HelpTopic.IsSelected) && section.IsSelected && ReferenceEquals(Selected, topic))
                    ShowPages(section);
            };
            foreach (var page in section.Pages)
            {
                page.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(HelpTopic.IsSelected) && page.IsSelected && ReferenceEquals(Selected, topic) && section.IsSelected)
                        Bind(page.Document);
                };
            }
        }
    }

    private static HelpTopic Section(string title, params string[ ] lines) => new(title, lines);

    private static string Head(string line) => "\x1f" + line;

    private static string Link(string url, string? label = null) => "\x1c" + url + "|" + (label ?? url);

    private static string Pack(string name, string version, string publisher, string license)
    {
        var url = "https://www.nuget.org/packages/" + name + "/" + version;
        return "\x1d" + name + "|" + version + "|" + url + "|" + publisher + "|" + license;
    }


    private static string Code(string line) => "\x1e" + line;

    private static string Term(string word, string means) => "\x1b" + word + "|" + means;

    private HelpTopic Topic(string title, params string[ ] lines)
    {
        var topic = new HelpTopic(title, lines);
        topic.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HelpTopic.IsSelected) && topic.IsSelected)
                Show(topic);
        };
        return topic;
    }
}

public sealed class HelpTopic: INotifyPropertyChanged
{
    private bool _isSelected;

    public HelpTopic(string title, IReadOnlyList<string> lines)
    {
        Title = title;
        Document = new FlowDocument { PagePadding = new System.Windows.Thickness(24) };
        Document.Blocks.Add(new Paragraph(new Run(title)) { FontSize = 22, FontWeight = FontWeights.SemiBold, Margin = new System.Windows.Thickness(0, 0, 0, 12) });
        var index = 0;
        while (index < lines.Count)
        {
            if (lines[index].StartsWith("\x1e", StringComparison.Ordinal))
            {
                Document.Blocks.Add(CodeBlock([lines[index][1..]]));
                index++;
                continue;
            }

            if (lines[index].StartsWith("\x16", StringComparison.Ordinal) || lines[index].StartsWith("\x1b", StringComparison.Ordinal))
            {
                var wide = lines[index].StartsWith("\x16", StringComparison.Ordinal);
                var mark = wide ? "\x16" : "\x1b";
                var terms = new List<string>( );
                while (index < lines.Count && lines[index].StartsWith(mark, StringComparison.Ordinal))
                {
                    terms.Add(lines[index][1..]);
                    index++;
                }
                Document.Blocks.Add(wide ? TwoColumnCards(terms) : GlossaryCard(terms));
                continue;
            }

            if (lines[index].StartsWith("\x1d", StringComparison.Ordinal))
            {
                var packs = new List<string>( );
                while (index < lines.Count && lines[index].StartsWith("\x1d", StringComparison.Ordinal))
                {
                    packs.Add(lines[index][1..]);
                    index++;
                }
                Document.Blocks.Add(PackageCard(packs));
                continue;
            }

            if (lines[index].StartsWith("\x1c", StringComparison.Ordinal))
            {
                var parts = lines[index][1..].Split('|', 2);
                Document.Blocks.Add(LinkParagraph(parts[0], parts.Length == 2 ? parts[1] : parts[0]));
                index++;
                continue;
            }

            var heading = lines[index].StartsWith("\x1f", StringComparison.Ordinal);
            var body = heading ? lines[index][1..] : lines[index];
            var paragraph = new Paragraph(new Run(body))
            {
                FontSize = heading ? 16 : 15,
                FontWeight = heading ? FontWeights.SemiBold : FontWeights.Normal,
                Margin = new System.Windows.Thickness(0, heading ? 8 : 0, 0, 8),
                LineHeight = 22
            };
            Document.Blocks.Add(paragraph);
            index++;
        }
    }

    private static void OpenHttps(Uri? uri)
    {
        if (uri is not null && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            return;
        }

        if (Application.Current?.MainWindow is MainWindow window)
            window.ViewModel.Status.Message = "That link was not opened.";
    }

    private static Paragraph LinkParagraph(string url, string label)
    {
        var link = new Hyperlink(new Run(label)) { NavigateUri = new Uri(url) };
        link.RequestNavigate += (_, e) =>
        {
            OpenHttps(e.Uri);
            e.Handled = true;
        };
        return new Paragraph(link) { FontSize = 15, Margin = new Thickness(0, 0, 0, 8) };
    }

    private static BlockUIContainer PackageCard(IReadOnlyList<string> rows)
    {
        var grid = new Grid( );
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
        AddPackageHeader(grid);
        var r = 1;
        foreach (var row in rows)
        {
            var parts = row.Split('|');
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddPackageCell(grid, r, 0, PackageLink(parts[0], parts[2]));
            AddPackageCell(grid, r, 1, PackageText(parts[1], mono: true));
            AddPackageCell(grid, r, 2, PackageText(parts[3]));
            AddPackageCell(grid, r, 3, PackageText(parts[4]));
            r++;
        }

        var border = new Border
        {
            Padding = new Thickness(14, 10, 14, 10),
            Margin = new Thickness(0, 0, 0, 12),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = grid
        };
        border.SetResourceReference(Border.BackgroundProperty, "Vestigium.Brushes.Surface.Sunken");
        border.SetResourceReference(Border.BorderBrushProperty, "Vestigium.Brushes.Stroke.Subtle");
        grid.SetResourceReference(TextElement.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        return new BlockUIContainer(border);
    }

    private static void AddPackageHeader(Grid grid)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddPackageCell(grid, 0, 0, PackageText("Package", heading: true));
        AddPackageCell(grid, 0, 1, PackageText("Version", heading: true));
        AddPackageCell(grid, 0, 2, PackageText("Publisher", heading: true));
        AddPackageCell(grid, 0, 3, PackageText("License", heading: true));
    }

    private static void AddPackageCell(Grid grid, int row, int column, TextBlock text)
    {
        Grid.SetRow(text, row);
        Grid.SetColumn(text, column);
        grid.Children.Add(text);
    }

    private static TextBlock PackageText(string value, bool mono = false, bool heading = false)
    {
        return new TextBlock
        {
            Text = value,
            FontSize = heading ? 12 : 14,
            FontWeight = heading ? FontWeights.SemiBold : FontWeights.Normal,
            FontFamily = mono ? new FontFamily("Consolas") : new FontFamily("Segoe UI"),
            Margin = new Thickness(0, 3, 12, 3)
        };
    }

    private static TextBlock PackageLink(string name, string url)
    {
        var link = new Hyperlink(new Run(name)) { NavigateUri = new Uri(url) };
        link.RequestNavigate += (_, e) =>
        {
            OpenHttps(e.Uri);
            e.Handled = true;
        };
        var text = new TextBlock { FontSize = 14, Margin = new Thickness(0, 3, 12, 3) };
        text.Inlines.Add(link);
        return text;
    }

    private static BlockUIContainer TwoColumnCards(IReadOnlyList<string> rows)
    {
        var mid = (rows.Count + 1) / 2;
        var host = new Grid( );
        host.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        host.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        host.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var left = Frame(GlossaryGrid(rows.Take(mid).ToArray( ), "Port", "Name"));
        var right = Frame(GlossaryGrid(rows.Skip(mid).ToArray( ), "Port", "Name"));
        Grid.SetColumn(right, 2);
        host.Children.Add(left);
        host.Children.Add(right);
        return new BlockUIContainer(host) { Margin = new Thickness(0, 0, 0, 12) };
    }

    private static BlockUIContainer GlossaryCard(IReadOnlyList<string> rows)
        => new(Frame(GlossaryGrid(rows, "Term", "Definition"))) { Margin = new Thickness(0, 0, 0, 12) };

    private static Border Frame(Grid grid)
    {
        var border = new Border
        {
            Padding = new Thickness(14, 8, 14, 8),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = grid
        };
        border.SetResourceReference(Border.BackgroundProperty, "Vestigium.Brushes.Surface.Sunken");
        border.SetResourceReference(Border.BorderBrushProperty, "Vestigium.Brushes.Stroke.Subtle");
        grid.SetResourceReference(TextElement.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        return border;
    }

    private static Grid GlossaryGrid(IReadOnlyList<string> rows, string term, string definition)
    {
        var grid = new Grid( );
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var termHead = new TextBlock { Text = term, FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 12, 6) };
        var defHead = new TextBlock { Text = definition, FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 6) };
        Grid.SetColumn(defHead, 1);
        grid.Children.Add(termHead);
        grid.Children.Add(defHead);
        var r = 1;
        foreach (var row in rows)
        {
            var parts = row.Split('|', 2);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var word = new TextBlock { Text = parts[0], FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 3, 12, 3) };
            var means = new TextBlock { Text = parts.Length == 2 ? parts[1] : string.Empty, FontSize = 14, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 3) };
            Grid.SetRow(word, r);
            Grid.SetColumn(word, 0);
            Grid.SetRow(means, r);
            Grid.SetColumn(means, 1);
            grid.Children.Add(word);
            grid.Children.Add(means);
            r++;
        }

        return grid;
    }

    private static BlockUIContainer CodeBlock(IReadOnlyList<string> lines)
    {
        var panel = new StackPanel( );
        foreach (var line in lines)
        {
            panel.Children.Add(new TextBlock
            {
                Text = line,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 4),
                TextWrapping = TextWrapping.NoWrap
            });
        }

        var border = new Border
        {
            Padding = new Thickness(14, 10, 14, 6),
            Margin = new Thickness(0, 0, 0, 12),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = panel
        };
        border.SetResourceReference(Border.BackgroundProperty, "Vestigium.Brushes.Surface.Sunken");
        border.SetResourceReference(Border.BorderBrushProperty, "Vestigium.Brushes.Stroke.Subtle");
        panel.SetResourceReference(TextElement.ForegroundProperty, "Vestigium.Brushes.Text.Primary");
        var sample = string.Join(Environment.NewLine, lines);
        var copy = new MenuItem { Header = "Copy" };
        copy.Click += (_, _) =>
        {
            try
            {
                Clipboard.SetText(sample);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
            }
        };
        var menu = new ContextMenu( );
        menu.Items.Add(copy);
        border.ContextMenu = menu;
        border.PreviewMouseRightButtonUp += (_, e) =>
        {
            menu.IsOpen = true;
            e.Handled = true;
        };
        return new BlockUIContainer(border) { Margin = new Thickness(0, 0, 0, 4) };
    }

    public string Title { get; }

    public FlowDocument Document { get; }

    public List<HelpTopic> Sections { get; } = new( );

    public List<HelpTopic> Pages { get; } = new( );

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
