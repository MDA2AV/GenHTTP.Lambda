using System.Globalization;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// A typical day as it is kept: what each hour of the day saw, the days
/// further back counting less - and what is read off it.
/// </summary>
/// <remarks>
/// <para>
/// An average per hour of the day, weighted so that a week ago counts half as
/// much as today: what an hour saw is added with the weight of a day, and
/// every weight shrinks with the time that passes, the same for every hour.
/// So 24 numbers hold weeks of days without a row for any of them, a burst
/// that happened once fades within a few weeks, and a lambda nobody visits
/// keeps the shape of its days while its figures go down - nothing has to be
/// written for it to age. Only what was visited is written, and only hourly.
/// </para>
/// <para>
/// What is read is put right for two things the weights alone get wrong. An
/// hour that was last added to nearly a day ago has shrunk for nearly a day
/// longer than the one added to an hour ago, which would make the hours
/// before the last write look quieter than they are; each is read as it was
/// when it was added. And the weights of the first days add up to less than
/// a day's, which would read as fewer requests than there were; each hour is
/// divided by what the days it was counted on add up to.
/// </para>
/// <para>
/// Not precise, and not meant to be: requests counted in the hour before a
/// restart are lost, an hour the server was down counts as one nobody asked
/// for anything in, and the day is one day - a weekend is not told from a
/// weekday. It says when a lambda, or the installation, is usually quiet,
/// which is all it is asked.
/// </para>
/// </remarks>
internal sealed class QuietProfile
{

    /// <summary>
    /// The hours of a day.
    /// </summary>
    public const int Hours = 24;

    /// <summary>
    /// How many hours the quietest stretch is.
    /// </summary>
    public const int Stretch = 3;

    /// <summary>
    /// Every hour of the day, as the bits of <see cref="Covered"/>.
    /// </summary>
    public const int AllHours = (1 << Hours) - 1;

    /// <summary>
    /// How many requests a typical day needs before its hours tell anything
    /// apart: a request an hour, on average.
    /// </summary>
    public const double LeastPerDay = Hours;

    /// <summary>
    /// How much a day counts against the one after it: a week counts half.
    /// </summary>
    private static readonly double Daily = Math.Pow(0.5, 1.0 / 7);

    private readonly double[] _weights;

    #region Get-/Setters

    /// <summary>
    /// When counting began.
    /// </summary>
    public DateTime Since { get; }

    /// <summary>
    /// What the weights are decayed to.
    /// </summary>
    public DateTime Updated { get; private set; }

    /// <summary>
    /// Which hours of the day were counted at all, a bit each from midnight on.
    /// </summary>
    public int Covered { get; private set; }

    #endregion

    #region Initialization

    private QuietProfile(double[] weights, DateTime since, DateTime updated, int covered)
    {
        _weights = weights;

        Since = since;
        Updated = updated;
        Covered = covered;
    }

    /// <summary>
    /// A day nothing was counted of yet, counted from a moment on.
    /// </summary>
    public static QuietProfile Start(DateTime since, DateTime now) => new(new double[Hours], since, now, 0);

