namespace GenHTTP.Lambda.Services.Analytics;

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
