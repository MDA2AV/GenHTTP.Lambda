using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Analytics;

[Summary("What was counted over some days, summed.")]
public sealed record AnalyticsTotalsResponse(
    [Summary("Every request answered - pages, the files they load, calls of an API")]
    long Hits,
    [Summary("The requests that were somebody opening a page - people and programs alike")]
    long Views,
    [Summary("Of the views, those of programs rather than people: crawlers, and whatever names no browser. The pages people opened are the views less these.")]
    long BotViews,
    [Summary("Different people who opened a page, each counted once a day - summed over days, one who came back is counted again")]
    long Visitors,
    [Summary("Requests answered with a server error")]
    long Failed,
    [Summary("Requests answered with a client error, not found included")]
    long Rejected,
    [Summary("What the responses carried, where that was known, in bytes")]
    long Bytes,
    [Summary("The mean time to answer a request, in milliseconds")]
    double AverageMillis
)
{

    public static AnalyticsTotalsResponse Of(AnalyticsTotals totals)
        => new(totals.Hits, totals.Views, totals.BotViews, totals.Visitors, totals.Failed, totals.Rejected, totals.Bytes,
               totals.Hits > 0 ? Math.Round(totals.Millis / totals.Hits, 2) : 0);

}
