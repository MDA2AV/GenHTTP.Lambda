using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// Everything worth knowing about a lambda at a glance, in one answer.
/// </summary>
/// <remarks>
/// What the control center opens on, and what an agent can ask for to see
/// whether what it deployed is actually working. One document rather than
/// five because the question it answers - is this all right? - is one
/// question.
/// </remarks>
[Summary("Everything worth knowing about a lambda at a glance.")]
public sealed record LambdaSummaryResponse(
    [Summary("The lambda")]
    LambdaResponse Lambda,
    [Summary("The version that is online, if one is")]
    VersionResponse? Live,
    [Summary("The newest version, which may be ahead of the one online")]
    VersionResponse? Latest,
    [Summary("How many versions are kept")]
    int Versions,
    [Summary("Since when the version online has been online; absent while none is")]
    ActivationResponse? Activation,
    [Summary("How much it is used, and how well it answers")]
    TrafficSummary Traffic,
    [Summary("The last few errors and warnings of its own log, newest first - not the requests that were merely refused")]
    IReadOnlyList<OwnerLogEntry> RecentProblems,
    [Summary("What it keeps, and how much of its allowances that takes")]
    StorageSummary Storage,
    [Summary("What its tier allows it")]
    SummaryLimits Limits,
    [Summary("What the documentation of the version the storage is about says, and which of its pages it has")]
    DocumentationSummary Documentation,
    [Summary("Whether the code of the version online, or of the newest, connects to other servers - as far as what is written tells. It compiles only where `lambda.connectivity.allowed`.")]
    bool Connects = false
);
