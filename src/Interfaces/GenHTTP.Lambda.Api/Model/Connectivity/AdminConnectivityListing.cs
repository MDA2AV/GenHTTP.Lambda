using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Lambda.Api.Model;

[Summary("The lambdas that asked to connect to other servers, and those that may.")]
public sealed record AdminConnectivityListing(
    [Summary("Those whose request waits for a decision, the longest waiting first")]
    IReadOnlyList<AdminConnectivityEntry> Requested,
    [Summary("Those the operator allowed, the most recently allowed first. Lambdas whose tier connects by default may connect without being listed here.")]
    IReadOnlyList<AdminConnectivityEntry> Allowed
);
