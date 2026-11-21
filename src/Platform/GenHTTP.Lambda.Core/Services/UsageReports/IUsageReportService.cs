namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// The installation's daily usage report: what it says, where it goes, and
/// how the last one fared.
/// </summary>
public interface IUsageReportService
{

    /// <summary>
    /// Whether reports are sent at all.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// How the last report fared, and what it said.
    /// </summary>
    UsageReportStatus Status { get; }

    /// <summary>
    /// Makes a report and sends it. Does nothing while reports are off.
    /// </summary>
    /// <remarks>
    /// Never throws for a report that did not arrive or could not be made:
    /// that is written down in the status and the log, and the next day's
    /// report tries again.
    /// </remarks>
    ValueTask<UsageReportStatus> SendAsync(CancellationToken cancellation = default);

}

/// <summary>
/// How the last usage report fared.
/// </summary>
/// <param name="Enabled">Whether reports are sent at all</param>
/// <param name="Endpoint">Where they are sent</param>
/// <param name="Attempted">When the last one was made, whether or not it arrived</param>
/// <param name="Sent">When the last one arrived</param>
/// <param name="Failure">Why the last attempt did not arrive; nothing if it did</param>
/// <param name="Report">The last report made, as the JSON that was sent</param>
public sealed record UsageReportStatus(bool Enabled, string? Endpoint, DateTime? Attempted, DateTime? Sent, string? Failure, string? Report)
{

    public static UsageReportStatus Initial(string? endpoint) => new(endpoint != null, endpoint, null, null, null, null);

}
