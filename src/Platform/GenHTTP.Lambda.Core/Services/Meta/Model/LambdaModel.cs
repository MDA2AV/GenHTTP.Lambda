using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// Everything the editor needs to know about a lambda.
/// </summary>
/// <param name="DeployedUntil">When it goes offline unless used, or nothing while it is offline or its tier keeps it online</param>
/// <param name="KeptUntil">When it is removed unless used, or nothing when its tier keeps it</param>
/// <param name="Domain">The domain it is configured to answer at, whether or not its tier lets it</param>
/// <param name="View">How its editor opens for somebody who has not chosen a view of their own</param>
/// <param name="PublicUrl">Its address below the hosting domain, which sends visitors on to its domain while it answers at one</param>
/// <param name="Address">Where to link to it: its domain while that is served, its address below the hosting domain otherwise</param>
/// <param name="Blocked">Since when the operator has blocked it, or nothing</param>
/// <param name="BlockReason">Why, in the operator's words - which its owner reads as well</param>
/// <param name="Connectivity">Whether it may connect to other servers, and where its request for that stands</param>
/// <param name="Policy">What its tier lets it have and how it is kept, as the installation set it</param>
/// <param name="Above">The tier it would be moved to for more, or nothing where there is none</param>
public sealed record LambdaInfo(
    string PublicKey,
    string PrivateKey,
    string Tier,
    TierPolicy Policy,
    TierPolicy? Above,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    DateTime? DeployedAt,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    string? Domain,
    string View,
    string PublicUrl,
    string Address,
    DateTime? Blocked = null,
    string? BlockReason = null,
    ConnectivityInfo? Connectivity = null
);

/// <summary>
/// An address a lambda was changed from with its editor key.
/// </summary>
/// <param name="FirstSeen">When it was first changed from there</param>
/// <param name="LastSeen">When it was last changed from there, to within an hour</param>
public sealed record EditorAddress(string Address, DateTime FirstSeen, DateTime LastSeen);

/// <summary>
/// One page of the lambdas on the installation.
/// </summary>
/// <param name="Lambdas">The page that was asked for</param>
/// <param name="Matched">How many the search found, which is what the pages are counted from</param>
/// <param name="Total">How many there are in total, search or no search</param>
/// <param name="Deployed">How many of the total are online</param>
public sealed record LambdaPage(
    IReadOnlyList<LambdaOverview> Lambdas,
    int Matched,
    int Total,
    int Deployed
);

/// <summary>
/// A stored version of the code of a lambda.
/// </summary>
/// <param name="Specification">What the user wanted and why, where whoever saved it said</param>
/// <param name="Change">What it changed, in a line</param>
/// <param name="Origin">Where it came from: template, api or agent</param>
public sealed record LambdaVersionInfo(int Version, DateTime Created, string? Specification = null, string? Change = null, string? Origin = null);

/// <summary>
/// A stored version, with its files - whose content is read by whoever needs it.
/// </summary>
public sealed record LambdaVersionContent(int Version, DateTime Created, IReadOnlyList<VersionFile> Files, string? Specification = null, string? Change = null,
                                          string? Origin = null);

/// <summary>
/// Why a version was written, as whoever wrote it tells it.
/// </summary>
/// <remarks>
/// Both parts are optional and neither is checked against the code: they are
/// the account of the author, kept beside what the author did. The origin is
/// not theirs to give - it is filled in by the door the version came through.
/// </remarks>
/// <param name="Specification">What the user wants from the version and why, in their words where possible</param>
/// <param name="Change">What the version changed, in a line</param>
/// <param name="Origin">Which door it came through</param>
public sealed record VersionNote(string? Specification = null, string? Change = null, string? Origin = null)
{

    /// <summary>
    /// How long a specification may be before the rest of it is cut.
    /// </summary>
    public const int MaxSpecification = 4000;

    /// <summary>
    /// How long the line about the change may be.
    /// </summary>
    public const int MaxChange = 500;

}

