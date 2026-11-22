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
