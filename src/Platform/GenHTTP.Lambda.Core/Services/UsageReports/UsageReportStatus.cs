namespace GenHTTP.Lambda.Services.UsageReports;

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
