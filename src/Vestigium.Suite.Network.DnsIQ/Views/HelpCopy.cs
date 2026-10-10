namespace Vestigium.Suite.Network.DnsIQ.Views;

public static class HelpCopy
{
    public sealed record Draft(string Title, IReadOnlyList<string> Lines, IReadOnlyList<Section> Sections);

    public sealed record Section(string Title, IReadOnlyList<string> Lines);

    public static IReadOnlyList<Draft> Topics(string version, IReadOnlyList<string> license)
        =>
        [
            new("Overview",
            [
                "DnsIQ asks a resolver and prints the answer. The point of the window is a record of what that resolver said about a name, or about the names already sitting in a file. It does not change DNS, edit a zone, shell to nslookup.exe, or use HTTP DNS. If the print is wrong, the correction is made elsewhere.",
                "The pages are DnsIQ, Hosts Viewer, Dashboard, Exports, and Settings. Help is this page. There is no File menu. DnsIQ is one name: Lookup writes the answer grid, and Probe is that lookup followed by a pulse. Hosts Viewer is the hosts in a HAR or a text dump, one line per answer, and opening a file that parses starts the DNS probe. Dashboard is a reading of the last run, not a second measurement. Exports writes the prints already loaded. Settings holds the port, the source, the pulse length, and the theme.",
                "Read a row for what it is. Lookup columns are Type, Name, Data, and Ttl. Hosts Viewer columns are Host, Ports, Hits, Sources, DNS, Error, Category, and Answer. DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Category is IPv4, IPv6, FQDN, or Address. A row that says Resolved is not Reachable. A name can answer and the site can still fail to load. Skipped is an address and is not sent.",
                "The Type box is a closed list. All asks A, AAAA, CNAME, MX, NS, PTR, TXT, and SOA, one after another. It is not a DNS ANY query. The meaning of each type is on Glossary, under Records. A type that is not in that list is not asked. An address typed in the Name box is still a forward question. Reverse is only on the hosts details window.",
                "Use DnsIQ when the question is one name. Use Hosts Viewer when the names are already in a file. Use Probe when one answer is not enough and you need the times. Use Exports when another desk needs the rows. The workbook stays on this machine. It is not uploaded."
            ], []),
            new("Lookup",
            [
                "Lookup asks one name and writes the answer grid. A blank Name means localhost, and that name is not persisted. The question is what this resolver says right now. It is not a pulse, and it does not open a file.",
                "Server, Port, Type, and Interface are the query. Server is the resolver address. An empty server is the first adapter DNS. The list also offers this PC and the public resolvers. Port defaults to 53. Type defaults to All, which asks the closed list one type at a time and is not a DNS ANY query. Interface is the adapters on this machine, plus Any. Source, on Settings, is the local bind address. It is not spoofing.",
                "Lookup writes Type, Name, Data, and Ttl. Read Type against Glossary, Records. Data is the answer for that type: an address, an alias, a mail host, a nameserver, a reverse name, a text string, or the zone's start of authority. Ttl is the resolver's time to live, in seconds. It is not a promise that the site will load.",
                "A failed Lookup clears the grid. One job in the process. Cancel stops it. Probe and Hosts Viewer use that same job. A second click while one is running does not start another. Lookup alone does not unlock Dashboard. An IPv6 literal in Name is not turned into a reverse lookup.",
                "Use Lookup when the question is one name and one answer set. If you need the times, use Probe. If the names are already in a file, use Hosts Viewer. The grid on this page is the print Export will write."
            ], []),
            new("Probe",
            [
                "Probe is that Lookup, then a pulse of N lookups over X seconds. The prelude fills the grid. The pulse does not append rows. The point is the times, not a second copy of the answers.",
                "Requests and Seconds live on Settings, on the Probe page. The defaults are 1000 and 60. The status bar during the pulse is sent/total. The center of the bar is the time rail. Cancel stops the job. Probe uses the same single job as Lookup and Hosts Viewer.",
                "NxDomain counts as answered. Timeout and Refused are counts only. They do not become rows, and they do not become samples. The status line at the end is the median, the rate, and those counts. That line is the summary. It is not a second measurement.",
                "Dashboard stays disabled until a Probe finishes. Lookup alone does not unlock it. A failed prelude does not start the pulse, and it does not unlock the charts. A later Probe replaces the sample list. It does not append to the last run.",
                "Use Probe when one answer is not enough and you need the times. The charts are a reading of those times. Export writes them as Index and RttMs. The pulse itself does not write a row."
            ], []),
            new("Hosts Viewer",
            [
                "Hosts Viewer is the hosts in a file. Open is the button on this page. It reads a HAR, or a UTF-8 .txt, or a .har that is not JSON. A file that parses starts the DNS probe. There is no paste box and no File menu. The file is the intake.",
                "The grid is one line per answer. Columns are Host, Ports, Hits, Sources, DNS, Error, Category, and Answer. Host, Ports, Hits, Sources, DNS, and Error repeat. Category is IPv4, IPv6, FQDN, or Address. A host with no answers is still one line, so NxDomain does not disappear. The list scrolls on its own bar.",
                "DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Skipped is an address and is not sent. Resolved is not Reachable. A name that answers is not a site that loaded. Error is the parse or probe failure for that host. It is not a second status line.",
                "Double-click a line and a details window opens for that host. The title contains the host. The host list stays up. The next double-click loads that host into the same window. Each answer is its own line. An address line asks PTR of the in-addr.arpa or ip6.arpa name, and that name is shown. A name host is checked with A and AAAA. Those checks do not write the grid. Close cancels checks that have not finished. The bar and the row menu are Lookup, or Lookup + Probe, for the selected host. This page is not a HAR analyzer: no waterfall, no cookies, no header dump, no replay.",
                "Use Hosts Viewer when the names are already in a dump, a pasted email, or a sheet saved as text. Export writes this grid, one row per answer. It does not write the raw file, and it does not wait for the details check."
            ], []),
            new("Dashboard",
            [
                "Dashboard is a reading of the last run. It is not a second measurement. The tab stays disabled until a Probe finishes. Lookup alone does not unlock it. The empty state points back to DnsIQ.",
                "The Lookup tab is the type mix of the last answer grid. Each slice is a type and a count. It is the same rows Lookup already wrote. A failed lookup that cleared the grid leaves this tab empty.",
                "The Probe tab is the RTT curve, the histogram, and the control chart when Analytics returns fences. The samples are the times the pulse kept. Timeout and Refused are not on the curve. A later Probe replaces the list.",
                "The charts do not ask the resolver again. They do not change the grid. They do not write the workbook. Export writes the rows and the times. The picture is a reading of those numbers.",
                "Use Dashboard after a Probe, when the question is the shape of the run rather than one row. If the tab is disabled, the pulse has not finished. Go back to DnsIQ and run Probe."
            ], []),
            new("Exports",
            [
                "Exports writes the prints already loaded. It does not query again. It does not re-read the file. Cover is always written. A checked print with rows becomes a sheet.",
                "The Hosts Viewer sheet is the host grid. One answer, one row. Columns are Host, Ports, Hits, Sources, DNS, Error, Category, and Answer. The joined Answers cell is gone. The details Check column is not a sheet. Lookup and Probe are unchanged.",
                "The workbook stays on this machine. It contains the name, the answers, the capture lines, and the probe times. It is not uploaded.",
                Head("When to use it"),
                "Use the workbook when another desk needs the print, or when a change request needs the rows the resolver returned. The file is the record. This tab does not compare two files."
            ], []),
            new("Settings",
            [
                "Settings holds the knobs that are not the name.",
                "Settings, on the DnsIQ page, holds the Port seed and Source. Source is the local bind address, not spoofing. Settings, on the Probe page, holds Requests and Seconds. Settings, on the Theme page, holds the palette, the status bar visible check, and the status bar dock.",
                "The file is settings.json under ProgramData. Name is not stored. Pulse samples are not stored. The details check is not stored."
            ], []),
            Glossary(),
            About(version),
            new("License", license.Count == 0 ? ["The license file was empty."] : license, [])
        ];

