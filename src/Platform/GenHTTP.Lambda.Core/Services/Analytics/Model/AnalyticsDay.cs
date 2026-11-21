namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// One day of a lambda's visits, in UTC.
/// </summary>
public sealed record AnalyticsDay(DateOnly Date, AnalyticsTotals Totals);