/// <summary>
/// One stretch of time a version was online.
/// </summary>
/// <param name="Origin">Who put it online: api, agent or admin</param>
/// <param name="Ended">When it went offline, or nothing while it is online</param>
/// <param name="EndedBy">replaced, stopped, expired or admin</param>
public sealed record LambdaActivation(int Version, DateTime Started, string? Origin, DateTime? Ended, string? EndedBy);

/// <summary>
/// A lambda that has been looked up by its public key and is ready to run.
/// </summary>
/// <param name="Tier">Read on every request, so a lambda moved to another tier runs under its limits from the next one on</param>
/// <param name="WorkspaceEnabled">Whether its owner left the workspace on, read on every request for the same reason as the tier</param>
/// <param name="Feature">
/// The feature whose preview is being served, or nothing for the lambda itself. A preview runs the feature's files
/// against the feature's copy of the data, and is kept out of the lambda's figures.
/// </param>
/// <param name="AnalyticsSince">
/// Since when its owner has its visits counted, or nothing while analytics are off - read on every request, so
/// switching them on counts from the next one, and switching them off stops at once.
/// </param>
/// <param name="Connectivity">
/// Whether it may connect to other servers, read on every request for the same reason as the tier: it is compiled in,
/// so a decision of the operator builds it again on the next one.
/// </param>
public sealed record ResolvedLambda(long Id, string PublicKey, LambdaTier Tier, int ActiveVersion, DateTime DeployedAt,
                                    bool WorkspaceEnabled = true, ResolvedFeature? Feature = null, DateTime? AnalyticsSince = null,
                                    bool Connectivity = false)
{

    /// <summary>
    /// What it is built with: whether it has a workspace, and whether it may
    /// connect to other servers.
    /// </summary>
    /// <remarks>
    /// Said once, for whatever asks for its handler: a build is served as it
    /// was only to settings that match, so the warm-up at startup and a
    /// request putting these together apart would build it twice.
    /// </remarks>
    public BuildSettings Settings => new(WorkspaceEnabled, Connectivity);

}

/// <summary>
/// The preview of a feature a request was addressed to.
/// </summary>
/// <param name="Id">The identity its files and data are kept under</param>
/// <param name="Key">What it is addressed by</param>
/// <param name="Preview">Which deployment of the preview is online, so a preview deployed again is built again</param>
public sealed record ResolvedFeature(long Id, string Key, int Preview);

/// <summary>
/// Everything anybody may know about a public key: whether it could be
/// claimed, and whether something is already answering there.
/// </summary>
/// <param name="PublicKey">The key as it would be stored</param>
/// <param name="Valid">Whether the key is one a lambda could have</param>
/// <param name="Exists">Whether a lambda has it</param>
/// <param name="Deployed">Whether that lambda is online</param>
/// <param name="Reason">Why it cannot be claimed, if it cannot</param>
public sealed record KeyStatus(string PublicKey, bool Valid, bool Exists, bool Deployed, string? Reason)
{

    /// <summary>
    /// Whether a new lambda could be created with this key.
    /// </summary>
    public bool Available => Valid && !Exists;

}

/// <summary>
/// The outcome of a deployment attempt - either the updated lambda or the
/// reasons why its code did not build.
/// </summary>
public sealed record DeploymentResult(bool Success, LambdaInfo? Lambda, IReadOnlyList<CompilationDiagnostic> Diagnostics);

/// <summary>
/// What a maintenance run has cleaned up.
/// </summary>
public sealed record MaintenanceReport(int Undeployed, int Deleted);

/// <summary>
/// How much the platform is holding, for the telemetry page.
/// </summary>
public sealed record LambdaCounts(int Lambdas, int Deployed, int Versions);

/// <summary>
/// One lambda as the administration panel lists it. No private key: the panel
/// acts on lambdas it does not own, and handing out the editor link of every
/// lambda on the server would make that irreversible for their owners.
/// </summary>
/// <param name="PrivateKey">
/// The editor key. Only ever leaves the process through the panel, which is
/// behind the administration token.
/// </param>
public sealed record LambdaOverview(
    string PublicKey,
    string PrivateKey,
    string Tier,
    TierPolicy Policy,
    DateTime Created,
    DateTime Modified,
    int? ActiveVersion,
    int? LatestVersion,
    int Versions,
    DateTime? DeployedUntil,
    DateTime? KeptUntil,
    string? Domain,
    DateTime? Blocked = null,
    ConnectivityInfo? Connectivity = null,
    LambdaUses? Uses = null,
    DateTime? Visited = null
);

