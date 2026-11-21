using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What waits for the operator.")]
public sealed record AdminAttentionResponse(
    [Summary("How many lambdas ask to connect to other servers and wait for a decision")]
    int ConnectivityWaiting,
    [Summary("Since when the one waiting longest asks; absent while none waits")]
    DateTime? ConnectivityWaitingSince,
    [Summary("How many addresses are blocked")]
    int AddressBlocks,
    [Summary("Whether the address this request came from is blocked - only this panel answers it then")]
    bool YouAreBlocked,
    [Summary("Whether the installation sends a usage report")]
    bool UsageReports,
    [Summary("Why the last usage report did not arrive; absent if it did, or none is sent")]
    string? UsageReportFailure
);
