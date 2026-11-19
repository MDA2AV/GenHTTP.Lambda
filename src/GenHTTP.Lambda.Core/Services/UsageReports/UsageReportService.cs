using System.Text;
using System.Text.Json;

using GenHTTP.Lambda.Configuration;

using Microsoft.Extensions.Logging;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// Makes the usage report, sends it, and keeps how that went for the panel.
/// </summary>
/// <remarks>
/// Off unless the host gives it an address to send to
/// (<see cref="LambdaOptions.UsageReportUrl"/>) - the community edition does,
/// unless its operator says otherwise. Off, nothing is gathered at all.
///
/// A report that does not arrive is said once as a warning and then, until
/// one arrives again, only at debug level: an installation behind a firewall
/// that keeps it in on purpose would otherwise be told so every day.
/// </remarks>
public sealed class UsageReportService(UsageSurvey survey, UsageReportSender sender, LambdaOptions options, ILogger<UsageReportService> logger)
    : IUsageReportService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private UsageReportStatus _status = UsageReportStatus.Initial(options.UsageReportUrl);

    private bool _failing;

    #region Get-/Setters

    public bool Enabled => options.UsageReportUrl != null;

    public UsageReportStatus Status => Volatile.Read(ref _status);

    #endregion

    #region Functionality

    public async ValueTask<UsageReportStatus> SendAsync(CancellationToken cancellation = default)
    {
        if (options.UsageReportUrl is not { } endpoint)
        {
            return Status;
        }

        var attempted = DateTime.UtcNow;

        string report;

        try
        {
            report = JsonSerializer.Serialize(await survey.TakeAsync(cancellation), Json);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to prepare usage report");

            return Record(Status with { Attempted = attempted, Failure = "the report could not be prepared" });
        }

        var failure = await sender.SendAsync(endpoint, Encoding.UTF8.GetBytes(report), cancellation);

        if (failure == null)
        {
            _failing = false;

            logger.LogInformation("Sent usage report to {Endpoint}", endpoint);

            return Record(Status with { Attempted = attempted, Sent = DateTime.UtcNow, Failure = null, Report = report });
        }

        logger.Log(_failing ? LogLevel.Debug : LogLevel.Warning, "Failed to send usage report to {Endpoint}: {Reason}", endpoint, failure);

        _failing = true;

        return Record(Status with { Attempted = attempted, Failure = failure, Report = report });
    }

    private UsageReportStatus Record(UsageReportStatus status)
    {
        Volatile.Write(ref _status, status);
        return status;
    }

    #endregion

}