    private static Draft Glossary()
        => new("Glossary",
            ["The words this window uses. A short meaning, in the sense of this print. Record types are under Records."],
            [
                new("Tabs",
                [
                    Term("Lookup", "One name, asked once. The answer grid is Type, Name, Data, and Ttl."),
                    Term("Probe", "That lookup, then a pulse. The pulse does not append rows."),
                    Term("Hosts Viewer", "Hosts taken from a HAR or a text dump. Opening a file that parses starts the DNS probe. The grid is one line per answer."),
                    Term("Details", "A window for one host. The list stays up. An address line is a PTR. A name host is checked with A and AAAA."),
                    Term("Export", "The checked prints, written to one workbook. Cover is always included."),
                    Term("Export All", "Every print that has rows, written to one workbook. The checks are ignored."),
                    Term("Cover", "The first sheet. Host, operator, time, the query on screen, and the count of each print that was written.")
                ]),
                new("Records",
                [
                    "These are the types the Type box can ask. A type that is not in this list is not sent.",
                    Term("All", "A, AAAA, CNAME, MX, NS, PTR, TXT, and SOA, asked one after another. All is not a DNS ANY query."),
                    Term("A", "The IPv4 address for the name."),
                    Term("AAAA", "The IPv6 address for the name."),
                    Term("CNAME", "An alias. Data is the other name, not an address."),
                    Term("MX", "A mail exchanger. Data is the preference and the host that receives mail for the name."),
                    Term("NS", "A nameserver for the zone. Data is the server name."),
                    Term("PTR", "The name that an address points back to. Used on a reverse lookup. The question name is in-addr.arpa or ip6.arpa."),
                    Term("TXT", "A text string stored on the name."),
                    Term("SOA", "Start of authority. The zone's primary record: the primary server, the contact, and the timers.")
                ]),
                new("Lookup",
                [
                    Term("Prelude", "The lookup that runs before the pulse. It fills the answer grid."),
                    Term("Pulse", "N lookups over X seconds. It does not append rows."),
                    Term("Source", "The local bind address. It is not spoofing.")
                ]),
                new("Hosts Viewer",
                [
                    Term("Host", "A name taken from the file. An address is listed and then skipped."),
                    Term("Hits", "How many times that host was seen."),
                    Term("Sources", "Where the name was found: a request, a redirect, a page, or the text."),
                    Term("Category", "IPv4, IPv6, FQDN, or Address. Blank when the host has no answer."),
                    Term("FQDN", "An answer that is a name, not an address."),
                    Term("Resolved", "A or AAAA returned an answer. Resolved is not Reachable."),
                    Term("Skipped", "The host is already an address. It is not sent on open. Details may still ask PTR."),
                    Term("Question", "The arpa name sent for a reverse. A name answer has none.")
                ]),
                new("Probe",
                [
                    Term("Requests", "How many lookups the pulse will send. The default is 1000."),
                    Term("Seconds", "How long the pulse runs. The default is 60.")
                ])
            ]);

    private static Draft About(string version)
        => new("About",
            ["DnsIQ " + version + "."],
            [
                new("App",
                [
                    "DnsIQ " + version + ". One name, a pulse, and the hosts in a file. Exports writes those prints to a workbook.",
                    "Copyright (c) 2026 Nathaniel Wilkinson. MIT License. The notice is the License topic.",
                    "The public source for this host is Vestigium.Suite.Network.",
                    Link("https://github.com/nwilkinsonlsnh/Vestigium.Suite.Network", "Vestigium.Suite.Network")
                ])
            ]);

    private static string Head(string line) => "\x1f" + line;

    private static string Link(string url, string label) => "\x1c" + url + "|" + label;

    private static string Term(string word, string means) => "\x1b" + word + "|" + means;
}
