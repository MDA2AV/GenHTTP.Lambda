using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Services.Analytics;

[Summary("What was counted of a lambda's visitors, as its owner is shown it.")]
[Remarks("While counting is switched off, everything from `totals` to `crawlers` is absent.")]
public sealed record AnalyticsResponse(
    [Summary("Whether its visits are counted")]
    bool Enabled,
    [Summary("When counting was switched on")]
    DateTime? Since,
    [Summary("`Sandbox`, `Production`, `System` or `Demo` - which decides how many days are shown")]
    string Tier,
    [Summary("How many days this answer covers, today included")]
    int Days,
    [Summary("The most days its tier shows")]
    int Window,
    [Summary("How many days are kept of every lambda, whatever its tier shows")]
    int Kept,
    [Summary("The first day of the answer, in UTC")]
    DateOnly From,
    [Summary("The last day of the answer: today, in UTC")]
    DateOnly To,
    [Summary("The first day anything was counted on - before `from` where more is kept than shown")]
    DateOnly? Earliest,
    [Summary("The days of the answer, summed")]
    AnalyticsTotalsResponse? Totals,
    [Summary("The same number of days before, summed - only where the tier shows those as well")]
    AnalyticsTotalsResponse? Previous,
    [Summary("Each day of the answer, oldest first, days without visits included")]
    IReadOnlyList<AnalyticsDayResponse>? Daily,
    [Summary("The paths that were answered, the most viewed first")]
    IReadOnlyList<AnalyticsValueResponse>? Pages,
    [Summary("The sites people came from, by host")]
    IReadOnlyList<AnalyticsValueResponse>? Referrers,
    [Summary("The countries their addresses are registered in, by ISO code; empty where not known")]
    IReadOnlyList<AnalyticsValueResponse>? Countries,
    [Summary("`desktop`, `mobile`, `tablet`, `bot` or `other`")]
    IReadOnlyList<AnalyticsValueResponse>? Devices,
    [Summary("The browsers of people - programs are not counted here")]
    IReadOnlyList<AnalyticsValueResponse>? Browsers,
    [Summary("The operating systems of people")]
    IReadOnlyList<AnalyticsValueResponse>? Systems,
    [Summary("Whose programs came: search engines, AIs, chats showing a preview. The names in `confirmedCrawlers` stand for visits confirmed by their addresses, `unconfirmed` for claims to be one of them from elsewhere, and every other value for a kind of crawler.")]
    IReadOnlyList<AnalyticsValueResponse>? Crawlers,
    [Summary("When visits were last written down; the visits since then are not in this answer yet")]
    DateTime? Updated,
    [Summary("How often visits are written down, in seconds")]
    int IntervalSeconds,
    [Summary("The operators whose crawlers are confirmed by the addresses they publish - Google, Bing, OpenAI, Apple, Perplexity - as far as those could be fetched")]
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
