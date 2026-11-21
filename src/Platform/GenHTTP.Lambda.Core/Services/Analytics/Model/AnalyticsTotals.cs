namespace GenHTTP.Lambda.Services.Analytics;

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
