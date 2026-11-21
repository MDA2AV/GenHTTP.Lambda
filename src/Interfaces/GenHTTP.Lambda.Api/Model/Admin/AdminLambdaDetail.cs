using GenHTTP.Api.Content.Documentation;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("Everything about one lambda, for the operator.")]
public sealed record AdminLambdaDetail(
    [Summary("The lambda, as its owner sees it - its editor key included")]
    LambdaResponse Lambda,
    [Summary("What it has been doing since the server came up")]
    LambdaTraffic Traffic,
    [Summary("Its versions, newest first")]
    IReadOnlyList<VersionResponse> Versions,
    [Summary("Every stretch of time it was online, newest first")]
    IReadOnlyList<ActivationResponse> Activations,
    [Summary("The tiers it may be moved to: every one but the demos'")]
    IReadOnlyList<string> Tiers,
    [Summary("Its block, while it has one")]
    LambdaBlockResponse? Block = null,
    [Summary("The addresses it was changed from with its editor key lately, most recent first - which a block may block as well. Never one of the installation's own networks, and none where the server keeps no addresses (`LAMBDA_LOG_CLIENT_ADDRESS`).")]
    IReadOnlyList<EditorAddressResponse>? EditorAddresses = null,
    [Summary("The address this request came from as a block would cover it, so the operator does not block themselves; absent where it is on one of the installation's own networks")]
    string? You = null,
    [Summary("Whether the code of its version online, or of its newest, connects to other servers - as far as what is written tells - which it may only where `lambda.connectivity.allowed`")]
    bool Connects = false,
    [Summary("The lambda at a glance, as its owner's overview shows it: its traffic over the last hour and day, its last problems, what it keeps against its allowances, and what its documentation says")]
    LambdaSummaryResponse? Summary = null,
    [Summary("Which of the platform's features it uses")]
    AdminLambdaUses? Uses = null,
    [Summary("Its entry in the showcase, while it has one")]
    ShowcaseResponse? Showcase = null,
    [Summary("How its code is published at `/source`, where its owner ever published it; `published` says whether it is now")]
    SourceSettingsResponse? Source = null,
    [Summary("Its features (drafts), the most recently changed first")]
    IReadOnlyList<FeatureResponse>? Features = null,
    [Summary("How busy it is on a typical day by the hour, from the requests it answered - not its drafts' previews")]
    TypicalDay? TypicalDay = null,
    [Summary("What the last optimization of its database did; absent before the first, and where it keeps none")]
    OptimizationRecord? Optimization = null
);
