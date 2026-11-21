using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The installation's daily usage report: whether it is sent, where to, how the last one fared and what it said.")]
public sealed record UsageReportResponse(
    [Summary("Whether the installation sends a usage report once a day")]
    bool Enabled,
    [Summary("Where it is sent; absent while none is")]
    string? Endpoint,
    [Summary("When the last one was made, whether or not it arrived; absent before the first")]
    DateTime? Attempted,
    [Summary("When the last one arrived; absent while none has")]
    DateTime? Sent,
    [Summary("Why the last attempt did not arrive; absent if it did")]
    string? Failure,
    [Summary("The last report made, as the JSON that was sent; absent before the first")]
    string? Report
);
