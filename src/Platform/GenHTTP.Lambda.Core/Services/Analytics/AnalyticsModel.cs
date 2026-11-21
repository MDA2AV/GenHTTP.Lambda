namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What visits are counted by, besides the day: a table each in the analytics
/// database.
/// </summary>
public enum AnalyticsDimension
{
    /// <summary>The path that was answered, relative to the lambda</summary>
    Pages,

    /// <summary>The host of the page a visitor came from, where it was another site</summary>
    Referrers,

    /// <summary>The country the visitor's address was allocated to, or empty where it is not known</summary>
    Countries,

    /// <summary>desktop, mobile, tablet, bot or other</summary>
    Devices,

    /// <summary>The browser of a person, or the app it was opened in - never of a bot</summary>
    Browsers,

    /// <summary>The operating system of a person, never of a bot</summary>
    Systems,

    /// <summary>
    /// A bot: whose it is where its operator confirms it by its address, "unconfirmed" where it only claimed to be one
    /// of those, and the kind of crawler the list says otherwise
    /// </summary>
    Crawlers
}

/// <summary>
/// The names the dimensions have in the database and in what is handed out.
/// </summary>
public static class AnalyticsDimensions
{

    public static IReadOnlyList<AnalyticsDimension> All { get; } = Enum.GetValues<AnalyticsDimension>();

    /// <summary>
    /// The table of a dimension, which is also its name in an answer.
    /// </summary>
    public static string TableOf(AnalyticsDimension dimension) => dimension switch
    {
        AnalyticsDimension.Pages => "pages",
        AnalyticsDimension.Referrers => "referrers",
        AnalyticsDimension.Countries => "countries",
        AnalyticsDimension.Devices => "devices",
        AnalyticsDimension.Browsers => "browsers",
        AnalyticsDimension.Systems => "systems",
        AnalyticsDimension.Crawlers => "crawlers",
        _ => throw new ArgumentOutOfRangeException(nameof(dimension))
    };

    /// <summary>
    /// How many different values one lambda may add to a dimension in a day,
    /// or nothing where what it can hold is bounded anyway. The rest is
    /// counted under <see cref="Other"/>.
    /// </summary>
    /// <remarks>
    /// Paths and referrers come from whoever sends a request, and a scanner
    /// trying ten thousand paths should not cost ten thousand rows a day.
    /// </remarks>
    public static int? MostOf(AnalyticsDimension dimension) => dimension switch
    {
        AnalyticsDimension.Pages => 200,
        AnalyticsDimension.Referrers => 100,
        _ => null
    };

    /// <summary>
    /// What the values past <see cref="MostOf"/> are counted under.
    /// </summary>
    public const string Other = "(other)";

}

/// <summary>
/// What one lambda's visitors did over a stretch of days.
/// </summary>
/// <param name="Totals">Everything in the stretch, summed</param>
/// <param name="Days">Each day of the stretch, oldest first, days without visits included</param>
/// <param name="Breakdowns">Each dimension's values, the most viewed first, then the most requested</param>
public sealed record AnalyticsReport(
    AnalyticsTotals Totals,
    IReadOnlyList<AnalyticsDay> Days,
    IReadOnlyDictionary<AnalyticsDimension, IReadOnlyList<AnalyticsValue>> Breakdowns
);

/// <summary>
/// What was counted, summed over some days.
/// </summary>
/// <param name="Hits">Every request answered - pages, the files they load, calls of an API</param>
/// <param name="Views">The requests that were somebody opening a page</param>
/// <param name="BotViews">Of the views, those of programs - crawlers, and whatever names no browser - rather than people</param>
/// <param name="Visitors">
/// Different visitors, each counted once a day: summed over several days, a visitor who came back on another day is
/// counted again
/// </param>
/// <param name="Failed">Answered with a server error</param>
/// <param name="Rejected">Answered with a client error, not found included</param>
/// <param name="Bytes">What the responses carried, where that was known</param>
/// <param name="Millis">The time it took to answer them, all together</param>
public sealed record AnalyticsTotals(long Hits, long Views, long BotViews, long Visitors, long Failed, long Rejected, long Bytes, double Millis)
{

    public static AnalyticsTotals None { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);

    public AnalyticsTotals Add(AnalyticsTotals other)
        => new(Hits + other.Hits, Views + other.Views, BotViews + other.BotViews, Visitors + other.Visitors, Failed + other.Failed,
               Rejected + other.Rejected, Bytes + other.Bytes, Millis + other.Millis);

}

/// <summary>
/// One day of a lambda's visits, in UTC.
/// </summary>
public sealed record AnalyticsDay(DateOnly Date, AnalyticsTotals Totals);

/// <summary>
/// One value of a dimension and how often it came up.
/// </summary>
/// <param name="Value">The path, the host, the country code or the name</param>
/// <param name="Hits">Requests it was part of</param>
/// <param name="Views">Pages opened it was part of</param>
/// <param name="BotViews">Of those, the pages programs opened rather than people</param>
public sealed record AnalyticsValue(string Value, long Hits, long Views, long BotViews);

/// <summary>
/// Whether a lambda's visits are counted, and what its owner may see of them.
/// </summary>
/// <param name="PublicKey">The lambda</param>
/// <param name="Since">When counting started, or nothing while it is off</param>
/// <param name="Shown">How many days this answer covers, today included</param>
/// <param name="Window">The most days the lambda's tier shows</param>
/// <param name="Kept">How many days are kept, whatever the tier shows</param>
/// <param name="From">The first day of the answer</param>
/// <param name="To">The last day of the answer, today</param>
/// <param name="Earliest">The first day anything was counted on, which may lie before what the tier shows</param>
/// <param name="Report">What was counted from <paramref name="From"/> to <paramref name="To"/>, or nothing while off</param>
/// <param name="Previous">The same number of days before, where the tier shows those too</param>
/// <param name="Updated">When counts were last written, or nothing since the server came up</param>
/// <param name="Interval">How often they are written</param>
/// <param name="Confirmed">The operators whose crawlers are confirmed by their addresses, as far as those are known</param>
public sealed record LambdaAnalytics(
    string PublicKey,
    string Tier,
    DateTime? Since,
    int Shown,
    int Window,
    int Kept,
    DateOnly From,
    DateOnly To,
    DateOnly? Earliest,
    AnalyticsReport? Report,
    AnalyticsTotals? Previous,
    DateTime? Updated,
    TimeSpan Interval,
    IReadOnlyList<string> Confirmed
);

/// <summary>
/// What one writing of the counts came to.
/// </summary>
/// <param name="Visits">The visits taken off the queue</param>
/// <param name="Counted">Those of lambdas whose owners count them, which were written</param>
/// <param name="Dropped">Visits not queued since the last writing, because the queue was full</param>
public sealed record AnalyticsFlush(int Visits, int Counted, long Dropped);
