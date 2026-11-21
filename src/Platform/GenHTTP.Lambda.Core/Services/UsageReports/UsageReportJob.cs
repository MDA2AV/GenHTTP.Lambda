using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Services.Background;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// Sends the usage report once a day, the first right after the server
/// came up.
/// </summary>
/// <remarks>
/// The scheduler starts its jobs once the server is answering, each on its
/// own, so a report that takes its full time limit to fail - a firewall that
/// drops the connection rather than refusing it - holds up nothing.
/// </remarks>
public sealed class UsageReportJob(IUsageReportService reports, LambdaOptions options) : IBackgroundJob
{

    public string Name => "Usage report";

    public TimeSpan Interval => options.UsageReportInterval;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        if (reports.Enabled)
        {
            await reports.SendAsync(cancellation);
        }
    }

}
