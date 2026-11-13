using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// Writes down the visits the owners' analytics count, every few minutes.
/// </summary>
/// <remarks>
/// The only writer of the analytics database, and off every request: what
/// the requests queued since the last time is summed into a row per lambda,
/// day and value and written in one transaction. A server that stops loses
/// what was queued since.
/// </remarks>
public sealed class AnalyticsJob(IAnalyticsService analytics, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Analytics";

    public TimeSpan Interval => options.AnalyticsInterval;

    public ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        analytics.Flush();

        return ValueTask.CompletedTask;
    }

}
