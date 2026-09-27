using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// The last few thousand things this process said, so an operator can read
/// them without a shell on the host.
/// </summary>
/// <remarks>
/// A ring rather than a file: this is for watching a server that is running,
/// not for keeping a record. Docker still has every line on stdout, and that
/// is what survives a restart - nothing here does, on purpose. It holds a
/// fixed number of lines and a fixed amount of text per line, so a lambda in
/// a print loop costs a bounded amount of memory and evicts history rather
/// than growing.
/// </remarks>
public sealed class LogBook
{

    #region Get-/Setters

    /// <summary>
    /// How many lines are kept before the oldest is dropped.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Everything ever written, including what has since been dropped.
    /// </summary>
    public long Written
    {
        get
        {
            lock (Gate)
            {
                return Next - 1;
            }
        }
    }

    private LogLine?[] Lines { get; }

    private Lock Gate { get; } = new();

    /// <summary>
    /// The sequence the next line will be given.
    /// </summary>
    private long Next { get; set; } = 1;

    private int Count { get; set; }

    /// <summary>
    /// How much text is held, in characters.
    /// </summary>
    private long Held { get; set; }

    /// <summary>
    /// How much text it may hold, in characters, before the oldest goes -
    /// whatever the line count says.
    /// </summary>
    /// <remarks>
    /// The line count alone is not a bound on memory: a line may be four
    /// thousand characters and carry a stack trace of its own, and a lambda
    /// throwing on every request is all it takes to ask for a ring of those.
    /// So depth is what is asked for and this is what is actually spent -
    /// long lines simply mean fewer of them fit.
    ///
    /// The default allows half a kilobyte of characters per line of depth,
    /// which is generous against real log lines: a request line is about
    /// seventy. A million of them at the default is therefore a ceiling of a
    /// gigabyte of characters and a reality of a couple of hundred megabytes.
    /// Set it explicitly where that guess is wrong for the installation.
    /// </remarks>
    public long Budget { get; }

    /// <summary>
    /// How long identical lines are gathered together instead of each being
    /// written. Zero writes every one.
    /// </summary>
    /// <remarks>
    /// A scanner knocking on the same five paths every two seconds is not
    /// fifteen hundred facts, it is five facts and a rate - and left alone it
    /// fills the ring and pushes everything worth reading out of the far end.
    /// Measured on this installation before it existed: the panel's own
    /// polling was four lines in every five.
    ///
    /// What the window bounds is how stale a count may be, not how many are
    /// folded into it. While one is open repeats are counted and nothing is
    /// written; the next repeat after it closes is written once, carrying how
    /// many it stands for. So a line already read never changes underneath
    /// the reader - which is what lets this work at all with a cursor that
    /// only ever moves forwards.
    /// </remarks>
    private TimeSpan Fold { get; }

