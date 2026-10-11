namespace Vestigium.Suite.Network.TraceIQ.Views;

public static class HelpCopy
{
    public sealed record Draft(string Title, IReadOnlyList<string> Lines, IReadOnlyList<Section> Sections);
    public sealed record Section(string Title, IReadOnlyList<string> Lines);

    public static IReadOnlyList<Draft> Topics(string version, IReadOnlyList<string> license)
        =>
        [
            Overview(),
            TraceIq(),
            PathPing(),
            Exports(),
            Dashboard(),
            Settings(),
            Glossary(),
            About(version),
            new("License", license.Count == 0 ? ["The license file was empty."] : license, [])
        ];

    private static Draft Overview()
        => new("Overview",
        [
            "TraceIQ shows how this machine reaches a target, and then how steady that path is. You give it a name or an address. It walks the path, writes one line for each step, and lets you grade the steps that answered.",
            "The walk is the first job. TraceIQ asks each step in turn and records who answered, what they are called, and how long the ask took. The result is a list you can read from the top. The first line is the first step out. The last line is the target, when the walk reaches it. A step that does not answer is still listed, so you can see where the path went quiet.",
            "The grade is the second job. Path Ping takes the steps the walk found and asks them again, for a window you set. Each step gets a count of what was sent, what came back, and the spread of the times. That is how you tell a step that is steady from a step that is dropping replies or getting slow.",
            "The reading is the third job. Dashboard takes the finished grade and draws it. Overview compares the steps by their middle time. Each step has its own page, with the shape of its times and the times in the order they came back. You can look at every reply, the middle half, the steady body, or the slow tail. The pictures are a reading of the grade you already ran.",
            "The record is the fourth job. Exports writes the rows to a workbook on the desktop. The walk is one sheet, with a column for each probe you asked for. The grade is a summary sheet and a sheet of every reply. You can open the file, open the folder, or hand the workbook to another desk.",
            "Settings holds the choices that shape those jobs. Which adapter the walk leaves on. How the walk asks. How far it will go, how many steps it asks at once, and how many times it asks each one. How hard the grade presses, how long it runs, and when a silent step is left behind. Which recent targets are kept, and which are pinned. The look of the window.",
            "A normal use is short. Type the target, run the walk, read the list. If the path looks right and you want to know whether it holds, run the grade. If the grade shows a step that is slow or lossy, open its page on the dashboard. If someone else needs the numbers, export them.",
            "The pages are TraceIQ, Path Ping, Exports, Dashboard, and Settings. Help is this page. Start on TraceIQ. Everything else reads what that walk found."
        ], []);

    private static Draft TraceIq()
        => new("TraceIQ",
        [
            "TraceIQ sends a walk to the target and writes one line per hop. The line is the hop number, the address, the name, and a time for each probe.",
            "The target box remembers recent names. A pin keeps a name at the top. Trace starts the walk. Cancel stops it. The bar under the target fills as hops come back.",
            "Reached says whether the walk got to the target. Protocol says what the walk used. The grid fills as answers arrive. A hop that is still out shows as waiting. A hop that never answers stays a star.",
            "The times are round trip, in milliseconds. RTT 1 is the first probe. RTT 2 is the second. The number of columns is the Probes setting. A time under one millisecond prints as less than one. A star is no answer.",
            "The walk stops at the target, or at Max hops, whichever comes first. Parallel is how many hops are asked at once. It is not how many probes. Probes are the times on one hop."
        ], []);

    private static Draft PathPing()
        => new("Path Ping",
        [
            "Path Ping grades the hops the walk found. It does not find a new path. It pings the hops that already answered, for the duration you set.",
            "The target is the one from the walk. It is shown and it is not editable. Path Ping starts the grade. Cancel stops it. The tab is off until a walk has reached the target.",
            "Each row is a hop. Sent is how many pings went out. Received is how many came back. Loss is the difference. Min, Q1, Median, Q3, and Max are the times of the ones that came back. A hop with no replies has no times.",
            "A hop that never answers is not graded for the whole window. After Dead after failed pings, that hop is marked and left. The rest keep going.",
            "Duration is the window. New pings stop when it ends. A ping already out is given only the time that is left, so the run does not walk past the mark."
        ], []);

    private static Draft Exports()
        => new("Exports",
        [
            "Exports writes the rows you already have. It does not run a walk and it does not run a grade.",
            "TraceIQ writes one row per hop, with the probe columns that were set. Three probes is RTT 1, RTT 2, RTT 3. Five is five. There are no empty columns past that.",
            "Path Ping writes the hop summary. Path Ping Samples writes one row per reply, the raw times. That is the population the summary came from.",
            "Open File after Save opens the workbook. Open Folder after Save opens the folder with the file selected. The folder is Desktop, then Vestigium, then Exports, then TraceIQ. The file stays on this machine."
        ], []);

