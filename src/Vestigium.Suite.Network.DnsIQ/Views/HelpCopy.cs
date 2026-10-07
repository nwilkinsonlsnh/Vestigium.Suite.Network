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
                "DnsIQ asks a resolver and prints the answer. It exists so an operator can see what a name resolves to, and whether that answer holds up over a short run, without opening a command window.",
                "The pages are DnsIQ, Capture, Dashboard, Exports, and Settings. Help is this page. DnsIQ is one name. Capture is the hosts in a file. Dashboard is a reading of the last run. Exports writes the prints already loaded. Settings is where the window is adjusted.",
                "DnsIQ does not change DNS, edit a zone, shell to nslookup.exe, or use HTTP DNS. If the print is wrong, the correction is made elsewhere. This host is the record of what the resolver said.",
                "A row that says Resolved is not Reachable. A name can answer and the site can still fail to load."
            ], []),
            new("Lookup",
            [
                "The DnsIQ tab is one name. Lookup writes the answer grid. A blank Name means localhost. The name is not persisted.",
                "Server, Port, Type, and Interface are the query. Server is the resolver. An empty server is the first adapter DNS. Port defaults to 53. Type defaults to All. Interface is a closed list from the adapters on this machine, plus Any.",
                "Lookup writes Type, Name, Data, and Ttl. A failed Lookup clears the grid. One job in the process. Cancel stops it. Probe and Capture use that same job. A second click while one is running does not start another.",
                Head("When to use it"),
                "Use Lookup when the question is what this resolver says about one name. It is not a pulse. It does not open a capture."
            ], []),
            new("Probe",
            [
                "Probe is that Lookup, then a pulse of N lookups over X seconds. The prelude fills the grid. The pulse does not append rows.",
                "Requests and Seconds live on Settings, on the Probe page. The defaults are 1000 and 60. The status bar during the pulse is sent/total. The center of the bar is the time rail.",
                "Dashboard stays disabled until a Probe finishes. Lookup alone does not unlock it. NxDomain counts as answered. Timeout and Refused are counts only.",
                Head("When to use it"),
                "Use Probe when one answer is not enough and you need the times. The charts are a reading of those times. They are not a second measurement."
            ], []),
            new("Capture",
            [
                "Capture is the hosts in a file. Open capture reads a HAR, or a UTF-8 .txt, or a .har that is not JSON. A file that parses starts the DNS probe. The row menu is Lookup, or Lookup + Probe, for that host.",
                "Columns are Host, Ports, Hits, Sources, DNS, Error, and Answers. DNS is Resolved, NxDomain, TimedOut, Refused, Failed, or Skipped. Skipped is an address and is not sent.",
                "Resolved is not Reachable. A name that answers is not a site that loaded. This page is not a HAR analyzer: no waterfall, no cookies, no header dump, no replay.",
                Head("When to use it"),
                "Use Capture when the names are already in a dump, a pasted email, or a sheet saved as text. The file is the intake. There is no paste box."
            ], []),
            new("Dashboard",
            [
                "Dashboard is a reading of the last run. It is not a second measurement.",
                "The Lookup tab is the type mix of the last answer grid. The Probe tab is the RTT curve, the histogram, and the control chart when Analytics returns fences. The empty state points back to DnsIQ.",
                "The tab stays disabled until a Probe finishes. Lookup alone does not unlock it."
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
            ["The words this window uses. A short meaning, in the sense of this print."],
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
