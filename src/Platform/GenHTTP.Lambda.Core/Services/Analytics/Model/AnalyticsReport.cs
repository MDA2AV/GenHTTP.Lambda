namespace GenHTTP.Lambda.Services.Analytics;

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