    /// <summary>
    /// Runs of identical lines currently being gathered, by what makes them
    /// identical.
    /// </summary>
    private Dictionary<string, Run> Runs { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// How many runs may be gathered at once. Past it, folding stops and lines
    /// are written as they come - worse than folding, never worse than not
    /// having it.
    /// </summary>
    private const int MostRuns = 4096;

    private sealed class Run
    {
        public DateTime Opened;

        public DateTime Last;

        public int Held;

        /// <summary>
        /// The most recent line of the run, kept so that closing it writes a
        /// real line rather than one rebuilt from its key.
        /// </summary>
        public LogLine? Latest;
    }

    /// <summary>
    /// Where the oldest line sits in the array.
    /// </summary>
    private int Head { get; set; }

    #endregion

    #region Initialization

    /// <param name="capacity">How many lines to keep</param>
    /// <param name="megabytes">
    /// How much memory the text in them may take. Zero takes the default,
    /// which is half a kilobyte of characters for every line of depth.
    /// </param>
    /// <param name="repeatWindow">
    /// How long an identical line is folded into the one before it rather than
    /// written again. Zero writes every line.
    /// </param>
    public LogBook(int capacity = 4000, int megabytes = 0, TimeSpan repeatWindow = default)
    {
        Fold = repeatWindow > TimeSpan.Zero ? repeatWindow : TimeSpan.Zero;

        Capacity = Math.Clamp(capacity, 100, 1_000_000);

        Budget = megabytes > 0
            // a char is two bytes, and this is counted in chars
            ? (long)Math.Clamp(megabytes, 1, 8192) * 1024 * 1024 / 2
            : Math.Max(1_000_000, (long)Capacity * 512);

        /*
         * Allocated up front rather than grown. At a million lines this array
         * alone is eight megabytes of references, which is the price of the
         * depth being asked for - but it is paid once, at startup, rather
         * than in a series of ever larger copies while the server is
         * answering requests.
         */
        Lines = new LogLine?[Capacity];
    }

    #endregion

    #region Functionality

    /// <summary>
    /// How long one line may be before the rest of it is cut.
    /// </summary>
    /// <remarks>
    /// Someone printing a whole response body should not push a hundred other
    /// lines out of the ring to do it.
    /// </remarks>
    public const int MaxText = 4000;

    private const int MaxDetail = 8000;

    /// <summary>
    /// Writes a line and returns the sequence it was given.
    /// </summary>
    /// <returns>
    /// The sequence the line was given, or zero where it was folded into one
    /// already written rather than written itself.
    /// </returns>
    /// <param name="folding">
    /// What makes this line the same as another for the purpose of gathering
    /// repeats: the parts that identify it, without the parts that merely
    /// measure it. Nothing means never fold this line.
    /// </param>
    /// <param name="lambdaId">The identity of the lambda named by <paramref name="lambda"/></param>
    /// <param name="domain">The lambda's own domain the request being served was addressed to</param>
    public long Append(string level, string source, string? lambda, string text, string? detail = null,
                       string? client = null, string? agent = null, string? country = null, string? place = null,
                       string? folding = null, long? lambdaId = null, string? domain = null)
    {
        var repeats = 1;

        // cut outside the lock: every request the server serves is logged, so
        // what is held here is held across all of them
        var at = DateTime.UtcNow;

        var where = Shorten(source, 80)!;

        var said = Shorten(text, MaxText) ?? string.Empty;

        var trace = Shorten(detail, MaxDetail);

        lock (Gate)
        {
            /*
             * What makes two lines the same is given by whoever wrote them,
             * not read off the text.
             *
             * A request line carries how long it took, and no two requests
             * take the same number of microseconds - so keying on the text
             * meant every line was unique and nothing ever folded. Sixty
             * identical requests came out as fifty-eight lines. The caller
             * knows which parts identify the thing and which parts are
             * measurements of it; only the caller can.
             */
            var key = folding == null
                ? null
                : $"{level}\u0000{where}\u0000{lambda}\u0000{lambdaId}\u0000{domain}\u0000{client}\u0000{folding}";

            Run? open = null;

            if (Fold > TimeSpan.Zero && key != null)
            {
                if (Runs.TryGetValue(key, out var run))
                {
                    if (at - run.Opened < Fold)
                    {
                        // inside the window: counted, not written, but the
                        // newest is kept so closing it says something current
                        run.Held++;
                        run.Last = at;
                        run.Latest = new LogLine(0, at, level, where, lambda, said, trace, client, agent, country, place, 1, lambdaId, domain);

                        return 0;
                    }

                    // the window closed, so this one is written and speaks for
                    // the ones held back as well as itself
                    repeats = run.Held + 1;

                    run.Opened = at;
                    run.Last = at;
                    run.Held = 0;

                    open = run;
                }
                else if (Runs.Count < MostRuns)
                {
                    Runs[key] = open = new Run { Opened = at, Last = at, Held = 0 };
                }
            }

            var seq = Next++;

            if (Count == Capacity)
            {
                // full, so the write lands on the oldest and the head moves up
                Drop();
            }

            var line = new LogLine(seq, at, level, where, lambda, said, trace, client, agent, country, place, repeats, lambdaId, domain);

            if (open != null)
            {
                open.Latest = line;
            }

            Lines[(Head + Count) % Capacity] = line;

            Count++;
            Held += Weigh(line);

            // long lines cost more than their place in the ring, so they take
            // more of it with them - but the newest line always stays
            while (Count > 1 && Held > Budget)
            {
                Drop();
            }

            return seq;
        }
    }

    /// <summary>
    /// Everything after the given sequence, oldest first.
    /// </summary>
    /// <param name="since">
    /// The last sequence the reader saw. Zero starts at whatever is still
    /// held, which for a reader that has just arrived is the tail rather than
    /// the whole ring - see <paramref name="limit"/>.
    /// </param>
    /// <param name="lambda">Only this lambda's lines, if given</param>
    /// <param name="minimum">Nothing below this level</param>
    /// <param name="limit">
    /// At most this many, taking the newest and reporting the rest as missed.
    /// A reader behind by more than it can carry is better off jumping to the
    /// tail than working through a backlog it will never catch up with.
    /// </param>
    /// <returns>
    /// The lines, the sequence to ask from next, and how many fell out of the
    /// ring or over the limit before the reader got to them.
    /// </returns>
    /// <param name="lambdaId">
    /// Only the lines of the lambda with this identity, if given. What an
    /// owner's view reads by, since a public key can change hands.
    /// </param>
    public (IReadOnlyList<LogLine> Lines, long Cursor, int Missed) Read(long since, string? lambda, LogLevel minimum, int limit,
                                                                        string? client = null, long? lambdaId = null)
    {
        var wanted = Math.Clamp(limit, 1, 50_000);

        var matched = new List<LogLine>();

        long cursor;

        int missed;

        lock (Gate)
        {
            Settle();

            cursor = Next - 1;

            var oldest = Next - Count;

            // a reader that has been away longer than the ring is deep, or one
            // arriving for the first time, cannot be given what is gone
            missed = since > 0 && since + 1 < oldest ? (int)(oldest - since - 1) : 0;

            for (var i = 0; i < Count; i++)
            {
                var line = Lines[(Head + i) % Capacity];

                if (line == null || line.Seq <= since)
                {
                    continue;
                }

                if (lambda != null && !string.Equals(line.Lambda, lambda, StringComparison.Ordinal))
                {
                    continue;
                }

                if (lambdaId != null && line.LambdaId != lambdaId)
                {
                    continue;
                }

                // a caller that arrived through a proxy is recorded as the
                // claim and the hop together, so asking for either finds it
                if (client != null && (line.Client == null || !line.Client.Contains(client, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (Rank(line.Level) < Rank(minimum))
                {
                    continue;
                }

                matched.Add(line);
            }
        }

        if (matched.Count > wanted)
        {
            /*
             * The newest, because the tail is the point.
             *
             * Only counted as missed for a reader that had a cursor. One
             * arriving without one is asking for the tail, not falling behind
             * it, and telling somebody who has just opened the page that two
             * hundred lines got past them is alarming and untrue.
             */
            if (since > 0)
            {
                missed += matched.Count - wanted;
            }

            matched.RemoveRange(0, matched.Count - wanted);
        }

        return (matched, cursor, missed);
    }

    /// <summary>
    /// Forgets the oldest line held.
    /// </summary>
    private void Drop()
    {
        Held -= Weigh(Lines[Head]);

        Lines[Head] = null;
        Head = (Head + 1) % Capacity;
        Count--;
    }

    private static int Weigh(LogLine? line)
        => line == null ? 0 : line.Text.Length + (line.Detail?.Length ?? 0) + line.Source.Length;

    /// <summary>
    /// Writes out the counts of runs that have stopped, and forgets them.
    /// </summary>
    /// <remarks>
    /// A run that is still going gets its count written by its next repeat.
    /// One that has stopped never would, so the last few of a burst would sit
    /// counted and unseen until the same line happened again - which for a
    /// scanner that has moved on is never. Called on the way into a read,
    /// because that is when somebody is there to care, and because the lock
    /// is already held.
    /// </remarks>
    private void Settle()
    {
        if (Fold <= TimeSpan.Zero || Runs.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;

        List<string>? done = null;

        foreach (var (key, run) in Runs)
        {
            if (now - run.Opened < Fold)
            {
                continue;
            }

            if (run.Held > 0 && run.Latest != null)
            {
                Write(run.Latest, run.Held);

                run.Held = 0;
                run.Opened = now;
            }
            else if (now - run.Last > Fold + Fold)
            {
                // quiet for two windows: the run is over, and holding the key
                // only costs memory
                (done ??= []).Add(key);
            }
        }

        if (done != null)
        {
            foreach (var key in done)
            {
                Runs.Remove(key);
            }
        }
    }

    /// <summary>
    /// Writes the tail of a finished run back out.
    /// </summary>
    private void Write(LogLine latest, int repeats)
    {
        var seq = Next++;

        if (Count == Capacity)
        {
            Drop();
        }

        var line = latest with { Seq = seq, Repeats = repeats };

        Lines[(Head + Count) % Capacity] = line;

        Count++;
        Held += Weigh(line);

        while (Count > 1 && Held > Budget)
        {
            Drop();
        }
    }

    /// <summary>
    /// Every caller the ring still holds, with what they have been doing.
    /// </summary>
    /// <remarks>
    /// Over the whole ring rather than over the page somebody is looking at,
    /// because the question this answers - who is out there and where from -
    /// is about the run and not about the last screenful. One pass, under the
    /// same lock as everything else, which at a million lines is a few
    /// milliseconds: not something to do per request, and fine for a panel
    /// that asks every second and a half.
    ///
    /// Grouped by address rather than by place: two callers in one town are
    /// two callers, and it is the address that identifies one.
    /// </remarks>
    public IReadOnlyList<CallerSummary> Callers(int limit = 500)
    {
        var found = new Dictionary<string, Tally>(StringComparer.Ordinal);

        lock (Gate)
        {
            for (var i = 0; i < Count; i++)
            {
                var line = Lines[(Head + i) % Capacity];

                if (line?.Client == null)
                {
                    continue;
                }

                if (!found.TryGetValue(line.Client, out var tally))
                {
                    if (found.Count >= 20_000)
                    {
                        // a scan from more addresses than this is a number, not
                        // a list, and building the list is the expensive part
                        continue;
                    }

                    found[line.Client] = tally = new Tally { First = line.At };
                }

                tally.Lines += line.Repeats;
                tally.Last = line.At;

                if (line.Level is "error" or "critical")
                {
                    tally.Failed += line.Repeats;
                }

                tally.Place ??= line.Place;
                tally.Country ??= line.Country;
                tally.Agent ??= line.Agent;
            }
        }

        return found.OrderByDescending(e => e.Value.Lines)
                    .Take(Math.Clamp(limit, 1, 5000))
                    .Select(e => new CallerSummary(e.Key, e.Value.Place, e.Value.Country, e.Value.Agent,
                                                   e.Value.Lines, e.Value.Failed, e.Value.First, e.Value.Last))
                    .ToList();
    }

    private sealed class Tally
    {
        public string? Place;
        public string? Country;
        public string? Agent;
        public long Lines;
        public long Failed;
        public DateTime First;
        public DateTime Last;
    }

    /// <summary>
    /// The level names as they are served, lowest first.
    /// </summary>
    public static string NameOf(LogLevel level) => level switch
    {
        LogLevel.Trace => "trace",
        LogLevel.Debug => "debug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "error",
        LogLevel.Critical => "critical",
        _ => "info"
    };

    private static int Rank(string level) => level switch
    {
        "trace" => 0,
        "debug" => 1,
        "info" => 2,
        "warn" => 3,
        "error" => 4,
        "critical" => 5,
        _ => 2
    };

    private static int Rank(LogLevel level) => Rank(NameOf(level));

    private static string? Shorten(string? value, int most)
    {
        if (value == null)
        {
            return null;
        }

        return value.Length <= most ? value : string.Concat(value.AsSpan(0, most), " …");
    }

    #endregion

}