    private static Draft Dashboard()
        => new("Dashboard",
        [
            "Dashboard reads the last Path Ping. It does not ping again. The tab is off until a grade finishes.",
            "Overview compares the hops. It is the median time of each hop, in order. It is not the whole grade as one pile.",
            "A hop tab is that hop only. The top chart is the shape of its times, with a line for the running share. The bottom chart is the times in the order they came back, with a center line and a band.",
            "All is every reply. IQR is the middle half. Steady is the replies inside the band. Tail is the replies that left the band. These cut the picture. They do not delete the samples. Overview has no cut. It is a comparison.",
            "Legend shows or hides the key on every chart. The choice is remembered."
        ], []);

    private static Draft Settings()
        => new("Settings",
        [
            "Settings holds the knobs that are not the target.",
            "The pages are TraceIQ, Advanced, MRU, Path Ping, and Theme. Each one is below."
        ],
        [
            new("TraceIQ",
            [
                "Interface is the adapter the walk leaves on. Any lets Windows pick.",
                "Source is the local address on that adapter. It is where the packets leave from. It is not a way to pretend to be another machine.",
                "Family is IPv4, IPv6, or All. All lets the walk use either."
            ]),
            new("Advanced",
            [
                "Protocol is how the walk asks. ICMP is the usual echo. UDP and TCP are the other asks. Hybrid tries ICMP and fails over.",
                "Second pass on silent hops is only for Hybrid. A hop that did not answer gets one more ask.",
                "Max hops is how far the walk will go. It stops sooner if it reaches the target.",
                "Parallel is how many hops are asked at once. A higher number finishes a long path sooner. It does not change what a hop is.",
                "Probes (RTT) is how many times each hop is asked. That number is how many RTT columns the grid and the export show."
            ]),
            new("MRU",
            [
                "MRU max is how many recent targets are kept. The oldest drops off when the list is full.",
                "Sticky max is how many can be pinned. A sticky stays at the top and does not count against the MRU max.",
                "The list is the targets. The box on the right pins or unpins one. A pin on a full sticky list does nothing until one is unpinned."
            ]),
            new("Path Ping",
            [
                "Saturation is the gap between pings. Low is gentle. Flood is 100 a second. The gap is the only thing it sets. How many hops are graded at once is the knob below.",
                "Hops at a time is how many hops are graded together. It is not the ping rate.",
                "Duration is the window, in seconds. New pings stop when it ends.",
                "Dead after is how many failed pings mark a hop as silent. That hop is then left, so the window is spent on the hops that answer."
            ]),
            new("Theme",
            [
                "Palette is the look of the window. The choice is saved and applied when the program starts.",
                "Status bar visible shows or hides the bar. Dock puts it at the top or the bottom.",
                "The left of the bar is a status change. It clears. The center is information. It stays. On TraceIQ and Path Ping the center is the elapsed time. On Dashboard it is the hop line."
            ])
        ]);

    private static Draft Glossary()
        => new("Glossary",
        [
            "The words this window uses. A short meaning, in the sense of this program."
        ],
        [
            new("Tabs",
            [
                Term("TraceIQ", "The walk. One target, one path, one line per hop."),
                Term("Path Ping", "A grade of the hops the walk found. Not a new path."),
                Term("Exports", "The rows already loaded, written to a workbook."),
                Term("Dashboard", "A reading of the last grade. Not a second measurement."),
                Term("Settings", "The knobs. Interface, protocol, MRU, the grade, and the theme.")
            ]),
            new("The walk",
            [
                Term("Hop", "One step on the path. A number, an address or a star, and times."),
                Term("TTL", "The hop number. One is the first step out. It counts up."),
                Term("Probe", "One ask of a hop. Probes (RTT) is how many asks."),
                Term("RTT", "Round trip time. How long that ask took to come back, in milliseconds."),
                Term("Reached", "The walk got to the target."),
                Term("Star", "No answer from that hop.")
            ]),
            new("The grade",
            [
                Term("Sent", "Pings that went out to that hop."),
                Term("Received", "Pings that came back."),
                Term("Loss", "Sent minus received."),
                Term("Median", "The middle time. Half the replies were faster, half were slower."),
                Term("IQR", "The middle half of the replies, from Q1 to Q3."),
                Term("Steady", "The replies inside the band. The hop as it behaves when it is not being pushed."),
                Term("Tail", "The replies that left the band. The slow ones."),
                Term("UCL", "The top of the band. A reply above it left the band."),
                Term("Saturation", "The gap between pings. Not how many hops at once.")
            ])
        ]);

    private static Draft About(string version)
        => new("About",
        [
            "TraceIQ " + version + "."
        ],
        [
            new("App",
            [
                "TraceIQ " + version + ". One target, one walk, a grade of the hops, and a workbook of the rows.",
                "Copyright (c) 2026 Nathaniel Wilkinson. MIT License. The notice is the License topic."
            ])
        ]);

    private static string Term(string word, string means) => "\x1b" + word + "|" + means;
}
