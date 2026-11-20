using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Integrations;
using GenHTTP.Lambda.Services.Census;
using GenHTTP.Lambda.Services.Integrations;
using GenHTTP.Lambda.Services.Meta.Model;
using GenHTTP.Lambda.Services.Optimization;
using GenHTTP.Lambda.Services.QuietHours;
using GenHTTP.Lambda.Services.Telemetry;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of every lambda on the installation.")]
public sealed record AdminListingResponse(
    [Summary("The lambdas of this page")]
    IReadOnlyList<AdminLambda> Lambdas,
    [Summary("How many lambdas there are - the demos left out, unless they were asked for")]
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
    ConnectivityResponse? Connectivity = null,
    [Summary("Which of the platform's features it uses")]
    AdminLambdaUses? Uses = null,
    [Summary("When it was last visited, as the platform writes it down - at most hourly, so it may be behind `lastSeen`; absent if it never was")]
    DateTime? Visited = null
);

[Summary("Which of the platform's features a lambda uses, as far as the platform's database tells.")]
public sealed record AdminLambdaUses(
    [Summary("Whether its database is switched on")]
    bool Database,
    [Summary("Whether its workspace is switched on")]
    bool Workspace,
    [Summary("Whether its secrets are switched on")]
    bool Secrets,
    [Summary("Since when its visits are counted; absent while they are not")]
    DateTime? AnalyticsSince,
    [Summary("Whether its owner put it in the showcase")]
    bool Showcase,
    [Summary("Whether its owner published its code at `/source`")]
    bool Source,
    [Summary("How many features (drafts) it has open")]
    int Drafts,
    [Summary("How its editor opens for somebody who has not chosen a view of their own: `Simple`, for an owner who does not write code, or `Full`")]
    string View
)
{

    public static AdminLambdaUses Of(LambdaUses uses)
        => new(uses.Database, uses.Workspace, uses.Secrets, uses.AnalyticsSince, uses.Showcase, uses.Source, uses.Drafts, uses.View.ToString());

}

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
    bool ChangeBox = true,
    [Summary("Whether the platform optimizes the databases itself - its own records and analytics, and every lambda's - each at most once a day, in its quiet hours (see `GET /admin/optimization`)")]
    bool OptimizeDatabases = true
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

[Summary("What the installation keeps some of its data in: its database, its files or its secrets.")]
public sealed record IntegrationResponse(
    [Summary("What it keeps: `database` - the platform's records, its analytics and the lambdas' databases; `files` - the lambdas' workspaces; `secrets` - their API keys and passwords")]
    string Kind,
    [Summary("What it is called: `SQLite`, `PostgreSQL`, `Local`, `Database`")]
    string Name,
    [Summary("The assembly it is in, which names it: what a host references to use it, such as `GenHTTP.Lambda.Integrations.Sqlite`")]
    string Assembly,
    [Summary("Where it keeps what it keeps, as label and value - never a password, a key or a connection string")]
    IReadOnlyList<IntegrationDetailResponse> Details
)
{

    public static IntegrationResponse Of(IntegrationState state)
        => new(KindOf(state.Info.Kind), state.Info.Name, state.Assembly, [.. state.Info.Details.Select(d => new IntegrationDetailResponse(d.Label, d.Value))]);

    /// <summary>
    /// What a kind of integration is called on the wire, which the panel reads.
    /// </summary>
    private static string KindOf(IntegrationKind kind) => kind switch
    {
        IntegrationKind.Database => "database",
        IntegrationKind.Files => "files",
        IntegrationKind.Secrets => "secrets",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no such kind of integration.")
    };

}

