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
            "TraceIQ is the walk. You give it a target and it finds the path from this machine to that target, one step at a time, and writes what it finds.",
            "You type a name or an address, or you pick one you have used. Recent targets stay in the box. A pin keeps one at the top so it is always there. Trace starts the walk. Cancel stops it. While it runs, the bar under the target fills and the center of the status bar counts the time.",
            "Each line is one step. The number is how far out that step is. One is the first step off this machine. The address is who answered. The name is what that address is called, when a name comes back. Then there is a time for each ask. RTT 1 is the first. RTT 2 is the second. The number of those columns is the Probes setting, so the grid shows exactly the asks you chose.",
            "The list builds as answers come in. A step that is still out shows as waiting. A step that answers fills in its address and its times. A step that never answers stays a star, and it stays on the list, so you can see where the path went quiet. The walk does not skip a quiet step and pretend it was not there.",
            "Reached tells you whether the walk got to the target. Protocol tells you how it asked. The walk stops when it reaches the target, or when it has gone as far as Max hops. Parallel is how many steps are asked at once, which is how a long path finishes sooner. It is separate from Probes. Probes are the times on a single step.",
            "A time is how long that ask took to come back, in milliseconds. A very fast one prints as less than one. A star in a time column is an ask that did not come back. Reading across a line tells you how that step behaved. Reading down the list tells you the path.",
            "When the walk finishes, the target is remembered and the elapsed time stays in the center of the bar. If the walk reached the target, Path Ping can grade the steps that answered. If it did not, the list is still the record of how far it got."
        ], []);

    private static Draft PathPing()
        => new("Path Ping",
        [
            "Path Ping grades the path the walk already found. The walk told you who is on the way. The grade tells you whether those steps hold.",
            "The page opens once a walk has reached the target. The target is the one from that walk. It is shown so you know what you are grading, and it is not a box you type in. A grade is of a path you have. It is not a search for a new one.",
            "Path Ping starts the grade. Cancel stops it. While it runs, the bar on the left fills and the center of the status bar counts the time. When it finishes, that time stays, so you can see how long the window took.",
            "Each row is one step from the walk. Sent is how many asks went out. Received is how many came back. Loss is the difference, as a count. Min is the fastest reply. Q1, Median, and Q3 split the replies into quarters. Max is the slowest. A step with no replies has counts and no times, because there is nothing to summarize.",
            "The grade asks several steps at once. How many is Hops at a time. The gap between asks is Saturation. Low leaves a long gap. Flood asks a hundred times a second. The two knobs are separate. One is how many steps. One is how fast each step is asked.",
            "Duration is the window. New asks stop when it ends, and an ask already out is given only the time that is left, so the run finishes at the mark you set. A step that never answers is not asked for the whole window. After Dead after failed asks, that step is marked and left, and the rest of the window is spent on the steps that speak.",
            "When the grade finishes you have a row for every step, with its counts and its spread. A step with loss, or a step whose max is far from its median, is the one to open on the dashboard. The rows are also what Exports writes, and what Dashboard reads."
        ], []);

    private static Draft Exports()
        => new("Exports",
        [
            "Exports writes the rows you already have to a workbook, so the numbers can leave the window and go to another desk. It writes what is loaded. It does not start a walk and it does not start a grade.",
            "You choose what goes in. TraceIQ writes the walk. Path Ping writes the grade. You can write one, or both. Open File after Save opens the workbook when it is written. Open Folder after Save opens the folder with the new file selected, so you can see it among the others.",
            "The TraceIQ sheet is one row per step. The columns are the step number, the address, the name, whether it was reached, and a time for each probe you asked for. Three probes is RTT 1, RTT 2, and RTT 3. Five is five. The sheet matches the grid. It does not add empty columns past the probes you set.",
            "The Path Ping sheet is the summary you see on the grade. One row per step, with sent, received, loss, and the five times. Path Ping Samples is the raw grade. One row per reply, with the step, the address, the sample number, and the time. That sheet is the population the summary came from, so a question about one slow reply can be answered from the file.",
            "The workbook is written to the desktop, under Vestigium, then Exports, then TraceIQ. The name carries the time, so a second export does not overwrite the first. The file stays on this machine. You open it, or you hand it on."
        ], []);

    private static Draft Dashboard()
        => new("Dashboard",
        [],
        [
            new("Overview",
            [
                "Dashboard is where a grade becomes an answer. The walk told you who is on the path. The grade counted what came back from each step. Dashboard draws that count, so you can see the path instead of reading a table.",
                "The question it answers is about the connection. Is the target far, or is one step on the way the problem. Is a step slow all the time, or only when it is asked hard. Did the grade push a step past what it can answer. Those are different findings, and they point at different things. A far target is distance. A slow step is a place on the path. A step that only slows under pressure is a step that cannot keep up with the rate you asked.",
                "Start here. Overview is the comparison. It draws the middle time of each step, in the order of the path. A step that stands above its neighbors is the one to open. A path that climbs steadily is distance, not a fault. A path that is flat and then jumps is a step.",
                "When a step stands out, open its tab. That page is the step alone. The top chart is the shape of its times. The bottom chart is the times in the order they came back. The pills change which replies those charts are drawn from. All is every reply. IQR is the middle half. Steady is the replies inside the band. Tail is the replies that left it. Read All first. Use the others when All is hard to read, or when you want one part of the story on its own.",
                "What you are looking for is a step you can name. The one that is slower than the steps around it. The one whose ordinary replies already wander. The one that holds until the asks come fast, and then climbs. Name that step, and you have answered where the connection slowed. The rest of the path is context."
            ]),
            new("All",
            new("All",
            [
                "All is every reply that came back. It is the place to start, because it is the grade with nothing hidden.",
                "On the top chart, look at where the tall bars sit. That cluster is how the step usually answers. A bar standing alone, far to the right, is a reply that was much slower than the rest. The rising line is the running share. It climbs fast through the cluster and flattens once most replies are counted. A flat line with a small bar far out means a few slow replies, not a step that is slow all the time.",
                "On the bottom chart, read left to right. A line that stays near the center is a step that held. A hook at the right edge is the step getting slower as the grade went on. That hook is the thing Flood can draw, when the asks come faster than the step will answer. Points above the upper band left the band. They are marked so you can see them.",
                "Use All when you want the whole story, including the slow ones. If the cluster is tight and the hook is small, the step is fine and the tail is noise. If the cluster is wide, or the hook climbs and stays up, the step was under pressure.",
                "\x1call.jpg"
            ]),
            new("IQR",
            [
                "IQR is the middle half of the replies, from the first quarter to the third. The fastest quarter and the slowest quarter are left out of the picture. The replies are still there. This view just does not draw them.",
                "Use it when All is hard to read because a few slow replies stretch the chart. The middle half is how the step behaves most of the time. A tight group here means the step is consistent. A wide group here means the step varies even without the extremes.",
                "What to look for is the width. A middle half that spans a millisecond or two is a steady step. A middle half that spans ten is a step whose ordinary replies already wander. That is a different finding from a tail. A tail is a few bad replies. A wide middle half is the step itself.",
                "IQR only redraws the top chart. The run chart underneath stays the full set, so you can still see the order. Read the top for the body. Read the bottom if you still want the sequence.",
                "\x1ciqr.jpg"
            ]),
            new("Steady",
            [
                "Steady is the replies inside the band. The band is the center line plus and minus the spread the grade computed. Replies past the top of the band are left out of this picture. What remains is the step as it behaves when it is not being pushed.",
                "Use it after you have seen a hook on All and you want to know what the step looks like without that hook. If Steady is a flat line near the center, the step is fine and the hook was the cost of asking too fast. If Steady is still wandering, the step is uneven on its own.",
                "What to look for is the line on the bottom chart. Flat and close to the center is the result you want. A line that steps up and stays up, even inside the band, is a step that got slower and did not come back. That is worth a note even though it never left the band.",
                "A step whose replies are all the same has no spread, so there is no band to cut. Steady shows that step as it is.",
                "\x1csteady.jpg"
            ]),
            new("Tail",
            [
                "Tail is the opposite of Steady. It draws only the replies that left the band, above the top or below the bottom. These are the slow ones, and the rare fast ones, on their own.",
                "Use it when All shows a hook and you want to see only that hook. How many points are there. Do they climb, or are they scattered. A climb at the end of the run is the step falling behind as the asks kept coming. Scattered points are single slow replies, which is ordinary on a long grade.",
                "What to look for is the shape. A rising curve is pressure. The step, or the path in front of it, could not keep up. A handful of points with no pattern is noise. A tail with no points means every reply stayed in the band, which is the best result on this page.",
                "Tail does not mean the data was bad. It means these replies were unlike the rest. Read them as the cost of the grade, or as a step that stumbled, depending on the shape.",
                "\x1ctail.jpg"
            ])
        ]);

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
