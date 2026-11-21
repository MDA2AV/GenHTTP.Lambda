namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What one writing of the counts came to.
/// </summary>
/// <param name="Visits">The visits taken off the queue</param>
/// <param name="Counted">Those of lambdas whose owners count them, which were written</param>
/// <param name="Dropped">Visits not queued since the last writing, because the queue was full</param>
public sealed record AnalyticsFlush(int Visits, int Counted, long Dropped);