[Summary("One fact about an integration, such as the server it talks to.")]
public sealed record IntegrationDetailResponse(
    [Summary("What it says, such as `Server`")]
    string Label,
    [Summary("What it is here")]
    string Value
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

/// <remarks>
/// Everything the operator's dashboards draw that is not about the machine:
/// the lambdas counted, what waits for the operator, and the room left. The
/// panel asks for it as it opens a section, which is how the count beside
/// Connectivity stays current without a timer.
/// </remarks>
[Summary("The installation at a glance, for whoever runs it: its lambdas counted, what waits for a decision, and the room left.")]
public sealed record AdminOverviewResponse(
    [Summary("The lambdas counted, the demos apart")]
    AdminCensusResponse Lambdas,
    [Summary("What waits for the operator")]
    AdminAttentionResponse Attention,
    [Summary("The volume of the data directory: how large it is and how much is free, read as this was asked for; absent where the system does not say")]
    AdminDiskResponse? Disk,
    [Summary("The newest lambdas, the newest first")]
    IReadOnlyList<AdminLambda> Newest,
    [Summary("The lambdas that answered with a server error in the last hour, the most first, with what they did in that hour")]
    IReadOnlyList<LambdaActivity> Failing
);

[Summary("The lambdas of the installation, counted. The demos are counted apart and in nothing else.")]
public sealed record AdminCensusResponse(
    [Summary("When they were counted")]
    DateTime Taken,
    [Summary("How many there are, the demos left out")]
    int Total,
    [Summary("How many demos the installation keeps")]
    int Demos,
    [Summary("How many have a version online")]
    int Online,
    [Summary("How many have none and are not blocked")]
    int Offline,
    [Summary("How many the operator blocked")]
    int Blocked,
    [Summary("How many are in each tier, by its name - `Sandbox`, `Production`, `System` and `Demo`")]
    IReadOnlyDictionary<string, int> Tiers,
    [Summary("How many were created in the last day")]
    int CreatedDay,
    [Summary("How many were created in the last seven days")]
    int CreatedWeek,
    [Summary("How many were created in the last thirty days")]
    int CreatedMonth,
    [Summary("How many were changed or visited in the last thirty days")]
    int ActiveMonth,
    [Summary("How many go offline within seven days unless they are used - in a tier that is not kept while unused")]
    int GoingOffline,
    [Summary("How many are removed within seven days unless they are used")]
    int Removed,
    [Summary("How many do either, each counted once - what `GET /admin/lambdas?state=expiring` lists")]
    int Expiring,
    [Summary("How many use each of the platform's features")]
    CensusUses Uses,
    [Summary("How many open in the simple view, for an owner who does not write code")]
    int Simple,
    [Summary("How many open in the full view")]
    int Full,
    [Summary("How many versions are kept, all together")]
    int Versions,
    [Summary("Versions saved in the last thirty days, by where they came from: `agent` (MCP), `api`, `git`, `template`, `admin` or `system`")]
    IReadOnlyDictionary<string, int> Saved
)
{

    public static AdminCensusResponse Of(LambdaCensus census) => new(
        census.Taken, census.Lambdas, census.Demos, census.Online, census.Offline, census.Blocked,
        census.Tiers.ToDictionary(t => t.Key.ToString(), t => t.Value),
        census.CreatedDay, census.CreatedWeek, census.CreatedMonth, census.ActiveMonth, census.GoingOffline, census.Removed, census.Expiring,
        census.Uses, census.Simple, census.Full, census.Versions, census.Saved);

}

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

[Summary("The volume the data directory is on.")]
public sealed record AdminDiskResponse(
    [Summary("How large it is, in bytes")]
    long Size,
    [Summary("How much of it is free for the platform, in bytes")]
    long Free
);

[Summary("The room the installation has, and what takes it.")]
public sealed record AdminStorageResponse(
    [Summary("The data directory, where everything the platform keeps is")]
    string Directory,
    [Summary("The volume it is on, read as this was asked for; absent where the system does not say")]
    AdminDiskResponse? Disk,
    [Summary("When what follows was measured - a walk over every file, done every hour, not on every request")]
    DateTime? Measured,
    [Summary("What the data directory holds, by part, the largest first: `versions`, `drafts`, `databases`, `workspaces`, `git`, `sources` (packed for `/source`), `compiled` and `resources` (what the lambdas were compiled and unpacked to, made again when missing), `platform` and `analytics` (the platform's two databases), `locations` and `crawlers` (the lists the log and the analytics read, downloaded again when missing), and `other`")]
    IReadOnlyList<StoragePart> Parts,
    [Summary("How many lambdas something was found of")]
    int Lambdas,
    [Summary("The lambdas that take the most room, the largest first")]
    IReadOnlyList<AdminLambdaStorage> Largest,
    [Summary("The lambdas whose data is beyond what their tier allows, the furthest first - kept as it is, and written to again only once it is within it. A lambda gets there when it is moved to a tier that allows less, or the operator lowers the limit.")]
    IReadOnlyList<AdminLambdaStorage> Over
)
{

    public static AdminStorageResponse Of(StorageCensus census) => new(
        census.Directory,
        census.DiskBytes is { } size && census.FreeBytes is { } free ? new AdminDiskResponse(size, free) : null,
        census.Measured,
        census.Parts,
        census.Lambdas,
        [.. census.Largest.Select(AdminLambdaStorage.Of)],
        [.. census.Over.Select(AdminLambdaStorage.Of)]);

}

[Summary("What one lambda takes of the room.")]
public sealed record AdminLambdaStorage(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("`Sandbox`, `Production`, `System` or `Demo`")]
    string Tier,
    [Summary("Its database and its workspace, counted as its allowance counts them, in bytes")]
    long DataBytes,
    [Summary("What its tier allows its data, in bytes")]
    long AllowedBytes,
    [Summary("Its versions, as they are kept, in bytes")]
    long VersionBytes,
    [Summary("Its features (drafts), with their copies of the data, in bytes")]
    long DraftBytes,
    [Summary("What is kept for it besides - its git history, its packed source, what it was compiled and unpacked to - in bytes")]
    long OtherBytes,
    [Summary("All of it together, in bytes")]
    long TotalBytes
)
{

    public static AdminLambdaStorage Of(LambdaStorage lambda)
        => new(lambda.PublicKey, lambda.Tier.ToString(), lambda.DataBytes, lambda.AllowedBytes, lambda.VersionBytes, lambda.DraftBytes, lambda.OtherBytes, lambda.TotalBytes);

}
