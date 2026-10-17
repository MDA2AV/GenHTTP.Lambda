using GenHTTP.Api.Content.Services;

using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of every lambda on the installation.")]
public sealed record AdminListingResponse(
    [Summary("The lambdas of this page")]
    IReadOnlyList<AdminLambda> Lambdas,
    [Summary("How many lambdas there are")]
    int Total,
    [Summary("How many of them are online")]
    int Deployed,
    [Summary("How many the search matched, which is what the pages count from")]
    int Matched,
    [Summary("The page this is, counting from 1")]
    int Page,
    [Summary("How many pages there are")]
    int Pages
);

/// <remarks>
/// This carries the editor key, so it only ever answers a request that has
/// presented the administration token.
/// </remarks>
[Summary("A lambda as the operator sees it: what is kept about it, and what it has been doing since the server came up.")]
public sealed record AdminLambda(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("Its editor key")]
    string PrivateKey,
    [Summary("`Free`, `Premium` or `Demo`")]
    string Tier,
    [Summary("When it was created")]
    DateTime Created,
    [Summary("When something about it last changed")]
    DateTime Modified,
    [Summary("The version online; absent while none is")]
    int? ActiveVersion,
    [Summary("Its newest version")]
    int? LatestVersion,
    [Summary("How many versions are kept")]
    int Versions,
    [Summary("When it is taken offline unless it is used before then")]
    DateTime? DeployedUntil,
    [Summary("When it is deleted unless it is used before then")]
    DateTime? KeptUntil,
    [Summary("Requests since the server came up")]
    long Requests,
    [Summary("Of those, answered with a server error")]
    long Failed,
    [Summary("When the last request arrived, since the server came up")]
    DateTime? LastSeen,
    [Summary("The domain of its own, if it has one")]
    string? Domain,
    [Summary("Whether it answers at that domain")]
    bool DomainServed
);

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
    [Summary("The tiers it may be moved to")]
    IReadOnlyList<string> Tiers
);

[Summary("The tier to move a lambda to.")]
public sealed record TierRequest(
    [Summary("`Free` or `Premium`, in any case")]
    string Tier
);

[Summary("The domain a lambda is to answer at.")]
public sealed record DomainChangeRequest(
    [Summary("The domain, as it should answer: `shop.example.com`, or `www.example.com` to answer there. Nothing, or an empty one, removes it.")]
    string? Domain
);

[Summary("What is left of a lambda after the operator acted on it.")]
public sealed record LambdaOverviewResponse(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("The version online; absent while none is")]
    int? ActiveVersion,
    [Summary("When it is taken offline unless it is used before then")]
    DateTime? DeployedUntil
);

[Summary("What the operator can switch on or off for the whole installation.")]
public sealed record SettingsModel(
    [Summary("Whether the header of the site links to the enterprise page")]
    bool EnterprisePage,
    [Summary("Whether `/build` offers its text box, where there is an agent")]
    bool BuildBox = true,
    [Summary("Whether the Change section of the editor offers its text box, where there is an agent")]
    bool ChangeBox = true
);

[Summary("What a lambda may have and do, as the operator set it.")]
public sealed record LimitsModel(
    [Summary("What a free lambda, or a demo, may have")]
    TierLimitsModel Free,
    [Summary("What a premium lambda may have; never less than a free one")]
    TierLimitsModel Premium,
    [Summary("How long a free lambda may go without a visit or an edit before it is taken offline, in hours")]
    int OfflineAfterHours,
    [Summary("How long a free lambda may go without a visit or an edit before it is deleted, in hours")]
    int RemovedAfterHours,
    [Summary("How large the picture of a showcase entry may be, in bytes")]
    int ShowcaseImageBytes,
    [Summary("How many requests one client may send to the lambdas in a second")]
    int RequestsPerSecond,
    [Summary("How many builds and changes one address may ask the build agent for in a day")]
    int BuildsPerDay
);

[Summary("What a lambda of one tier may have.")]
public sealed record TierLimitsModel(
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many of its versions are kept")]
    int Versions,
    [Summary("How many features it may have open at once")]
    int Features,
    [Summary("How many days of its analytics its owner is shown, of the year that is kept")]
    int AnalyticsDays
);
