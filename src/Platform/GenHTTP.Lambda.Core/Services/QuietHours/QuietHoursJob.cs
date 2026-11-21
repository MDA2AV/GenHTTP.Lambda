using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.QuietHours;

/// <summary>
/// Writes down what the requests counted towards the quiet hours, hourly.
/// </summary>
/// <remarks>
/// The only writer of the quiet hours, and off every request. A server that
/// stops loses what was counted since its last run - an hour at most, which
/// an average over weeks does not notice.
/// </remarks>
public sealed class QuietHoursJob(IQuietHoursService quietHours, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Quiet hours";

    public TimeSpan Interval => options.QuietHoursInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation) => await quietHours.FlushAsync(DateTime.UtcNow, cancellation);

}
