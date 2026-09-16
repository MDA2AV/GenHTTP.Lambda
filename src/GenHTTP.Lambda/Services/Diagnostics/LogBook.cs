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
    /// How much it may hold before the oldest goes, whatever the line count.
    /// </summary>
    /// <remarks>
    /// The line count alone is not a bound on memory. Four thousand lines of
    /// four thousand characters each, every one carrying a stack trace, is
    /// sixty megabytes - and a lambda throwing on every request is all it
    /// takes to ask for that. Half a kilobyte a line on average is generous
    /// for a log, so this is what the ring costs at the stated depth and long
    /// lines simply mean fewer of them.
    /// </remarks>
    private long Budget { get; }

    /// <summary>
    /// Where the oldest line sits in the array.
    /// </summary>
    private int Head { get; set; }

    #endregion

    #region Initialization

    public LogBook(int capacity = 4000)
    {
        Capacity = Math.Clamp(capacity, 100, 200_000);
        Budget = Math.Max(1_000_000, (long)Capacity * 512);
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
    public long Append(string level, string source, string? lambda, string text, string? detail = null)
    {
        // cut outside the lock: every request the server serves is logged, so
        // what is held here is held across all of them
        var at = DateTime.UtcNow;

        var where = Shorten(source, 80)!;

        var said = Shorten(text, MaxText) ?? string.Empty;

        var trace = Shorten(detail, MaxDetail);

        lock (Gate)
        {
            var seq = Next++;

            if (Count == Capacity)
            {
                // full, so the write lands on the oldest and the head moves up
                Drop();
            }

            var line = new LogLine(seq, at, level, where, lambda, said, trace);

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
    public (IReadOnlyList<LogLine> Lines, long Cursor, int Missed) Read(long since, string? lambda, LogLevel minimum, int limit)
    {
        var wanted = Math.Clamp(limit, 1, 5000);

        var matched = new List<LogLine>();

        long cursor;

        int missed;

        lock (Gate)
        {
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
