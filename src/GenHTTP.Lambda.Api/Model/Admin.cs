using GenHTTP.Api.Content.Documentation;

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
    [Summary("`Sandbox`, `Production`, `System` or `Demo`")]
    string Tier,
    [Summary("What the installation calls its tier on its pages; absent where it goes by its own name")]
    string? TierLabel,
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
    bool DomainServed,
    [Summary("Since when it is blocked; absent while it is not")]
    DateTime? Blocked = null,
    [Summary("Whether it may connect to other servers, and where its request for that stands")]
    ConnectivityResponse? Connectivity = null
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
    [Summary("The tiers it may be moved to: every one but the demos'")]
    IReadOnlyList<string> Tiers,
    [Summary("Its block, while it has one")]
    LambdaBlockResponse? Block = null,
    [Summary("The addresses it was changed from with its editor key lately, most recent first - which a block may block as well. Never one of the installation's own networks, and none where the server keeps no addresses (`LAMBDA_LOG_CLIENT_ADDRESS`).")]
    IReadOnlyList<EditorAddressResponse>? EditorAddresses = null,
    [Summary("The address this request came from as a block would cover it, so the operator does not block themselves; absent where it is on one of the installation's own networks")]
    string? You = null,
    [Summary("Whether the code of its version online, or of its newest, connects to other servers - as far as what is written tells - which it may only where `lambda.connectivity.allowed`")]
    bool Connects = false
);

[Summary("That the operator blocked a lambda, and why.")]
public sealed record LambdaBlockResponse(
    [Summary("Since when")]
    DateTime Since,
    [Summary("Why, in the operator's words; never shown to the owner")]
    string? Reason
);

[Summary("An address a lambda was changed from with its editor key.")]
public sealed record EditorAddressResponse(
    [Summary("The address, as the connection came from it")]
    string Address,
    [Summary("What a block of it covers: the address, or the /64 of an IPv6 one")]
    string Range,
    [Summary("When the lambda was first changed from it")]
    DateTime FirstSeen,
    [Summary("When it was last changed from it, to within an hour")]
    DateTime LastSeen,
    [Summary("Whether a block covers it now")]
    bool Blocked
);

[Summary("Blocks a lambda, and the addresses it was changed from if any are named.")]
public sealed record BlockRequest(
    [Summary("Why, in the operator's words, up to 500 characters. Kept with the block and with the blocks of the addresses; never shown to the owner.")]
    string? Reason = null,
    [Summary("Addresses to refuse as well for two weeks, usually some of the lambda's `editorAddresses`: an IPv4 or IPv6 address - which blocks the /64 it belongs to - or such a /64")]
    IReadOnlyList<string>? Addresses = null
);

[Summary("Every address the server refuses, and the address of whoever asks.")]
public sealed record AddressBlockListing(
    [Summary("The blocks, newest first")]
    IReadOnlyList<AddressBlockResponse> Blocks,
    [Summary("The address this request came from as a block would cover it - so the operator can tell whether one of these is theirs; absent where it is on one of the installation's own networks")]
    string? You
);

[Summary("An address the server refuses, before it does anything else for it - apart from the administration panel.")]
public sealed record AddressBlockResponse(
    [Summary("What `DELETE /admin/blocks/{id}` takes")]
    long Id,
    [Summary("An IPv4 address, or the /64 of an IPv6 address: `2001:db8:1:2::/64`")]
    string Address,
    [Summary("Why, in the operator's words")]
    string? Reason,
    [Summary("The lambda it was blocked for, by the public key it had then; it may have gone since")]
    string? Lambda,
    [Summary("When it was blocked")]
    DateTime Created,
    [Summary("When it is let go of by itself, two weeks after it was blocked")]
    DateTime Expires,
    [Summary("How many requests it refused since the server came up")]
    long Refused,
    [Summary("When it last refused one, since the server came up")]
    DateTime? LastRefused
);

[Summary("The tier to move a lambda to.")]
public sealed record TierRequest(
    [Summary("`Sandbox`, `Production` or `System`, in any case. `Free` and `Premium`, what the sandbox and production were called before, are read as them.")]
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
    [Summary("Whether the site offers the text box that has the agent build an app, where there is an agent")]
    bool BuildBox = true,
    [Summary("Whether the Change section of the editor offers its text box, where there is an agent")]
    bool ChangeBox = true
);

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

[Summary("What a lambda may have and do, as the operator set it.")]
public sealed record LimitsModel(
    [Summary("The tier a new lambda starts in: `Sandbox` or `Production`")]
    string DefaultTier,
    [Summary("What a sandbox lambda, or a demo, may have")]
    TierLimitsModel Sandbox,
    [Summary("What a lambda in production may have; never less than a sandbox one")]
    TierLimitsModel Production,
    [Summary("What a system lambda, one the installation itself depends on, may have")]
    TierLimitsModel System,
    [Summary("How many versions of a lambda are kept, whatever its tier; older ones are removed, never the one online")]
    int Versions,
    [Summary("How many features a lambda may have open at once, whatever its tier")]
    int Features,
    [Summary("How long a sandbox lambda may go without a visit or an edit before it is taken offline, in hours")]
    int OfflineAfterHours,
    [Summary("How long a sandbox lambda may go without a visit or an edit before it is deleted, in hours")]
    int RemovedAfterHours,
    [Summary("How large the picture of a showcase entry may be, in bytes")]
    int ShowcaseImageBytes,
    [Summary("How many requests one client may send to the lambdas in a second")]
    int RequestsPerSecond,
    [Summary("How many builds and changes one address may ask the build agent for in a day")]
    int BuildsPerDay
);

[Summary("What a lambda of one tier may have, and what the tier is called.")]
public sealed record TierLimitsModel(
    [Summary("What a version may come to, its code and its resources together, in bytes")]
    long BuildBytes,
    [Summary("The room its data may take, its database and its workspace together, in bytes")]
    long DataBytes,
    [Summary("How many days of its analytics its owner is shown, of the year that is kept")]
    int AnalyticsDays,
    [Summary("Whether a lambda in it may connect to other servers while the operator decided nothing about it. A decision about the lambda - allowed or declined - comes first either way.")]
    bool Connectivity,
    [Summary("What the tier is called on the installation's pages, shown as written in every language, up to 24 characters; absent or empty for its own name, which the pages translate")]
    string? Label = null
);