/// <summary>
/// Which of the platform's features a lambda uses, as far as the platform's
/// database tells - what the operator would otherwise open the editor for.
/// </summary>
/// <param name="AnalyticsSince">Since when its visits are counted, or nothing while they are not</param>
/// <param name="Showcase">Whether its owner put it in the showcase</param>
/// <param name="Source">Whether its owner published its code</param>
/// <param name="Drafts">How many features (drafts) it has open</param>
/// <param name="View">How its editor opens: the simple view for an owner who does not write code</param>
public sealed record LambdaUses(
    bool Database,
    bool Workspace,
    bool Secrets,
    DateTime? AnalyticsSince,
    bool Showcase,
    bool Source,
    int Drafts,
    EditorView View
);

/// <summary>
/// What the operator narrows the listing of the lambdas to, and in which order it comes.
/// </summary>
/// <remarks>
/// Only what the platform's database holds can narrow it: the requests and
/// errors of a lambda are counted in memory, page by page, and a filter on
/// them could not be paged.
/// </remarks>
/// <param name="Search">Only lambdas whose public key or domain contains this</param>
/// <param name="Tier">Only lambdas of this tier</param>
/// <param name="State">Only lambdas in this state</param>
/// <param name="Uses">Only lambdas that use every one of these</param>
/// <param name="View">Only lambdas whose editor opens in this view</param>
/// <param name="CreatedWithin">Only lambdas created in this time, up to now</param>
/// <param name="Demos">
/// Whether the installation's demos are listed - and counted - with the rest;
/// they are where the tier asked for is theirs. Left out otherwise, as every
/// count of the panel leaves them out.
/// </param>
public sealed record LambdaQuery(
    string? Search = null,
    LambdaTier? Tier = null,
    LambdaState? State = null,
    IReadOnlyCollection<LambdaFeature>? Uses = null,
    EditorView? View = null,
    TimeSpan? CreatedWithin = null,
    LambdaOrder Order = LambdaOrder.Newest,
    bool Demos = false
)
{

    /// <summary>
    /// How soon a lambda that goes offline or is removed unless it is used
    /// counts as expiring: soon enough that its owner may not notice in time.
    /// </summary>
    public static TimeSpan ExpiringWithin { get; } = TimeSpan.FromDays(7);

}

/// <summary>
/// A state the listing can be narrowed to.
/// </summary>
public enum LambdaState
{

    /// <summary>A version is online.</summary>
    Online,

    /// <summary>No version is online, and it is not blocked.</summary>
    Offline,

    /// <summary>The operator blocked it.</summary>
    Blocked,

    /// <summary>
    /// A lambda of a tier that is not kept while unused which goes offline or
    /// is removed within <see cref="LambdaQuery.ExpiringWithin"/>, unless it
    /// is used before then.
    /// </summary>
    Expiring

}

/// <summary>
/// One of the platform's features a lambda may use, to narrow the listing to.
/// </summary>
public enum LambdaFeature
{

    Database,

    Workspace,

    Secrets,

    Analytics,

    Showcase,

    Source,

    Domain,

    /// <summary>It has a feature (a draft) open.</summary>
    Drafts,

    /// <summary>Its code may connect to other servers, whether its tier includes it or the operator allowed it.</summary>
    Connects,

    /// <summary>Its owner asked to connect to other servers, and waits for a decision.</summary>
    Requested

}

/// <summary>
/// The orders the listing comes in.
/// </summary>
public enum LambdaOrder
{

    /// <summary>The newest first.</summary>
    Newest,

    /// <summary>The oldest first.</summary>
    Oldest,

    /// <summary>The one changed most recently first.</summary>
    Changed,

    /// <summary>The one visited most recently first, as the sweep last wrote it down; never visited last.</summary>
    Visited,

    /// <summary>By public key, alphabetically.</summary>
    Key

}
