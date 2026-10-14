using GenHTTP.Lambda.Services.Analytics;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// How a lambda's visitors reach it, as its owner is shown.
/// </summary>
/// <param name="Enabled">Whether its visits are counted; nothing below <paramref name="Earliest"/> is there while they are not</param>
/// <param name="Since">When counting was switched on</param>
/// <param name="Tier">Free, Premium or Demo - what decides how many days are shown</param>
/// <param name="Days">How many days this answer covers, today included</param>
/// <param name="Window">The most days its tier shows</param>
/// <param name="Kept">How many days are kept of every lambda, whatever its tier shows</param>
/// <param name="From">The first day of the answer, in UTC</param>
/// <param name="To">The last day of the answer: today, in UTC</param>
/// <param name="Earliest">The first day anything was counted on - before <paramref name="From"/> where more is kept than shown</param>
/// <param name="Totals">The days of the answer, summed</param>
/// <param name="Previous">The same number of days before, summed - only where the tier shows those as well</param>
/// <param name="Daily">Each day of the answer, oldest first, days without visits included</param>
/// <param name="Pages">The paths that were answered, the most viewed first - a page opened is a view, every request a hit</param>
/// <param name="Referrers">The sites visitors came from, by host</param>
/// <param name="Countries">The countries their addresses were allocated to, by ISO code; empty where not known</param>
/// <param name="Devices">desktop, mobile, tablet, bot or other</param>
/// <param name="Browsers">The browsers of people - bots are not counted here</param>
/// <param name="Systems">The operating systems of people</param>
/// <param name="Crawlers">Whose bots came: search engines, AIs, chats showing a preview</param>
/// <param name="Updated">When visits were last written down; visits since then are not in this answer yet</param>
/// <param name="IntervalSeconds">How often they are written down</param>
/// <param name="ConfirmedCrawlers">
/// The operators whose crawlers are confirmed by the addresses they publish - Google, Bing, OpenAI, Apple, Perplexity -
/// as far as those could be fetched. In <paramref name="Crawlers"/> their names stand for confirmed visits,
/// "unconfirmed" for claims to be theirs from elsewhere, and every other value for a kind of crawler.
/// </param>
public sealed record AnalyticsResponse(
    bool Enabled,
    DateTime? Since,
    string Tier,
    int Days,
    int Window,
    int Kept,
    DateOnly From,
    DateOnly To,
    DateOnly? Earliest,
    AnalyticsTotalsResponse? Totals,
    AnalyticsTotalsResponse? Previous,
    IReadOnlyList<AnalyticsDayResponse>? Daily,
    IReadOnlyList<AnalyticsValueResponse>? Pages,
    IReadOnlyList<AnalyticsValueResponse>? Referrers,
    IReadOnlyList<AnalyticsValueResponse>? Countries,
    IReadOnlyList<AnalyticsValueResponse>? Devices,
    IReadOnlyList<AnalyticsValueResponse>? Browsers,
    IReadOnlyList<AnalyticsValueResponse>? Systems,
    IReadOnlyList<AnalyticsValueResponse>? Crawlers,
    DateTime? Updated,
    int IntervalSeconds,
    IReadOnlyList<string> ConfirmedCrawlers
)
{

    public static AnalyticsResponse Of(LambdaAnalytics analytics)
    {
        var report = analytics.Report;

        IReadOnlyList<AnalyticsValueResponse>? Of(AnalyticsDimension dimension)
            => report?.Breakdowns[dimension].Select(v => new AnalyticsValueResponse(v.Value, v.Hits, v.Views, v.BotViews)).ToList();

        return new AnalyticsResponse(
            analytics.Since != null,
            analytics.Since,
            analytics.Tier,
            analytics.Shown,
            analytics.Window,
            analytics.Kept,
            analytics.From,
            analytics.To,
            analytics.Earliest,
            report == null ? null : AnalyticsTotalsResponse.Of(report.Totals),
            analytics.Previous == null ? null : AnalyticsTotalsResponse.Of(analytics.Previous),
            report?.Days.Select(d => new AnalyticsDayResponse(d.Date, AnalyticsTotalsResponse.Of(d.Totals))).ToList(),
            Of(AnalyticsDimension.Pages),
            Of(AnalyticsDimension.Referrers),
            Of(AnalyticsDimension.Countries),
            Of(AnalyticsDimension.Devices),
            Of(AnalyticsDimension.Browsers),
            Of(AnalyticsDimension.Systems),
            Of(AnalyticsDimension.Crawlers),
            analytics.Updated,
            (int)analytics.Interval.TotalSeconds,
            analytics.Confirmed
        );
    }

}

/// <summary>
/// What was counted over some days, summed.
/// </summary>
/// <param name="Hits">Every request answered - pages, the files they load, calls of an API</param>
/// <param name="Views">The requests that were somebody opening a page - people and programs alike</param>
/// <param name="BotViews">
/// Of the views, those of programs rather than people: crawlers, and whatever names no browser. The pages people
/// opened are the views less these.
/// </param>
/// <param name="Visitors">Different visitors, each counted once a day - summed over days, one who came back is counted again</param>
/// <param name="Failed">Answered with a server error</param>
/// <param name="Rejected">Answered with a client error, not found included</param>
/// <param name="Bytes">What the responses carried, where that was known</param>
/// <param name="AverageMillis">The mean time to answer a request</param>
public sealed record AnalyticsTotalsResponse(
    long Hits,
    long Views,
    long BotViews,
    long Visitors,
    long Failed,
    long Rejected,
    long Bytes,
    double AverageMillis
)
{

    public static AnalyticsTotalsResponse Of(AnalyticsTotals totals)
        => new(totals.Hits, totals.Views, totals.BotViews, totals.Visitors, totals.Failed, totals.Rejected, totals.Bytes,
               totals.Hits > 0 ? Math.Round(totals.Millis / totals.Hits, 2) : 0);

}

/// <summary>
/// One day, in UTC.
/// </summary>
public sealed record AnalyticsDayResponse(DateOnly Date, AnalyticsTotalsResponse Totals);

/// <summary>
/// One value of what visits are counted by.
/// </summary>
/// <param name="Value">The path, the host, the country code or the name; "(other)" gathers what did not fit in a day</param>
/// <param name="Hits">Requests it was part of</param>
/// <param name="Views">Pages opened it was part of</param>
/// <param name="BotViews">Of those, the pages programs opened rather than people</param>
public sealed record AnalyticsValueResponse(string Value, long Hits, long Views, long BotViews);
