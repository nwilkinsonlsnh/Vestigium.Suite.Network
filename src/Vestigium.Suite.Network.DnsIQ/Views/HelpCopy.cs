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
                "The pages are DnsIQ, Capture, Dashboard, Exports, and Settings. Help is this page. DnsIQ is one name: Lookup writes the answer grid, and Probe is that lookup followed by a pulse. Capture is the hosts in a HAR or a text dump, and opening a file that parses starts the DNS probe. Dashboard is a reading of the last run, not a second measurement. Exports writes the prints already loaded. Settings holds the port, the source, the pulse length, and the theme.",
                "Read a row for what it is. Lookup columns are Type, Name, Data, and Ttl. Capture adds DNS and Error. DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. A row that says Resolved is not Reachable. A name can answer and the site can still fail to load. Skipped is an address and is not sent.",
                "The Type box is a closed list. All asks A, AAAA, CNAME, MX, NS, PTR, TXT, and SOA, one after another. It is not a DNS ANY query. The meaning of each type is on Glossary, under Records. A type that is not in that list is not asked.",
                "Use DnsIQ when the question is one name. Use Capture when the names are already in a file. Use Probe when one answer is not enough and you need the times. Use Exports when another desk needs the rows. The workbook stays on this machine. It is not uploaded."
            ], []),
            new("Lookup",
            [
                "Lookup asks one name and writes the answer grid. A blank Name means localhost, and that name is not persisted. The question is what this resolver says right now. It is not a pulse, and it does not open a capture.",
                "Server, Port, Type, and Interface are the query. Server is the resolver address. An empty server is the first adapter DNS. Port defaults to 53. Type defaults to All, which asks the closed list one type at a time and is not a DNS ANY query. Interface is the adapters on this machine, plus Any. Source, on Settings, is the local bind address. It is not spoofing.",
                "Lookup writes Type, Name, Data, and Ttl. Read Type against Glossary, Records. Data is the answer for that type: an address, an alias, a mail host, a nameserver, a reverse name, a text string, or the zone's start of authority. Ttl is the resolver's time to live, in seconds. It is not a promise that the site will load.",
                "A failed Lookup clears the grid. One job in the process. Cancel stops it. Probe and Capture use that same job. A second click while one is running does not start another. Lookup alone does not unlock Dashboard.",
                "Use Lookup when the question is one name and one answer set. If you need the times, use Probe. If the names are already in a file, use Capture. The grid on this page is the print Export will write."
            ], []),
            new("Probe",
            [
                "Probe is that Lookup, then a pulse of N lookups over X seconds. The prelude fills the grid. The pulse does not append rows. The point is the times, not a second copy of the answers.",
                "Requests and Seconds live on Settings, on the Probe page. The defaults are 1000 and 60. The status bar during the pulse is sent/total. The center of the bar is the time rail. Cancel stops the job. Probe uses the same single job as Lookup and Capture.",
                "NxDomain counts as answered. Timeout and Refused are counts only. They do not become rows, and they do not become samples. The status line at the end is the median, the rate, and those counts. That line is the summary. It is not a second measurement.",
                "Dashboard stays disabled until a Probe finishes. Lookup alone does not unlock it. A failed prelude does not start the pulse, and it does not unlock the charts. A later Probe replaces the sample list. It does not append to the last run.",
                "Use Probe when one answer is not enough and you need the times. The charts are a reading of those times. Export writes them as Index and RttMs. The pulse itself does not write a row."
            ], []),
            new("Capture",
            [
                "Capture is the hosts in a file. Open capture reads a HAR, or a UTF-8 .txt, or a .har that is not JSON. A file that parses starts the DNS probe. There is no paste box. The file is the intake.",
                "Columns are Host, Ports, Hits, Sources, DNS, Error, and Answers. Host is the name taken from the file. Ports and Hits say where it appeared and how often. Sources says whether it came from a request, a redirect, a page, or the text. DNS is the probe result. Answers is what A and AAAA returned.",
                "DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Skipped is an address and is not sent. Resolved is not Reachable. A name that answers is not a site that loaded. Error is the parse or probe failure for that row. It is not a second status line.",
                "The row menu is Lookup, or Lookup + Probe, for that host. Both use the one job in the process. This page is not a HAR analyzer: no waterfall, no cookies, no header dump, no replay. Opening a new file replaces the grid. It does not append.",
                "Use Capture when the names are already in a dump, a pasted email, or a sheet saved as text. Export writes this grid, including Error. It does not write the raw file."
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
                "The workbook stays on this machine. It contains the name, the answers, the capture hosts, and the probe times. It is not uploaded.",
                Head("When to use it"),
                "Use the workbook when another desk needs the print, or when a change request needs the rows the resolver returned. The file is the record. This tab does not compare two files."
            ], []),
            new("Settings",
            [
                "Settings holds the knobs that are not the name.",
                "Settings, on the DnsIQ page, holds the Port seed and Source. Source is the local bind address, not spoofing. Settings, on the Probe page, holds Requests and Seconds. Settings, on the Theme page, holds the palette and the status bar.",
                "The file is settings.json under ProgramData. Name is not stored. Pulse samples are not stored."
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
                    Term("Capture", "Hosts taken from a HAR or a text dump. Opening a file that parses starts the DNS probe."),
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
                    Term("PTR", "The name that an address points back to. Used on a reverse lookup."),
                    Term("TXT", "A text string stored on the name."),
                    Term("SOA", "Start of authority. The zone's primary record: the primary server, the contact, and the timers.")
                ]),
                new("Lookup",
                [
                    Term("Prelude", "The lookup that runs before the pulse. It fills the answer grid."),
                    Term("Pulse", "N lookups over X seconds. It does not append rows."),
                    Term("Source", "The local bind address. It is not spoofing.")
                ]),
                new("Capture",
                [
                    Term("Host", "A name taken from the file. An address is listed and then skipped."),
                    Term("Hits", "How many times that host was seen."),
                    Term("Sources", "Where the name was found: a request, a redirect, a page, or the text."),
                    Term("Resolved", "A or AAAA returned an answer. Resolved is not Reachable."),
                    Term("Skipped", "The host is already an address. It is not sent.")
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
                    "DnsIQ " + version + ". One name, a pulse, and the hosts in a capture. Exports writes those prints to a workbook.",
                    "Copyright (c) 2026 Nathaniel Wilkinson. MIT License. The notice is the License topic.",
                    "The public source for this host is Vestigium.Suite.Network.",
                    Link("https://github.com/nwilkinsonlsnh/Vestigium.Suite.Network", "Vestigium.Suite.Network")
                ])
            ]);

    private static string Head(string line) => "\x1f" + line;

    private static string Link(string url, string label) => "\x1c" + url + "|" + label;

    private static string Term(string word, string means) => "\x1b" + word + "|" + means;
}
