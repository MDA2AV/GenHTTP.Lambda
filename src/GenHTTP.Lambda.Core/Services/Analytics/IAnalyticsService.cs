namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// How a lambda's visitors reach it, counted by day for its owner where the
/// owner asked for it.
/// </summary>
/// <remarks>
/// Opt-in: nothing is counted of a lambda until its owner switches analytics
/// on, and switching them off deletes what was counted. A year is kept of
/// every lambda; how much of it the owner is shown depends on the tier
/// (<c>TierLimits.AnalyticsDays</c>), so a lambda that becomes premium shows
/// what was counted before it did.
///
/// Apart from the figures of the overview and the stats, which are counted in
/// memory since the server came up and are there for every lambda: these are
/// kept, and are about the audience rather than the program - who came, from
/// where, to which pages, on what.
/// </remarks>
public interface IAnalyticsService
{

    /// <summary>
    /// What the lambda behind an editor key has counted, over the last days.
    /// </summary>
    /// <param name="days">How many days, today included - at most what its tier shows, which is also what is shown without one</param>
    /// <param name="now">What time it is, which tests move</param>
    LambdaAnalytics Get(string privateKey, int? days = null, DateTime? now = null);

    /// <summary>
    /// Starts counting the visits of a lambda. Nothing happens where they are
    /// counted already.
    /// </summary>
    LambdaAnalytics Enable(string privateKey);

    /// <summary>
    /// Stops counting the visits of a lambda and deletes what was counted.
    /// </summary>
    /// <returns>The public key of the lambda</returns>
    string Disable(string privateKey);

    /// <summary>
    /// Writes down the visits that are waiting, and removes the days that are
    /// no longer kept. Done by a job every few minutes.
    /// </summary>
    /// <param name="now">What time it is, which tests move</param>
    AnalyticsFlush Flush(DateTime? now = null);

}