    /// <summary>
    /// The day a row keeps.
    /// </summary>
    /// <remarks>
    /// A row that cannot be read is taken for a day nothing was counted of
    /// since it was written, rather than failing whoever asks.
    /// </remarks>
    public static QuietProfile Read(QuietHoursEntity row)
    {
        var weights = new double[Hours];

        var values = row.Hours.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (values.Length == Hours)
        {
            for (var hour = 0; hour < Hours; hour++)
            {
                weights[hour] = double.TryParse(values[hour], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0 ? value : 0;
            }
        }

        return new QuietProfile(weights, row.Since, row.Updated, row.Covered & AllHours);
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Adds what was counted, by the hour of the day, as of a moment.
    /// </summary>
    public void Add(long[] counts, DateTime now)
    {
        Decay(now);

        for (var hour = 0; hour < Hours; hour++)
        {
            _weights[hour] += (1 - Daily) * counts[hour];
        }
    }

    /// <summary>
    /// Marks the hours of the day as counted that lie wholly between two
    /// moments the server was counting at.
    /// </summary>
    public void Cover(DateTime from, DateTime to)
    {
        // the first hour that began at or after the start
        var start = new DateTime(from.Ticks - from.Ticks % TimeSpan.TicksPerHour, DateTimeKind.Utc);

        if (start < from)
        {
            start = start.AddHours(1);
        }

        for (var hour = start; hour.AddHours(1) <= to && Covered != AllHours; hour = hour.AddHours(1))
        {
            Covered |= 1 << hour.Hour;
        }
    }

    /// <summary>
    /// Writes the day into its row.
    /// </summary>
    public void WriteTo(QuietHoursEntity row)
    {
        row.Hours = string.Join(' ', _weights.Select(w => w.ToString("0.####", CultureInfo.InvariantCulture)));
        row.Covered = Covered;
        row.Since = Since;
        row.Updated = Updated;
    }

    /// <summary>
    /// What the day says as of a moment: the requests of each hour, and the
    /// quietest of them.
    /// </summary>
    /// <param name="covered">The hours counted at all - the installation's, which counts whenever the server does</param>
    public TypicalDay Describe(DateTime now, int covered)
    {
        var hours = Typical(now);

        var total = hours.Sum();

        var from = Quietest(hours);

        var quiet = Enumerable.Range(0, Stretch).Sum(i => hours[(from + i) % Hours]);

        var known = (covered & AllHours) == AllHours && now - Since >= TimeSpan.FromDays(1) && total >= LeastPerDay;

        return new TypicalDay(known, [.. hours.Select(h => Math.Round(h, 2))], from, Stretch, total > 0 ? Math.Round(quiet / total, 4) : 0, Since, Updated);
    }

    /// <summary>
    /// The requests of each hour of a typical day, as of a moment.
    /// </summary>
    private double[] Typical(DateTime now)
    {
        var typical = new double[Hours];

        // what every weight has shrunk by since the row was written, the same
        // for every hour: a lambda nobody visits goes down as a whole
        var aged = Math.Pow(Daily, Math.Max(0, (now - Updated).TotalDays));

        var written = Updated.TimeOfDay.TotalHours;

        for (var hour = 0; hour < Hours; hour++)
        {
            // how long before the write the hour was last over, and added to:
            // the hour the write fell in was being counted as it was written
            var over = hour == Updated.Hour ? 0 : ((written - (hour + 1)) % Hours + Hours) % Hours;

            var last = Updated.AddHours(-over);

            if (last < Since)
            {
                // not over once since counting began
                continue;
            }

            // how often it was added to: what those days weigh together is
            // what its weight is read against, so an hour the first days saw
            // once is not read as fewer requests than one they saw twice
            var times = Math.Floor((last - Since).TotalDays) + 1;

            typical[hour] = _weights[hour] / (Math.Pow(Daily, over / Hours) * (1 - Math.Pow(Daily, times))) * aged;
        }

        return typical;
    }

    /// <summary>
    /// The first hour of the quietest stretch of hours, which may wrap past
    /// midnight - the earliest of them where several are as quiet.
    /// </summary>
    private static int Quietest(double[] hours)
    {
        var best = 0;
        var least = double.MaxValue;

        for (var from = 0; from < Hours; from++)
        {
            var sum = 0.0;

            for (var i = 0; i < Stretch; i++)
            {
                sum += hours[(from + i) % Hours];
            }

            // within a rounding of each other, the earlier one stays
            if (sum < least - 1e-9)
            {
                least = sum;
                best = from;
            }
        }

        return best;
    }

    private void Decay(DateTime now)
    {
        var days = (now - Updated).TotalDays;

        if (days <= 0)
        {
            return;
        }

        var factor = Math.Pow(Daily, days);

        for (var hour = 0; hour < Hours; hour++)
        {
            _weights[hour] *= factor;
        }

        Updated = now;
    }

    #endregion

}
